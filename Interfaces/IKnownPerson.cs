using System;

namespace MTA.Interfaces
{
    public interface IKnownPerson
    {
        uint ID { get; set; }
        string Name { get; set; }
        bool IsOnline { get; }
        Client.GameState Client { get; }
    }
}
