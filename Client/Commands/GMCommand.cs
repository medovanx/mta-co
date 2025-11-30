using System;
using System.Linq;
using MTA.Network.GamePackets;
using MTA.Network;
using MTA.Game.ConquerStructures;
using MTA.Game;
using MTA.MaTrix;

namespace MTA.Client.Commands
{
    public static class GMCommands
    {
        public static bool CheckCommand(GameState client, string message)
        {
            try
            {
                if (message.StartsWith("@"))
                {
                    string message_ = message.Substring(1).ToLower();
                    string Mess = message.Substring(1);
                    string[] Data = message_.Split(' ');

                    // Try NPC commands first
                    if (NpcCommands.HandleCommand(client, Data, Mess))
                        return true;

                    // Try Item commands
                    if (ItemCommands.HandleCommand(client, Data, Mess))
                        return true;

                    // Try Stuff commands
                    if (StuffCommands.HandleCommand(client, Data, Mess))
                        return true;

                    // Try Currency commands
                    if (EntityCommands.HandleCommand(client, Data, Mess))
                        return true;

                    if (Data[0] == "mob" || Data[0] == "effect")
                        Data = message.Substring(1).Split(' ');

                    switch (Data[0])
                    {
                        case "xzero":
                            {
                                byte[] tets = new byte[12 + 8];
                                Writer.Ushort(12, 0, tets);
                                Writer.Ushort(2710, 2, tets);
                                Writer.Uint(uint.Parse(Data[1]), 4, tets);
                                client.Send(tets);
                                break;
                            }

                        case "xfloor":
                            {
                                FloorItem floorItem = new FloorItem(true);
                                floorItem.ItemID = uint.Parse(Data[1]);
                                floorItem.MapID = client.Entity.MapID;
                                floorItem.Type = FloorItem.Effect;
                                floorItem.X = (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5);
                                floorItem.Y = (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5);
                                floorItem.OnFloor = Time32.Now;
                                floorItem.Owner = client;
                                floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                while (client.Map.FloorItems.ContainsKey(floorItem.UID))
                                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;

                                floorItem.MaxLife = 25;
                                floorItem.Life = 25;
                                floorItem.mColor = 13;
                                floorItem.OwnerUID = client.Entity.UID;
                                floorItem.OwnerGuildUID = client.Entity.GuildID;
                                floorItem.FlowerType = byte.Parse(Data[2]);
                                floorItem.Timer = Kernel.TqTimer(DateTime.Now.AddSeconds(7));
                                floorItem.Name = "AuroraLotus";
                                client.Map.AddFloorItem(floorItem);
                                client.SendScreenSpawn(floorItem, true);
                                break;
                            }
                        case "transpoint":
                            {
                                client.Entity.TransferPoints = 5000;
                                break;
                            }
                        case "floor":
                            {
                                var id = ++client.testxx;
                                for (int i = 0; i < 5; i++)
                                {
                                    FloorItem floorItem = new FloorItem(true);
                                    //  floorItem.ItemID = FloorItem.DaggerStorm;
                                    floorItem.ItemID = id;
                                    floorItem.MapID = client.Entity.MapID;
                                    floorItem.ItemColor = (Enums.Color)i;
                                    floorItem.Type = FloorItem.Effect;
                                    floorItem.X = (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5);
                                    floorItem.Y = (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5);
                                    floorItem.OnFloor = Time32.Now;
                                    floorItem.Owner = client;
                                    while (client.Map.Npcs.ContainsKey(floorItem.UID))
                                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                    client.Map.AddFloorItem(floorItem);
                                    client.SendScreenSpawn(floorItem, true);
                                }

                                client.Send(new Message(client.testxx.ToString(), Message.Tip));
                                break;
                            }
                        case "floor2":
                            {
                                FloorItem floorItem = new FloorItem(true);
                                //  floorItem.ItemID = FloorItem.DaggerStorm;
                                floorItem.ItemID = uint.Parse(Data[1]);
                                floorItem.MapID = client.Entity.MapID;
                                floorItem.ItemColor = Enums.Color.Black;
                                floorItem.Type = FloorItem.Effect;
                                floorItem.X = client.Entity.X;
                                floorItem.Y = client.Entity.Y;
                                floorItem.OnFloor = Time32.Now;
                                floorItem.Owner = client;
                                while (client.Map.Npcs.ContainsKey(floorItem.UID))
                                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                client.Map.AddFloorItem(floorItem);
                                client.SendScreenSpawn(floorItem, true);
                                break;
                            }
                        case "serverid3":
                            {
                                client.Entity.CUID = client.Entity.UID;
                                client.Entity.UID = (uint.MaxValue - client.Entity.UID);
                                byte[] tets = new byte[16 + 8];
                                Writer.Ushort(16, 0, tets);
                                Writer.Ushort(2501, 2, tets);
                                Writer.Uint(client.Entity.CUID, 8, tets);
                                Writer.Uint(client.Entity.UID, 12, tets);
                                client.Send(tets);

                                _String str = new _String(true);
                                str.Type = 61;
                                str.Texts.Add("Matrix");
                                client.Send(str);

                                client.Send(new Data(true)
                                {
                                    UID = client.Entity.UID,
                                    ID = Network.GamePackets.Data.ChangePKMode,
                                    dwParam = (uint)Enums.PkMode.CS
                                });
                                break;
                            }
                        case "transferserver":
                            {
                                Data data = new Network.GamePackets.Data(true);
                                data.UID = client.Entity.UID;
                                data.dwParam = 666;
                                data.ID = 126;
                                client.Send(data);
                                break;
                            }
                        case "serverid":
                            {
                                client.Entity.ServerID = byte.Parse(Data[1]);
                                client.SendScreenSpawn(client.Entity, true);
                                break;
                            }
                        case "testaura84":
                            {
                                if (client.Team != null)
                                {
                                    foreach (var item in client.Team.Teammates)
                                    {
                                        Update update = new Update(true);
                                        update.UID = item.Entity.UID;
                                        update.Append(52, 1320);
                                        item.Send(update);

                                        //   if (!item.Team.TeamLider(item))
                                        {
                                            var data = new Data(true);
                                            data.UID = client.Team.Lider.Entity.UID;
                                            data.dwParam = client.Team.Lider.Entity.MapID;
                                            data.ID = 101;
                                            data.wParam1 = client.Team.Lider.Entity.X;
                                            data.wParam2 = client.Team.Lider.Entity.Y;
                                            item.Send(data);
                                        }
                                    }
                                }

                                break;
                            }
                        case "rev2":
                            {
                                foreach (var item in client.Screen.Objects)
                                {
                                    if (item.MapObjType == MapObjectType.Player)
                                    {
                                        var Entity = item as Entity;
                                        Entity.Action = MTA.Game.Enums.ConquerAction.None;
                                        client.ReviveStamp = Time32.Now;
                                        client.Attackable = false;

                                        Entity.TransformationID = 0;
                                        Entity.RemoveFlag(Update.Flags.Dead);
                                        Entity.RemoveFlag(Update.Flags.Ghost);
                                        Entity.Hitpoints = client.Entity.MaxHitpoints;
                                        Entity.Mana = client.Entity.MaxMana;
                                    }
                                }

                                break;
                            }
                        case "progressbar":
                            {
                                new Franko.ProgressBar(client, "Loading", null, "Completed", uint.Parse(Data[1]));
                                break;
                            }
                        case "gmchi":
                            {
                                PacketHandler.CheckCommand("@matrixchi 1 1 1", client);
                                PacketHandler.CheckCommand("@matrixchi 1 2 6", client);
                                PacketHandler.CheckCommand("@matrixchi 1 3 7", client);
                                PacketHandler.CheckCommand("@matrixchi 1 4 5", client);

                                PacketHandler.CheckCommand("@matrixchi 2 1 1", client);
                                PacketHandler.CheckCommand("@matrixchi 2 2 6", client);
                                PacketHandler.CheckCommand("@matrixchi 2 3 7", client);
                                PacketHandler.CheckCommand("@matrixchi 2 4 5", client);

                                PacketHandler.CheckCommand("@matrixchi 3 1 1", client);
                                PacketHandler.CheckCommand("@matrixchi 3 2 6", client);
                                PacketHandler.CheckCommand("@matrixchi 3 3 7", client);
                                PacketHandler.CheckCommand("@matrixchi 3 4 5", client);

                                PacketHandler.CheckCommand("@matrixchi 4 1 1", client);
                                PacketHandler.CheckCommand("@matrixchi 4 2 6", client);
                                PacketHandler.CheckCommand("@matrixchi 4 3 7", client);
                                PacketHandler.CheckCommand("@matrixchi 4 4 5", client);
                                break;
                            }

                        case "nobiltypole":
                            {
                                NobiltyPoleWar.StartWar();
                                break;
                            }

                        //case "classpole":
                        //    {
                        //        if (!ClassPoleWar.IsWar)
                        //            ClassPoleWar.StartWar();
                        //        else
                        //            ClassPoleWar.EndWar();
                        //        break;
                        //    }
                        case "guildhit":
                            {
                                if (!GuildScoreWar.IsWar)
                                    GuildScoreWar.StartWar();
                                else
                                    GuildScoreWar.EndWar();
                                break;
                            }
                        case "guildPole":
                            {
                                if (!GuildPoleWar.IsWar)
                                    GuildPoleWar.StartWar();
                                else
                                    GuildPoleWar.EndWar();
                                break;
                            }
                        case "caspr":
                            {
                                client.Entity.Update(byte.Parse(Data[1]) /*Network.GamePackets.Update.DoubleExpTimer*/,
                                    ulong.Parse(Data[2]), ulong.Parse(Data[3]), false);
                                break;
                            }
                        //case "teamai":
                        //    {
                        //        Game.Features.Tournaments.TeamElitePk.TeamTournament.Open();
                        //        foreach (var clien in Kernel.GamePool.Values)
                        //        {
                        //            if (clien.Team == null)
                        //                clien.Team = new Game.ConquerStructures.Team(clien);
                        //            Game.Features.Tournaments.TeamElitePk.TeamTournament.Join(clien, 3);
                        //        }
                        //        int count = int.Parse(Data[1]);
                        //        for (int i = 0; i < count; i++)
                        //        {
                        //            var ai = new MaTrix.AI(client.Entity.MapID, (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5),
                        //                              (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5), MaTrix.AI.BotLevel.MaTrix, (p) => { return IsVaildForTeamPk(p) == true; });
                        //            //     ai.Disguise(client);
                        //            if (ai.Bot.Team == null)
                        //                ai.Bot.Team = new Game.ConquerStructures.Team(ai.Bot);
                        //            Game.Features.Tournaments.TeamElitePk.TeamTournament.Join(ai.Bot, 3);

                        //        }
                        //        break;

                        //    }
                        case "stamina":
                            {
                                client.Entity.Stamina = byte.Parse(Data[1]);
                                break;
                            }
                        //case "narutostyle":
                        //    {
                        //
                        //        int count = int.Parse(Data[1]);
                        //        for (int i = 0; i < count; i++)
                        //        {
                        //            var ai = new MaTrix.AI(client.Entity.MapID, (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5),
                        //                              (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5), MaTrix.AI.BotLevel.MaTrix, p => p.Entity.UID != client.Entity.UID || !p.Fake);
                        //            ai.Disguise(client);
                        //        }
                        //        break;

                        //    }
                        case "kingtime":
                            {
                                switch (Data[1])
                                {
                                    case "on":
                                        {
                                            Program.KingsTime = DateTime.Now;
                                            break;
                                        }
                                    case "off":
                                        {
                                            Program.KingsTime = DateTime.Now.AddHours(-1);
                                            break;
                                        }
                                }

                                break;
                            }
                        //case "ai":
                        //    {
                        //        Map dynamicMap = Kernel.Maps[700].MakeDynamicMap();
                        //        client.Entity.Teleport(700, dynamicMap.ID, 50, 50);

                        //        client.AI = new MaTrix.AI(client, MaTrix.AI.BotLevel.MaTrix);
                        //        new MaTrix.AI(client.Entity.MapID, client.Entity.X, client.Entity.Y, MaTrix.AI.BotLevel.MaTrix);

                        //        break;
                        //    }
                        case "studypoints":
                            {
                                client.Entity.SubClasses.StudyPoints = ushort.Parse(Data[1]);
                                client.Entity.SubClasses.Send(client);
                                break;
                            }
                        case "monsterpoints":
                            {
                                client.Entity.MonstersPoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "darkpoints":
                            {
                                client.Entity.DarkPoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "onlinepoints":
                            {
                                client.Entity.OnlinePoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "killerpoints":
                            {
                                client.Entity.killerpoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "retreat":
                            {
                                byte[] bytes = new byte[15];
                                Writer.Ushort(7, 0, bytes);
                                Writer.Ushort(2536, 2, bytes);
                                Writer.Ushort(ushort.Parse(Data[1]), 4, bytes);
                                Writer.Byte(byte.Parse(Data[2]), 6, bytes);
                                client.Send(bytes);
                                break;
                            }
                        case "retreat2":
                            {
                                uint count = uint.Parse(Data[1]);
                                byte[] bytes = new byte[8 + 8 + 21 * count];
                                Writer.Ushort((ushort)(bytes.Length - 8), 0, bytes);
                                Writer.Ushort(2537, 2, bytes);
                                Writer.Uint(count, 4, bytes); //count
                                int Offset = 8;
                                for (int i = 1; i < count + 1; i++)
                                {
                                    bytes[Offset] = (byte)i;
                                    Offset++;

                                    //     Writer.Uint(1406241635, Offset, bytes);
                                    var now = DateTime.Now.AddHours(-1);
                                    uint secs = (uint)(now.Year % 100 * 100000000 +
                                                       (now.Month) * 1000000 +
                                                       now.Day * 10000 +
                                                       now.Hour * 100 +
                                                       now.Minute);
                                    //   uint secs = (uint)(DateTime.UtcNow.AddDays(5) - new DateTime(1970, 1, 1)).TotalSeconds;
                                    //   uint secs = Common.TimeGet((TimeType)uint.Parse(Data[2]));
                                    Writer.Uint(secs, Offset, bytes);
                                    Offset += 4;
                                    var powers = client.ChiPowers[i - 1];
                                    var attributes = powers.Attributes;
                                    foreach (var attribute in attributes)
                                    {
                                        Writer.WriteInt32(attribute, Offset, bytes);
                                        Offset += 4;
                                    }
                                }

                                client.Send(bytes);
                                break;
                            }
                        case "credit":
                            {
                                client.Entity.Update(0x80, 8888, false);
                                byte[] bytes = new byte[55];
                                Writer.Ushort(47, 0, bytes);
                                Writer.Ushort(10010, 2, bytes);
                                Writer.Uint(client.Entity.UID, 8, bytes);
                                Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, bytes);
                                Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 20, bytes);
                                Writer.WriteUInt32(0xcd, 24, bytes);
                                Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 36, bytes);
                                Writer.WriteUInt32(01, 41, bytes);
                                client.Send(bytes);
                                break;
                            }
                        case "dropped":
                            {
                                Data data = new Data(true);
                                data.UID = client.Entity.UID;
                                data.ID = Network.GamePackets.Data.DragonBallDropped;
                                //  data.dwParam = uint.Parse(Data[2]);
                                client.SendScreen(data);
                                //     data.Send(client);
                                break;
                            }

                        case "testinbox":
                            {
                                byte[] inboxpacket = new byte[112];
                                Writer.WriteUInt16(104, 0, inboxpacket);
                                Writer.WriteUInt16(1046, 2, inboxpacket);
                                Writer.WriteUInt32(1, 4, inboxpacket);
                                Writer.WriteUInt32(1, 12, inboxpacket);
                                Writer.WriteUInt32(126113, 16, inboxpacket);
                                Writer.WriteString("TQSupport", 20, inboxpacket);
                                Writer.WriteString("Reservations for Mortal Strike", 52, inboxpacket);
                                Writer.WriteUInt16(32768, 92, inboxpacket);
                                Writer.WriteUInt16(7, 94, inboxpacket);
                                client.Send(inboxpacket);
                                break;
                            }
                        case "home2":
                            {
                                client["guildtelport"] = uint.Parse(Data[1]);
                                NpcRequest req = new NpcRequest(5);
                                req.Mesh = 1477;
                                req.NpcTyp = Enums.NpcType.Talker;
                                client.Send(req);
                                break;
                            }
                        case "home":
                            {
                                NpcRequest req = new NpcRequest(5);
                                req.Mesh = ushort.Parse(Data[1]);
                                req.NpcTyp = Enums.NpcType.Furniture;
                                client.Send(req);
                                break;
                            }
                        case "effect":
                            {
                                _String str = new _String(true);
                                str.UID = client.Entity.UID;
                                str.TextsCount = 1;
                                str.Type = _String.Effect;
                                str.Texts.Add(Data[1]);
                                client.SendScreen(str, true);
                                break;
                            }
                        case "mob":
                            {
                                Database.MonsterInformation mt;
                                Database.MonsterInformation.MonsterInformations.TryGetValue(1, out mt);
                                //  client.Map.SpawnMonsterNearToHero(mob, client);
                                if (mt == null) break;
                                mt.RespawnTime = 5;
                                MTA.Game.Entity entity = new MTA.Game.Entity(EntityFlag.Monster, false);
                                entity.MapObjType = MTA.Game.MapObjectType.Monster;
                                entity.MonsterInfo = mt.Copy();
                                entity.MonsterInfo.Owner = entity;
                                entity.Name = Data[2];
                                entity.Body = ushort.Parse(Data[1]);
                                entity.MinAttack = mt.MinAttack;
                                entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
                                entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
                                entity.Level = mt.Level;
                                entity.UID = client.Map.EntityUIDCounter.Next;
                                entity.MapID = client.Map.ID;
                                entity.SendUpdates = true;
                                entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(5));
                                entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(5));
                                client.Map.AddEntity(entity);
                                entity.SendSpawn(client);
                                break;
                            }
                        case "gotoplayer":
                        case "goto":
                            {
                                if (Data.Length < 2)
                                {
                                    client.Send(new Message("Usage: @gotoplayer <player_name>", System.Drawing.Color.Red,
                                        Message.Tip));
                                    break;
                                }

                                string playerName = Mess.Substring(Data[0].Length + 1);
                                GameState targetPlayer = null;

                                // Search through all online players
                                foreach (var player in Kernel.GamePool.Values)
                                {
                                    if (player.Entity != null &&
                                        player.Entity.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        targetPlayer = player;
                                        break;
                                    }
                                }

                                if (targetPlayer != null && targetPlayer.Entity != null)
                                {
                                    client.Entity.Teleport(targetPlayer.Entity.MapID, targetPlayer.Entity.X,
                                        targetPlayer.Entity.Y);
                                    client.Send(new Message("Teleported to player [" + targetPlayer.Entity.Name + "]",
                                        System.Drawing.Color.Green, Message.Tip));
                                }
                                else
                                {
                                    client.Send(new Message("Player [" + playerName + "] not found or offline!",
                                        System.Drawing.Color.Red, Message.Tip));
                                }

                                break;
                            }
                        case "summon":
                        case "bring":
                            {
                                if (Data.Length < 2)
                                {
                                    client.Send(new Message("Usage: @bring <player_name>", System.Drawing.Color.Red,
                                        Message.Tip));
                                    break;
                                }

                                string playerName = Mess.Substring(Data[0].Length + 1);
                                GameState targetPlayer = null;

                                // Search through all online players
                                foreach (var player in Kernel.GamePool.Values)
                                {
                                    if (player.Entity != null &&
                                        player.Entity.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        targetPlayer = player;
                                        break;
                                    }
                                }

                                if (targetPlayer != null && targetPlayer.Entity != null)
                                {
                                    targetPlayer.Entity.Teleport(client.Entity.MapID, client.Entity.X, client.Entity.Y);
                                    targetPlayer.Send(new Message("You have been summoned by [" + client.Entity.Name + "]",
                                        System.Drawing.Color.Green, Message.Tip));
                                    client.Send(new Message("Brought player [" + targetPlayer.Entity.Name + "] to you",
                                        System.Drawing.Color.Green, Message.Tip));
                                }
                                else
                                {
                                    client.Send(new Message("Player [" + playerName + "] not found or offline!",
                                        System.Drawing.Color.Red, Message.Tip));
                                }

                                break;
                            }
                        case "summonall":
                            {
                                foreach (var player in Kernel.GamePool.Values)
                                {
                                    if (player.Entity != null)
                                        player.Entity.Teleport(client.Entity.MapID, client.Entity.X, client.Entity.Y);
                                }

                                break;
                            }
                        case "reward":
                            {
                                byte[] ids = new byte[] { 9, 10, 15, 16, 17, 11, 14, 19, 24, 22 };
                                byte[] Buffer = new byte[8 + 8 + 12 * ids.Length];
                                Writer.WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                                Writer.WriteUInt16(1316, 2, Buffer);
                                Buffer[4] = 1;
                                Buffer[6] = (byte)ids.Length;
                                int offset = 8;
                                for (int i = 0; i < ids.Length; i++)
                                {
                                    Writer.WriteUInt32(ids[i], offset, Buffer); //12
                                    offset += 4;
                                    Writer.WriteUInt32(0, offset, Buffer); //12
                                    offset += 4;
                                    Writer.WriteUInt32(200000, offset, Buffer); //16
                                    offset += 4;
                                }

                                client.Send(Buffer);
                                break;
                            }

                        case "twinwar":
                            {
                                //Game.TwinWar.StartTwinWar();
                                //Game.TwinWar.Join(client); 
                                //    foreach (var c in Program.Values)
                                //       c.MessageBox("Twin War Start Would You Like To jion and Won 50K",
                                //               p => { Game.BigBOsQuests.TwinWar.Join(p); }, null);
                                break;
                            }
                        case "test2102":
                            {
                                int count = int.Parse(Data[1]);
                                byte[] Buffer = new byte[8 + 48 + count * 32];
                                Writer.WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                                Writer.WriteUInt16(2102, 2, Buffer);
                                Writer.Uint(1, 4, Buffer);
                                Writer.Uint((uint)count, 12, Buffer);
                                int offset = 16;
                                for (int i = 0; i < count; i++)
                                {
                                    Writer.Uint((uint)i, offset, Buffer); //level
                                    offset += 4;
                                    Writer.Uint((uint)i, offset, Buffer); //online
                                    offset += 4;
                                    Writer.Uint((uint)i, offset, Buffer); //bp
                                    offset += 4;
                                    //  Writer.Uint((uint)Enums.GuildMemberRank.DeputyLeader, offset, Buffer);//unkown1
                                    offset += 4;
                                    Writer.String("Matrix-" + i, offset, Buffer);
                                    offset += 16;
                                }

                                client.Send(Buffer);
                                break;
                            }
                        case "blue":
                            {
                                Attack attack = new Attack(true);
                                attack.Attacker = client.Screen.Objects.First().UID;
                                attack.Attacked = client.Entity.UID;
                                attack.X = client.Entity.X;
                                attack.Y = client.Entity.Y;
                                attack.Effect1 = Attack.AttackEffects1.None;
                                attack.Effect1 |= (Attack.AttackEffects1)byte.Parse(Data[1]);
                                attack.AttackType = Attack.Melee;
                                attack.Damage = 500;
                                client.Send(attack);
                                break;
                            }
                        case "xspell":
                            {
                                foreach (var skill in client.Spells.Values)
                                {
                                    Network.GamePackets.Data data = new Data(true);
                                    data.UID = client.Entity.UID;
                                    data.dwParam = client.Spells[skill.ID].ID;
                                    data.ID = 109;
                                    client.Send(data);
                                    var s = new Spell(true)
                                    {
                                        ID = client.Spells[skill.ID].ID,
                                        Level = client.Spells[skill.ID].Level,
                                        PreviousLevel = client.Spells[skill.ID].PreviousLevel,
                                        Experience = 0,
                                        Souls = Spell.Soul_Level.Level_Four,
                                        Available = true
                                    };
                                    skill.Souls = s.Souls;
                                    //  Writer.WriteByte(1, 24, s.ToArray());                                  
                                    //    Writer.WriteByte(byte.Parse(Data[1]), byte.Parse(Data[2]), s.ToArray());
                                    //    Writer.WriteByte(byte.Parse(Data[1]), 28, s.ToArray());

                                    //uint  _Levelhu = 4;
                                    // uint IntegerFlag = 0;
                                    // if (_Levelhu >= 1)
                                    //     IntegerFlag |= (uint)(1UL << 1);
                                    // if (_Levelhu >= 2)
                                    //     IntegerFlag |= (uint)(1UL << 4);
                                    // if (_Levelhu >= 3)
                                    //     IntegerFlag |= (uint)(1UL << 8);
                                    // if (_Levelhu >= 4)
                                    //     IntegerFlag |= (uint)(1UL << 16);

                                    client.Send(s.ToArray());
                                    // client.Spells[skill.ID].Send(client);
                                }

                                break;
                            }
                        case "inbox2":
                            {
                                int count = int.Parse(Data[1]);
                                for (int i = 0; i < count; i++)
                                {
                                    MaTrix.Inbox.AddPrize(client, "Matrix" + i.ToString(), "Inbox Test" + i.ToString(),
                                        "Message" + i.ToString(), 5000000, 5000000,
                                        600, /*p => { p.Entity.Level = 255; p.Entity.ConquerPoints = 0; }*/null);
                                    /*   MaTrix.Inbox.PrizeInfo prize = new MaTrix.Inbox.PrizeInfo()
                                       {
                                           ID = (uint)i,
                                           Time = 600,
                                           Sender = "Matrix" + i.ToString(),
                                           Subject = "Inbox Test" + i.ToString(),
                                           Message = "Message" + i.ToString(),
                                           MessageOrGift = true,
                                           itemprize = p => { p.Entity.Level = 255; p.Entity.ConquerPoints = 0; },
                                           goldprize = 5000000,
                                           cpsprize = 5000000
                                       };
                                       client.Prizes.Add(prize.ID, prize);*/
                                }

                                break;
                            }
                        case "inbox48":
                            {
                                string text = Mess.Remove(0, 7);
                                byte[] inbox = new byte[272];
                                Writer.Ushort((ushort)(inbox.Length - 8), 0, inbox);
                                Writer.Ushort(1048, 2, inbox);
                                Writer.Uint(0, 4, inbox); //id    
                                Writer.WriteString(text, 8, inbox); //string
                                client.Send(inbox);
                                break;
                            }
                        case "inbox46":
                            {
                                uint count = uint.Parse(Data[1]);
                                byte[] inbox = new byte[20 + 92 * count];
                                Writer.Ushort((ushort)(inbox.Length - 8), 0, inbox);
                                Writer.Ushort(1046, 2, inbox);
                                Writer.Uint(count, 4, inbox); //count    
                                Writer.Uint(count, 12, inbox); //count                               
                                int offset = 16;
                                for (uint i = 0; i < count; i++)
                                {
                                    Writer.Uint(i, offset, inbox); //uid 
                                    offset += 4;
                                    Writer.String("Sender Name", offset, inbox); //sender
                                    offset += 32;
                                    Writer.String("Subject", offset, inbox); //Subject
                                    offset += 40;
                                    Writer.Uint(600, offset, inbox); //Time
                                    offset += 12;
                                }

                                client.Send(inbox);
                                break;
                            }
                        case "testxx":
                            {
                                client.Send(Network.GamePackets.Data.Custom(client.testxx++, client.Entity.UID));
                                client.Send(new Message(client.testxx.ToString(), Message.Tip));
                                break;
                            }
                        case "testxx2":
                            {
                                client.testxx = uint.Parse(Data[1]);
                                break;
                            }
                        case "nvend":
                            {
                                switch (Data[1])
                                {
                                    case "add":
                                        {
                                            Game.ConquerStructures.Booth booth = new Game.ConquerStructures.Booth();
                                            SobNpcSpawn Base = new SobNpcSpawn();
                                            Base.UID = uint.Parse(Data[2]);
                                            if (Booth.Booths2.ContainsKey(Base.UID))
                                                Booth.Booths2.Remove(Base.UID);
                                            Booth.Booths2.Add(Base.UID, booth);
                                            Base.Mesh = 100;
                                            Base.Type = Game.Enums.NpcType.Booth;
                                            Base.ShowName = true;
                                            Base.Name = "matrix™[" + Base.UID.ToString() + "]";
                                            Base.MapID = client.Entity.MapID;
                                            Base.X = (ushort)(client.Entity.X + 1);
                                            Base.Y = client.Entity.Y;
                                            if (client.Map.Npcs.ContainsKey(Base.UID))
                                                client.Map.Npcs.Remove(Base.UID);
                                            client.Map.Npcs.Add(Base.UID, Base);
                                            client.SendScreenSpawn(Base, true);
                                            break;
                                        }
                                    case "remove":
                                        {
                                            uint UID = uint.Parse(Data[2]);
                                            if (client.Map.Npcs.ContainsKey(UID))
                                                client.Map.Npcs.Remove(UID);

                                            client.Screen.FullWipe();
                                            client.Screen.Reload();
                                            break;
                                        }
                                    case "clear":
                                        {
                                            Game.ConquerStructures.Booth booth = null;
                                            uint UID = uint.Parse(Data[2]);
                                            Booth.TryGetValue2(UID, out booth);
                                            if (booth == null) break;
                                            booth.ItemList.Clear();
                                            break;
                                        }
                                    case "additem":
                                        {
                                            Booth booth = null;
                                            uint UID = uint.Parse(Data[2]);
                                            Booth.TryGetValue2(UID, out booth);
                                            if (booth == null) break;
                                            //  booth.ItemList.Clear();
                                            Game.ConquerStructures.BoothItem item = new Game.ConquerStructures.BoothItem();
                                            item.Cost = uint.Parse(Data[3]);
                                            item.Item = new ConquerItem(true);
                                            item.Item.ID = uint.Parse(Data[4]);
                                            item.Item.UID = Program.NextItemID;
                                            //Program.NextItemID++;
                                            if (Data.Length > 5)
                                            {
                                                item.Item.Plus = byte.Parse(Data[5]);
                                                if (Data.Length > 6)
                                                {
                                                    item.Item.Enchant = byte.Parse(Data[6]);
                                                    if (Data.Length > 7)
                                                    {
                                                        item.Item.Bless = byte.Parse(Data[7]);
                                                        if (Data.Length > 9)
                                                        {
                                                            item.Item.SocketOne = (Enums.Gem)byte.Parse(Data[8]);
                                                            item.Item.SocketTwo = (Enums.Gem)byte.Parse(Data[9]);
                                                        }
                                                    }
                                                }
                                            }

                                            Database.ConquerItemBaseInformation CIBI = null;
                                            CIBI = Database.ConquerItemInformation.BaseInformations[item.Item.ID];
                                            if (CIBI == null)
                                                break;
                                            item.Item.Durability = CIBI.Durability;
                                            item.Item.MaximDurability = CIBI.Durability;
                                            item.Cost_Type = Game.ConquerStructures.BoothItem.CostType.ConquerPoints;
                                            booth.ItemList.Add(item.Item.UID, item);

                                            break;
                                        }
                                }

                                break;
                            }
                    }
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                MTA.Console.WriteLine(e);
                client.Send(new Message("Impossible to handle this command. Check your syntax.", System.Drawing.Color.BurlyWood, Message.TopLeft));
                return false;
            }
        }
    }
}