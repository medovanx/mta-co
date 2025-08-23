using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;

namespace MTA.Game
{
    public class ClassPk
    {
        public static bool ClassPks = false;
        public static ushort Map = 7001;
        public static bool signup = false;
        public static int howmanyinmap = 0;
        public static int TopDlClaim = 0;
        public static int TopGlClaim = 0;
        public static int ClanClaim=0;
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
        public static void CheackAlive()
        {
            howmanyinmap = 0;
            foreach (Client.GameState client in Program.Values)
            {
                if (client.Entity.MapID == 7001 && client.Entity.Hitpoints >= 1)
                {
                    howmanyinmap += 1;
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Players Alive in ClassPk Now: " + howmanyinmap + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void SignUp()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 10 && client.Entity.Class <= 15)
                    client.Entity.RemoveTopStatus(Network.GamePackets.Update.Flags.TopTrojan);
        }
        public static void SignUp1()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 20 && client.Entity.Class <= 25)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags.TopWarrior);
        }
        public static void SignUp2()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 40 && client.Entity.Class <= 45)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags.TopArcher);
        }
        public static void SignUp3()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 50 && client.Entity.Class <= 55)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags.TopNinja);
        }
        public static void SignUp4()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 60 && client.Entity.Class <= 65)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags2.TopMonk);
        }
        public static void SignUp5()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 130 && client.Entity.Class <= 135)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags.TopWaterTaoist);
        }
        public static void SignUp6()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 140 && client.Entity.Class <= 145)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags.TopFireTaoist);
        }
        public static void SignUp8()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 70 && client.Entity.Class <= 75)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags2.TopPirate);
        }
        public static void SignUp9()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class >= 80 && client.Entity.Class <= 85)
                    client.Entity.RemoveTopStatus(MTA.Network.GamePackets.Update.Flags3.DragonWarriorTop);
        }
        public static void End()
        {
            if (DateTime.Now.Minute == 59)
            {
                signup = false;
                ClassPks = false;
                foreach (Client.GameState client in Program.Values)
                {
                    if (DateTime.Now.Minute == 59)
                    {
                        client.Entity.ConquerPoints += rates.ClassPk;
                        MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message(" ClassPk Has Ended Come Next Week ", System.Drawing.Color.Red, MTA.Network.GamePackets.Message.TopLeft), Program.Values);
                    }
                    if (client.Entity.MapID == 7001)
                    {
                        client.Entity.Teleport(1002, 301, 266);
                    }
                    client.Entity.RemoveFlag(Update.Flags.Flashy);
                }
            }
        }
    }
}
