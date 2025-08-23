using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Tournaments
{
    public class HeroOfGame
    {
        public HeroOfGame()
        {
            open = false;
        }

        public void CheakUp()
        {
            if (Matrix_Times.Start.HeroOfGame && !open)
            {
                Start();
            }
        }

        uint Secounds = 0;
        private IDisposable Subscribe;
        bool open = false;
        bool SendInvitation = false;
        public void Start()
        {
            if (!open)
            {
                Secounds = 0;
                SendInvitation = false;
                open = true;
                Subscribe = World.Subscribe(Work, 1000);
            }

        }
        public void SendMapMessaj(string packet)
        {
            var data = new Network.GamePackets.Message(packet, System.Drawing.Color.Yellow, Network.GamePackets.Message.Center);
            var MapDictionary = Kernel.GamePool.Values.Where(p => p.Entity.MapID == 1507).ToArray();
            foreach (var client in MapDictionary)
            {
                client.Send(data);
            }
        }
        public int CkeckUPAlive()
        {
            int Count = 0;
            var MapDictionary = Kernel.GamePool.Values.Where(p => p.Entity.MapID == 1507).ToArray();
            foreach (var client in MapDictionary)
            {
                if (client.Entity.Hitpoints > 0)
                    Count++;
            }
            return Count;
        }
        public void Close()
        {
            open = false;
            Subscribe.Dispose();
        }
        public void Work(int time)
        {
            if (!SendInvitation)
            {
                foreach (var client in Kernel.GamePool.Values)
                {
                    client.MessageBox("Hero OF Game has begun! Would you like to join?",
                          (p) => { p.Entity.Teleport(1507, 96, 113); }, null, 40);
                    //client.Entity.PKMode = Game.Enums.PKMode.PK;
                    //  client.Send(new Data(true) { UID = client.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)client.Entity.PKMode });
                }
                SendInvitation = true;
            }
            else
            {
                if (open)
                {
                    if (Secounds == 30)
                        SendMapMessaj("Hero Of Game will start in 30 Secounds");
                    else if (Secounds == 40)
                        SendMapMessaj("Hero Of Game will start in 20 Secounds");
                    else if (Secounds == 50)
                        SendMapMessaj("Hero Of Game will start in 10 Secounds");
                    else if (Secounds == 55)
                        SendMapMessaj("Hero Of Game will start in 5 Secounds");
                    else if (Secounds == 58)
                        SendMapMessaj("Hero Of Game will start in 3 Secounds");
                    else if (Secounds == 60)
                    {
                        var MapDictionary = Kernel.GamePool.Values.Where(p => p.Entity.MapID == 1507).ToArray();
                        foreach (var client in MapDictionary)
                        {
                            client.Entity.AllowToAttack = true;
                        }
                        SendMapMessaj("Fight !");
                    }
                    else if (Secounds > 60)
                    {
                        byte[] Messaje = new Network.GamePackets.Message("Alive Players : [ " + CkeckUPAlive() + " ]", System.Drawing.Color.Yellow, Network.GamePackets.Message.FirstRightCorner).ToArray();
                        var MapDictionar = Kernel.GamePool.Values.Where(p => p.Entity.MapID == 1507).ToArray();
                        foreach (var client in MapDictionar)
                            client.Send(Messaje);

                        if (CkeckUPAlive() == 1)
                        {
                            var client = Kernel.GamePool.Values.SingleOrDefault(p => p.Entity.MapID == 1507 && p.Entity.Hitpoints > 0);
                            if (client != null)
                            {
                                Game.Statue statue = new Statue(client.Entity.SpawnPacket);
                                statue = new Statue(client.Entity.SpawnPacket, 105177, Enums.ConquerAction.Cool, (byte)Enums.ConquerAngle.SouthWest, 318, 143, true);
                                statue = new Statue(client.Entity.SpawnPacket, 105177, Enums.ConquerAction.Wave, (byte)Enums.ConquerAngle.SouthEast, 301, 371, true);

                                client.Entity.ConquerPoints += 4000000;
                                client.Entity.AddTopStatus(Update.Flags2.Top3Trojan, 2, DateTime.Now.AddMinutes(58));
                                Kernel.SendWorldMessage(new Message("Congratulation ! " + client.Entity.Name + " Win The Hero OF Game, He Receice " + 4000000 + " CPS!", Color.White, Message.BroadcastMessage), Program.Values);
                                Close();
                            }
                            foreach (var player in Kernel.GamePool.Values)
                            {
                                if (player.Entity.MapID == 1507)
                                    player.Entity.Teleport(1002, 301, 278);
                                player.Entity.AllowToAttack = false;
                            }
                        }
                        else if (CkeckUPAlive() == 0)
                        {
                            Close();
                        }
                    }
                    Secounds++;
                }
            }
        }
    }
}
