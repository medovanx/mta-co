using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.MaTrix {
    public struct Auras {
        public GameState TeamAuraOwner;
        public ulong TeamAuraStatusFlag;
        public uint TeamAuraPower;
        public uint TeamAuraLevel;
        public Update.AuraType aura;
    }
}