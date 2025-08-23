using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.IO;

namespace MTA.Game
{
    public class VariableVault
    {
        private ConcurrentDictionary<ulong, DynamicVariable> values;
        public bool Changed = false;
        public VariableVault()
        {
            values = new ConcurrentDictionary<ulong, DynamicVariable>();
        }

        public DynamicVariable this[string variable]
        {
            get 
            {
                DynamicVariable var;
                if (!values.TryGetValue(variable.Get64HashCode(), out var))
                {
                    var = new DynamicVariable();
                    this[variable] = var;
                    Changed = true;
                }
                return var;
            }
            set 
            { 
                values[variable.Get64HashCode()] = value;
                Changed = true; 
            }
        }

        public byte[] Serialize()
        {
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(values.Count);
                foreach (var kvp in values)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Type);
                    if (kvp.Value.Type == 1) writer.Write((bool)kvp.Value);
                    else if (kvp.Value.Type == 2) writer.Write((ulong)kvp.Value);
                    else if (kvp.Value.Type == 3) writer.Write((double)kvp.Value);
                    else if (kvp.Value.Type == 4) writer.Write(kvp.Value.ToString());
                    else if (kvp.Value.Type == 5) writer.Write(((DateTime)kvp.Value).Ticks);
                    else if (kvp.Value.Type == 6) { writer.Write(((byte[])kvp.Value).Length); writer.Write((byte[])kvp.Value); }
                }
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    ulong key = reader.ReadUInt64();
                    byte type = reader.ReadByte();
                    if (type == 1) values.Add(key, reader.ReadBoolean());
                    else if (type == 2) values.Add(key, reader.ReadUInt64());
                    else if (type == 3) values.Add(key, reader.ReadDouble());
                    else if (type == 4) values.Add(key, reader.ReadString());
                    else if (type == 5) values.Add(key, DateTime.FromBinary(reader.ReadInt64()));
                    else if (type == 6) values.Add(key, reader.ReadBytes(reader.ReadInt32()));
                }
            }
        }
    }
    public class DynamicVariable
    {
        private byte type;
        public byte Type { get { return type; } }
        private bool boolValue = false;
        private ulong ulongValue = 0;
        private double doubleValue = 0.0;
        private string stringValue = "";
        private DateTime dateValue = DateTime.Now.AddDays(-1);
        private byte[] byteData; 

        #region Conversion in
        public static implicit operator bool(DynamicVariable obj)
        {
            return obj.boolValue;
        }
        public static implicit operator byte(DynamicVariable obj)
        {
            return (byte)obj.ulongValue;
        }
        public static implicit operator short(DynamicVariable obj)
        {
            return (short)obj.ulongValue;
        }
        public static implicit operator ushort(DynamicVariable obj)
        {
            return (ushort)obj.ulongValue;
        }
        public static implicit operator int(DynamicVariable obj)
        {
            return (int)obj.ulongValue;
        }
        public static implicit operator uint(DynamicVariable obj)
        {
            return (uint)obj.ulongValue;
        }
        public static implicit operator long(DynamicVariable obj)
        {
            return (long)obj.ulongValue;
        }
        public static implicit operator ulong(DynamicVariable obj)
        {
            return obj.ulongValue;
        }
        public static implicit operator float(DynamicVariable obj)
        {
            if (obj.type == 2)
            {
                if ((long)obj.ulongValue < 0)
                    return (float)((long)obj.ulongValue);
                return (float)obj.ulongValue;
            }
            return (float)obj.doubleValue;
        }
        public static implicit operator double(DynamicVariable obj)
        {
            if (obj.type == 2)
            {
                if ((long)obj.ulongValue < 0)
                    return (double)((long)obj.ulongValue);
                return (double)obj.ulongValue;
            }
            return obj.doubleValue;
        }
        public static implicit operator string(DynamicVariable obj)
        {
            if (obj.type == 1) return obj.boolValue.ToString();
            else if (obj.type == 2) return obj.ulongValue.ToString();
            else if (obj.type == 3) return obj.doubleValue.ToString();
            else if (obj.type == 5) return obj.dateValue.ToString();
            else if (obj.type == 6) return Convert.ToBase64String(obj.byteData);
            return obj.stringValue.ToString();
        }
        public static implicit operator DateTime(DynamicVariable obj)
        {
            return obj.dateValue;
        }
        public static implicit operator byte[](DynamicVariable obj)
        {
            return obj.byteData;
        }
        #endregion
        #region Conversion out
        public static implicit operator DynamicVariable(bool obj)
        {
            return new DynamicVariable() { type = 1, boolValue = obj };
        }
        public static implicit operator DynamicVariable(byte obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = obj };
        }
        public static implicit operator DynamicVariable(short obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = (ulong)obj };
        }
        public static implicit operator DynamicVariable(ushort obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = obj };
        }
        public static implicit operator DynamicVariable(int obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = (ulong)obj };
        }
        public static implicit operator DynamicVariable(uint obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = obj };
        }
        public static implicit operator DynamicVariable(long obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = (ulong)obj };
        }
        public static implicit operator DynamicVariable(ulong obj)
        {
            return new DynamicVariable() { type = 2, ulongValue = obj };
        }
        public static implicit operator DynamicVariable(float obj)
        {
            return new DynamicVariable() { type = 3, doubleValue = obj };
        }
        public static implicit operator DynamicVariable(double obj)
        {
            return new DynamicVariable() { type = 3, doubleValue = obj };
        }
        public static implicit operator DynamicVariable(string obj)
        {
            return new DynamicVariable() { type = 4, stringValue = obj };
        }
        public static implicit operator DynamicVariable(DateTime obj)
        {
            return new DynamicVariable() { type = 5, dateValue = obj };
        }
        public static implicit operator DynamicVariable(byte[] obj)
        {
            return new DynamicVariable() { type = 6, byteData = obj };
        }
        #endregion

        public override string ToString()
        {
            if (type == 4)
                return stringValue;
            return "";
        }
    }
}