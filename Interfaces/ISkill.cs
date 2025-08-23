using System;

namespace MTA.Interfaces
{
    public interface ISkill
    {
        uint Experience { get; set; }
        ushort ID { get; set; }
        byte Level { get; set; }
        byte PreviousLevel { get; set; }
        void AddFlag(Network.GamePackets.Spell.Soul_Level flag);
        bool ContainsFlag(Network.GamePackets.Spell.Soul_Level flag);
        void RemoveFlag(Network.GamePackets.Spell.Soul_Level flag);
        Network.GamePackets.Spell.Soul_Level Souls { get; set; }
        byte LevelHu2 { get; set; }
        byte LevelHu { get; set; }
       // byte TempLevel { get; set; }
        bool Available { get; set; }
        void Send(Client.GameState client);
    }
}
