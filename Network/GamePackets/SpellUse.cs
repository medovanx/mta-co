using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace MTA.Network.GamePackets
{
    public class Attacked { public ushort X, Y; public MTA.Game.Enums.ConquerAngle Facing; }
    public class TryTrip : Interfaces.IPacket
    {
        public class DamageClass
        {
            public uint Damage;
            public bool Hit;

            public static implicit operator uint(DamageClass dmg)
            {
                return dmg.Damage;
            }
            public static implicit operator DamageClass(uint dmg)
            {
                return new DamageClass() { Damage = dmg, Hit = true };
            }
        }
        byte[] Buffer;

        public TryTrip(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[8 + 20];
                Writer.WriteUInt16(20, 0, Buffer);
                Writer.WriteUInt16(1105, 2, Buffer);
            }
        }

        public uint Attacker
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Writer.WriteUInt32(value, 4, Buffer); }
        }
        public uint Attacked
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Writer.WriteUInt32(value, 8, Buffer); }
        }

        public ushort SpellID
        {
            get { return BitConverter.ToUInt16(Buffer, 12); }
            set { Writer.WriteUInt16(value, 12, Buffer); }
        }

        public ushort SpellLevel
        {
            get { return BitConverter.ToUInt16(Buffer, 14); }
            set { Writer.WriteUInt16(value, 14, Buffer); }
        }

        public SafeDictionary<uint, DamageClass> Targets = new SafeDictionary<uint, DamageClass>();

        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
        }
        public uint Damage
        {
            get { return BitConverter.ToUInt32(Buffer, 24); }
            set { Writer.WriteUInt32(value, 24, Buffer); }

        }
        public byte[] ToArray()
        {
            byte[] buffer = new byte[156];
            Writer.WriteUInt16(148, 0, buffer);
            Writer.WriteUInt16(1105, 2, buffer);
            Writer.WriteUInt32(Attacker, 4, buffer);
            Writer.WriteUInt32(Attacked, 8, buffer);
            Writer.WriteUInt16(SpellID, 12, buffer);
            Writer.WriteUInt16(SpellLevel, 14, buffer);
            Writer.WriteUInt32(3, 19, buffer);
            int offset = 24;
            for (int i = 0; i < 3; i++)
            {
                Writer.WriteUInt32(Attacked, offset, buffer);
                Writer.WriteUInt32(Damage / 3, offset + 4, buffer);
                offset += 32;
            }
            return buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
    public class SpellUse : Interfaces.IPacket
    {
        public class DamageClass
        {
            public uint Damage;
            public bool Hit;
            public ushort newX, newY;
            public Attack.AttackEffects1 Eff1;
            public Attack.AttackEffects2 Eff2;

            public static implicit operator uint(SpellUse.DamageClass dmg)
            {
                return dmg.Damage;
            }

            public static implicit operator SpellUse.DamageClass(uint dmg)
            {
                return new SpellUse.DamageClass { Damage = dmg, Hit = true };
            }
        }
        byte[] Buffer;

        public SpellUse(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[8 + 20];
                Writer.WriteUInt16(20, 0, Buffer);
                Writer.WriteUInt16(1105, 2, Buffer);
                LevelHu = (byte)Kernel.Random.Next(1, 2);
            }
        }
        public byte SoulLevel
        {
            get { return Buffer[20]; }
            set { Writer.WriteByte(value, 20, Buffer); }
        }
        public byte SoulType
        {
            get { return Buffer[21]; }
            set { Writer.WriteByte(value, 21, Buffer); }
        }
        public uint Attacker
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Writer.WriteUInt32(value, 4, Buffer); }
        }
        public uint Attacker1
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Writer.WriteUInt32(value, 8, Buffer); }
        }

        public ushort X
        {
            get { return BitConverter.ToUInt16(Buffer, 8); }
            set { Writer.WriteUInt16(value, 8, Buffer); }
        }

        public ushort Y
        {
            get { return BitConverter.ToUInt16(Buffer, 10); }
            set { Writer.WriteUInt16(value, 10, Buffer); }
        }

        public ushort SpellID
        {
            get { return BitConverter.ToUInt16(Buffer, 12); }
            set { Writer.WriteUInt16(value, 12, Buffer); }
        }

        public ushort SpellLevel
        {
            get { return BitConverter.ToUInt16(Buffer, 14); }
            set { Writer.WriteUInt16(value, 14, Buffer); }
        }

        public byte LevelHu
        {
            get { return Buffer[18]; }
            set
            {

                Buffer[18] = value;
            }
        }
        public byte SpellEffect
        {
            get { return Buffer[20]; }
            set { Writer.WriteByte(value, 20, Buffer); }
        }
        public Attack.AttackEffects1 Effect1
        {
            get;
            set;
        }
        public Attack.AttackEffects2 Effect2
        {
            get;
            set;
        }

        public SafeDictionary<uint, DamageClass> Targets = new SafeDictionary<uint, DamageClass>();

        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
        }
        const int TargetLimit = 30;
        public byte[] ToArray()
        {
            if (Targets.Count <= TargetLimit)
            {
                byte[] buffer = new byte[64 + Targets.Count * 32];
                Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Writer.WriteUInt16(1105, 2, buffer);
                Writer.WriteUInt32(Attacker, 4, buffer);
                Writer.WriteUInt16(X, 8, buffer);
                Writer.WriteUInt16(Y, 10, buffer);

                Writer.WriteUInt16(SpellID, 12, buffer);
                Writer.WriteUInt16(SpellLevel, 14, buffer);

                if (Kernel.GamePool.ContainsKey(Attacker))
                    if (Kernel.GamePool[Attacker].Spells != null)
                        if (Kernel.GamePool[Attacker].Spells[SpellID] != null)
                            LevelHu = Kernel.GamePool[Attacker].Spells[SpellID].LevelHu2;
                Writer.WriteByte(LevelHu, 18, buffer);


                Writer.WriteUInt32((uint)(Targets.Count/* << 8*/), 19, buffer);
                Writer.WriteByte((byte)SpellEffect, 20, buffer);
                if (SpellID == 12400)
                    Writer.WriteUInt32(1, 20, buffer);
                ushort offset = 24;
                foreach (KeyValuePair<uint, DamageClass> target in Targets)
                {
                    Writer.WriteUInt32(target.Key, offset, buffer); offset += 4;
                    Writer.WriteUInt32(target.Value.Damage, offset, buffer); offset += 4;
                    Writer.WriteBoolean(target.Value.Hit, offset, buffer); offset += 4;
                    Writer.WriteByte((Byte)target.Value.Eff1, offset, buffer); offset += 1;
                    Writer.WriteByte((Byte)target.Value.Eff2, offset, buffer); offset += 1;
                    offset += 6;
                    Writer.WriteUInt32(target.Value.newX, offset, buffer); offset += 4;
                    Writer.WriteUInt32(target.Value.newY, offset, buffer); offset += 8;
                }
                return buffer;
            }
            else
            {
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    var array = Targets.ToArray();
                    for (int i = 0; i < array.Length; i += TargetLimit)
                    {
                        int targets = array.Length - i;
                        if (targets > TargetLimit) targets = TargetLimit;

                        byte[] buffer = new byte[64 + targets * 32];
                        Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                        Writer.WriteUInt16(1105, 2, buffer);
                        Writer.WriteUInt32(Attacker, 4, buffer);
                        Writer.WriteUInt16(X, 8, buffer);
                        Writer.WriteUInt16(Y, 10, buffer);
                        Writer.WriteUInt16(SpellID, 12, buffer);
                        Writer.WriteUInt16(SpellLevel, 14, buffer);

                        if (Kernel.GamePool.ContainsKey(Attacker))
                            if (Kernel.GamePool[Attacker].Spells != null)
                                if (Kernel.GamePool[Attacker].Spells[SpellID] != null)
                                    LevelHu = Kernel.GamePool[Attacker].Spells[SpellID].LevelHu2;
                        Writer.WriteByte(LevelHu, 18, buffer);



                        Writer.WriteUInt32((uint)(targets/* << 8*/), 19, buffer);
                        Writer.WriteByte((byte)SpellEffect, 20, buffer);
                        ushort offset = 24;
                        for (int j = 0; j < targets; j++)
                        {
                            KeyValuePair<uint, DamageClass> target = array[i + j];
                            Writer.WriteUInt32(target.Key, offset, buffer); offset += 4;
                            Writer.WriteUInt32(target.Value.Damage, offset, buffer); offset += 4;
                            Writer.WriteBoolean(target.Value.Hit, offset, buffer); offset += 4;
                            Writer.WriteByte((Byte)target.Value.Eff1, offset, buffer); offset += 1;
                            Writer.WriteByte((Byte)target.Value.Eff2, offset, buffer); offset += 1;
                            offset += 6;
                            Writer.WriteUInt32(target.Value.newX, offset, buffer); offset += 4;
                            Writer.WriteUInt32(target.Value.newY, offset, buffer); offset += 8;
                        }
                        Writer.WriteString("TQServer", buffer.Length - 8, buffer);
                        writer.Write(buffer, 0, buffer.Length);
                    }
                    return stream.ToArray();
                }
            }
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }

        /* internal SpellUse AddTarget(uint uid, uint damage, Attack attack)
         {
             DamageClass cls = new DamageClass() { Damage = damage, Hit = true };
             if (attack != null)
             {
                 cls.Eff1 = attack.Effect1;
                 cls.Eff2 = attack.Effect2;
             }
             Targets.Add(uid, cls);
             return this;
         }*/
        internal SpellUse AddTarget(Interfaces.IMapObject obj, DamageClass damage, Attack attack)
        {
            if (attack != null)
            {
                damage.Eff1 = attack.Effect1;
                damage.Eff2 = attack.Effect2;
            }
            if (obj.MapObjType == Game.MapObjectType.SobNpc)
            {
                var attacked = obj as SobNpcSpawn;
                Targets.Add(attacked.UID, damage);
            }
            else if (obj.MapObjType == Game.MapObjectType.Player || obj.MapObjType == Game.MapObjectType.Monster)
            {
                var attacked = obj as Game.Entity;
                Targets.Add(attacked.UID, damage);
            }
            return this;
        }
    }
}