using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Interfaces;
using MTA.Client;

namespace MTA.Game.Features.Kisses
{
    public class KissSystem
    {
        private KissPacket Packet;
        public KissSystem(byte[] BasePacket, Client.GameState Caller)
        {
            Packet = new KissPacket(false);
            Packet.Deserialize(BasePacket);
            if (!Kernel.GamePool.ContainsKey(Packet.UID1)) return;
            if (Caller.Entity.Level < 50) return;
            if (Caller.Entity.Body == 2003 || Caller.Entity.Body == 2004) return;
            if (Kernel.GamePool[Packet.UID1].Entity.Body == 1001 || Kernel.GamePool[Packet.UID1].Entity.Body == 1002) return;
            if (Packet.ItemUID == 0)
            {
                if (Caller.Entity.Kisses == null) Caller.Entity.Kisses.LastKissesSent = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                if (Packet.KissesType != KissType.Kisses && Packet.Amount != 1) return;
                if (Caller.Entity.Kisses.LastKissesSent.AddDays(1) <= DateTime.Now)
                {
                    Caller.Entity.Kisses.LastKissesSent = DateTime.Now;
                    KissPacket NewPacket = new KissPacket(true);
                    NewPacket.sub = 1;
                    NewPacket.SenderName = Caller.Entity.Name;
                    NewPacket.ReceiverName = Kernel.GamePool[Packet.UID1].Entity.Name;
                    NewPacket.SendAmount = 1;
                    NewPacket.SendKissesType = KissType.Kisses;
                    NewPacket.remove = 1;
                    Kernel.GamePool[Packet.UID1].SendScreen(NewPacket, true);
                    Kernel.GamePool[Packet.UID1].Entity.Kisses.Kisses2++;
                    Kernel.GamePool[Packet.UID1].Entity.Kisses.Kisses2day++;
                    Database.KissSystemTable.SaveKissTable(Kernel.GamePool[Packet.UID1]);
                    Database.KissSystemTable.SaveKissTable(Caller);
                }
                else
                    Caller.Send(Constants.OneKissADay);
            }
            else
            {
                ConquerItem Item = null;
                if (Caller.Inventory.TryGetValue(Packet.ItemUID, out Item))
                {
                    KissType Kisses = KissType.Unknown;
                    switch (Item.ID / 1000)
                    {
                        case 755: Kisses = KissType.Kisses; break;
                        case 756: Kisses = KissType.Letters; break;
                        case 757: Kisses = KissType.Wine; break;
                        case 758: Kisses = KissType.Jades; break;
                    }
                    if (Kisses != KissType.Unknown)
                    {
                        switch (Kisses)
                        {
                            case KissType.Kisses: Kernel.GamePool[Packet.UID1].Entity.Kisses.Kisses2 += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Kisses.Kisses2day += Item.Durability; break;
                            case KissType.Letters: Kernel.GamePool[Packet.UID1].Entity.Kisses.Letters1 += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Kisses.LetterToday1 += Item.Durability; break;
                            case KissType.Wine: Kernel.GamePool[Packet.UID1].Entity.Kisses.Wine += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Kisses.Wine2day += Item.Durability; break;
                            case KissType.Jades: Kernel.GamePool[Packet.UID1].Entity.Kisses.Jades += Item.Durability; Kernel.GamePool[Packet.UID1].Entity.Kisses.Jades2day += Item.Durability; break;
                        }
                        KissPacket NewPacket = new KissPacket(true);
                        NewPacket.SenderName = Caller.Entity.Name;
                        NewPacket.ReceiverName = Kernel.GamePool[Packet.UID1].Entity.Name;
                        NewPacket.SendAmount = Item.Durability;
                        NewPacket.SendKissesType = Kisses;
                        Database.KissSystemTable.SaveKissTable(Kernel.GamePool[Packet.UID1]);
                        Kernel.GamePool[Packet.UID1].SendScreen(NewPacket, true);
                        Caller.Inventory.Remove(Item, Enums.ItemUse.Remove);
                        Database.ConquerItemTable.RemoveItem(Item.UID);
                    }
                }
            }
        }
    }
}