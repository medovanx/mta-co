using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using MTA.Interfaces;
using MTA.Client;

using MTA.Network.GamePackets;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Generic;

namespace MTA.Game
{
    public class Screen
    {
        private static TimerRule<GameState> MonsterBuffers, Guards, AliveMonsters, Items, MatrixMobs;
        public static void CreateTimerFactories()
        {
            MonsterBuffers = new TimerRule<GameState>(monsterBuffersCallback, 500);
            Guards = new TimerRule<GameState>(guardsCallback, 700);
            MatrixMobs = new TimerRule<GameState>(MatrixMobsCallback, 700);
            AliveMonsters = new TimerRule<GameState>(aliveMonstersCallback, 500);
            Items = new TimerRule<GameState>(itemsCallback, 1000);

        }

        private static void monsterBuffersCallback(GameState client, int time)
        {
            if (!client.Socket.Alive)
            {
                client.Screen.DisposeTimers();
                return;
            }
            if (client.Entity == null)
                return;
            if (client.Map == null)
                return;
            if (client.Map.FreezeMonsters)
                return;

            #region Stamina
            if (client.Entity.StaminaStamp.Next(500, time: time))
            {
                if (client.Vigor < client.MaxVigor)
                {
                    ushort amount = (ushort)(3 + (client.Entity.Action == Game.Enums.ConquerAction.Sit ? 2 : 0));
                    if (client.Vigor + amount > client.MaxVigor)
                    {
                        amount = client.MaxVigor;
                        client.Vigor = amount;
                    }
                    else
                    {
                        client.Vigor += amount;
                    }

                    Vigor vigor = new Vigor(true);
                    vigor.Amount = client.Vigor;
                    vigor.Send(client);
                }  
                if (!client.Entity.ContainsFlag(Update.Flags.Ride) && !client.Entity.ContainsFlag(Update.Flags.Fly) || client.Equipment.TryGetItem(18) != null)
                {
                    int limit = 0;
                    if (client.Entity.HeavenBlessing > 0)
                        limit = 50;
                    if (client.Spells != null)
                    {
                        if (client.Spells.ContainsKey(12560))
                        {
                            var spell = client.Spells[12560];
                            var skill = Database.SpellTable.SpellInformations[12560][spell.Level];
                            limit += (int)skill.Power;
                        }
                    }
                    if (client.Entity.Stamina != 100 + limit)
                    {
                        if (client.Entity.Action == Enums.ConquerAction.Sit)
                        {
                            if (client.Entity.Stamina <= 93 + limit)
                            {
                                client.Entity.Stamina += 7;
                            }
                            else
                            {
                                if (client.Entity.Stamina != 100 + limit)
                                    client.Entity.Stamina = (byte)(100 + limit);
                            }
                        }
                        else
                        {
                            if (client.Entity.Stamina <= 97 + limit)
                            {
                                client.Entity.Stamina += 3;
                            }
                            else
                            {
                                if (client.Entity.Stamina != 100 + limit)
                                    client.Entity.Stamina = (byte)(100 + limit);
                            }
                        }
                    }
                    client.Entity.StaminaStamp = new Time32(time);
                }
            }
            #endregion
            #region LotusEnergy
            if (!client.Entity.Dead)
            {
                if (client.Entity.LotusEnergyStamp.Next(1000, time: time))
                {
                    if (client.Entity.ContainsFlag3(Update.Flags3.AuroraLotus))
                    {
                        if (client.Entity.AuroraLotusEnergy < 220)
                            client.Entity.AuroraLotusEnergy = (uint)Math.Min(220, client.Entity.AuroraLotusEnergy + 2);
                        client.Entity.Lotus(client.Entity.AuroraLotusEnergy, Update.AuroraLotus);
                    }
                    if (client.Entity.ContainsFlag3(Update.Flags3.FlameLotus))
                    {
                        if (client.Entity.FlameLotusEnergy < 330)
                            client.Entity.FlameLotusEnergy = (uint)Math.Min(330, client.Entity.FlameLotusEnergy + 3);
                        client.Entity.Lotus(client.Entity.FlameLotusEnergy, Update.FlameLotus);

                    }
                    client.Entity.LotusEnergyStamp = new Time32(time);
                }
            }
            #endregion
            
            foreach (IMapObject obj in client.Screen.Objects)
            {
                if (obj != null)
                {
                    if (obj.MapObjType == MapObjectType.Monster)
                    {
                        Entity monster = obj as Entity;
                        if (monster == null) continue;

                        if (monster.ContainsFlag(Network.GamePackets.Update.Flags.Stigma))
                        {
                            if (monster.StigmaStamp.AddSeconds(monster.StigmaTime).Next(time: time) || monster.Dead)
                            {
                                monster.StigmaTime = 0;
                                monster.StigmaIncrease = 0;
                                monster.RemoveFlag(Update.Flags.Stigma);
                            }
                        }
                        if (monster.ContainsFlag(Update.Flags.Dodge))
                        {
                            if (monster.DodgeStamp.AddSeconds(monster.DodgeTime).Next(time: time) || monster.Dead)
                            {
                                monster.DodgeTime = 0;
                                monster.DodgeIncrease = 0;
                                monster.RemoveFlag(Network.GamePackets.Update.Flags.Dodge);
                            }
                        }
                        if (monster.ContainsFlag(Update.Flags.Invisibility))
                        {
                            if (monster.InvisibilityStamp.AddSeconds(monster.InvisibilityTime).Next(time: time) || monster.Dead)
                            {
                                monster.RemoveFlag(Update.Flags.Invisibility);
                            }
                        }
                        if (monster.ContainsFlag(Update.Flags.StarOfAccuracy))
                        {
                            if (monster.StarOfAccuracyTime != 0)
                            {
                                if (monster.StarOfAccuracyStamp.AddSeconds(monster.StarOfAccuracyTime).Next(time: time) || monster.Dead)
                                {
                                    monster.RemoveFlag(Update.Flags.StarOfAccuracy);
                                }
                            }
                            else
                            {
                                if (monster.AccuracyStamp.AddSeconds(monster.AccuracyTime).Next(time: time) || monster.Dead)
                                {
                                    monster.RemoveFlag(Update.Flags.StarOfAccuracy);
                                }
                            }
                        }
                        if (monster.ContainsFlag(Update.Flags.MagicShield))
                        {
                            if (monster.MagicShieldTime != 0)
                            {
                                if (monster.MagicShieldStamp.AddSeconds(monster.MagicShieldTime).Next(time: time) || monster.Dead)
                                {
                                    monster.MagicShieldIncrease = 0;
                                    monster.MagicShieldTime = 0;
                                    monster.RemoveFlag(Update.Flags.MagicShield);
                                }
                            }
                            else
                            {
                                if (monster.ShieldStamp.AddSeconds(monster.ShieldTime).Next(time: time) || monster.Dead)
                                {
                                    monster.ShieldIncrease = 0;
                                    monster.ShieldTime = 0;
                                    monster.RemoveFlag(Update.Flags.MagicShield);
                                }
                            }
                        }
                        if (monster.Dead || monster.Killed)
                        {
                            if (!monster.ContainsFlag(Update.Flags.Ghost) || monster.Killed)
                            {
                                monster.Killed = false;
                                monster.MonsterInfo.InSight = 0;
                                monster.AddFlag(Network.GamePackets.Update.Flags.Ghost);
                                monster.AddFlag(Network.GamePackets.Update.Flags.Dead);
                                monster.AddFlag(Network.GamePackets.Update.Flags.FadeAway);
                                Network.GamePackets.Attack attack = new Network.GamePackets.Attack(true);
                                attack.Attacker = monster.Killer.UID;
                                attack.Attacked = monster.UID;
                                attack.AttackType = Network.GamePackets.Attack.Kill;
                                attack.X = monster.X;
                                attack.Y = monster.Y;
                                client.Map.Floor[monster.X, monster.Y, MapObjectType.Monster, monster] = true;
                                attack.KOCount = ++monster.Killer.KOCount;
                                if (monster.Killer.EntityFlag == EntityFlag.Player)
                                {
                                    monster.MonsterInfo.ExcludeFromSend = monster.Killer.UID;
                                    monster.Killer.Owner.Send(attack);
                                }
                                monster.MonsterInfo.SendScreen(attack);
                                monster.MonsterInfo.ExcludeFromSend = 0;
                            }
                            if (monster.DeathStamp.AddSeconds(4).Next(time: time))
                            {
                                Data data = new Data(true);
                                data.UID = monster.UID;
                                data.ID = Network.GamePackets.Data.RemoveEntity;
                                monster.MonsterInfo.SendScreen(data);
                            }
                        }
                    }
                }
            }
        }
        private static void guardsCallback(GameState client, int time)
        {
            if (!client.Socket.Alive)
            {
                client.Screen.DisposeTimers();
                return;
            }
            if (client.Entity == null)
                return;
            if (client.Map == null)
                return;
            if (client.Map.FreezeMonsters)
                return;

            Time32 Now = new Time32(time);
            foreach (IMapObject obj in client.Screen.Objects)
            {
                if (obj != null)
                {
                    if (obj.MapObjType == MapObjectType.Monster)
                    {
                        Entity monster = obj as Entity;
                        if (monster.Companion) continue;
                        if (monster.Boss == 1 || monster.MonsterInfo.Boss) continue;
                        if (monster.Dead || monster.Killed) continue;

                        if (monster.MonsterInfo.Guard)
                        {
                            if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.MinimumSpeed))
                            {
                                if (monster.MonsterInfo.InSight == 0)
                                {
                                    ushort xx = (ushort)Kernel.Random.Next(monster.X - 2, monster.X + 2);
                                    ushort yy = (ushort)Kernel.Random.Next(monster.Y - 2, monster.Y + 2);
                                    if (monster.X != monster.MonsterInfo.BoundX || monster.Y != monster.MonsterInfo.BoundY)
                                    {
                                        monster.X = monster.MonsterInfo.BoundX;
                                        monster.Y = monster.MonsterInfo.BoundY;
                                        TwoMovements jump = new TwoMovements();
                                        jump.X = monster.MonsterInfo.BoundX;
                                        jump.Y = monster.MonsterInfo.BoundY;
                                        jump.EntityCount = 1;
                                        jump.FirstEntity = monster.UID;
                                        jump.MovementType = TwoMovements.Jump;
                                        client.SendScreen(jump, true);
                                    }

                                    if (client.Entity.ContainsFlag(Update.Flags.FlashingName))
                                        monster.MonsterInfo.InSight = client.Entity.UID;
                                }
                                else
                                {
                                    if (client.Entity.ContainsFlag(Update.Flags.FlashingName))
                                    {
                                        if (monster.MonsterInfo.InSight == client.Entity.UID)
                                        {
                                            if (!client.Entity.Dead)
                                            {
                                                if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                                {
                                                    short distance = Kernel.GetDistance(monster.X, monster.Y, client.Entity.X, client.Entity.Y);

                                                    if (distance <= monster.MonsterInfo.AttackRange)
                                                    {
                                                        monster.MonsterInfo.LastMove = Time32.Now;
                                                        new Game.Attacking.Handle(null, monster, client.Entity);
                                                    }
                                                    else
                                                    {
                                                        if (distance <= monster.MonsterInfo.ViewRange)
                                                        {
                                                            TwoMovements jump = new TwoMovements();
                                                            jump.X = client.Entity.X;
                                                            jump.Y = client.Entity.Y;
                                                            monster.X = client.Entity.X;
                                                            monster.Y = client.Entity.Y;
                                                            jump.EntityCount = 1;
                                                            jump.FirstEntity = monster.UID;
                                                            jump.MovementType = TwoMovements.Jump;
                                                            client.SendScreen(jump, true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (monster.MonsterInfo.InSight == client.Entity.UID)
                                        {
                                            monster.MonsterInfo.InSight = 0;
                                        }
                                    }
                                }

                                foreach (IMapObject obj2 in client.Screen.Objects)
                                {
                                    if (obj2 == null) continue;
                                    //if (obj2.MapObjType == MapObjectType.Item)
                                    //{
                                    //    FloorItem flooritem = obj2 as FloorItem;
                                    //    if (flooritem == null) continue;
                                    //    if (flooritem.ValueType == FloorItem.FloorValueType.Money)
                                    //    {
                                    //        short distance = Kernel.GetDistance(monster.X, monster.Y, flooritem.X, flooritem.Y);
                                    //        if (distance <= 15)
                                    //        {
                                    //            FloorItem item = new FloorItem(true); 
                                    //            item.Type = 3;
                                    //            item.UID = client.Entity.UID;
                                    //            item.X = client.Entity.X;
                                    //            item.Y = client.Entity.Y;
                                    //            //  client.Send(Constants.PickupGold(floorItem.Value));
                                    //            monster.MonsterInfo.SendScreen(item);
                                    //            flooritem.Type = 2;
                                    //            client.RemoveScreenSpawn(flooritem, true);

                                    //        }
                                    //    }
                                    //}
                                    if (obj2.MapObjType == MapObjectType.Monster)
                                    {
                                        Entity monster2 = obj2 as Entity;

                                        if (monster2 == null) continue;
                                        if (monster2.Dead) continue;

                                        if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                        {
                                            if (!monster2.MonsterInfo.Guard && (!monster2.Companion || monster2.Owner.Entity.ContainsFlag(Update.Flags.FlashingName)))
                                            {
                                                short distance = Kernel.GetDistance(monster.X, monster.Y, monster2.X, monster2.Y);

                                                if (distance <= monster.MonsterInfo.AttackRange)
                                                {
                                                    monster.MonsterInfo.LastMove = Time32.Now;
                                                    new Game.Attacking.Handle(null, monster, monster2);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void aliveMonstersCallback(GameState client, int time)
        {
            if (!client.Socket.Alive)
            {
                client.Screen.DisposeTimers();
                return;
            }
            if (client.Entity == null)
                return;
            if (client.Map == null)
                return;
            if (client.Map.FreezeMonsters)
                return;

            Time32 Now = new Time32(time);
            foreach (IMapObject obj in client.Screen.Objects)
            {
                if (obj != null)
                {
                    if (obj.MapObjType == MapObjectType.Monster)
                    {
                        Entity monster = obj as Entity;
                        if (monster == null) continue;
                        if (monster.Name == "CasprGuard" && !monster.Dead)
                        {
                            if (client.Entity.Dead && !client.Entity.ContainsFlag2(Update.Flags2.SoulShackle))
                            {
                                if (MyMath.Success(50.0))
                                {
                                    client.Entity.BringToLife();
                                    SpellUse use = new SpellUse(true);
                                    use.Attacker = monster.UID;
                                    use.X = client.Entity.X;
                                    use.Y = client.Entity.Y;
                                    use.SpellID = 1100;
                                    use.AddTarget(client.Entity, 0, null);
                                    Kernel.SendWorldMessage(use, Program.Values, monster.MapID);
                                    if (MyMath.Success(50.0))
                                    {
                                        use = new SpellUse(true);
                                        use.Attacker = monster.UID;
                                        use.X = client.Entity.X;
                                        use.Y = client.Entity.Y;
                                        use.SpellID = 30000;
                                        use.AddTarget(client.Entity, 0, null);
                                        Kernel.SendWorldMessage(use, Program.Values, monster.MapID);

                                        client.Entity.AzureShieldDefence = (ushort)(3000 * 4);
                                        client.Entity.AzureShieldLevel = 4;
                                        client.Entity.MagicShieldStamp = Time32.Now;

                                        client.Entity.AzureShieldStamp = DateTime.Now;
                                        client.Entity.AddFlag2(Update.Flags2.AzureShield);
                                        client.Entity.MagicShieldTime = 60;
                                        client.Entity.AzureShieldPacket();

                                        if (client.Entity.EntityFlag == EntityFlag.Player)
                                            client.Send(Constants.Shield(12000, client.Entity.MagicShieldTime));
                                    }
                                    else
                                    {
                                        use = new SpellUse(true);
                                        use.Attacker = monster.UID;
                                        use.X = client.Entity.X;
                                        use.Y = client.Entity.Y;
                                        use.SpellID = 6002;
                                        use.AddTarget(client.Entity, 0, null);
                                        Kernel.SendWorldMessage(use, Program.Values, monster.MapID);

                                        client.Entity.AddFlag2(Update.Flags2.EffectBall);
                                        client.Entity.NoDrugsStamp = Time32.Now;
                                        client.Entity.NoDrugsTime = (short)60;
                                        if (client.Entity.EntityFlag == EntityFlag.Player)
                                            client.Send(Constants.NoDrugs(60));
                                    }
                                }
                                else
                                {
                                    SpellUse use = new SpellUse(true);
                                    use.Attacker = monster.UID;
                                    use.X = client.Entity.X;
                                    use.Y = client.Entity.Y;
                                    use.SpellID = 10405;
                                    use.AddTarget(client.Entity, 0, null);
                                    Kernel.SendWorldMessage(use, Program.Values, monster.MapID);

                                    if (!client.Entity.ContainsFlag2(Update.Flags2.SoulShackle))
                                    {
                                        client.Entity.AddFlag2(Update.Flags2.SoulShackle);//Give them shackeld effect                                                         
                                        client.Entity.ShackleStamp = Time32.Now;//Set stamp so source can remove the flag after X seconds
                                        client.Entity.ShackleTime = 35;//(short)(30 + 15 * spell.Level);//double checking here. Could be db has this wrong.
                                        Network.GamePackets.Update upgrade = new Network.GamePackets.Update(true);
                                        upgrade.UID = client.Entity.UID;
                                        upgrade.Append(Network.GamePackets.Update.SoulShackle
                                            , 111
                                            , 35, 0, 4);
                                        client.Entity.Owner.Send(upgrade.ToArray());

                                        if (client.Entity.EntityFlag == EntityFlag.Player)
                                            client.Send(Constants.Shackled(client.Entity.ShackleTime));
                                    }
                                }
                            }

                        }
                        if (monster.MonsterInfo.Guard || monster.Companion || monster.Dead) continue;
                        
                        if (monster.MonsterInfo.Reviver)
                        {
                            if (client.Entity.Dead && Now > client.Entity.DeathStamp.AddSeconds(5))
                            {
                                client.Entity.BringToLife();
                                SpellUse use = new SpellUse(true);
                                use.Attacker = monster.UID;
                                use.X = client.Entity.X;
                                use.Y = client.Entity.Y;
                                use.SpellID = 1100;
                                use.AddTarget(client.Entity, 0, null);
                                Kernel.SendWorldMessage(use, Program.Values, monster.MapID);
                            }
                            return;
                        }
                        short distance = Kernel.GetDistance(monster.X, monster.Y, client.Entity.X, client.Entity.Y);
                        if (distance > Constants.pScreenDistance)
                        {
                            client.Screen.Remove(obj);
                            continue;
                        }
                        if (monster.MonsterInfo.InSight != 0 && monster.MonsterInfo.InSight != client.Entity.UID)
                        {
                            if (monster.MonsterInfo.InSight > 1000000)
                            {
                                GameState cl;
                                if (Kernel.GamePool.TryGetValue(monster.MonsterInfo.InSight, out cl))
                                {
                                    short dst = Kernel.GetDistance(monster.X, monster.Y, cl.Entity.X, cl.Entity.Y);
                                    if (dst > Constants.pScreenDistance)
                                        monster.MonsterInfo.InSight = 0;
                                }
                                else
                                    monster.MonsterInfo.InSight = 0;
                            }
                            //else
                            //{
                            //    Entity companion = client.Map.Companions[monster.MonsterInfo.InSight];
                            //    if (companion != null)
                            //    {
                            //        short dst = Kernel.GetDistance(monster.X, monster.Y, companion.X, companion.Y);
                            //        if (dst > Constants.pScreenDistance)
                            //            monster.MonsterInfo.InSight = 0;
                            //    }
                            //    else
                            //        monster.MonsterInfo.InSight = 0;
                            //}
                        }
                        if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.MinimumSpeed))
                        {
                            if (distance <= Constants.pScreenDistance)
                            {
                                #region Companions
                                foreach (var pet in client.Pet.Pets.Values)
                                {
                                    if (pet != null)
                                    {
                                        #region Pets
                                        if (pet.Entity.Companion && !pet.Entity.Dead)
                                        {
                                            short distance2 = Kernel.GetDistance(monster.X, monster.Y, pet.Entity.X, pet.Entity.Y);
                                            if (distance > distance2 || client.Entity.ContainsFlag(Update.Flags.Invisibility) || client.Entity.ContainsFlag(Update.Flags.Fly))
                                            {
                                                if (monster.MonsterInfo.InSight == 0)
                                                {
                                                    monster.MonsterInfo.InSight = pet.Entity.UID;
                                                }
                                                else
                                                {
                                                    if (monster.MonsterInfo.InSight == pet.Entity.UID)
                                                    {
                                                        if (distance2 > Constants.pScreenDistance)
                                                        {
                                                            monster.MonsterInfo.InSight = 0;
                                                        }
                                                        else
                                                        {
                                                            if (distance2 <= monster.MonsterInfo.AttackRange)
                                                            {
                                                                if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                                                {
                                                                    monster.MonsterInfo.LastMove = Time32.Now;
                                                                    new Game.Attacking.Handle(null, monster, pet.Entity);
                                                                    if (Time32.Now >= monster.MonsterInfo.Lastpop.AddSeconds(30))
                                                                    {
                                                                        monster.MonsterInfo.Lastpop = Time32.Now;

                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (distance2 > monster.MonsterInfo.ViewRange / 2)
                                                                {
                                                                    if (distance2 < Constants.pScreenDistance)
                                                                    {
                                                                        if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.RunSpeed))
                                                                        {
                                                                            monster.MonsterInfo.LastMove = Time32.Now;

                                                                            Enums.ConquerAngle facing = Kernel.GetAngle(monster.X, monster.Y, pet.Entity.X, pet.Entity.Y);
                                                                            if (!monster.Move(facing))
                                                                            {
                                                                                facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                                                                                if (monster.Move(facing))
                                                                                {
                                                                                    monster.Facing = facing;
                                                                                    GroundMovement move = new GroundMovement(true);
                                                                                    move.Direction = facing;
                                                                                    move.UID = monster.UID;
                                                                                    move.GroundMovementType = GroundMovement.Run;
                                                                                    monster.MonsterInfo.SendScreen(move);
                                                                                    continue;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                monster.Facing = facing;
                                                                                GroundMovement move = new GroundMovement(true);
                                                                                move.Direction = facing;
                                                                                move.UID = monster.UID;
                                                                                move.GroundMovementType = GroundMovement.Run;
                                                                                monster.MonsterInfo.SendScreen(move);
                                                                                continue;
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        monster.MonsterInfo.InSight = 0;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.MoveSpeed))
                                                                    {
                                                                        monster.MonsterInfo.LastMove = Time32.Now;
                                                                        Enums.ConquerAngle facing = Kernel.GetAngle(monster.X, monster.Y, pet.Entity.X, pet.Entity.Y);
                                                                        if (!monster.Move(facing))
                                                                        {
                                                                            facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                                                                            if (monster.Move(facing))
                                                                            {
                                                                                monster.Facing = facing;
                                                                                GroundMovement move = new GroundMovement(true);
                                                                                move.Direction = facing;
                                                                                move.UID = monster.UID;
                                                                                monster.MonsterInfo.SendScreen(move);
                                                                                continue;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            monster.Facing = facing;
                                                                            GroundMovement move = new GroundMovement(true);
                                                                            move.Direction = facing;
                                                                            move.UID = monster.UID;
                                                                            monster.MonsterInfo.SendScreen(move);
                                                                            continue;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }

                                }
                                #endregion
                                #region Player
                                if (monster.MonsterInfo.InSight == 0)
                                {
                                    if (distance <= monster.MonsterInfo.ViewRange)
                                    {
                                        if (!client.Entity.ContainsFlag(Update.Flags.Invisibility))
                                        {
                                            if (monster.MonsterInfo.SpellID != 0 || !client.Entity.ContainsFlag(Update.Flags.Fly))
                                            {
                                                monster.MonsterInfo.InSight = client.Entity.UID;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (monster.MonsterInfo.InSight == client.Entity.UID)
                                    {
                                        if (monster.MonsterInfo.SpellID == 0 && client.Entity.ContainsFlag(Update.Flags.Fly))
                                        {
                                            monster.MonsterInfo.InSight = 0;
                                            return;
                                        }

                                        if (client.Entity.Dead)
                                        {
                                            monster.MonsterInfo.InSight = 0;
                                            return;
                                        }
                                        if (distance > Constants.pScreenDistance)
                                        {
                                            monster.MonsterInfo.InSight = 0;
                                        }
                                        else
                                        {
                                            if (distance <= monster.MonsterInfo.AttackRange)
                                            {

                                                if (client.Entity.ContainsFlag3((ulong)1UL << 53))
                                                {
                                                    var attack = new Attack(true);
                                                    attack.Attacker = client.Entity.UID;
                                                    attack.AttackType = Attack.Melee;
                                                    var spell = Database.SpellTable.GetSpell(12700, client);
                                                    foreach (var obj1 in client.Screen.Objects)
                                                    {
                                                        if (Kernel.GetDistance(obj1.X, obj1.Y, obj.X, obj.Y) <= 8)
                                                        {
                                                            if (obj1.MapObjType == MapObjectType.Monster || obj1.MapObjType == MapObjectType.Player)
                                                            {
                                                                var attacked = obj1 as Entity;
                                                                if (Attacking.Handle.CanAttack(client.Entity, attacked, spell, false))
                                                                {
                                                                    uint damage = Game.Attacking.Calculate.Melee(client.Entity, attacked, spell, ref attack);
                                                                    damage = (damage * 100) / 100;
                                                                    attack.Damage = damage;
                                                                    attack.Attacked = attacked.UID;
                                                                    attack.X = attacked.X;
                                                                    attack.Y = attacked.Y;

                                                                    Attacking.Handle.ReceiveAttack(client.Entity, attacked, attack, ref damage, spell);
                                                                }
                                                            }
                                                            else if (obj1.MapObjType == MapObjectType.SobNpc)
                                                            {
                                                                var attacked = obj1 as SobNpcSpawn;
                                                                if (Attacking.Handle.CanAttack(client.Entity, attacked, spell))
                                                                {
                                                                    uint damage = Game.Attacking.Calculate.Melee(client.Entity, attacked, ref attack);
                                                                    damage = (damage * 100) / 100;

                                                                    attack.Damage = damage;
                                                                    attack.Attacked = attacked.UID;
                                                                    attack.X = attacked.X;
                                                                    attack.Y = attacked.Y;

                                                                    Attacking.Handle.ReceiveAttack(client.Entity, attacked, attack, damage, spell);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }  
                                                if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                                {
                                                    monster.MonsterInfo.LastMove = Time32.Now;
                                                    new Game.Attacking.Handle(null, monster, client.Entity);
                                                    if (Time32.Now >= monster.MonsterInfo.Lastpop.AddSeconds(30))
                                                    {
                                                        monster.MonsterInfo.Lastpop = Time32.Now;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (distance > monster.MonsterInfo.ViewRange / 2)
                                                {
                                                    if (distance < Constants.pScreenDistance)
                                                    {
                                                        if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.RunSpeed))
                                                        {
                                                            monster.MonsterInfo.LastMove = Time32.Now;

                                                            Enums.ConquerAngle facing = Kernel.GetAngle(monster.X, monster.Y, client.Entity.X, client.Entity.Y);
                                                            if (!monster.Move(facing))
                                                            {
                                                                facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                                                                if (monster.Move(facing))
                                                                {
                                                                    monster.Facing = facing;
                                                                    GroundMovement move = new GroundMovement(true);
                                                                    move.Direction = facing;
                                                                    move.UID = monster.UID;
                                                                    move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                                                                    monster.MonsterInfo.SendScreen(move);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                monster.Facing = facing;
                                                                GroundMovement move = new GroundMovement(true);
                                                                move.Direction = facing;
                                                                move.UID = monster.UID;
                                                                move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                                                                monster.MonsterInfo.SendScreen(move);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        monster.MonsterInfo.InSight = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.MoveSpeed))
                                                    {
                                                        monster.MonsterInfo.LastMove = Time32.Now;
                                                        Enums.ConquerAngle facing = Kernel.GetAngle(monster.X, monster.Y, client.Entity.X, client.Entity.Y);
                                                        if (!monster.Move(facing))
                                                        {
                                                            facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                                                            if (monster.Move(facing))
                                                            {
                                                                monster.Facing = facing;
                                                                GroundMovement move = new GroundMovement(true);
                                                                move.Direction = facing;
                                                                move.UID = monster.UID;
                                                                monster.MonsterInfo.SendScreen(move);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            monster.Facing = facing;
                                                            GroundMovement move = new GroundMovement(true);
                                                            move.Direction = facing;
                                                            move.UID = monster.UID;
                                                            monster.MonsterInfo.SendScreen(move);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    else if (obj.MapObjType == MapObjectType.Item)
                    {
                        FloorItem item = obj as FloorItem;
                        if (item == null) continue;

                        if (item.Type == FloorItem.Effect)
                        {
                            if (item.ItemID == FloorItem.DaggerStorm || item.ItemID == FloorItem.FuryofEgg || item.ItemID == FloorItem.ShacklingIce)
                            {
                                if (item.Owner == client)
                                {
                                    if (Time32.Now > item.UseTime.AddSeconds(1))
                                    {
                                        item.UseTime = Time32.Now;
                                        var spell = Database.SpellTable.GetSpell(11600, client);

                                        var attack = new Attack(true);
                                        attack.Attacker = item.Owner.Entity.UID;
                                        attack.AttackType = Attack.Melee;

                                        foreach (var obj1 in client.Screen.Objects)
                                        {
                                            if (Kernel.GetDistance(obj1.X, obj1.Y, obj.X, obj.Y) <= 3)
                                            {
                                                if (obj1.MapObjType == MapObjectType.Monster || obj1.MapObjType == MapObjectType.Player)
                                                {
                                                    var attacked = obj1 as Entity;
                                                    if (Attacking.Handle.CanAttack(client.Entity, attacked, spell, false))
                                                    {
                                                        uint damage = Game.Attacking.Calculate.Melee(client.Entity, attacked, spell, ref attack);

                                                        attack.Damage = damage;
                                                        attack.Attacked = attacked.UID;
                                                        attack.X = attacked.X;
                                                        attack.Y = attacked.Y;

                                                        Attacking.Handle.ReceiveAttack(client.Entity, attacked, attack, ref damage, spell);
                                                    }
                                                }
                                                else if (obj1.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    var attacked = obj1 as SobNpcSpawn;
                                                    if (Attacking.Handle.CanAttack(client.Entity, attacked, spell))
                                                    {
                                                        uint damage = Game.Attacking.Calculate.Melee(client.Entity, attacked, ref attack);
                                                        attack.Damage = damage;
                                                        attack.Attacked = attacked.UID;
                                                        attack.X = attacked.X;
                                                        attack.Y = attacked.Y;

                                                        Attacking.Handle.ReceiveAttack(client.Entity, attacked, attack, damage, spell);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
      
        private static void itemsCallback(GameState client, int time)
        {
            if (!client.Socket.Alive)
            {
                client.Screen.DisposeTimers();
                return;
            }
            if (client.Entity == null)
                return;
            if (client.Map == null)
                return;
            if (client.Map.FreezeMonsters)
                return;

            Time32 Now = new Time32(time);
            foreach (IMapObject obj in client.Screen.Objects)
            {
                if (obj != null)
                {
                    if (obj.MapObjType == MapObjectType.Item)
                    {
                        FloorItem item = obj as FloorItem;
                        if (item == null) continue;

                        if (item.Type == FloorItem.Effect)
                        {
                            if (item.ItemID == 1397)
                            {
                                if (item.OnFloor.AddSeconds(3).Next(time: time))
                                {
                                    if (!item.Owner.Spells.ContainsKey(12550))
                                        return;
                                    var spell = Database.SpellTable.GetSpell(item.Owner.Spells[12550].ID, item.Owner.Spells[12550].Level);

                                    var attack = new Attack(true);
                                    attack.Attacker = item.Owner.Entity.UID;
                                    attack.X = item.X;
                                    attack.Y = item.Y;
                                    attack.Damage = spell.ID;
                                    attack.AttackType = Attack.Magic;
                                    SpellUse suse = new SpellUse(true);
                                    suse.Attacker = item.Owner.Entity.UID;
                                    suse.SpellID = spell.ID;
                                    suse.SpellLevel = spell.Level;
                                    suse.X = item.X;
                                    suse.Y = item.Y;
                                    suse.SpellEffect = 1;

                                    foreach (var _Ob in item.Owner.Screen.Objects)
                                    {
                                        if (_Ob == null)
                                            continue;
                                        if (_Ob.MapObjType == MapObjectType.Monster || _Ob.MapObjType == MapObjectType.Player)
                                        {
                                            var attacked = _Ob as Entity;
                                            if (Kernel.GetDistance(item.X, item.Y, attacked.X, attacked.Y) <= spell.Range)
                                            {
                                                if (Game.Attacking.Handle.CanAttack(item.Owner.Entity, attacked, spell, attack.AttackType == Attack.Ranged))
                                                {
                                                    uint damage = Game.Attacking.Calculate.Melee(item.Owner.Entity, attacked, ref attack);
                                                    damage = (uint)(damage * 0.7);
                                                    suse.Effect1 = attack.Effect1;
                                                    Game.Attacking.Handle.ReceiveAttack(item.Owner.Entity, attacked, attack, ref damage, spell);                                                  
                                                    suse.AddTarget(attacked, damage, attack);

                                                }
                                            }
                                        }
                                    }
                                    item.Owner.SendScreen(suse, true);
                                    item.Type = FloorItem.Effect;
                                    client.Map.RemoveFloorItem(item);

                                    client.RemoveScreenSpawn(item, true);
                                }
                            }
                            if (item.ItemID == FloorItem.DaggerStorm || item.ItemID == FloorItem.FuryofEgg || item.ItemID == FloorItem.ShacklingIce)
                            {
                                if (item.ItemID == FloorItem.Twilight)
                                {
                                    if (item.OnFloor.AddMilliseconds(500).Next(time: time))
                                    {
                                        item.Type = Network.GamePackets.FloorItem.RemoveEffect;
                                        //client.SendScreen(item, true);
                                        client.Map.RemoveFloorItem(item);
                                        client.RemoveScreenSpawn(item, true);
                                    }
                                }
                            }
                            if (item.ItemID == FloorItem.AuroraLotus)
                            {
                                if (item.OnFloor.AddSeconds(7).Next(time: time))
                                {
                                    item.Type = Network.GamePackets.FloorItem.RemoveEffect;                                   
                                    client.Map.RemoveFloorItem(item);
                                    client.RemoveScreenSpawn(item, true);
                                    Attacking.Handle.AuroraLotus(item);
                                }
                            }
                            if (item.ItemID == FloorItem.FlameLotus)
                            {
                                if (item.OnFloor.AddSeconds(7).Next(time: time))
                                {
                                    item.Type = Network.GamePackets.FloorItem.RemoveEffect;                                  
                                    client.Map.RemoveFloorItem(item);
                                    client.RemoveScreenSpawn(item, true);
                                    Attacking.Handle.FlameLotus(item);
                                }
                            }
                            if (item.ItemID == FloorItem.Twilight)
                            {
                                if (item.OnFloor.AddMilliseconds(500).Next(time: time))
                                {
                                    item.Type = Network.GamePackets.FloorItem.RemoveEffect;
                                    //client.SendScreen(item, true);
                                    client.Map.RemoveFloorItem(item);
                                    client.RemoveScreenSpawn(item, true);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void MatrixMobsCallback(GameState client, int time)
        {
            if (!client.Socket.Alive)
            {
                client.Screen.DisposeTimers();
                return;
            }
            if (client.Entity == null)
                return;
            if (client.Map == null)
                return;
            if (client.Map.FreezeMonsters)
                return;

            Time32 Now = new Time32(time);
            foreach (IMapObject obj in client.Screen.Objects)
            {
                if (obj != null)
                {
                    if (obj.MapObjType == MapObjectType.Monster)
                    {
                        Entity monster = obj as Entity;
                        if (monster.Companion) continue;
                        if (monster.Dead || monster.Killed) continue;

                        if (monster.MonsterInfo.Type == 2 || monster.Mesh == 482)
                        {
                            if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.MinimumSpeed))
                            {
                                if (monster.MonsterInfo.InSight == 0)
                                {
                                    ushort xx = (ushort)Kernel.Random.Next(monster.X - 10, monster.X + 10);
                                    ushort yy = (ushort)Kernel.Random.Next(monster.Y - 10, monster.Y + 10);
                                    if (monster.X != xx || monster.Y != yy)
                                    {
                                        monster.X = xx;
                                        monster.Y = yy;
                                        TwoMovements jump = new TwoMovements();
                                        jump.X = xx;
                                        jump.Y = yy;
                                        jump.EntityCount = 1;
                                        jump.FirstEntity = monster.UID;
                                        jump.MovementType = TwoMovements.Jump;
                                        client.SendScreen(jump, true);
                                    }
                                    // if (client.Entity.ContainsFlag(Update.Flags.FlashingName))
                                    //    monster.MonsterInfo.InSight = client.Entity.UID;
                                }
                                else
                                {
                                    //  if (client.Entity.ContainsFlag(Update.Flags.FlashingName))
                                    {
                                        if (monster.MonsterInfo.InSight == client.Entity.UID)
                                        {
                                            if (!client.Entity.Dead)
                                            {
                                                if (Now >= monster.MonsterInfo.LastMove.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                                {
                                                    short distance = Kernel.GetDistance(monster.X, monster.Y, client.Entity.X, client.Entity.Y);

                                                    if (distance <= monster.MonsterInfo.AttackRange)
                                                    {
                                                        monster.MonsterInfo.LastMove = Time32.Now;
                                                        new Game.Attacking.Handle(null, monster, client.Entity);
                                                        if (Time32.Now >= monster.MonsterInfo.Lastpop.AddSeconds(30))
                                                        {
                                                            monster.MonsterInfo.Lastpop = Time32.Now;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (distance <= monster.MonsterInfo.ViewRange)
                                                        {
                                                            TwoMovements jump = new TwoMovements();
                                                            jump.X = client.Entity.X;
                                                            jump.Y = client.Entity.Y;
                                                            monster.X = client.Entity.X;
                                                            monster.Y = client.Entity.Y;
                                                            jump.EntityCount = 1;
                                                            jump.FirstEntity = monster.UID;
                                                            jump.MovementType = TwoMovements.Jump;
                                                            client.SendScreen(jump, true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IDisposable[] TimerSubscriptions;
        private object DisposalSyncRoot;

        private Interfaces.IMapObject[] _objects;
        public Interfaces.IMapObject[] Objects { get { return _objects; } }

        private ConcurrentDictionary<uint, Interfaces.IMapObject> _objectDictionary;
        public ConcurrentDictionary<uint, Game.Statue> Statue = new ConcurrentDictionary<uint, Statue>();

        public Client.GameState Owner;

        public Screen(Client.GameState client)
        {
            Owner = client;
            _objects = new Interfaces.IMapObject[0];
            _objectDictionary = new ConcurrentDictionary<uint, IMapObject>();
            PokerTables = new ConcurrentDictionary<uint, Game.ConquerStructures.PokerTable>();
            TimerSubscriptions = new IDisposable[] 
                {
                    MonsterBuffers.Add(client),
                    Guards.Add(client),
                    AliveMonsters.Add(client),
                    Items.Add(client),
                    MatrixMobs.Add(client)
                };
            DisposalSyncRoot = new object();

        }
        ~Screen()
        {
            DisposeTimers();
            Clear();
            _objects = null;
            _objectDictionary = null;
            Owner = null;
        }

        public void DisposeTimers()
        {
            lock (DisposalSyncRoot)
            {
                if (TimerSubscriptions == null) return;
                for (int i = 0; i < TimerSubscriptions.Length; i++)
                {
                    if (TimerSubscriptions[i] != null)
                    {
                        TimerSubscriptions[i].Dispose();
                        TimerSubscriptions[i] = null;
                    }
                }
            }
        }

        private void updateBase()
        {
            _objects = _objectDictionary.Values.ToArray();
        }

        public bool Add(Interfaces.IMapObject _object)
        {
            if (_object == null) return false;

            if (_objectDictionary.ContainsKey(_object.UID))
                if (_objectDictionary[_object.UID] == null) // should never happen
                    _objectDictionary.Remove(_object.UID);

            if (!_objectDictionary.ContainsKey(_object.UID))
            {
                if (Kernel.GetDistance(_object.X, _object.Y, Owner.Entity.X, Owner.Entity.Y) <= Constants.pScreenDistance)
                {
                    _objectDictionary[_object.UID] = _object;
                    updateBase();
                    return true;
                }
            }
            return false;
        }
        public bool Remove(Interfaces.IMapObject _object)
        {
            if (_object == null) return false;

            if (_objectDictionary.Remove(_object.UID))
            {
                updateBase();
                if (_object.MapObjType == MapObjectType.Item)
                {
                    FloorItem item = _object as FloorItem;
                    if (item.Type >= FloorItem.Effect)
                    {
                        item.Type = FloorItem.RemoveEffect;
                        Owner.Send(item);
                    }
                    else
                    {
                        item.Type = FloorItem.Remove;
                        Owner.Send(item);
                        item.Type = FloorItem.Drop;
                    }
                }
                else if (_object.MapObjType == MapObjectType.Player)
                {
                    Owner.Send(new Data(true)
                    {
                        UID = _object.UID,
                        ID = Network.GamePackets.Data.RemoveEntity
                    });
                }
                else if (_object.MapObjType == MapObjectType.StaticEntity)
                {
                    Owner.Send(new Data(true)
                    {
                        UID = _object.UID,
                        ID = Network.GamePackets.Data.RemoveEntity
                    });
                }
                return true;
            }
            return false;
        }

        public bool TryGetValue(uint uid, out Entity entity)
        {
            entity = null;
            Interfaces.IMapObject imo = null;
            if (_objectDictionary.TryGetValue(uid, out imo))
            {
                if (imo == null)
                {
                    _objectDictionary.Remove(uid);
                    updateBase();
                    return false;
                }
                if (imo is Entity)
                {
                    entity = imo as Entity;
                    return true;
                }
            }
            return false;
        }
        public bool GetRaceObject(Func<IMapObject, bool> predicate, out StaticEntity entity)
        {
            entity = null;
            foreach (var obj in Objects)
                if (obj is StaticEntity)
                    if (predicate(obj))
                        entity = obj as StaticEntity;
            return entity != null;
        }
        public bool TryGetSob(uint uid, out SobNpcSpawn sob)
        {
            sob = null;
            Interfaces.IMapObject imo = null;
            if (_objectDictionary.TryGetValue(uid, out imo))
            {
                if (imo == null)
                {
                    _objectDictionary.Remove(uid);
                    updateBase();
                    return false;
                }
                if (imo is SobNpcSpawn)
                {
                    sob = imo as SobNpcSpawn;
                    return true;
                }               
            }
            return false;
        }
        public bool TryGetFloorItem(uint uid, out FloorItem item)
        {
            item = null;
            Interfaces.IMapObject imo = null;
            if (_objectDictionary.TryGetValue(uid, out imo))
            {
                if (imo == null)
                {
                    _objectDictionary.Remove(uid);
                    updateBase();
                    return false;
                }
                if (imo is FloorItem)
                {
                    item = imo as FloorItem;
                    return true;
                }
            }
            return false;
        }


        public IEnumerable<T> Select<T>(MapObjectType type) where T : class
        {
            foreach (var obj in Objects)
                if (obj != null)
                    if (obj.MapObjType == type)
                        yield return obj as T;
        }
        public IEnumerable<T> Where<T>(Func<IMapObject, bool> predicate) where T : class
        {
            foreach (var obj in Objects)
                if (obj != null)
                    if (predicate(obj))
                        yield return obj as T;
        }
        public IEnumerable<T> SelectWhere<T>(MapObjectType type, Func<T, bool> predicate) where T : class
        {
            foreach (var obj in Objects)
                if (obj != null)
                    if (obj.MapObjType == type)
                        if (predicate(obj as T))
                            yield return obj as T;
        }


        public bool Contains(Interfaces.IMapObject _object)
        {
            if (_object == null) return false;
            return _objectDictionary.ContainsKey(_object.UID);
        }
        public bool Contains(uint uid)
        {
            return _objectDictionary.ContainsKey(uid);
        }

        public void CleanUp(Interfaces.IPacket spawnWith)
        {
            bool remove;
            try
            {
                foreach (IMapObject Base in Objects)
                {
                    if (Base == null) continue;
                    remove = false;
                    if (Base.MapObjType == MapObjectType.Monster)
                    {
                        if ((Base as Entity).Dead)
                        {
                            if (Time32.Now > (Base as Entity).DeathStamp.AddSeconds(8))
                                remove = true;
                            else
                                remove = false;
                        }
                        if (Kernel.GetDistance(Owner.Entity.X, Owner.Entity.Y, Base.X, Base.Y) >= Constants.remScreenDistance)
                            remove = true;
                        if (remove)
                        {
                            if ((Base as Entity).MonsterInfo.InSight == Owner.Entity.UID)
                                (Base as Entity).MonsterInfo.InSight = 0;
                        }
                    }
                    else if (Base.MapObjType == MapObjectType.Player)
                    {
                        if (remove = (Kernel.GetDistance(Owner.Entity.X, Owner.Entity.Y, Base.X, Base.Y) >= Constants.pScreenDistance))
                        {
                            GameState pPlayer = Base.Owner as GameState;
                            pPlayer.Screen.Remove(Owner.Entity);
                        }
                    }
                    else if (Base.MapObjType == MapObjectType.Item)
                    {
                        remove = (Kernel.GetDistance(Owner.Entity.X, Owner.Entity.Y, Base.X, Base.Y) >= 22);

                    }
                    else
                    {
                        remove = (Kernel.GetDistance(Owner.Entity.X, Owner.Entity.Y, Base.X, Base.Y) >= Constants.remScreenDistance);
                    }
                    if (Base.MapID != Owner.Map.ID)
                        remove = true;
                    if (remove)
                    {
                        Remove(Base);
                    }
                }
            }
            catch (Exception e) { Program.SaveException(e); }
        }
        public void FullWipe()
        {
            foreach (IMapObject Base in Objects)
            {
                if (Base == null) continue;

                Data data = new Data(true);
                data.UID = Base.UID;
                data.ID = Network.GamePackets.Data.RemoveEntity;
                Owner.Send(data);

                if (Base.MapObjType == Game.MapObjectType.Player)
                {
                    GameState pPlayer = Base.Owner as GameState;
                    pPlayer.Screen.Remove(Owner.Entity);
                }
            }
            Clear();
        }
        public void Clear()
        {
            PokerTables.Clear();
            _objectDictionary.Clear();
            _objects = new IMapObject[0];
        }
        private static ConcurrentDictionary<uint, Game.ConquerStructures.PokerTable> PokerTables;
        public void Reload(Interfaces.IPacket spawnWith = null)
        {
            try
            {
                var Map = Owner.Map;
                #region House
                var spouse = MaTrix.House.SpouseHouse(Owner.Entity.Spouse);
                if (Owner.Map.ID == (ushort)Owner.Entity.UID)
                {
                    if (MaTrix.House.Houses.ContainsKey(Owner.Entity.UID))
                    {
                        var info = MaTrix.House.Houses[Owner.Entity.UID];
                        foreach (var fur in info.Furnitures.Values)
                        {
                            if (fur == null) continue;
                            if (Kernel.GetDistance(fur.X, fur.Y, Owner.Entity.X, Owner.Entity.Y) > 16) continue;
                            if (Contains(fur.UID)) continue;
                            fur.SendSpawn(Owner, false);
                        }
                    }
                }
                else if (spouse != null)
                {
                    if (Owner.Map.ID == spouse.ID)
                    {
                        foreach (var fur in spouse.Furnitures.Values)
                        {
                            if (fur == null) continue;
                            if (Kernel.GetDistance(fur.X, fur.Y, Owner.Entity.X, Owner.Entity.Y) > 16) continue;
                            if (Contains(fur.UID)) continue;
                            fur.SendSpawn(Owner, false);
                        }
                    }
                }
                #endregion
                #region Npcs
                foreach (Interfaces.INpc npc in Map.Npcs.Values)
                {
                    if (npc == null) continue;
                    if (Kernel.GetDistance(npc.X, npc.Y, Owner.Entity.X, Owner.Entity.Y) > 16) continue;
                    if (Contains(npc.UID)) continue;
                    npc.SendSpawn(Owner, false);
                }
                foreach (var npc in Database.GuildCondutors.GuildConductors.Values)
                {
                    if (npc == null) continue;
                    if (npc.npc.MapID == Owner.Entity.MapID)
                    {
                        if (Kernel.GetDistance(npc.npc.X, npc.npc.Y, Owner.Entity.X, Owner.Entity.Y) > 16)
                            continue;
                        if (Contains(npc.npc.UID))
                            continue;
                        npc.npc.SendSpawn(Owner, false);
                    }
                }
                #endregion
                #region Items + map effects
                foreach (var item in Map.FloorItems.Values)
                {
                    if (item == null) continue;
                    if (Kernel.GetDistance(item.X, item.Y, Owner.Entity.X, Owner.Entity.Y) > 16) continue;
                    if (Contains(item.UID)) continue;
                    if (item.Type == FloorItem.Effect)
                    {
                        if (item.ItemID == FloorItem.DaggerStorm || item.ItemID == FloorItem.FuryofEgg || item.ItemID == FloorItem.ShacklingIce)
                        {
                            if (item.OnFloor.AddSeconds(4).Next(time: Time32.Now.AllMilliseconds()))
                            {
                                item.Type = Network.GamePackets.FloorItem.RemoveEffect;
                                foreach (Interfaces.IMapObject _obj in Objects)
                                    if (_obj != null)
                                        if (_obj.MapObjType == MapObjectType.Player)
                                            (_obj as Entity).Owner.Send(item);
                                Map.RemoveFloorItem(item);
                            }
                            else
                                item.SendSpawn(Owner, false);
                        }
                        else
                            item.SendSpawn(Owner, false);
                    }
                    else
                    {
                        if ((Time32.Now > item.OnFloor.AddSeconds(Constants.FloorItemSeconds)) || item.PickedUpAlready)
                        {
                            item.Type = Network.GamePackets.FloorItem.Remove;
                            Map.RemoveFloorItem(item);
                        }
                    }
                    item.SendSpawn(Owner);
                }
                #endregion

                MaTrix.AI.CheckScreen(Owner, spawnWith);

                foreach (Game.Entity monster in Map.Entities.Values)
                {
                    if (monster == null) continue;
                    if (Kernel.GetDistance(monster.X, monster.Y, Owner.Entity.X, Owner.Entity.Y) <= 16 && !Contains(monster.UID))
                    {
                        if (!monster.Dead)
                        {
                            monster.SendSpawn(Owner, false);
                            if (monster.MaxHitpoints > 65535)
                            {
                                Update upd = new Update(true) { UID = monster.UID };
                                // upd.Append(Update.MaxHitpoints, monster.MaxHitpoints);
                                upd.Append(Update.Hitpoints, monster.Hitpoints);
                                Owner.Send(upd);
                            }
                        }
                    }
                }
                if (Owner.Map.ID == MaTrix.Roulette.Database.Roulettes.RouletteTable.MapID)
                {
                    foreach (var R in MaTrix.Roulette.Database.Roulettes.RoulettesPoll.Values)
                    {
                        if (Kernel.GetDistance(R.SpawnPacket.X, R.SpawnPacket.Y, Owner.Entity.X, Owner.Entity.Y) <= Constants.nScreenDistance && !Contains(R.SpawnPacket.UID))
                        {
                            Owner.Send(R.SpawnPacket);
                        }
                    }
                }
                #region RaceItems
                if (Owner.Map.StaticEntities.Count != 0)
                {
                    foreach (var item in Owner.Map.StaticEntities.Values)
                    {
                        if (item == null) continue;
                        if (!item.Viable) continue;
                        if (Kernel.GetDistance(item.X, item.Y, Owner.Entity.X, Owner.Entity.Y) > 16) continue;
                        if (Contains(item.UID)) continue;
                        item.SendSpawn(Owner);
                    }
                }
                #endregion
                #region PokerTables
                if (Owner.Map.ID == 1858 || Owner.Map.ID == 8881)
                {
                    foreach (Game.ConquerStructures.PokerTable T in Database.PokerTables.Tables.Values.ToList())
                    {
                        if (T.Map == Owner.Map.ID)
                        {
                            if (Kernel.GetDistance(T.X, T.Y, Owner.Entity.X, Owner.Entity.Y) <= 16)
                            {
                                if (!PokerTables.ContainsKey(T.UID))
                                {
                                    T.Spawn(Owner);
                                    PokerTables.Add(T.UID, T);
                                }
                            }
                            else
                            {
                                if (PokerTables.ContainsKey(T.UID))
                                {
                                    PokerTables.Remove(T.UID);
                                }
                            }
                        }
                        else
                        {
                            if (PokerTables.ContainsKey(T.UID))
                            {
                                PokerTables.Remove(T.UID);
                            }
                        }
                    }
                }
                else PokerTables.Clear();
                #endregion
                #region Players
                CleanUp(spawnWith);
                if (Owner.Entity.MapID == 1002)
                {
                    foreach (var statue in Game.Statue.Statues.Values)
                    {
                        if (statue > Owner)
                        {
                            Statue.TryAdd(statue.UID, statue);
                        }
                        else if (statue < Owner)
                        {
                            Game.Statue astatue;
                            Statue.TryRemove(statue.UID, out astatue);
                        }
                    }
                }
                else
                {
                    if (Statue.Count > 0)
                        Statue.Clear();
                }
                foreach (GameState pClient in Kernel.GamePool.Values)
                {
                    if (pClient == null) return;
                    if (pClient.Entity == null) return;
                    if (Owner == null) return;
                    if (Owner.Entity == null) return;
                    if (pClient.Entity.UID != Owner.Entity.UID)
                    {
                        if (pClient.Map.ID == Owner.Map.ID)
                        {
                            short dist = Kernel.GetDistance(pClient.Entity.X, pClient.Entity.Y, Owner.Entity.X, Owner.Entity.Y);
                            if (dist <= Constants.pScreenDistance && !Contains(pClient.Entity))
                            {
                                if (pClient.Guild != null)
                                    pClient.Guild.SendName(Owner);


                                if (Owner.Guild != null)
                                    Owner.Guild.SendName(pClient);
                                if (pClient.Entity.InteractionInProgress && pClient.Entity.InteractionWith != Owner.Entity.UID && pClient.Entity.InteractionSet)
                                {
                                    if (pClient.Entity.Body == 1003 || pClient.Entity.Body == 1004)
                                    {
                                        if (pClient.Entity.InteractionX == pClient.Entity.X && pClient.Entity.Y == pClient.Entity.InteractionY)
                                        {
                                            Network.GamePackets.Attack atak = new MTA.Network.GamePackets.Attack(true);
                                            atak.Attacker = pClient.Entity.UID;
                                            atak.Attacked = pClient.Entity.InteractionWith;
                                            atak.X = pClient.Entity.X;
                                            atak.Y = pClient.Entity.Y;
                                            atak.AttackType = 49;
                                            atak.Damage = pClient.Entity.InteractionType;
                                            Owner.Send(atak);
                                        }
                                    }
                                    else
                                    {
                                        if (MTA.Kernel.GamePool.ContainsKey(pClient.Entity.InteractionWith))
                                        {
                                            Client.GameState Cs = MTA.Kernel.GamePool[pClient.Entity.InteractionWith] as Client.GameState;
                                            if (Cs.Entity.X == pClient.Entity.InteractionX && pClient.Entity.Y == pClient.Entity.InteractionY)
                                            {
                                                Network.GamePackets.Attack atak = new MTA.Network.GamePackets.Attack(true);
                                                atak.Attacker = pClient.Entity.UID;
                                                atak.Attacked = pClient.Entity.InteractionWith;
                                                atak.X = pClient.Entity.X;
                                                atak.Y = pClient.Entity.Y;
                                                atak.AttackType = 49;
                                                atak.Damage = pClient.Entity.InteractionType;
                                                Owner.Send(atak);
                                            }
                                        }
                                    }
                                }
                                if (pClient.Map.BaseID == 700)
                                {
                                    if (Owner.InQualifier())
                                    {
                                        if (pClient.InQualifier())
                                        {
                                            Owner.Entity.SendSpawn(pClient);
                                            pClient.Entity.SendSpawn(Owner);
                                            if (pClient.Guild != null)
                                                Owner.Entity.SendSpawn(pClient, false);
                                            if (Owner.Guild != null)
                                                pClient.Entity.SendSpawn(Owner, false);
                                            if (spawnWith != null)
                                                pClient.Send(spawnWith);
                                        }
                                        else
                                        {
                                            Owner.Entity.SendSpawn(pClient);

                                            if (pClient.Guild != null)
                                                Owner.Entity.SendSpawn(pClient, false);
                                            Add(pClient.Entity);
                                            if (spawnWith != null)
                                                pClient.Send(spawnWith);
                                        }
                                    }
                                    else
                                    {
                                        if (pClient.InQualifier())
                                        {
                                            pClient.Entity.SendSpawn(Owner);
                                            if (Owner.Guild != null)
                                                pClient.Entity.SendSpawn(Owner, false);
                                            pClient.Screen.Add(Owner.Entity);
                                            if (spawnWith != null)
                                                Owner.Send(spawnWith);
                                        }
                                        else
                                        {
                                            Owner.Entity.SendSpawn(pClient);
                                            pClient.Entity.SendSpawn(Owner);

                                            if (pClient.Guild != null)
                                                Owner.Entity.SendSpawn(pClient, false);
                                            if (Owner.Guild != null)
                                                pClient.Entity.SendSpawn(Owner, false);

                                            if (spawnWith != null)
                                                pClient.Send(spawnWith);
                                        }
                                    }
                                }
                                else
                                {
                                    Owner.Entity.SendSpawn(pClient);
                                    pClient.Entity.SendSpawn(Owner);

                                    if (pClient.Guild != null)
                                        Owner.Entity.SendSpawn(pClient, false);
                                    if (Owner.Guild != null)
                                        pClient.Entity.SendSpawn(Owner, false);

                                    if (spawnWith != null)
                                        pClient.Send(spawnWith);
                                }

                                #region Other Pet & Clones
                                if (pClient.Entity.MyClones.Count > 0)
                                {
                                    foreach (var clone in pClient.Entity.MyClones.Values)
                                    {
                                        if (clone == null) continue;
                                        if (Kernel.GetDistance(clone.X, clone.Y, Owner.Entity.X, Owner.Entity.Y) <= 18 && !Contains(clone.UID))
                                        {
                                            if (!clone.Dead)
                                                clone.SendSpawn(Owner);
                                        }
                                    }
                                }
                                if (pClient.Pet.Pets.Count > 0)
                                {
                                    foreach (var pet in pClient.Pet.Pets.Values)
                                    {
                                        if (pet == null) continue;
                                        if (pet.Entity == null) continue;
                                        if (Kernel.GetDistance(pet.Entity.X, pet.Entity.Y, Owner.Entity.X, Owner.Entity.Y) <= 18 && !Contains(pet.Entity.UID))
                                        {
                                            if (!pet.Entity.Dead)
                                                pet.Entity.SendSpawn(Owner);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
                #region My Pet & Clones
                if (Owner.Entity.MyClones.Count > 0)
                {
                    foreach (var clone in Owner.Entity.MyClones.Values)
                    {
                        if (clone == null) continue;
                        if (Kernel.GetDistance(clone.X, clone.Y, Owner.Entity.X, Owner.Entity.Y) <= 18 && !Contains(clone.UID))
                        {
                            if (!clone.Dead)
                                clone.SendSpawn(Owner);
                        }
                    }
                }
                if (Owner.Pet.Pets.Count > 0)
                {
                    foreach (var pet in Owner.Pet.Pets.Values)
                    {
                        if (pet == null) continue;
                        if (pet.Entity == null) continue;
                        if (Kernel.GetDistance(pet.Entity.X, pet.Entity.Y, Owner.Entity.X, Owner.Entity.Y) <= 18/* && !Contains(pet.Entity.UID)*/)
                        {
                            if (!pet.Entity.Dead)
                                Owner.Send(pet.Entity.SpawnPacket);
                            //  pet.Entity.SendSpawn(Owner, false);
                        }
                    }
                }
                #endregion
                #endregion

            }
            catch (Exception e) { Program.SaveException(e); }
        }

        public void SendScreen(Interfaces.IPacket buffer, bool self)
        {
            foreach (Interfaces.IMapObject _obj in Objects)
            {
                if (_obj != null)
                {
                    if (_obj.UID != Owner.Entity.UID)
                    {
                        if (_obj.MapObjType == Game.MapObjectType.Player)
                        {
                            GameState client = _obj.Owner as GameState;
                            if (Owner.WatchingGroup != null && client.WatchingGroup == null)
                                continue;
                            if (Owner.TeamWatchingGroup != null && client.TeamWatchingGroup == null)
                                continue;
                            client.Send(buffer);
                        }
                    }
                }
            }

            if (self)
                Owner.Send(buffer);
        }

        public bool TryGetNpc(uint uid, out INpc sob)
        {
            sob = null;
            Interfaces.IMapObject imo = null;
            if (_objectDictionary.TryGetValue(uid, out imo))
            {
                if (imo == null)
                {
                    _objectDictionary.Remove(uid);
                    updateBase();
                    return false;
                }
                if (imo is NpcSpawn)
                {
                    sob = imo as NpcSpawn;
                    return true;
                }
            }
            return false;
        }
    }
}
