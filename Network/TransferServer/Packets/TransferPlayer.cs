using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Network.GamePackets;
using MTA.Game;
using System.Collections.Concurrent;
using MTA.Interfaces;
using MTA.Network;
using System.IO;
using MTA.Game.ConquerStructures;

namespace MTA.TransferServer
{
    public class Transfer : Network.Writer
    {
        public byte[] Buff;
        public Transfer(MTA.Client.GameState client, string FromServer)
        {
            Buff = new byte[60000];
            WriteUInt16((ushort)Buff.Length, 0, Buff);
            WriteUInt32(5052, 2, Buff);

            WriteByte((byte)(FromServer.Length), 160, Buff);
            WriteString(FromServer, 161, Buff);
            WriteByte((byte)(client.Entity.Name.Length), 180, Buff);
            WriteString(client.Entity.Name, 181, Buff);

            WriteByte((byte)(client.Account.State), 23999, Buff);
            WriteByte((byte)(client.Account.Username.Length), 24000, Buff);
            WriteString(client.Account.Username, 24001, Buff);
            WriteByte((byte)(client.Account.Password.Length), 24016, Buff);
            WriteString(client.Account.Password, 24017, Buff);
            WriteByte((byte)(client.Account.IP.Length), 24032, Buff);
            WriteString(client.Account.IP, 24033, Buff);

            WriteUInt32(client.Entity.UID, 78, Buff);
            WriteUInt64(client.Entity.Money, 6, Buff);
            WriteUInt32((uint)client.Entity.ConquerPoints, 10, Buff);
            WriteUInt16(client.Entity.Class, 14, Buff);
            WriteUInt16(client.Entity.SecondRebornClass, 16, Buff);
            WriteUInt32(client.Entity.FirstRebornClass, 18, Buff);
            WriteUInt32(client.Entity.Reborn, 22, Buff);
            WriteUInt32(client.Entity.HairStyle, 30, Buff);
            WriteUInt32(client.Entity.Body, 34, Buff);
            WriteUInt32(client.Entity.Level, 38, Buff);
            WriteUInt32(client.Entity.Strength, 42, Buff);
            WriteUInt32(client.Entity.Agility, 46, Buff);
            WriteUInt32(client.Entity.Spirit, 50, Buff);
            WriteUInt32(client.Entity.Vitality, 54, Buff);
            WriteUInt32(client.Entity.Atributes, 58, Buff);
            WriteUInt32(client.Entity.Face, 62, Buff);
            WriteUInt64(client.Entity.QuizPoints, 66, Buff);
            WriteUInt32(client.Entity.VIPLevel, 70, Buff);
            if (client.Entity.SubClasses != null)
                WriteUInt32(client.Entity.SubClasses.StudyPoints, 74, Buff);
            if (client.NobilityInformation != null)
            {
                WriteUInt64(client.NobilityInformation.Donation, 82, Buff);
                WriteByte((byte)client.NobilityInformation.Rank, 90, Buff);
            }
            if (client.Guild != null)
            {
                WriteUInt16(client.Entity.GuildID, 100, Buff);
                WriteUInt16(client.Entity.GuildRank, 102, Buff);
                WriteString(client.Guild.Name, 104, Buff);
                WriteString(client.Guild.LeaderName, 120, Buff);

            }

            byte[] ItemsArray = MTA.Database.ConquerItemTable.GetItemsAraay(client);
            for (ushort x = 0; x < ItemsArray.Length; x++)
                WriteByte(ItemsArray[x], x + 200, Buff);

            ////11000
            byte[] Spells = Database.SkillTable.GetSpellsArray(client);
            WriteUInt32((uint)Spells.Length, 11000, Buff);
            for (ushort x = 0; x < Spells.Length; x++)
                WriteByte(Spells[x], x + 11004, Buff);

            byte[] Profs = Database.SkillTable.GetArrayProfs(client);
            WriteUInt32((uint)Profs.Length, 12000, Buff);
            for (ushort x = 0; x < Profs.Length; x++)
                WriteByte(Profs[x], x + 12004, Buff);
            ////12000


            byte[] Artefacts = MTA.Database.ItemAddingTable.GetArray(client);

            for (ushort x = 0; x < Artefacts.Length; x++)
                WriteByte(Artefacts[x], x + 14000, Buff);

            if (client.Entity.MyJiang != null)
            {
                string Jiang = client.Entity.MyJiang.ToString();
                WriteUInt32((uint)Jiang.Length, 20000, Buff);
                WriteString(Jiang, 20004, Buff);
            }
            if (client.ChiPowers != null)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((byte)client.ChiPowers.Count);
                foreach (var chiPower in client.ChiPowers)
                    chiPower.Serialize(writer);

                byte[] chi = stream.ToArray();
                WriteUInt32((uint)chi.Length, 21000, Buff);
                for (ushort x = 0; x < chi.Length; x++)
                    WriteByte(chi[x], x + 21004, Buff);
            }
        }
        public byte[] GetArray() { return Buff; }
    }

}
