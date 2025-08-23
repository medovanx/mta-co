using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;

namespace MTA.Game.ConquerStructures
{
    public class Trade
    {
        public ulong Money;
        public uint ConquerPoints, TraderUID;
        public List<ConquerItem> Items;
        public bool Accepted, InTrade;
        public Trade()
        {
            InTrade = Accepted = false;
            ConquerPoints = TraderUID = 0;
            Money = TraderUID = 0;
            Items = new List<ConquerItem>();
        }
    }
}
