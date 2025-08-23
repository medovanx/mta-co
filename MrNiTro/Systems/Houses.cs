namespace Conquer_Online_Server.Game
{
    using Conquer_Online_Server.Client;
    using Conquer_Online_Server.Database;
    using Conquer_Online_Server.ServerBase;
    using System;
    using Conquer_Online_Server.Interfaces;
    using Conquer_Online_Server.Network.GamePackets;

    internal class House
    {
        public static void LoadHouse(Client.GameState client)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(MySqlCommandType.SELECT);
                command.Select("house").Where("id", (long)client.Account.EntityID);
                MySqlReader reader = new MySqlReader(command);
                while (reader.Read())
                {
                    MapsTable.MapInformation information = new MapsTable.MapInformation
                    {
                        ID = ((ushort)client.Entity.UID),
                        BaseID = reader.ReadUInt16("mapdoc"),
                        HouseLevel = reader.ReadUInt32("HouseLevel"),
                        Status = 7,
                        Weather = 0,
                        Owner = client.Entity.UID,
                        Box = reader.ReadUInt32("Box"),
                        BoxX = reader.ReadUInt32("BoxX"),
                        BoxY = reader.ReadUInt32("BoxY")

                    };
                    if (information.HouseLevel == 0)
                        information.HouseLevel = 1;
                    MapsTable.MapInformations.Add(information.ID, information);
                    Kernel.Maps.Remove(information.ID);
                    if (!Kernel.Maps.ContainsKey(information.ID))
                        new Map(information.ID, information.BaseID, DMaps.MapPaths[information.BaseID]);


                }
                command = new MySqlCommand(MySqlCommandType.SELECT);
                command.Select("house").Where("id", (long)client.Entity.UID);
                reader = new MySqlReader(command);
                if (!reader.Read())
                {
                    ushort id = (ushort)client.Entity.UID;
                    if (Kernel.Maps.ContainsKey(id))
                        Kernel.Maps.Remove(id);
                    if (MapsTable.MapInformations.ContainsKey(id))
                        MapsTable.MapInformations.Remove(id);

                }
            }
            catch (Exception exception)
            {
                Program.SaveException(exception);
            }
        }
        public static void AddBox(GameState client)
        {
            MapsTable.MapInformation information;
            MapsTable.MapInformations.TryGetValue(((ushort)client.Entity.UID), out information);
            information.Box = 1;
            information.BoxX = client.Entity.X;
            information.BoxY = client.Entity.Y;
            new MySqlCommand(MySqlCommandType.UPDATE).Update("house")
                .Set("Box", "1").Set("BoxX", client.Entity.X)
                .Set("BoxY", client.Entity.Y)
                .Where("id", client.Entity.UID).Execute();
            INpc npc = new NpcSpawn
            {
                UID = (uint)Game.ConquerStructures.Warehouse.WarehouseID.ItemBox,
                Mesh = 8200,
                Type = Enums.NpcType.WareHouse,
                X = (ushort)information.BoxX,
                Y = (ushort)information.BoxY,
                MapID = (ushort)information.Owner
            };
            client.Map.AddNpc(npc);
        }
        public static void createhouse(GameState client)
        {
            MySqlCommand command = new MySqlCommand(MySqlCommandType.INSERT);
            command.Insert("house").Insert("id", (long)client.Entity.UID)
                .Insert("mapdoc", "1098").Insert("owner", (long)client.Entity.UID)
                .Insert("HouseLevel", "1");
            command.Execute();
            MapsTable.MapInformation information = new MapsTable.MapInformation
            {
                ID = (ushort)client.Entity.UID,
                BaseID = 1098,
                Status = 7,
                Weather = 0,
                Owner = client.Entity.UID,
                HouseLevel = 1
            };
            MapsTable.MapInformations.Add(information.ID, information);
            if (!Kernel.Maps.ContainsKey(information.ID))
            {
                new Map(information.ID, information.BaseID, DMaps.MapPaths[information.BaseID]);
            }
        }
        public static void UpgradeHouse(GameState client, byte level)
        {
            ushort _base = 1098;
            if (level == 1)
                _base = 1099;
            if (level == 2)
                _base = 2080;
            if (level == 3)
                _base = 1765;
            if (level == 4)
                _base = 3024;
            //1098 level 1
            //1099 level 2
            //2080 level 3
            //1765 level 4
            //3024 level 5
            level++;
            if (level > 5)
                return;
            MapsTable.MapInformations.Remove((ushort)client.Entity.UID);
            Kernel.Maps.Remove(((ushort)client.Entity.UID));
            MapsTable.MapInformation information = new MapsTable.MapInformation
            {
                ID = (ushort)client.Entity.UID,
                BaseID = _base,
                Status = 7,
                Weather = 0,
                Owner = client.Entity.UID,
                HouseLevel = level
            };
            MapsTable.MapInformations.Add(information.ID, information);
            new MySqlCommand(MySqlCommandType.UPDATE).Update("house").Set("HouseLevel", level)
                .Set("mapdoc", _base).Set("Box", 0).Set("BoxX", 0)
                .Set("BoxY", 0).Where("id", (long)client.Entity.UID).Execute();

            if (!Kernel.Maps.ContainsKey(information.ID))
            {
                new Map(information.ID, information.BaseID, DMaps.MapPaths[information.BaseID]);
            }
        }
    }
}



