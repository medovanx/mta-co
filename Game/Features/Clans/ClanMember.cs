using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA
{
    public class ClanMember
    {
        public UInt32 Identifier, Donation;
        public String Name , LeaderName;
        public Clan.Ranks Rank;
        public Byte Level, Class;
        public uint UID = 0;
        public bool IsOnline
        {
            get
            {
                return Kernel.GamePool.ContainsKey(UID);
            }
        }
        public Client.GameState Client
        {
            get
            {
                return Kernel.GamePool.ContainsKey(UID) ? Kernel.GamePool[UID] : null;
            }
        }
    }
}
