using MTA.Client;
using MTA.Network.GamePackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.MaTrix
{
    public struct Auras
    {
        public GameState TeamAuraOwner;
        public ulong TeamAuraStatusFlag;
        public uint TeamAuraPower;
        public uint TeamAuraLevel;
        public Update.AuraType aura;
    }
}
