using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game.ConquerStructures.Society
{
    public class Enemy : Interfaces.IKnownPerson
    {
        public uint ID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool IsOnline
        {
            get
            {
                return Kernel.GamePool.ContainsKey(ID);
            }
        }
        public MTA.Game.ConquerStructures.NobilityRank NobilityRank
        {
            get
            {
                if (Kernel.GamePool.ContainsKey(ID))
                    return Kernel.GamePool[ID].Entity.NobilityRank;
                else
                    return 0;
            }
        }
        public bool IsBoy
        {
            get
            {
                if (Kernel.GamePool.ContainsKey(ID))
                    return Network.PacketHandler.IsBoy(Kernel.GamePool[ID].Entity.Body);
                else
                    return false;
            }
        }
        public Client.GameState Client
        {
            get
            {
                return Kernel.GamePool[ID];
            }
        }
    }
}