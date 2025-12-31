using System;
using MTA.Network.GamePackets;

namespace MTA.Game
{
    public class ClassPk
    {
        public static bool ClassPks;
        public static ushort Map = 7001;
        public static bool signup;
        public static int howmanyinmap;
        public static int TopDlClaim;
        public static int TopGlClaim;
        public static int ClanClaim = 0;
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
                if (client.Entity is { MapID: 7001, Hitpoints: >= 1 })
                {
                    howmanyinmap += 1;
                    Kernel.SendWorldMessage(new Message("Players Alive in ClassPk Now: " + howmanyinmap + " ", System.Drawing.Color.Black, Message.FirstRightCorner), Program.Values);
                }

            }
        }
        public static void SignUp()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 10 and <= 15)
                    client.Entity.RemoveTopStatus(Update.Flags.TopTrojan);
        }
        public static void SignUp1()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 20 and <= 25)
                    client.Entity.RemoveTopStatus(Update.Flags.TopWarrior);
        }
        public static void SignUp2()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 40 and <= 45)
                    client.Entity.RemoveTopStatus(Update.Flags.TopArcher);
        }
        public static void SignUp3()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 50 and <= 55)
                    client.Entity.RemoveTopStatus(Update.Flags.TopNinja);
        }
        public static void SignUp4()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 60 and <= 65)
                    client.Entity.RemoveTopStatus(Update.Flags2.TopMonk);
        }
        public static void SignUp5()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 130 and <= 135)
                    client.Entity.RemoveTopStatus(Update.Flags.TopWaterTaoist);
        }
        public static void SignUp6()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 140 and <= 145)
                    client.Entity.RemoveTopStatus(Update.Flags.TopFireTaoist);
        }
        public static void SignUp8()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 70 and <= 75)
                    client.Entity.RemoveTopStatus(Update.Flags2.TopPirate);
        }
        public static void SignUp9()
        {
            signup = true;
            ClassPks = true;
            var date = DateTime.Now;
            foreach (Client.GameState client in Program.Values)
                if (date.Minute == 00 && client.Entity.Class is >= 80 and <= 85)
                    client.Entity.RemoveTopStatus(Update.Flags3.DragonWarriorTop);
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
                        client.Entity.ConquerPoints += Rates.ClassPk;
                        Kernel.SendWorldMessage(new Message(" ClassPk Has Ended Come Next Week ", System.Drawing.Color.Red, Message.TopLeft), Program.Values);
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
