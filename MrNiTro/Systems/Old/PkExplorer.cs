using System;
using System.IO;
using System.Text;
using MTA.Game;
using MTA.Client;

namespace MTA.Network.GamePackets
{
    public class PkExplorer
    {
        public ushort Size;
        public ushort Type;
        public uint SubType;
        public uint Values;
        public uint MaxCount;
        public Client.GameState client;
        Game.PkExpeliate[] PkValues = new Game.PkExpeliate[0];

        public PkExplorer(byte[] packet, Client.GameState _c)
        {
            client = _c;
            MaxCount = (uint)client.Entity.PkExplorerValues.Count;
            if (MaxCount >= 10)
            {
                MaxCount = 10;
            }

            Values = (uint)client.Entity.PkExplorerValues.Count;
            PkValues = new Game.PkExpeliate[client.Entity.PkExplorerValues.Count];
            client.Entity.PkExplorerValues.Values.CopyTo(PkValues, 0);
            BinaryReader Reader = new BinaryReader(new MemoryStream(packet));
            Size = Reader.ReadUInt16();
            Type = Reader.ReadUInt16();
            SubType = Reader.ReadUInt32();
        }

        public byte[] Build()
        {
            MemoryStream Stream = new MemoryStream();
            BinaryWriter Writer = new BinaryWriter(Stream);

            Writer.Write((ushort)0);
            Writer.Write((ushort)2220);
            Writer.Write((uint)0);
            Writer.Write((uint)SubType);
            Writer.Write((uint)Values);
            Writer.Write((uint)MaxCount);
            foreach (Game.PkExpeliate e in PkValues)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (i < e.Name.Length)
                    {
                        Writer.Write((byte)e.Name[i]);
                    }
                    else
                        Writer.Write((byte)0);
                }

                Writer.Write((uint)e.Times);
                Writer.Write((ushort)e.LostExp);
                Writer.Write((byte)e.Level);
                Writer.Write((byte)0);
                for (int i = 0; i < 16; i++)
                {
                    if (i < e.KilledAt.Length)
                    {
                        Writer.Write((byte)e.KilledAt[i]);
                    }
                    else
                        Writer.Write((byte)0);
                }

                Writer.Write((ulong)0);
                Writer.Write((ulong)0);
                Writer.Write((uint)0);
                Writer.Write((uint)e.Potency);
            }

            int packetlength = (int)Stream.Length;
            Stream.Position = 0;
            Writer.Write((ushort)packetlength);
            Stream.Position = Stream.Length;
            Writer.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            Stream.Position = 0;
            byte[] buf = new byte[Stream.Length];
            Stream.Read(buf, 0, buf.Length);
            Writer.Close();
            Stream.Close();
            return buf;
        }
    }

    public class PKExplorer : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public PKExplorer()
        {
        }

        public byte[] ToArray()
        {
            Buffer = new byte[20 + 4];
            WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
            WriteUInt16(2220, 2, Buffer);
            WriteUInt32(1, 8, Buffer);
            WriteUInt32(1, 12, Buffer);
            WriteUInt32(2, 16, Buffer);
            return Buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }

        public void Send(Client.GameState client)
        {
            client.Send(ToArray());
        }
    }
}

namespace MTA.Game
{
    public class PkExpeliate
    {
        public uint UID;
        public uint killedUID;
        public string Name;
        public uint Times;
        public uint LostExp;
        public byte Level;
        public string KilledAt;
        public uint Potency;
        public DateTime Time;
    }
}

namespace MTA.Database
{
    public class PkExpelTable
    {
        public static void Load(GameState client)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(MySqlCommandType.SELECT);
                command.Select("pk_explorer").Where("uid", (long)client.Entity.UID);
                MySqlReader reader = new MySqlReader(command);
                while (reader.Read())
                {
                    PkExpeliate expeliate = new PkExpeliate();
                    expeliate.UID = reader.ReadUInt32("killed_uid");
                    expeliate.Name = reader.ReadString("killed_name");
                    expeliate.KilledAt = reader.ReadString("killed_map");
                    expeliate.LostExp = reader.ReadUInt32("lost_exp");
                    expeliate.Times = reader.ReadUInt32("times");
                    expeliate.Potency = reader.ReadUInt32("battle_power");
                    expeliate.Level = reader.ReadByte("level");
                    if (!client.Entity.PkExplorerValues.ContainsKey(expeliate.UID))
                        client.Entity.PkExplorerValues.Add(expeliate.UID, expeliate);
                }
            }
            catch (Exception exception)
            {
                Program.SaveException(exception);
            }
        }

        public static void PkExploitAdd(Client.GameState client, Game.PkExpeliate pk)
        {
            try
            {
                MySqlCommand cmds = new MySqlCommand(MySqlCommandType.SELECT);
                cmds.Select("pk_explorer");
                MySqlReader rdr = new MySqlReader(cmds);
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.INSERT);
                    cmd.Insert("pk_explorer").Insert("uid", client.Entity.UID)
                        .Insert("killed_uid", pk.killedUID)
                        .Insert("killed_name", pk.Name)
                        .Insert("killed_map", pk.KilledAt)
                        .Insert("lost_exp", pk.LostExp)
                        .Insert("times", pk.Times)
                        .Insert("battle_power", pk.Potency).Insert("level", pk.Level);
                    cmd.Execute();

                    if (!client.Entity.PkExplorerValues.ContainsKey(pk.UID))
                        client.Entity.PkExplorerValues.Add(pk.UID, pk);
                }
            }
            catch (Exception exception)
            {
                Program.SaveException(exception);
            }
        }

        public static void Update(Client.GameState client, Game.PkExpeliate pk)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
                cmd.Update("pk_explorer")
                    .Set("killed_name", pk.Name)
                    .Set("killed_map", pk.KilledAt)
                    .Set("lost_exp", pk.LostExp)
                    .Set("times", pk.Times)
                    .Set("battle_power", pk.Potency)
                    .Set("level", pk.Level)
                    .Where("uid", client.Entity.UID).And("killed_uid", pk.killedUID);
                cmd.Execute();

                if (!client.Entity.PkExplorerValues.ContainsKey(pk.UID))
                    client.Entity.PkExplorerValues.Add(pk.UID, pk);
            }
            catch (Exception exception)
            {
                Program.SaveException(exception);
            }
        }
    }
}
