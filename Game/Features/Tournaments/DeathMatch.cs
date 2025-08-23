using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;

namespace MTA.Game
{
    public class DeathMatch2
    {
        public const ushort MAPID = 18883;
        public static int[] Points = new int[4];
        public const uint
            SpainTeam = 0,
            ArgentinaTeam = 1,
            BrazilTeam = 2,
            GermanyTeam = 3;
        public static bool IsOn = false;
        public static bool CouplesWar = false;
        public static IDisposable TimerA, TimerB, TimerC;
        public static void SendTimer()
        {
            TimerA = World.Subscribe(SignUp, 1000);
        }
        public static bool signup = false;
        public static bool send = false;
        public static bool end = false;

        public static void SignUp(int ttime)
        {
            var time = DateTime.Now;
            if ((time.Minute == 10) && signup == false)
            {
                TimerA.Dispose();
                TimerB = World.Subscribe(Send, 1000);
                for (int i = 0; i < Points.Length; i++) Points[i] = 0;
                send = false;
                end = false;
                signup = true;
                IsOn = true;
                Kernel.SendWorldMessage(new Message("TeamMatch have started. Sign Up in TwinCity! You have one minute", System.Drawing.Color.Red, Message.Center), Program.Values);
                foreach (var player in Program.Values)
                {
                    player.MessageBox("Team Match has started. Do you want to join?",
                        (p) => { p.Entity.Teleport(1002, 456, 378); }, null, 60);
                }
            }
        }
        static Time32 LastUpdate;
        public static void Send(int ttime)
        {
            Time32 now = new Time32(ttime);

            var time = DateTime.Now;
            if ((time.Minute == 11) && send == false)
            {
                TimerC = World.Subscribe(End, 1000);
                signup = false;
                send = true;
                LastUpdate = now;
                Kernel.SendWorldMessage(new Network.GamePackets.Message("Kill!!", System.Drawing.Color.Red, Network.GamePackets.Message.Center), Program.Values);
                SendTeam();
            }
            if (send)
            {
                if (now > LastUpdate.AddSeconds(5))
                {
                    LastUpdate = now;

                    var array = new[] 
                    {
                        string.Format("Spain team: {0}", Points[SpainTeam]),
                        string.Format("Argentina team: {0}", Points[ArgentinaTeam]),
                        string.Format("Brazil team: {0}", Points[BrazilTeam]),
                        string.Format("Germany team: {0}", Points[GermanyTeam]),
                    };
                    int Place = 0;
                    foreach (var str in array)
                    {
                        Message msg = new Message(str, System.Drawing.Color.Red, Place == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                        Kernel.SendWorldMessage(msg, Program.Values, (ushort)MAPID);
                        Place++;
                    }
                }
            }
        }
        public static void SendTeam()
        {
            foreach (Client.GameState C in Program.Values)
            {
                if (C.Entity.Tournament_Signed == true)
                {
                    C.Entity.TeamDeathMatch_Hits2 = 0;
                    C.Entity.AppearanceBkp = C.Entity.Appearance;
                    Network.PacketHandler.ChangeAppearance(new Data(true) { ID = Data.AppearanceType, UID = C.Entity.UID, dwParam = (byte)AppearanceType.Garment }, C);
                    if (C.Entity.TeamDeathMatch_BlackTeam2 == true)
                    {
                        C.Entity.Teleport(MAPID, 042, 051);
                    }
                    if (C.Entity.TeamDeathMatch_BlueTeam2 == true)
                    {
                        C.Entity.Teleport(MAPID, 060, 042);
                    }
                    if (C.Entity.TeamDeathMatch_WhiteTeam2 == true)
                    {
                        C.Entity.Teleport(MAPID, 066, 064);
                    }
                    if (C.Entity.TeamDeathMatch_RedTeam2 == true)
                    {
                        C.Entity.Teleport(MAPID, 039, 036);
                    }
                }
            }
        }
        public static void End(int ttime)
        {
            var time = DateTime.Now;
            if ((time.Minute == 16) && end == false)
            {
                signup = false;
                end = true;
                IsOn = false;
                foreach (Client.GameState client in Program.Values)
                {
                    if (client.Entity.MapID == MAPID)
                    {
                        client.Entity.Teleport(1002, 301, 266);
                        client.Entity.RemoveFlag(Update.Flags.Flashy);
                    }
                    if (client.Entity.Tournament_Signed2)
                    {
                        client.Entity.Tournament_Signed2 = false;
                        Network.PacketHandler.ChangeAppearance(new Data(true) { ID = Data.AppearanceType, UID = client.Entity.UID, dwParam = (byte)client.Entity.AppearanceBkp }, client);
                        client.SetNewArmorLook(client.BackupArmorLook);
                        client.BackupArmorLook = 0;
                    }
                }
                Reward();
                TimerB.Dispose();
                TimerC.Dispose();
            }
        }

        public static bool redwin = false;
        public static bool blackwin = false;
        public static bool bluewin = false;
        public static bool whitewin = false;

        public static void Reward()
        {
            Dictionary<int, int> top = new Dictionary<int, int>();
            for (int i = 0; i < Points.Length; i++) top.Add(i, Points[i]);
            var array = top.OrderByDescending(o => o.Value).ToArray();
            if (array[0].Key == BrazilTeam)
            {
                redwin = true;
            }
            else if (array[0].Key == ArgentinaTeam)
            {
                bluewin = true;
            }
            else if (array[0].Key == GermanyTeam)
            {
                whitewin = true;
            }
            else
            {
                blackwin = true;
            }
            int winKey = array[0].Key;
            int hits = DeathMatch.Points[winKey];
            uint prize = 10 * (uint)hits;
            string name = "BrazilTeam";
            if (bluewin) name = "ArgentinaTeam";
            if (blackwin) name = "SpainTeam";
            if (whitewin) name = "GermanyTeam";
            Kernel.SendWorldMessage(new Network.GamePackets.Message(name + " have won TeamMatch! The Winner Team Have Gained " + prize + " ConquerPoints", System.Drawing.Color.Red, MTA.Network.GamePackets.Message.Center), Program.Values);

            foreach (Client.GameState C in Program.Values)
            {
                #region Winner
                if (C.Entity.TeamDeathMatch_RedTeam2 && redwin)
                    C.Entity.ConquerPoints += prize;
                if (C.Entity.TeamDeathMatch_BlueTeam2 == bluewin)
                    C.Entity.ConquerPoints += prize;
                if (C.Entity.TeamDeathMatch_BlackTeam2 == blackwin)
                    C.Entity.ConquerPoints += prize;
                if (C.Entity.TeamDeathMatch_WhiteTeam2 == whitewin)
                    C.Entity.ConquerPoints += prize;
                #endregion

                C.Entity.Tournament_Signed2 = false;
                C.Entity.TeamDeathMatch_Hits2 = 0;
                C.Entity.TeamDeathMatch_RedCaptain2 = false;
                C.Entity.TeamDeathMatch_RedTeam2 = false;
                C.Entity.TeamDeathMatch_BlueCaptain2 = false;
                C.Entity.TeamDeathMatch_BlueTeam2 = false;
                C.Entity.TeamDeathMatch_BlackCaptain2 = false;
                C.Entity.TeamDeathMatch_BlackTeam2 = false;
                C.Entity.TeamDeathMatch_WhiteCaptain2 = false;
                C.Entity.TeamDeathMatch_WhiteTeam2 = false;
            }
        }
    }

    public class DeathMatch
    {
        public const ushort MAPID = 8883;
        public static int[] Points = new int[4];
        public const uint
            BlackTeam = 0,
            BlueTeam = 1,
            RedTeam = 2,
            WhiteTeam = 3;
        public static bool IsOn = false;
        public static bool CouplesWar = false;
        public static IDisposable TimerA, TimerB, TimerC;
        public static void SendTimer()
        {
            TimerA = World.Subscribe(SignUp, 1000);
        }
        public static bool signup = false;
        public static bool send = false;
        public static bool end = false;

        public static void SignUp(int ttime)
        {
            var time = DateTime.Now;
            if (time.DayOfWeek == DayOfWeek.Tuesday && (time.Hour == 15 || time.Hour == 23) && time.Minute == 00 && signup == false)
            {
                TimerA.Dispose();
                TimerB = World.Subscribe(Send, 1000);
                for (int i = 0; i < Points.Length; i++) Points[i] = 0;
                send = false;
                end = false;
                signup = true;
                IsOn = true;
                Kernel.SendWorldMessage(new Message("TeamDeathMatch have started. Sign Up in TwinCity! You have one minute", System.Drawing.Color.Red, Message.Center), Program.Values);
                foreach (var player in Program.Values)
                {
                    player.MessageBox("Team DeathMatch has started. Do you want to join?",
                        (p) => { p.Entity.Teleport(1002, 456, 378); }, null, 60);
                }
            }
        }
        static Time32 LastUpdate;
        public static void Send(int ttime)
        {
            Time32 now = new Time32(ttime);

            var time = DateTime.Now;
            if (time.DayOfWeek == DayOfWeek.Tuesday && (time.Hour == 15 || time.Hour == 23) && time.Minute == 1 && send == false)
            {
                TimerC = World.Subscribe(End, 1000);
                signup = false;
                send = true;
                LastUpdate = now;
                Kernel.SendWorldMessage(new Network.GamePackets.Message("Kill!!", System.Drawing.Color.Red, Network.GamePackets.Message.Center), Program.Values);
                SendTeam();
            }
            if (send)
            {
                if (now > LastUpdate.AddSeconds(5))
                {
                    LastUpdate = now;

                    var array = new[] 
                    {
                        string.Format("Black team: {0}", Points[BlackTeam]),
                        string.Format("Blue team: {0}", Points[BlueTeam]),
                        string.Format("Red team: {0}", Points[RedTeam]),
                        string.Format("White team: {0}", Points[WhiteTeam]),
                    };
                    int Place = 0;
                    foreach (var str in array)
                    {
                        Message msg = new Message(str, System.Drawing.Color.Red, Place == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                        Kernel.SendWorldMessage(msg, Program.Values, (ushort)MAPID);
                        Place++;
                    }
                }
            }
        }
        public static void SendTeam()
        {
            foreach (Client.GameState C in Program.Values)
            {
                if (C.Entity.Tournament_Signed == true)
                {
                    C.Entity.SpawnProtection = true;
                    C.Entity.TeamDeathMatch_Hits = 0;
                    C.Entity.AppearanceBkp = C.Entity.Appearance;
                    Network.PacketHandler.ChangeAppearance(new Data(true) { ID = Data.AppearanceType, UID = C.Entity.UID, dwParam = (byte)AppearanceType.Garment }, C);
                    if (C.Entity.TeamDeathMatch_BlackTeam == true)
                    {
                        C.Entity.Teleport(8883, 042, 051);
                    }
                    if (C.Entity.TeamDeathMatch_BlueTeam == true)
                    {
                        C.Entity.Teleport(8883, 060, 042);
                    }
                    if (C.Entity.TeamDeathMatch_WhiteTeam == true)
                    {
                        C.Entity.Teleport(8883, 066, 064);
                    }
                    if (C.Entity.TeamDeathMatch_RedTeam == true)
                    {
                        C.Entity.Teleport(8883, 039, 036);
                    }
                }
            }
        }
        public static void End(int ttime)
        {
            var time = DateTime.Now;
            if (time.DayOfWeek == DayOfWeek.Tuesday && (time.Hour == 15 || time.Hour == 23) && time.Minute == 6 && end == false)
            {
                signup = false;
                end = true;
                IsOn = false;
                foreach (Client.GameState client in Program.Values)
                {
                    if (client.Entity.MapID == 8883)
                    {
                        client.Entity.Teleport(1002, 301, 266);
                        client.Entity.RemoveFlag(Update.Flags.Flashy);
                    }
                    if (client.Entity.Tournament_Signed)
                    {
                        client.Entity.Tournament_Signed = false;
                        Network.PacketHandler.ChangeAppearance(new Data(true) { ID = Data.AppearanceType, UID = client.Entity.UID, dwParam = (byte)client.Entity.AppearanceBkp }, client);
                        client.SetNewArmorLook(client.BackupArmorLook);
                        client.BackupArmorLook = 0;
                    }
                }
                Reward();
                TimerB.Dispose();
                TimerC.Dispose();
            }
        }

        public static bool redwin = false;
        public static bool blackwin = false;
        public static bool bluewin = false;
        public static bool whitewin = false;

        public static void Reward()
        {
            Dictionary<int, int> top = new Dictionary<int, int>();
            for (int i = 0; i < Points.Length; i++) top.Add(i, Points[i]);
            var array = top.OrderByDescending(o => o.Value).ToArray();
            if (array[0].Key == RedTeam)
            {
                redwin = true;
            }
            else if (array[0].Key == BlueTeam)
            {
                bluewin = true;
            }
            else if (array[0].Key == WhiteTeam)
            {
                whitewin = true;
            }
            else
            {
                blackwin = true;
            }
            int winKey = array[0].Key;
            int hits = DeathMatch.Points[winKey];
            uint prize = 250 * (uint)hits;
            string name = "RedTeam";
            if (bluewin) name = "BlueTeam";
            if (blackwin) name = "BlackTeam";
            if (whitewin) name = "WhiteTeam";
            Kernel.SendWorldMessage(new Network.GamePackets.Message(name + " have won TeamDeathMatch! The Winner Team Have Gained " + prize + " ConquerPoints", System.Drawing.Color.Red, MTA.Network.GamePackets.Message.Center), Program.Values);

            foreach (Client.GameState C in Program.Values)
            {
                #region Winner
                if (C.Entity.TeamDeathMatch_RedTeam && redwin)
                    C.Entity.ConquerPoints += prize;
                if (C.Entity.TeamDeathMatch_BlueTeam == bluewin)
                    C.Entity.ConquerPoints += prize;
                if (C.Entity.TeamDeathMatch_BlackTeam == blackwin)
                    C.Entity.ConquerPoints += prize;
                if (C.Entity.TeamDeathMatch_WhiteTeam == whitewin)
                    C.Entity.ConquerPoints += prize;
                #endregion

                C.Entity.Tournament_Signed = false;
                C.Entity.TeamDeathMatch_Hits = 0;
                C.Entity.TeamDeathMatch_RedCaptain = false;
                C.Entity.TeamDeathMatch_RedTeam = false;
                C.Entity.TeamDeathMatch_BlueCaptain = false;
                C.Entity.TeamDeathMatch_BlueTeam = false;
                C.Entity.TeamDeathMatch_BlackCaptain = false;
                C.Entity.TeamDeathMatch_BlackTeam = false;
                C.Entity.TeamDeathMatch_WhiteCaptain = false;
                C.Entity.TeamDeathMatch_WhiteTeam = false;
            }
        }
    }
}
