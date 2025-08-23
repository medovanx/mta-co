using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Client;


namespace MTA.Game
{
    using Coordinate = Tuple<ushort, ushort, ushort>;
    using System.Drawing;
    using MTA.Game.ConquerStructures.Society;
    using MySql.Data.MySqlClient;
    using System.Collections.Concurrent;
    using System.IO;

    public class PoleDomination
    {
        public const uint MaxHitPoints = 30000000;
        private SobNpcSpawn Pole = new SobNpcSpawn()
        {
            Hitpoints = MaxHitPoints,
            MaxHitpoints = MaxHitPoints,
            Mesh = 1137,
            ShowName = true,
            Name = "Domination Pole",
            Type = (Enums.NpcType)10,
            Sort = 21,
            UID = 123456
        };

        private IDisposable Subscriber;
        private Coordinate[] Locations = new[]
        {
            new Coordinate(1002, 300, 288),
            new Coordinate(1011, 286, 310),
            new Coordinate(1020, 622, 592),
            new Coordinate(1000, 507, 711),
            new Coordinate(1015, 737, 641)
        };
        private int currentLocation;
        private bool poleKilled;
        private Map currentMap;
        private Coordinate currentCoordinate;
        private uint prize;
        private Guild lastKiller;
        public uint KillerGuildID { get { if (lastKiller == null) return 0; return lastKiller.ID; } }
        private ConcurrentDictionary<Guild, ulong> damages;
        private List<string> winners;

        public PoleDomination(uint prize)
        {
            Subscriber = World.Subscribe(work, 1000);
            currentLocation = -1;
            this.prize = prize;
            damages = new ConcurrentDictionary<Guild, ulong>();
            winners = new List<string>();
            File.Open(Constants.DatabaseBasePath + "poledomination.txt", FileMode.OpenOrCreate).Close();
            foreach (var name in File.ReadAllLines(Constants.DatabaseBasePath + "poledomination.txt"))
                if (name.Length != 0)
                    winners.Add(name);
        }

        private void work(int time)
        {
            Time32 time32 = new Time32(time);
            DateTime time64 = DateTime.Now;
            //  if (time64.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (Matrix_Times.Start.PoleDomnation/*time64.Hour == 00*/)
                //if (time64.Hour == 17)
                {
                    if (time64.Minute % 12 == 0)
                    {
                        if (currentLocation != time64.Minute / 12)
                        {
                            ushort ID = 0;
                            if (currentLocation != -1)
                            {
                                ID = currentMap.ID;
                                removePole();
                            }
                            setWinner();
                            bool killedPole = poleKilled;
                            poleKilled = false;
                            lastKiller = null;
                            currentLocation = time64.Minute / 12;
                            currentCoordinate = Locations[currentLocation];
                            if (!Kernel.Maps.ContainsKey(currentCoordinate.Item1))
                                new Map(currentCoordinate.Item1, "");
                            currentMap = Kernel.Maps[currentCoordinate.Item1];
                            Pole.MapID = currentMap.ID;
                            Pole.X = currentCoordinate.Item2;
                            Pole.Y = currentCoordinate.Item3;
                            currentMap.AddPole(Pole);
                            Pole.Hitpoints = Pole.MaxHitpoints = MaxHitPoints;
                            Kernel.Execute((client) =>
                            {
                                if (client.Entity.MapID == currentMap.ID)
                                    if (client.Screen.Add(Pole))
                                        client.Send(Pole);
                                client.MessageOK = (pClient) => { joinClient(pClient); };
                                client.Send(new NpcReply(NpcReply.MessageBox, "Pole Domination began in " + ((Enums.Maps)currentMap.ID).ToString() + ". Would you like to join?"));
                            });
                            currentMap.WasPKFree = Constants.PKFreeMaps.Contains(currentMap.ID);
                            if (!currentMap.WasPKFree) Constants.PKFreeMaps.Add(currentMap.ID);
                            if (currentLocation != -1 && (killedPole && lastKiller != null && currentMap != null))
                                Kernel.SendWorldMessage(new Message("Guild " + lastKiller.Name + " won the pole in " + ((Enums.Maps)ID).ToString() + "!" + "The Pole Domination moved to " + ((Enums.Maps)currentMap.ID).ToString() + "!", Color.Red, Message.Guild));
                            else
                                Kernel.SendWorldMessage(new Message("The Pole Domination moved to " + ((Enums.Maps)currentMap.ID).ToString() + "!", Color.Red, Message.Guild));
                        }
                    }
                    if (time64.Second % 5 == 0)
                    {
                        var array = damages.OrderByDescending((p) => p.Value);
                        int Place = 0;
                        foreach (KeyValuePair<Guild, ulong> entry in array)
                        {
                            string str = "No  " + (Place + 1).ToString() + ": " + entry.Key.Name + "(" + entry.Value + ")";

                            Message msg = new Message(str, System.Drawing.Color.Red, Place == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                            Kernel.SendWorldMessage(msg, Program.Values, (ushort)currentMap.ID);
                            Place++;
                            if (Place == 4) break;
                        }
                    }
                }
                else
                {
                    if (Pole.MapID != 0)
                    {
                        currentMap = Kernel.Maps[Pole.MapID];
                        removePole();
                        currentLocation = -1;
                        setWinner();
                        if (poleKilled)
                            Kernel.SendWorldMessage(new Message("Guild " + lastKiller.Name + " won the pole in " + ((Enums.Maps)currentMap.ID).ToString() + "!", Color.Red, Message.Center));
                        poleKilled = false;
                        lastKiller = null;
                        Pole.MapID = 0;
                    }
                }
            }
        }
        private void setWinner()
        {
            if (poleKilled)
            {
                Kernel.SendWorldMessage(new Message("Guild " + lastKiller.Name + " won the pole in " + ((Enums.Maps)currentMap.ID).ToString() + "!", Color.Red, Message.Guild));
                /*if (lastKiller.Leader.IsOnline)
                    lastKiller.Leader.Client.Entity.ConquerPoints += prize;
                else
                {
                    using (var conn = Database.DataHolder.MySqlConnection)
                    {
                        conn.Open();
                        using (var cmd = new MySqlCommand("update entities set conquerpoints = conquerpoints + " + prize + " where UID=" + lastKiller.Leader.ID, conn))
                            cmd.ExecuteNonQuery();
                    }
                }*/
                winners.Add(lastKiller.Name);
                SaveWinners();
            }
        }

        public void SaveWinners()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var line in winners) builder.AppendLine(line);
            File.WriteAllText(Constants.DatabaseBasePath + "poledomination.txt", builder.ToString());
        }
        public uint GetWinnerPrize(string name)
        {
            return (uint)(prize * winners.Count(p => p == name));
        }
        public void RemoveWinner(string name)
        {
            winners.RemoveAll(p => p == name);
        }

        private void joinClient(GameState client)
        {
            int x = Kernel.Random.Next(36) - 18, y = Kernel.Random.Next(36) - 18;
            int times = 1000;
            while (times-- > 0)
            {
                if (!currentMap.Floor[Pole.X + x, Pole.Y + y, MapObjectType.Player, null])
                {
                    x = Kernel.Random.Next(36) - 18;
                    y = Kernel.Random.Next(36) - 18;
                }
                else break;
            }
            if (times == 0) return;
            client.Entity.Teleport(currentMap.BaseID, (ushort)(Pole.X + x), (ushort)(Pole.Y + y));;

        }

        internal void KillPole()
        {
            var array = damages.OrderByDescending((p) => p.Value).ToArray();
            if (array.Length != 0)
                lastKiller = array[0].Key;
            Pole.Hitpoints = Pole.MaxHitpoints = MaxHitPoints;
            Pole.Name = lastKiller.Name;
            Kernel.Execute((client) =>
            {
                if (client.Entity.MapID == currentMap.ID)
                    client.Send(Pole);
            });
            Kernel.SendWorldMessage(new Message("Guild " + lastKiller.Name + " have taken the pole in " + ((Enums.Maps)currentMap.ID).ToString() + "! Are they going to win?", Color.Red, Message.Guild));
            poleKilled = true;
            damages.Clear();
            //prize(killer.Owner);
            //poleKilled = true;
        }

        private void removePole()
        {
            currentMap.RemovePole(Pole);
            Kernel.Execute((client) =>
            {
                if (client.Entity.MapID == currentMap.ID)
                {
                    client.Screen.Remove(Pole);
                    client.Send(new Data(true) { UID = Pole.UID, ID = Data.RemoveEntity });
                }
            });
            if (!currentMap.WasPKFree)
                Constants.PKFreeMaps.Remove(currentMap.ID);
            damages.Clear();
        }

        internal void AddScore(uint damage, Guild guild)
        {
            if (guild == null) return;
            if (!damages.ContainsKey(guild)) damages[guild] = 0;
            damages[guild] += damage;
        }
    }
}
