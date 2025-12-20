using System;
using MTA.Network.GamePackets;

namespace MTA.Game
{
    public class KillTheTerrorist
    {
        public static bool IsOn;
        public static bool Terrorist;
        public static ushort Map = 1801;
        public static void SendTimer()
        {
            Console.WriteLine("KTT Timer started!");
            System.Timers.Timer TimerA = new System.Timers.Timer(1000.0);
            TimerA.Start();
            TimerA.Elapsed += delegate { SignUp(); };

            System.Timers.Timer TimerB = new System.Timers.Timer(1000.0);
            TimerB.Start();
            TimerB.Elapsed += delegate { Send(); };

            System.Timers.Timer TimerC = new System.Timers.Timer(1000.0);
            TimerC.Start();
            TimerC.Elapsed += delegate { End(); };

            System.Timers.Timer TimerD = new System.Timers.Timer(1000.0);
            TimerD.Start();
            TimerD.Elapsed += delegate { TeleEnd(); };
        }
        public static bool signup;
        public static bool send;
        public static bool end;
        public static bool teleend;

        public static void SignUp()
        {
            if (DateTime.Now.Minute == 30 && !signup)
            {
                send = false;
                end = false;
                teleend = false;
                signup = true;
                IsOn = true;
                Kernel.SendWorldMessage(new Message("KillTheTerrorist have started. Sign Up in TwinCity! You have one minute", System.Drawing.Color.Red, Message.Center), Program.Values);
            }
        }
        public static void Send()
        {
            if (DateTime.Now.Minute == 31 && !send)
            {
                send = true;
                Terrorist = false;
                Kernel.SendWorldMessage(new Message("Kill The Terrorist! <!His Flashy!> ", System.Drawing.Color.Red, Message.Center), Program.Values);
                foreach (Client.GameState client in Program.Values)
                {
                    if (client.Entity.Tournament_Signed && !client.Entity.KillTheTerrorist_IsTerrorist)
                    {
                        client.Entity.SpawnProtection = true;
                        client.Entity.Teleport(1801, 55, 55);
                    }
                    if (client.Entity.Tournament_Signed && client.Entity.KillTheTerrorist_IsTerrorist)
                    {
                        client.Entity.Teleport(1801, 55, 50);
                        client.Entity.AddFlag(Update.Flags.Flashy);
                    }
                }
            }
        }
        public static void End()
        {
            if (DateTime.Now.Minute == 40 && !end)
            {
                signup = false;
                end = true;
                Terrorist = false;
                IsOn = false;
                foreach (Client.GameState client in Program.Values)
                {
                    if (client.Entity.KillTheTerrorist_IsTerrorist)
                    {
                        client.Entity.ConquerPoints += 150;
                        Kernel.SendWorldMessage(new Message(":" + client.Entity.Name + " was the terrorist in the end and have won 150 cps ", System.Drawing.Color.Red, Message.Center), Program.Values);
                    }
                    if (client.Entity.MapID == 1801)
                    {
                        client.Entity.Teleport(1002, 301, 266);
                    }
                    client.Entity.RemoveFlag(Update.Flags.Flashy);
                    client.Entity.KillTheTerrorist_IsTerrorist = false;
                    client.Entity.Tournament_Signed = false;
                }
            }
        }
        public static void TeleEnd()
        {
            if (DateTime.Now.Minute == 36)
            {
                Terrorist = false;
                IsOn = false;
                foreach (Client.GameState client in Program.Values)
                {
                    if (client.Entity.MapID == 1801)
                    {
                        client.Entity.Teleport(1002, 301, 266);
                    }
                    if (client.Entity.KillTheTerrorist_IsTerrorist)
                    {
                        client.Entity.ConquerPoints += 100;
                        Kernel.SendWorldMessage(new Message(":" + client.Entity.Name + " was the terrorist in the end and have won 100 cps", System.Drawing.Color.Red, Message.Center), Program.Values);
                    }
                    client.Entity.RemoveFlag(Update.Flags.Flashy);
                    client.Entity.KillTheTerrorist_IsTerrorist = false;
                    client.Entity.Tournament_Signed = false;
                }
            }
        }
    }
}
