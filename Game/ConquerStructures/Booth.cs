using System;
using System.Collections.Generic;
using MTA.Network.GamePackets;

namespace MTA.Game.ConquerStructures
{
    public struct BoothItem
    {
        public void Regenerate(BoothItem item, Booth booth)
        {
            booth.ItemList.Remove(item.Item.UID);
            Cost = item.Cost;
            Item = new ConquerItem(true);
            Item.ID = item.Item.ID;
            Item.UID = Program.GetNextItemId();
            Item.Plus = item.Item.Plus;
            Item.Enchant = item.Item.Enchant;
            Item.Bless = item.Item.Bless;
            Item.SocketOne = item.Item.SocketOne;
            Item.SocketTwo = item.Item.SocketTwo;
            Item.StackSize = item.Item.StackSize;
            Item.Bound = false;
            Database.ConquerItemBaseInformation CIBI = null;
            CIBI = Database.ConquerItemInformation.BaseInformations[Item.ID];
            if (CIBI == null)
                return;
            Item.Durability = CIBI.Durability;
            Item.MaximDurability = CIBI.Durability;
            Cost_Type = item.Cost_Type;
            booth.ItemList.Add(Item.UID, this);


        }
        public enum CostType : byte
        {
            Silvers = 1,
            ConquerPoints = 3,
            BoundCps = 2

        }
        public ConquerItem Item;
        public uint Cost;
        public CostType Cost_Type;


    }
    public class Booth
    {
        public static Counter BoothCounter = new Counter(1) { Finish = 10000 };
        private static Dictionary<uint, Booth> Booths = new Dictionary<uint, Booth>();
        public static Dictionary<uint, Booth> Booths2 = new Dictionary<uint, Booth>();
        public static object SyncRoot = new Object();
        public static bool TryGetValue(uint uid, out Booth booth)
        {
            lock (SyncRoot)
                return Booths.TryGetValue(uid, out booth);
        }
        public static bool TryGetValue2(uint uid, out Booth booth)
        {
            lock (SyncRoot)
                return Booths2.TryGetValue(uid, out booth);
        }

        public SafeDictionary<uint, BoothItem> ItemList;
        Client.GameState Owner;
        public SobNpcSpawn Base;
        public Message HawkMessage;
        public Booth(Client.GameState client, Data data)
        {
            Owner = client;
            Owner.Booth = this;
            Owner.Entity.Action = Enums.ConquerAction.Sit;
            ItemList = new SafeDictionary<uint, BoothItem>(20);
            Base = new SobNpcSpawn();
            Base.Owner = Owner;
            lock (SyncRoot)
            {
                Base.UID = BoothCounter.Next;
                while (Booths.ContainsKey(Base.UID))
                    Base.UID = BoothCounter.Next;
                Booths.Add(Base.UID, this);
            }
            Base.Mesh = 406;
            Base.Type = Enums.NpcType.Booth;
            Base.ShowName = true;
            Base.Name = Name;
            Base.MapID = client.Entity.MapID;
            Base.X = (ushort)(Owner.Entity.X + 1);
            Base.Y = Owner.Entity.Y;
            Owner.SendScreenSpawn(Base, true);
            data.dwParam = Base.UID;
            data.wParam1 = Base.X;
            data.wParam2 = Base.Y;
            data.ID = Data.OwnBooth;
            Owner.Send(data);
        }

        public Booth()
        {
            ItemList = new SafeDictionary<uint, BoothItem>(20);

        }
        public string Name
        {
            get
            {
                return Owner.Entity.Name;
            }
        }
        public static implicit operator byte[](Booth booth)
        {
            return booth.Base.ToArray();
        }
        public static implicit operator SobNpcSpawn(Booth booth)
        {
            return booth.Base;
        }
        public void Remove()
        {
            Data data = new Data(true);
            data.UID = Base.UID;
            data.ID = Data.RemoveEntity;
            Owner.SendScreen(data);
            lock (SyncRoot) Booths.Remove(Base.UID);
        }
    }
}
