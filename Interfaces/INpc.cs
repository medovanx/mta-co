using System;
using MTA.Network.GamePackets;

namespace MTA.Interfaces
{
    public interface INpc
    {
        Game.Enums.NpcType Type { get; set; }
        uint UID { get; set; }
        string Name { get; set; }
        ushort X { get; set; }
        ushort Y { get; set; }
        ushort Mesh { get; set; }
        ushort MapID { get; set; }
       _String Effect { get; set; }
        string effect { get; set; }
        void SendSpawn(Client.GameState Client);
        void SendSpawn(Client.GameState Client, bool checkScreen);
    }
    public interface table
    {
        Game.Enums.NpcType Type { get; set; }
        uint UID { get; set; }
        ushort X { get; set; }
        ushort Y { get; set; }
        uint Mesh { get; set; }
        uint TableUID { get; set; }
        ushort BE { get; set; }
        uint Other { get; set; }
        ushort MapID { get; set; }
        void SendSpawn(Client.GameState Client);
        void SendSpawn(Client.GameState Client, bool checkScreen);
    }
}
