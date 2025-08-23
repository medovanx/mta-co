using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Features
{
    public class BreakingDown
    {
        public const ushort RoundTime = 4;
        public static Dictionary<uint, BreakingDown> BreakingDownQuests = new Dictionary<uint, BreakingDown>();
        public DateTime CountDown;
        public ushort MapID;
        public ushort Stage;
        public uint UID;
        private static readonly object SyncRoot = new object();
        public QuestTypes QuestType { get; set; }

        public enum QuestTypes
        {
            Normal,
            Ultra
        }

        public BreakingDown(DateTime EndTime, ushort QuestMapID, ushort QuestLevel, uint HeroUID, QuestTypes questTypes)
        {
            CountDown = EndTime;
            MapID = QuestMapID;
            Stage = QuestLevel;
            UID = HeroUID;
            QuestType = questTypes;
        }
        public static void Work(int time)
        {
            foreach (var players in BreakingDownQuests.Values.ToList())
            {
                if (players == null) continue;
                if (!Kernel.Maps.ContainsKey(players.MapID)) return;
                var map = Kernel.Maps[players.MapID];
                if (map == null) continue;
                var monsters = map.Entities.Values.Where(z => !z.Dead).ToList();
                //  if (monsters == null) continue;
                if (monsters.Count == 0)
                    players.CountDown = DateTime.Now;
                if (DateTime.Now < players.CountDown) continue;
                if (players.Stage == 5 && monsters.Count == 0)
                {
                    foreach (var item in Program.Values.Where(s => s.Map.ID == players.MapID).ToList())
                    {
                        item.Entity.Teleport(1002, 300, 263);
                    }
                    Kernel.Maps[players.MapID].Dispose();
                    BreakingDownQuests.Remove(players.UID);

                }
                if (monsters.Count != 0)
                {
                    foreach (var player in Program.Values.Where(s => s.Map.ID == players.MapID).ToList())
                    {
                        player.Entity.Teleport(1002, 301, 278);
                        player.Send(new Message("You didn`t kill all the monesters in the map, You have been sent back to TwilCity. ", System.Drawing.Color.Red, Message.Whisper));
                    }
                    Kernel.Maps[players.MapID].Dispose();
                    BreakingDownQuests.Remove(players.UID);
                    continue;
                }
                players.Stage++;
                players.CountDown = DateTime.Now.AddMinutes(RoundTime);
                ushort x = 0, y = 0, amount = 0;
                switch (players.Stage)
                {
                    case 2:
                        x = 499;
                        y = 348;
                        amount = 50;
                        break;
                    case 3:
                        x = 253;
                        y = 123;
                        amount = 60;
                        break;
                    case 4:
                        x = 113;
                        y = 221;
                        amount = 70;
                        break;
                    case 5:
                        {
                            x = 269;
                            y = 257;
                            amount = 1;
                            break;
                        }
                }
                players.Spawn(x, y, amount);
                foreach (var player in Program.Values.Where(s => s.Map.ID == players.MapID).ToList())
                {
                    player.Send(new Message("You Finished This Stage, You are Now in stage " + players.Stage, System.Drawing.Color.Red, Message.Whisper));
                    player.Send(new Data(true) { UID = player.Entity.UID, dwParam = (RoundTime * 60), ID = Data.CountDown });
                    player.Send(new Data(true) { UID = player.Entity.UID, wParam1 = x, wParam2 = y, ID = Data.TeamSearchForMember });
                }
            }
        }

        public void Spawn(ushort x, ushort y, ushort Amount)
        {
            var id = (ushort)(Stage != 5 ? 7487 : 7483);
            MonsterInformation mt;
            MonsterInformation.MonsterInformations.TryGetValue(id, out mt);
            if (mt == null) return;
            mt.IsRespawnAble = false;
            if (Stage == 5) mt.Boss = true;
            //       if (QuestType == QuestTypes.Ultra) mt.Boss = true;
            mt.RespawnTime += 5;
            for (var i = 0; i < Amount; i++)
            {
                var entity = new Entity(EntityFlag.Monster, false)
                {
                    MapObjType = MapObjectType.Monster,
                    MonsterInfo = mt.Copy()
                };
                entity.MonsterInfo.Owner = entity;
                entity.Name = string.Intern(mt.Name);
                entity.MinAttack = mt.MinAttack;
                entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
                entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
                entity.Body = mt.Mesh;
                entity.Level = mt.Level;
                entity.MapID = MapID;
                entity.SendUpdates = true;
                entity.UID = Kernel.Maps[MapID].EntityUIDCounter.Next;
                entity.X = (ushort)(x + Kernel.Random.Next(0, 15));
                entity.Y = (ushort)(y + Kernel.Random.Next(0, 15));
                Kernel.Maps[MapID].AddEntity(entity);
            }
        }

        public static void Add(GameState client, QuestTypes questTypes)
        {
            lock (SyncRoot)
            {
                var UID = client.Team == null ? client.Entity.UID : client.Team.UID;
                if (UID == 0) return;
                if (BreakingDownQuests.ContainsKey(UID))
                {
                    var Quest = BreakingDownQuests[UID];
                    client.Entity.Teleport(Quest.MapID, 269, 257);
                    client.Send(new Data(true)
                    {
                        UID = client.Entity.UID,
                        dwParam = (uint)(Quest.CountDown - DateTime.Now).TotalSeconds,
                        ID = Data.CountDown
                    });
                    client.Send(new Data(true)
                    {
                        UID = client.Entity.UID,
                        wParam1 = 378,
                        wParam2 = 474,
                        ID = Data.TeamSearchForMember
                    });

                }
                else
                {
                    var DynamicMap = Kernel.Maps[1201].MakeDynamicMap();
                    DynamicMap.AddMonesterTimer();
                    var BD = new BreakingDown(DateTime.Now.AddMinutes(RoundTime), DynamicMap.ID, 1, UID, questTypes);
                    BreakingDownQuests.Add(UID, BD);
                    client.Entity.Teleport(DynamicMap.ID, 269, 257);
                    BD.Spawn(378, 474, 40);
                    client.Send(new Data(true)
                    {
                        UID = client.Entity.UID,
                        wParam1 = 378,
                        wParam2 = 474,
                        ID = Data.TeamSearchForMember
                    });
                    client.Send(new Data(true) { UID = client.Entity.UID, dwParam = (RoundTime * 60), ID = Data.CountDown });
                }
            }
        }
    }
}