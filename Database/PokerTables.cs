using System;
using System.Collections.Generic;

namespace MTA.Database
{
    public class PokerTables
    {
        public static Dictionary<uint, Game.ConquerStructures.PokerTable> Tables = new Dictionary<uint, Game.ConquerStructures.PokerTable>(50);

        public static void LoadTables()
        {
            string[] TDs = System.IO.File.ReadAllLines(Constants.DataHolderPath + "PokerTables.txt");
            foreach (string Tinfo in TDs)
            {
                string[] line = Tinfo.Split(',');

                Game.ConquerStructures.PokerTable T = new Game.ConquerStructures.PokerTable();
                T.UID = Convert.ToUInt32(line[0]);
                T.Map = Convert.ToUInt16(line[1]);
                T.X = Convert.ToUInt16(line[2]);
                T.Y = Convert.ToUInt16(line[3]);
                T.Mesh = Convert.ToUInt32(line[4]);
                T.Number = (byte)Convert.ToUInt32(line[5]);
                T.Unlimited = true;
                T.BetType = (Game.Enums.PokerBetType)(byte)Convert.ToUInt32(line[7]);
                T.MinLimit = (uint)Convert.ToUInt64(line[8]);
                T.Start();
                if (!Tables.ContainsKey(T.UID))
                    Tables.Add(T.UID, T);
            }
        }
    }
}
