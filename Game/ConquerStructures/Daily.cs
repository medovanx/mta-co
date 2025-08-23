using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
namespace MTA.Game.ConquerStructures
{
    public class Daily
    {
        public static bool DailyPks = false;
        public static ushort Map = 8877;
        public static bool signup = false;
        public static int howmanyinmap = 0;
        public static int howmanyinmap22 = 0;
        public static int howmanyinmap23 = 0;
        public static int howmanyinmap24 = 0;
        public static int howmanyinmap29 = 0;
        public static int howmanyinmap53 = 0;
        public static int howmanyinmap25 = 0;
        public static int howmanyinmap2 = 0;
        public static int howmanyinmap12 = 0;
        public static int howmanyinmap3 = 0;
        public static int howmanyinmap11 = 0;
        public static int howmanyinmap10 = 0;
        public static int howmanyinmap52 = 0;
        public static int howmanyinmap180 = 0;
        public static int howmanyinmap181 = 0;
        public static int howmanyinmap182 = 0;
        public static int howmanyinmap183 = 0;
        public static int howmanyinmap184 = 0;
        public static int howmanyinmap185 = 0;
        public static int howmanyinmap186 = 0;
        public static int howmanyinmap187 = 0;
        public static int howmanyinmap188 = 0;
        public static int howmanyinmap189 = 0;
        public static int howmanyinmap190 = 0;
        public static int howmanyinmap191 = 0;
        public static int howmanyinmap192 = 0;
        public static int howmanyinmap193 = 0;
        public static int howmanyinmap194 = 0;
        public static int howmanyinmap195 = 0;
        public static int howmanyinmap196 = 0;
        public static int howmanyinmap197 = 0;
        public static int TopDlClaim = 0;
        public static int TopGlClaim = 0;
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
                if (client.Entity.MapID == 3691 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap22 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Nobility King: " + howmanyinmap22 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive25()
        {
            howmanyinmap25 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3073 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap25 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in KillerWar: " + howmanyinmap25 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive24()
        {
            howmanyinmap24 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3693 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap24 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Nobility Duke: " + howmanyinmap24 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive23()
        {
            howmanyinmap23 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3692 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap23 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Nobility Prince: " + howmanyinmap23 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive29()
        {
            howmanyinmap29 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3694 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap29 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Nobility Earl: " + howmanyinmap29 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }
            }
        }
        public static void CheackAlive53()
        {
            howmanyinmap53 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3071 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap53 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in BlackName Now: " + howmanyinmap53 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive10()
        {
            howmanyinmap10 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 1701 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap10 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in TopMaster Now: " + howmanyinmap10 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive52()
        {
            howmanyinmap52 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3070 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap52 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in HorsePk Now: " + howmanyinmap52 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive11()
        {
            howmanyinmap11 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 1702 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap11 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in TopConquer Now: " + howmanyinmap11 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive()
        {
            howmanyinmap = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8877 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in DailyPk Now: " + howmanyinmap + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive2()
        {
            howmanyinmap2 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3333 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap2 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in LastManStanding: " + howmanyinmap2 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive3()
        {
            howmanyinmap12 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 3333 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap12 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in LastManStanding: " + howmanyinmap2 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive180()
        {
            howmanyinmap180 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8510 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap180 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in ConquerPK Now: " + howmanyinmap180 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive181()
        {
            howmanyinmap181 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8511 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap181 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Ghostpk Now: " + howmanyinmap181 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive182()
        {
            howmanyinmap182 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8512 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap182 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in StayAlivePK Now: " + howmanyinmap182 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive183()
        {
            howmanyinmap183 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8513 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap183 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Fighter Now: " + howmanyinmap183 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive184()
        {
            howmanyinmap184 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8514 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap184 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in [T]The Prince Now: " + howmanyinmap184 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive185()
        {
            howmanyinmap185 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8515 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap185 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in RedName Now: " + howmanyinmap185 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive186()
        {
            howmanyinmap186 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8516 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap186 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Master Now: " + howmanyinmap186 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive187()
        {
            howmanyinmap187 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8517 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap187 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in [T]Final-WaR Now: " + howmanyinmap187 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive188()
        {
            howmanyinmap188 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8518 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap188 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Rabbit Now: " + howmanyinmap188 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive189()
        {
            howmanyinmap189 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8519 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap189 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Assassin Now: " + howmanyinmap189 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive190()
        {
            howmanyinmap190 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8520 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap190 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in AitherWar Now: " + howmanyinmap190 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive191()
        {
            howmanyinmap191 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8521 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap191 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in [T]Class PK Now: " + howmanyinmap191 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive192()
        {
            howmanyinmap192 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8522 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap192 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in [T]Dead World Now: " + howmanyinmap192 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive193()
        {
            howmanyinmap193 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8523 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap193 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in MemberAlter Now: " + howmanyinmap193 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive194()
        {
            howmanyinmap194 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8524 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap194 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in FirstKiller Now: " + howmanyinmap194 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive195()
        {
            howmanyinmap195 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8525 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap195 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in Top Death Now: " + howmanyinmap195 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive196()
        {
            howmanyinmap196 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8526 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap196 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in ReVengerWar Now: " + howmanyinmap196 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void CheackAlive197()
        {
            howmanyinmap197 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 8527 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap197 += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in AttackersWar Now: " + howmanyinmap197 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }

        public static void CheackSpouse()
        {
            howmanyinmap3 = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 1090 && client.Entity.Hitpoints >= 1)
                {
                    if (client.Entity.Body == 1003 || client.Entity.Body == 1004)
                    {
                        howmanyinmap3 += 1;
                        MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Teams Alive in CouplesPk: " + howmanyinmap3 + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);

                    }
                }
            }
        }
        public static void SignUp()
        {
            foreach (Client.GameState client in Program.Values)
                if (DateTime.Now.Minute == 00 && signup == false && client.Entity.Class >= 10 && client.Entity.Class <= 15)
                {
                    signup = true;
                    DailyPks = true;
                    client.Entity.Status = 0;
                    client.Entity.RemoveFlag(MTA.Network.GamePackets.Update.Flags.TopTrojan);
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
                        MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("[PhoenixCo]: Daily Has Ended Come Next Hour ", System.Drawing.Color.Red, MTA.Network.GamePackets.Message.TopLeft), Program.Values);
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
