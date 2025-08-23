using System;
using System.Collections.Generic;
using System.Linq;

namespace MTA.Network.GamePackets
{
    public class Spell : Writer, Interfaces.IPacket, Interfaces.ISkill
    {

        public static List<Soul_Level> SkillSoul_values = new List<Soul_Level> { Soul_Level.Default, Soul_Level.Level_One, Soul_Level.Level_Two, Soul_Level.Level_Three, Soul_Level.Level_Four };
        public enum Soul_Level : byte
        {
            Default = 1 << 0,
            LevelOne = 1 << 1,
            LevelTwo = 1 << 2,
            LevelThree = 1 << 3,
            LevelFour = 1 << 4,
            Level_One = Default | LevelOne,
            Level_Two = Level_One | LevelTwo,
            Level_Three = Level_Two | LevelThree,
            Level_Four = Level_Three | LevelFour,
        }
        byte[] Buffer;
        private byte _PreviousLevel;
        private bool _Available;
        public Spell(bool Create)
        {
            Buffer = new byte[32 + 8];
            WriteUInt16(32, 0, Buffer);
            WriteUInt16(1103, 2, Buffer);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
            Souls = Soul_Level.Default;
            // LevelHu = 4;
            //  Buffer[28] = 1;
            _Available = false;
        }

        byte _Levelhu;
        public byte LevelHu
        {
            get { return _Levelhu; }
            set
            {
                _Levelhu = value;
                uint IntegerFlag = 0;
                if (_Levelhu >= 1)
                    IntegerFlag |= (uint)(1UL << 1);
                if (_Levelhu >= 2)
                    IntegerFlag |= (uint)(1UL << 4);
                if (_Levelhu >= 3)
                    IntegerFlag |= (uint)(1UL << 8);
                if (_Levelhu >= 4)
                    IntegerFlag |= (uint)(1UL << 16);
                WriteUInt32(IntegerFlag, 25, Buffer);

            }
        }
        byte _Souls;
        public Soul_Level Souls
        {
            get { return (Soul_Level)_Souls; }
            set
            {

                _Souls = (byte)value;

                if (_Souls == 0)
                    _Souls = (byte)Soul_Level.Default;
                Buffer[24] = (byte)_Souls;
                Buffer[28] = Buffer[24];
            }
        }
        public void AddFlag(Soul_Level flag)
        {
            Souls |= flag;
        }
        public bool ContainsFlag(Soul_Level flag)
        {
            Soul_Level aux = Souls;
            aux &= ~flag;
            return !(aux == Souls);
        }
        public void RemoveFlag(Soul_Level flag)
        {
            if (ContainsFlag(flag))
            {
                Souls &= ~flag;
            }
        }
        public byte Level
        {
            get { return (byte)BitConverter.ToUInt32(Buffer, 16); }
            set
            {
                if (Database.SpellTable.SpellInformations != null)
                    if (Database.SpellTable.SpellInformations.ContainsKey(ID))
                        if (!Database.SpellTable.SpellInformations[ID].ContainsKey(value))
                            value = Database.SpellTable.SpellInformations[ID].Keys.LastOrDefault();
                WriteUInt16(value, 16, Buffer);
            }
        }
        byte _Levelhu2;
        public byte LevelHu2
        {
            get { return _Levelhu2; }
            set
            {
                _Levelhu2 = value;
                Buffer[20] = value;               
            }
        }
        public uint Experience
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public ushort ID
        {
            get { return (ushort)BitConverter.ToUInt16(Buffer, 12); }
            set { WriteUInt16(value, 12, Buffer); }
        }
        public byte PreviousLevel
        {
            get { return _PreviousLevel; }
            set { _PreviousLevel = value; }
        }

        public byte[] ToArray()
        {
            return Buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }

        public void Send(Client.GameState client)
        {
            client.Send(this);
        }

        public bool Available
        {
            get
            {
                return _Available;
            }
            set
            {
                _Available = value;
            }
        }
    }
}
