using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.MaTrix.Roulette
{
    class GamePackets
    {
        public const ushort MsgRouletteShareBetting = 2810,
            MsgRoulettedAddNewPlayer = 2809,
            MsgRouletteOpenGui = 2808,
            MsgRouletteTable = 2807,
            MsgRouletteSignUp = 2805,
            MsgRouletteAction = 2804,
            MsgRouletteScreen = 2803,
            MsgRouletteRecord = 2802,
            MsgRouletteNoWinner = 2801,
            MsgRouletteCheck = 2800;
    }
}
