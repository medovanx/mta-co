using System;
using MTA.Client;
using MTA.Database;

namespace MTA.Game {
    public class ZiraxCoins {
        public static bool Running;
        public static GameState client;

        public static void Start() {
            if (DateTime.Now.Hour == 16 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                Running = true;
            }
        }

        public static void CheckRunning() {
            if (!Running) {
                if (client.Entity.MapID == cMap && client.Account.State != AccountTable.AccountState.GM) {
                    client.Entity.Teleport(1002, 429, 378);
                    client.Send("ZiraxCoins has Ended and You have teleported to tc");
                }
            }
        }

        public static void End() {
            if (DateTime.Now.Hour == 17 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                Running = false;
                foreach (GameState client in Program.Values) {
                    if (client.Entity.MapID == cMap) {
                        client.Entity.BringToLife();
                        client.Entity.Teleport(tMap, tX, tY);
                        client.Send("ZiraxCoins has Ended and You have teleported to tc");
                    }
                }
            }
        }

        #region CoinsMap

        public static ushort cMap = 7015;
        public static ushort cX = 60;
        public static ushort cY = 60;

        #endregion

        #region CoinsID

        public static uint GoldCoin = 711609;
        public static uint SilverCoin = 711610;
        public static uint CopperCoin = 711611;
        public static uint ZiraxCoin = 710208;

        #endregion

        #region TradeMap

        public static ushort tMap = 7010;
        public static ushort tX = 59;
        public static ushort tY = 59;

        #endregion
    }
}