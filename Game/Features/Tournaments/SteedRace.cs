using MTA.Game;
using MTA.Network.GamePackets;
using System;
using System.Threading.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game
{
    using Colors = System.Drawing.Color;
    using MTA.Client;
    using System.Threading.Generic;

    public class SteedRace
    {
        public uint[][] RaceSettings = new[]
        {
            new uint[]{ (uint)Enums.Maps.MarketRace, 88, 149, 
                420, 431, 4,  
                65, 174, 621,
                123, 243, 60, 
                214, 334, 70, 
                346, 459, 100},
            new uint[]{ (uint)Enums.Maps.IceRace, 175, 250, 
                200, 153, 6, //255, 535*
                154, 267, 621,
                146, 392, 70,
                283, 351, 100,
                295, 079, 100},
            new uint[]{ (uint)Enums.Maps.IslandRace, 60, 400, 
                899, 816, 10,
                96, 392, 621,
                220, 234, 200,
                472, 160, 200,
                777, 464, 300},
            new uint[]{ (uint)Enums.Maps.DungeonRace, 450, 520,
                682, 484 , 10,
                435, 559, 621,
                471, 759, 200,
                714, 598, 250,
                489, 679, 20},
            new uint[]{ (uint)Enums.Maps.LavaRace, 150, 350,
                330, 170, 6,
                101, 397, 623,
                327, 553, 100,
                526, 477, 200,
                283, 275, 100}
        };
        public static ushort MAPID = 1950;
        public static uint[] Settings;
        public static uint RaceRecord;

        private Map Map;
        private bool isOn, InvitationsOut, InvitationsExpired, FiveSecondsLeft, GateOpen;
        private DateTime InvitationsSentOut, InvitationsExpireDate, Last5Seconds, GateOpened;

        public int SecondsLeftUntilStart { get { return (int)(InvitationsSentOut.AddMinutes(1) - DateTime.Now).TotalSeconds - 5; } }
        public bool CanJoin { get { return isOn && !GateOpen; } }
        public bool IsOn { get { return isOn; } }
        private SobNpcSpawn Gate;
        private ushort GateSetX, GateSetY;
        public ushort GateX { get { return GateSetX; } }
        public ushort GateY { get { return GateSetY; } }

        private volatile int Records;
        private IDisposable Subscriber;

        public SteedRace()
        {
            Subscriber = World.Subscribe(work, 1000);
        }

        public void Create()
        {
            while (true)
            {
                //Console.WriteLine("Looking for a race map!");
                int rand = Kernel.Random.Next(RaceSettings.Length);
                if (Database.MapsTable.MapInformations.ContainsKey((ushort)RaceSettings[rand][0]))
                {
                    if (!Kernel.Maps.ContainsKey((ushort)RaceSettings[rand][0]))
                        new Map((ushort)RaceSettings[rand][0], "");
                    if (Kernel.Maps.ContainsKey((ushort)RaceSettings[rand][0]))
                        Create(RaceSettings[rand][0]);
                    break;
                }
            }
        }
        public void Create(uint mapId)
        {
            int index = -1;
            for (int i = 0; i < RaceSettings.Length; i++)
            {
                if (RaceSettings[i][0] == mapId)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1) return;
            Settings = RaceSettings[index];
            MAPID = (ushort)Settings[0];
            var info = Database.MapsTable.MapInformations[MAPID];

            RaceRecord = info.RaceRecord;
            Map = Kernel.Maps[MAPID];
            foreach (var item in Map.StaticEntities.Values)
                Map.Floor[item.X, item.Y, MapObjectType.StaticEntity, null] = true;
            Map.StaticEntities.Clear();
            Map.Npcs.Clear();
            Map.Entities.Clear();

            Gate = new SobNpcSpawn();
            Gate.UID = 19501;
            Gate.X = (ushort)Settings[6];
            Gate.Y = (ushort)Settings[7];
            Gate.Mesh = (ushort)Settings[8];
            Gate.ShowName = true;
            Gate.Name = " ";
            Gate.Type = Enums.NpcType.Furniture;
            GateSetX = Gate.X; GateSetY = Gate.Y;
            Map.AddNpc(Gate);//, false);
            MAPID = (ushort)mapId;
            Init();
        }

        private void Init()
        {
            isOn = InvitationsOut = InvitationsExpired = GateOpen = FiveSecondsLeft = false;
            GeneratePotions();
            Records = 0;
        }

        private void GeneratePotions()
        {
            uint count = 100;
            ushort x, y;
            Tuple<ushort, ushort, int>[] limits = new[]
            {
                new  Tuple<ushort,ushort,int>( (ushort)Settings[9], (ushort)Settings[10], (int)Settings[11] ),
                new  Tuple<ushort,ushort,int>( (ushort)Settings[12], (ushort)Settings[13], (int)Settings[14] ),
                new  Tuple<ushort,ushort,int>( (ushort)Settings[15], (ushort)Settings[16], (int)Settings[17] )
            };

            while (count > 0)
            {
                x = (ushort)Kernel.Random.Next(0, Map.Floor.Bounds.Width);
                y = (ushort)Kernel.Random.Next(0, Map.Floor.Bounds.Height);
                bool valid = false;
                foreach (var range in limits)
                    valid |= (Kernel.GetDistance(x, y, range.Item1, range.Item2) < range.Item3);
                if (valid)
                {
                    if (Map.Floor[x, y, MapObjectType.StaticEntity, null] && Map.Floor[x, y, MapObjectType.Player, null])
                    {
                        bool v = true;
                        // so they wont be anywhere near the bounds
                        // and also there wont be one too near to another
                        for (int i = 0; i < Game.Map.XDir.Length; i++)
                            if ((!Map.Floor[x + Game.Map.XDir[i], y + Game.Map.YDir[i], MapObjectType.Player, null] ||
                                !Map.Floor[x + Game.Map.XDir[i], y + Game.Map.YDir[i], MapObjectType.StaticEntity, null]) && v)
                                v = false;
                        if (!v) continue;

                        StaticEntity item = new StaticEntity((uint)(x * 1000 + y), x, y, MAPID);
                        item.Pick();

                        item.MapID = MAPID;
                        Map.AddStaticEntity(item);
                        count--;
                    }
                }
            }
        }

        public void work(int time)
        {
            DateTime now = DateTime.Now;
            //bool rightHour = now.Hour == 8 || now.Hour == 12 || now.Hour == 18;
            bool rightHour = now.Hour == 1 || now.Hour == 2 || now.Hour == 3 ||
                 now.Hour == 4 || now.Hour == 5 || now.Hour == 6 || now.Hour == 7 ||
                  now.Hour == 8 || now.Hour == 9 || now.Hour == 10 || now.Hour == 11 ||
                   now.Hour == 12 || now.Hour == 13 || now.Hour == 14 || now.Hour == 15 ||
                    now.Hour == 16 || now.Hour == 17 || now.Hour == 18 || now.Hour == 19 ||
                     now.Hour == 20 || now.Hour == 21 || now.Hour == 22 || now.Hour == 23 ||
                      now.Hour == 24;
            if (rightHour && now.Minute < 31)
            {
                if (!InvitationsOut)
                {
                    if (now.Minute == 00 && now.Second == 07)
                    {
                        Create();
                        SendInvitations();
                    }
                }
                else if (!InvitationsExpired)
                {
                    if (now >= InvitationsExpireDate)
                    {
                        InvitationsExpired = true;
                        FiveSecondsLeft = false;
                        Last5Seconds = InvitationsSentOut.AddMinutes(1).AddSeconds(-12);
                    }
                }
                else if (!FiveSecondsLeft)
                {
                    if (now > Last5Seconds)
                    {
                        FiveSecondsLeft = true;
                        SendData(Data.BeginSteedRace, uid: 1);
                        Last5Seconds = Last5Seconds.AddSeconds(5);
                    }
                }
                else if (!GateOpen)
                {
                    if (now > Last5Seconds)
                    {
                        OpenGate();
                    }
                }
            }
            else if (rightHour && now.Minute >= 31)
            {
                if (isOn)
                {
                    End();
                }
            }
        }

        private void SendInvitations()
        {
            isOn = true;
            InvitationsOut = true;
            InvitationsExpired = false;
            InvitationsSentOut = DateTime.Now;
            InvitationsExpireDate = InvitationsSentOut.AddSeconds(30);

            foreach (var client in Program.Values)
            {
                if (client.Entity.MapID >= 6000 && client.Entity.MapID <= 6002) continue;
                client.MessageCancel = (pClient) =>
                {
                    pClient.Send(new Message("If you change your mind about joining the Steed Race you can see the Mount Trainer (Twin City qqq,www).", System.Drawing.Color.Red, Message.World));
                };
                client.MessageOK = (pClient) =>
                {
                    if (!isOn)
                    {
                        pClient.Send(new Message("The tournament has ended.", System.Drawing.Color.Red, Message.Center));
                    }
                    else if (isOn && InvitationsExpired)
                    {
                        pClient.Send(new Message("You lost your chance to join the steed race.", System.Drawing.Color.Red, Message.Center));
                    }
                    else if (InvitationsOut && !InvitationsExpired)
                    {
                        if (!pClient.Spells.ContainsKey(7001))
                        {
                            pClient.Send("You need learn the riding skill!");
                        }
                        else
                        {
                            if (!pClient.Equipment.Free(ConquerItem.Steed))
                                Join(pClient);
                            else
                                pClient.Send("You need to wear a horse first!");
                        }
                    }
                };
                client.Send(new Network.GamePackets.NpcReply(Network.GamePackets.NpcReply.MessageBox, "Would you like to join the Steed Race?"));
                client.Send(new Data(true) { UID = client.Entity.UID, ID = Data.CountDown, dwParam = 60 });
            }
            Kernel.SendWorldMessage(new Network.GamePackets.Message("SteedRace has started You have 1 minute to signup go to TC HorseRaceManager!.", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);
        }

        public void Join(Client.GameState client)
        {
            int seconds = SecondsLeftUntilStart;
            if (seconds > 0)
                client.Send(new Data(true) { UID = client.Entity.UID, ID = Data.CountDown, dwParam = (uint)seconds });
            client.Entity.AddFlag(Update.Flags.Ride);
            client.Entity.Teleport(MAPID, (ushort)Settings[1], (ushort)Settings[2]);
            client.Send(new RaceRecord()
            {
                Type = RaceRecordTypes.BestTime,
                Rank = (int)Database.MapsTable.MapInformations[MAPID].RaceRecord, // best time in milliseconds
                dwParam = 1800000 //a constant? 
            });
            client.Send(new RacePotion(true) { PotionType = Enums.RaceItemType.Null, Amount = 1 });
            client.Send(new RacePotion(true) { PotionType = Enums.RaceItemType.Null, Amount = 0 });
            client.Potions = new UsableRacePotion[5];
        }
        private void OpenGate()
        {
            GateOpened = DateTime.Now;
            GateOpen = true;
            Gate.X = 0;
            Gate.Y = 0;
            Send(new Data(true) { UID = Gate.UID, ID = Data.RemoveEntity });
        }

        private void Send(Interfaces.IPacket packet)
        {
            Kernel.SendWorldMessage(packet, Program.Values, MAPID);
        }
        private void SendData(ushort ID, uint value = 0, uint uid = 0)
        {
            Data data = null;
            if (uid != 0)
                data = new Data(true) { UID = uid, ID = ID, dwParam = value };

            foreach (var player in Program.Values)
            {
                if (player.Entity.MapID == MAPID)
                {
                    if (uid == 0)
                    {
                        data = new Data(true) { UID = player.Entity.UID, ID = ID, dwParam = value };
                        player.Send(data);
                    }
                    else
                    {
                        player.Send(data);
                    }
                }
            }
        }

        private void Status(Client.GameState client, int rank, int time, int award)
        {
            var packet = new RaceRecord()
            {
                Type = RaceRecordTypes.AddRecord,
                Rank = rank,
                Name = client.Entity.Name,
                Time = time,
                Prize = award
            };
            Send(packet);
        }

        private void End()
        {
            InvitationsOut = false;
            Gate.X = GateSetX;
            Gate.Y = GateSetY;
            foreach (var player in Program.Values)
                if (player.Entity.MapID == MAPID)
                    Exit(player);
            Init();
        }

        public void FinishRace(Client.GameState client)
        {
            // if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, (ushort)Settings[3], (ushort)Settings[4]) > 22)
            //  {
            //      return;
            //  }
            //  else
            {
                if (Records < 5)
                {
                    Records++;
                    int rank = Records;
                    TimeSpan span = DateTime.Now - GateOpened;
                    var key = Database.MapsTable.MapInformations[MAPID];
                    if (key.RaceRecord > span.TotalMilliseconds)
                    { // new best record
                        key.RaceRecord = (uint)span.TotalMilliseconds;
                        Database.MapsTable.SaveRecord(key);
                    }
                    int award = AwardPlayer(client, (int)span.TotalMilliseconds, rank);
                    client.RacePoints += (uint)award;
                    Status(client, rank, (int)span.TotalMilliseconds, award);
                    client.Send(new RaceRecord()
                    {
                        Type = RaceRecordTypes.EndTime,
                        Rank = rank,
                        dwParam = (int)span.TotalMilliseconds,
                        dwParam2 = award,
                        Time = (int)span.TotalMilliseconds,
                        Prize = award
                    });
                }
                Exit(client);
            }
        }

        private int AwardPlayer(Client.GameState client, int time, int rank)
        {
            return Math.Max(3500, 100000 / rank - time * 2);
        }

        public void Exit(Client.GameState client)
        {
            switch (client.Entity.PreviousMapID)
            {
                default:
                    {
                        client.Entity.Teleport(1002, 301, 278);
                        break;
                    }
                case 1000:
                    {
                        client.Entity.Teleport(1000, 500, 650);
                        break;
                    }
                case 1020:
                    {
                        client.Entity.Teleport(1020, 565, 562);
                        break;
                    }
                case 1011:
                    {
                        client.Entity.Teleport(1011, 188, 264);
                        break;
                    }
                case 1015:
                    {
                        client.Entity.Teleport(1015, 717, 571);
                        break;
                    }
            }
        }
    }
}