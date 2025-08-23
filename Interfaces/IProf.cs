using System;

namespace MTA.Interfaces
{
    public interface IProf
    {
        uint Experience { get; set; }
        uint NeededExperience { get; set; }
        ushort ID { get; set; }
        byte Level { get; set; }
        byte PreviousLevel { get; set; }
        byte TempLevel { get; set; }
        bool Available { get; set; }
        void Send(Client.GameState client);
    }
}
