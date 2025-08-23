using System;
using System.Linq;
using System.Collections.Generic;
using MTA.Network.GamePackets;

namespace MTA.Game.ConquerStructures
{
    public class Warehouse
    {
        Dictionary<uint, ConquerItem> items;
        ConquerItem[] objects;
        Client.GameState Owner;
        private WarehouseID _ID;
        private byte MaxCount = 20;

        //private uint MaxCount
        //{
        //    get
        //    {
        //        if (Owner != null)
        //        {
        //            if (Owner.Account != null)
        //                if (Owner.Account.EntityID == (uint)_ID)
        //                    return Owner.SashSlots;
        //        }
        //        else
        //        {
        //            var item = Database.ConquerItemTable.LoadItem((uint)_ID);
        //            if (item != null)
        //            {
        //                switch (item.ID)
        //                {
        //                    case 1100003:
        //                        return 3;
        //                    case 1100006:
        //                        return 6;
        //                    case 1100009:
        //                        return 9;
        //                }
        //            }
        //        }                
        //        return 60;
        //    }
        //}
        public byte GetMaxCount(Client.GameState client, WarehouseID ID)
        {
            var item = Database.ConquerItemTable.LoadItem((uint)ID);
            if (item != null)
            {
                switch (item.ID)
                {
                    case 1100003:
                        return 3;
                    case 1100006:
                        return 6;
                    case 1100009:
                        return 9;
                }
            }
            return 60;
        }
        public Warehouse(Client.GameState client, WarehouseID ID, byte maxCount = 60)
        {
            Owner = client;
            _ID = ID;
            MaxCount = maxCount;
            items = new Dictionary<uint, ConquerItem>(MaxCount);
            objects = new ConquerItem[0];
        }

        public bool Add(ConquerItem item)
        {
            if (!items.ContainsKey(item.UID) && Count < MaxCount)
            {
                item.Warehouse = (uint)_ID;
                item.Position = 0;
                Owner.Inventory.Remove(item, Game.Enums.ItemUse.Move);
                items.Add(item.UID, item);
                objects = items.Values.ToArray();
                return true;
            }
            return false;
        }
        
        public bool Remove(ConquerItem item)
        {
            if (items.ContainsKey(item.UID))
            {
                item.Warehouse = 0;
                if (Owner.Inventory.Add(item, Enums.ItemUse.Move))
                {
                    items.Remove(item.UID);
                    objects = items.Values.ToArray();
                    Network.GamePackets.Warehouse warehouse = new MTA.Network.GamePackets.Warehouse(true);
                    warehouse.Type = Network.GamePackets.Warehouse.RemoveItem;
                    warehouse.Count = 1;
                    warehouse.Append(item);
                    Owner.Send(warehouse);
                    return true;
                }
            }
            return false;
        }

        public bool Remove(uint UID)
        {
            if (items.ContainsKey(UID))
            {
                ConquerItem item = items[UID];
                item.Warehouse = 0;
                if (Owner.Inventory.Add(item, Enums.ItemUse.Move))
                {
                    items.Remove(item.UID);
                    objects = items.Values.ToArray();
                    return true;
                }
            }
            return false;
        }

        #region House
        public bool Add2(ConquerItem item, Client.GameState client)
        {
            if (!items.ContainsKey(item.UID) && Count < MaxCount)
            {
                item.Warehouse = (uint)_ID;
                item.Position = 0;
                if (client != null)
                    client.Inventory.Remove(item, Game.Enums.ItemUse.Move);
                items.Add(item.UID, item);
                objects = items.Values.ToArray();
                return true;
            }
            return false;
        }

        public bool Remove2(uint UID, Client.GameState client)
        {
            if (items.ContainsKey(UID))
            {
                ConquerItem item = items[UID];
                item.Warehouse = 0;
                if (client.Inventory.Add(item, Enums.ItemUse.Move))
                {
                    items.Remove(item.UID);
                    objects = items.Values.ToArray();
                    return true;
                }
            }
            return false;
        }
        #endregion House

        public ConquerItem[] Objects
        { 
            get 
            {
                return objects; 
            }
        }

        public byte Count { get { return (byte)Objects.Length; } }

        public ConquerItem GetItem(uint UID)
        {
            ConquerItem item = null;
            items.TryGetValue(UID, out item);
            return item;
        }

        public bool ContainsUID(uint UID)
        {
            return items.ContainsKey(UID);
        }

        public enum WarehouseID : uint
        {
         //   ItemBox = 8200,
            TwinCity = 8,
            PhoenixCity = 10012,
            ApeCity = 10028,
            DesertCity = 10011,
            BirdCity = 10027,
            StoneCity = 4101,
            Poker = 19111,
            Market = 44
        }
    }
}
