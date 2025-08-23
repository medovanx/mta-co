namespace MTA.Game
{
    using MTA;
    using MTA.Client;
    using MTA.Database;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class PrizeNPC
    {
        public static SafeDictionary<long, PrizeNpcInfo> PrizeNpcInformations = new SafeDictionary<long, PrizeNpcInfo>(0xc350);

        public static void AddCps(GameState client, uint Amount)
        {
            PrizeNpcInfo info = new PrizeNpcInfo
            {
                Owner = (long)client.Entity.UID,
                type = 1,
                amount = Amount,
                itemid = 0
            };
            PrizeNpcInformations.Add(info.Owner, info);
        }

        public static void AddItem(GameState client, uint itemid)
        {
            PrizeNpcInfo info = new PrizeNpcInfo
            {
                Owner = (long)client.Entity.UID,
                type = 2,
                amount = 0,
                itemid = itemid
            };
            PrizeNpcInformations.Add(info.Owner, info);
        }

        public static void Load()
        {
            MySqlReader reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("prizenpc"));
            if (reader.Read())
            {
                PrizeNpcInfo info = new PrizeNpcInfo
                {
                    Owner = (long)reader.ReadUInt32("Owner"),
                    type = reader.ReadUInt32("type"),
                    amount = reader.ReadUInt32("amount"),
                    itemid = reader.ReadUInt32("itemid")
                };
                PrizeNpcInformations.Add(info.Owner, info);
            }
            //MTA.Console.WriteLine("PrizeNpc Loaded.");
            //  Reader.Close();
            ////  Reader.Dispose();
        }

        public static void RemoveCps(GameState client)
        {
            MySqlCommand command = new MySqlCommand(MySqlCommandType.DELETE);
            command.Delete("prizenpc", "Owner", (long)client.Entity.UID).And("type", "1").Execute();
            PrizeNpcInformations.Remove((long)client.Entity.UID);
            MySqlReader reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("prizenpc").Where("Owner", (long)client.Entity.UID));
            if (reader.Read())
            {
                PrizeNpcInfo info = new PrizeNpcInfo
                {
                    Owner = (long)reader.ReadUInt32("Owner"),
                    type = reader.ReadUInt32("type"),
                    amount = reader.ReadUInt32("amount"),
                    itemid = reader.ReadUInt32("itemid")
                };
                PrizeNpcInformations.Add(info.Owner, info);
            }
            //  Reader.Close();
            ////  Reader.Dispose();
        }

        public static void RemoveItem(GameState client)
        {
            MySqlCommand command = new MySqlCommand(MySqlCommandType.DELETE);
            command.Delete("prizenpc", "Owner", (long)client.Entity.UID).And("type", "2").Execute();
            PrizeNpcInformations.Remove((long)client.Entity.UID);
            MySqlReader reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("prizenpc").Where("Owner", (long)client.Entity.UID));
            if (reader.Read())
            {
                PrizeNpcInfo info = new PrizeNpcInfo
                {
                    Owner = (long)reader.ReadUInt32("Owner"),
                    type = reader.ReadUInt32("type"),
                    amount = reader.ReadUInt32("amount"),
                    itemid = reader.ReadUInt32("itemid")
                };
                PrizeNpcInformations.Add(info.Owner, info);
            }
            //  Reader.Close();
            ////  Reader.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PrizeNpcInfo
        {
            public long Owner;
            public uint type;
            public uint amount;
            public uint itemid;
        }
    }
}

