using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Database;

namespace MTA.Game
{
    public class Achievement : Network.Writer
    {
        public const byte File_Size = 13;

        private Byte[] _buffer;
        private Entity Object;

        private BitVector32 BitVector32;
        public Achievement(Entity _obj)
        {
            Object = _obj;
            _buffer = new byte[76];

            WriteUInt16(68, 0, _buffer);
            WriteUInt16(1136, 2, _buffer);
            WriteUInt32(Object.UID, 8, _buffer);

            BitVector32 = new BitVector32(32 * File_Size);

            WriteUInt32(File_Size, 12, _buffer);//count int[]
        }

        public void CreateFlaID(int id)
        {
            int flagid = (int)((id / 100) % 100 - 1) * 32 + (int)(id % 100 - 1);
            AddFlag(flagid);
        }

        public void AddFlag(int flag)
        {
            if (!BitVector32.Contain(flag))
            {
                BitVector32.Add(flag);

                ShowScreen(flag);
                Send();
            }
        }
        public void Send()
        {
            for (byte x = 0; x < BitVector32.bits.Length; x++)
                WriteUInt32(BitVector32.bits[x], (ushort)(16 + 4 * x), _buffer);
            Object.Owner.Send(_buffer);
        }
        private void ShowScreen(int flag)
        {
            uint FRAG_ID = (uint)(10100 + (uint)(100 * (byte)(flag / 32)) + (byte)(flag % 32) + 1);

            byte[] data = new byte[28];
            WriteUInt16(20, 0, data);
            WriteUInt16(1136, 2, data);
            WriteUInt32(2, 4, data);
            WriteUInt32(Object.UID, 8, data);
            WriteUInt32(FRAG_ID, 12, data);
            Object.Owner.Send(data);

            string Mesajje = "" + Object.Name + " received [Achievement " + FRAG_ID + "]! ";
            Network.GamePackets.Message mesaj = new Network.GamePackets.Message(Mesajje, Object.Name, global::System.Drawing.Color.Red, Network.GamePackets.Message.System);
            Object.Owner.Send(mesaj.ToArray());

            if (!Object.Owner.Fake && !Object.Owner.TransferedPlayer)
            {
                Database.MySqlCommand command = new Database.MySqlCommand(MySqlCommandType.SELECT);
                command.Select("entities").Where("UID", (long)Object.Owner.Entity.UID);
                MySqlReader reader = new MySqlReader(command);
                if (reader.Read())
                {

                    command = new Database.MySqlCommand(MySqlCommandType.UPDATE);
                    command.Update("entities").Set("Achievement", Object.MyAchievement.ToString());
                    command.Where("UID", (long)Object.UID).Execute();
                }
            }


        }
        public override string ToString()
        {
            string line = "";
            foreach (uint bits in BitVector32.bits)
                line += bits.ToString() + "#";
            return line;
        }
        public byte[] ToArray()
        {
            return _buffer;
        }
        public byte[] ViewOpen()
        {
            byte[] packet = _buffer.ToArray();
            packet[4] = 1;
            return packet;
        }

        public void Load(string bas_line)
        {
            if (bas_line.Length == 0) return;
            string[] line = bas_line.Split('#');
            for (byte x = 0; x < 13; x++)
                BitVector32.bits[x] = uint.Parse(line[x]);
        }
    }
}
