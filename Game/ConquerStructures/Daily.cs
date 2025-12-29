using System;
using MTA.Network.GamePackets;
namespace MTA.Game.ConquerStructures
{
    public class Daily
    {
        public static bool DailyPks;
        public static ushort Map = 8877;
        public static bool signup;
        public static int howmanyinmap;
        public static int howmanyinmap22;
        public static int howmanyinmap23;
        public static int howmanyinmap24;
        public static int howmanyinmap29;
        public static int howmanyinmap53;
        public static int howmanyinmap25;
        public static int howmanyinmap2;
        public static int howmanyinmap12;
        public static int howmanyinmap3;
        public static int howmanyinmap11;
        public static int howmanyinmap10;
        public static int howmanyinmap52;
        public static int howmanyinmap180;
        public static int howmanyinmap181;
        public static int howmanyinmap183;
        public static int howmanyinmap185;
        public static int howmanyinmap186;
        public static int howmanyinmap187;
        public static int howmanyinmap188;
        public static int howmanyinmap189;
        public static int howmanyinmap190;
        public static int howmanyinmap191;
        public static int howmanyinmap194;
        public static int howmanyinmap195;
        public static int TopDlClaim;
        public static int TopGlClaim;
        public static void AddDl()
        {
            TopDlClaim++;
            //return;
        }
        public static void AddGl()
        {
            TopGlClaim++;
            //return;
        }
        public static void CheackAlive22()
        {
            howmanyinmap22 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3691, Hitpoints: >= 1 })
                {
                    howmanyinmap22 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Nobility King: " + howmanyinmap22 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive25()
        {
            howmanyinmap25 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3073, Hitpoints: >= 1 })
                {
                    howmanyinmap25 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in KillerWar: " + howmanyinmap25 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive24()
        {
            howmanyinmap24 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3693, Hitpoints: >= 1 })
                {
                    howmanyinmap24 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Nobility Duke: " + howmanyinmap24 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive23()
        {
            howmanyinmap23 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3692, Hitpoints: >= 1 })
                {
                    howmanyinmap23 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Nobility Prince: " + howmanyinmap23 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive29()
        {
            howmanyinmap29 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3694, Hitpoints: >= 1 })
                {
                    howmanyinmap29 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Nobility Earl: " + howmanyinmap29 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive53()
        {
            howmanyinmap53 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3071, Hitpoints: >= 1 })
                {
                    howmanyinmap53 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in BlackName Now: " + howmanyinmap53 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive10()
        {
            howmanyinmap10 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 1701, Hitpoints: >= 1 })
                {
                    howmanyinmap10 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in TopMaster Now: " + howmanyinmap10 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive52()
        {
            howmanyinmap52 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3070, Hitpoints: >= 1 })
                {
                    howmanyinmap52 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in HorsePk Now: " + howmanyinmap52 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive11()
        {
            howmanyinmap11 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 1702, Hitpoints: >= 1 })
                {
                    howmanyinmap11 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in TopConquer Now: " + howmanyinmap11 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheckAlive()
        {
            howmanyinmap = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8877, Hitpoints: >= 1 })
                {
                    howmanyinmap += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in DailyPk Now: " + howmanyinmap + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive2()
        {
            howmanyinmap2 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3333, Hitpoints: >= 1 })
                {
                    howmanyinmap2 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in LastManStanding: " + howmanyinmap2 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive3()
        {
            howmanyinmap12 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 3333, Hitpoints: >= 1 })
                {
                    howmanyinmap12 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in LastManStanding: " + howmanyinmap2 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive180()
        {
            howmanyinmap180 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8510, Hitpoints: >= 1 })
                {
                    howmanyinmap180 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in ConquerPK Now: " + howmanyinmap180 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive181()
        {
            howmanyinmap181 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8511, Hitpoints: >= 1 })
                {
                    howmanyinmap181 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Ghostpk Now: " + howmanyinmap181 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive183()
        {
            howmanyinmap183 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8513, Hitpoints: >= 1 })
                {
                    howmanyinmap183 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Fighter Now: " + howmanyinmap183 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive185()
        {
            howmanyinmap185 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8515, Hitpoints: >= 1 })
                {
                    howmanyinmap185 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in RedName Now: " + howmanyinmap185 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive186()
        {
            howmanyinmap186 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8516, Hitpoints: >= 1 })
                {
                    howmanyinmap186 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Master Now: " + howmanyinmap186 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive187()
        {
            howmanyinmap187 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8517, Hitpoints: >= 1 })
                {
                    howmanyinmap187 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in [T]Final-WaR Now: " + howmanyinmap187 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive188()
        {
            howmanyinmap188 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8518, Hitpoints: >= 1 })
                {
                    howmanyinmap188 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Rabbit Now: " + howmanyinmap188 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive189()
        {
            howmanyinmap189 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8519, Hitpoints: >= 1 })
                {
                    howmanyinmap189 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Assassin Now: " + howmanyinmap189 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive190()
        {
            howmanyinmap190 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8520, Hitpoints: >= 1 })
                {
                    howmanyinmap190 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in AitherWar Now: " + howmanyinmap190 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive191()
        {
            howmanyinmap191 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8521, Hitpoints: >= 1 })
                {
                    howmanyinmap191 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in [T]Class PK Now: " + howmanyinmap191 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive194()
        {
            howmanyinmap194 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8524, Hitpoints: >= 1 })
                {
                    howmanyinmap194 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in FirstKiller Now: " + howmanyinmap194 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive195()
        {
            howmanyinmap195 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 8525, Hitpoints: >= 1 })
                {
                    howmanyinmap195 += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in Top Death Now: " + howmanyinmap195 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }

        public static void CheackSpouse()
        {
            howmanyinmap3 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity is { MapID: 1090, Hitpoints: >= 1 })
                {
                    if (client.Entity.Body == 1003 || client.Entity.Body == 1004)
                    {
                        howmanyinmap3 += 1;
                        Kernel.SendWorldMessage(new Message("Teams Alive in CouplesPk: " + howmanyinmap3 + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);

                    }
                }
            }
        }
        public static void SignUp()
        {
            foreach (Client.GameState client in Program.Values)
                if (DateTime.Now.Minute == 00 && !signup && client.Entity.Class is >= 10 and <= 15)
                {
                    signup = true;
                    DailyPks = true;
                    client.Entity.Status = 0;
                    client.Entity.RemoveFlag(Update.Flags.TopTrojan);
                }
        }


        public static void End()
        {
            if (DateTime.Now.Minute == 30)
            {
                //signup = false;
                //DailyPks = false;
                foreach (Client.GameState client in Program.Values)
                {
                    if (DateTime.Now.Minute == 30)
                    {
                        client.Entity.ConquerPoints += 1500000;
                        Kernel.SendWorldMessage(new Message("[PhoenixCo]: Daily Has Ended Come Next Hour ", System.Drawing.Color.Red, Message.TopLeft), Program.Values);
                    }
                    if (client.Entity.MapID == 8877)
                    {
                        client.Entity.Teleport(1002, 400, 400);
                    }
                    client.Entity.RemoveFlag(Update.Flags.Flashy);
                }
            }
        }
    }
}
