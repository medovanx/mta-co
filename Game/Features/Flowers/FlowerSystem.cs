using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Interfaces;
using MTA.Client;

namespace MTA.Game.Features.Flowers
{
    public class FlowerSystem
    {
        private FlowerPacket Packet;

        public FlowerSystem(byte[] BasePacket, Client.GameState Caller)
        {
            Packet = new FlowerPacket(false);
            Packet.Deserialize(BasePacket);
            if (!Kernel.GamePool.ContainsKey(Packet.UID1)) return;
            if (Caller.Entity.Level < 50) return;
            //if (Caller.Entity.Body == 2001 || Caller.Entity.Body == 2002) return;//Female
            if (Kernel.GamePool[Packet.UID1].Entity.Body == 1003 || Kernel.GamePool[Packet.UID1].Entity.Body == 1004) return;//Male
            if (Packet.ItemUID == 0)
            {
                if (Caller.Entity.Flowers.LastFlowerSent == null) Caller.Entity.Flowers.LastFlowerSent = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                if (Packet.FlowerType != FlowerType.RedRoses && Packet.Amount < 1) return;
                if (Caller.Entity.Flowers.LastFlowerSent.AddDays(1) <= DateTime.Now)
                {
                    Caller.Entity.Flowers.LastFlowerSent = DateTime.Now;
                    FlowerPacket NewPacket = new FlowerPacket(true);
                    NewPacket.SenderName = Caller.Entity.Name;
                    NewPacket.ReceiverName = Kernel.GamePool[Packet.UID1].Entity.Name;
                    NewPacket.SendAmount = 30;
                    NewPacket.SendFlowerType = FlowerType.RedRoses;
                    Kernel.GamePool[Packet.UID1].Send(NewPacket);
                    Kernel.GamePool[Packet.UID1].Entity.Flowers.RedRoses += 30;
                    Kernel.GamePool[Packet.UID1].Entity.Flowers.RedRoses2day += 30;
                    Kernel.GamePool[Packet.UID1].Entity.Flowers.LastFlowerSent = DateTime.Now;
                    Database.FlowerSystemTable.SaveFlowerTable(Kernel.GamePool[Packet.UID1]);
                    Database.FlowerSystemTable.SaveFlowerTable(Caller);
                    Caller.Send(new FlowerPacket(Caller.Entity.Flowers, Caller));
                }
                else
                {
                    Caller.Send(Constants.OneFlowerADay);
                }
            }
            else
            {
                ConquerItem Item = null;
                if (Caller.Inventory.TryGetValue(Packet.ItemUID, out Item))
                {
                    FlowerType Flower = FlowerType.Unknown;
                    switch (Item.ID / 1000)
                    {
                        case 751: Flower = FlowerType.RedRoses; break;
                        case 752: Flower = FlowerType.Lilies; break;
                        case 753: Flower = FlowerType.Orchids; break;
                        case 754: Flower = FlowerType.Tulips; break;
                    }
                    if (Flower != FlowerType.Unknown)
                    {
                        switch (Flower)
                        {
                            case FlowerType.RedRoses: Kernel.GamePool[Packet.UID1].Entity.Flowers.RedRoses += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Flowers.RedRoses2day += Item.Durability; break;
                            case FlowerType.Lilies: Kernel.GamePool[Packet.UID1].Entity.Flowers.Lilies += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Flowers.Lilies2day += Item.Durability; break;
                            case FlowerType.Orchids: Kernel.GamePool[Packet.UID1].Entity.Flowers.Orchads += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Flowers.Orchads2day += Item.Durability; break;
                            case FlowerType.Tulips: Kernel.GamePool[Packet.UID1].Entity.Flowers.Tulips += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Flowers.Tulips2day += Item.Durability; break;
                        }

                        FlowerPacket NewPacket = new FlowerPacket(true);
                        NewPacket.SenderName = Caller.Entity.Name;
                        NewPacket.ReceiverName = Kernel.GamePool[Packet.UID1].Entity.Name;
                        Kernel.GamePool[Packet.UID1].Entity.Flowers.LastFlowerSent = DateTime.Now;
                        NewPacket.SendAmount = Item.Durability;
                        NewPacket.SendFlowerType = Flower;
                        Database.FlowerSystemTable.SaveFlowerTable(Kernel.GamePool[Packet.UID1]);
                        Kernel.GamePool[Packet.UID1].Send(NewPacket);
                        Caller.Inventory.Remove(Item, Enums.ItemUse.Remove);
                        Database.ConquerItemTable.RemoveItem(Item.UID);

                    }
                }
            }
        }
    }
}