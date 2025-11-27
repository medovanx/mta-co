using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace MTA.Game
{
    public class Quarantine
    {
        public static ConcurrentDictionary<uint, Client.GameState> White =
            new ConcurrentDictionary<uint, Client.GameState>();
        public static ConcurrentDictionary<uint, Client.GameState> Black =
            new ConcurrentDictionary<uint, Client.GameState>();

        public static int BlackScore, WhiteScore = 0;
        public static bool Started = false;
        public static ushort Map = 1844;
        public static ushort X = 119;
        public static ushort Y = 159;


    }
}
