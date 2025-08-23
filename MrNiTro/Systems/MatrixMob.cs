using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Generic;
using MTA.Game;
using MTA.Database;
using MTA.Network.GamePackets;
using System.Drawing;

namespace MTA.MaTrix
{
    public class MatrixMob
    {
        public static SafeDictionary<uint, SafeDictionary<uint, MatrixMob>> Mobs = new SafeDictionary<uint, SafeDictionary<uint, MatrixMob>>();
        private static TimerRule<MatrixMob> Buffers;
        public static void CreateTimerFactories()
        {

            Buffers = new TimerRule<MatrixMob>(BuffersCallback, 500);
            Load();
        }
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("matrixspwans"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    var monsterID = reader.ReadUInt32("monster");
                    var MapID = reader.ReadUIntArray("map", new string[] { "|" });
                    var x = reader.ReadUInt16("x");
                    var y = reader.ReadUInt16("y");
                    var minute = reader.ReadUInt16("minute");
                    MatrixMob MatrixMob = new MatrixMob(monsterID, MapID, x, y, minute);
                    MatrixMob.cpsarray = reader.ReadUIntArray("cps", new string[] { "|" });
                    MatrixMob.itemsarray = reader.ReadUIntArray("items", new string[] { "|" });

                }
            }
            Console.WriteLine("New Maps and Monster loaded.");
        }
        private static void BuffersCallback(MatrixMob Mob, int time)
        {
            DateTime now = DateTime.Now;
            if (now.Minute == Mob.Minute && now.Second == 0 && !Mob.summoned)
            {
                Mob.summoned = true;
                Mob.Summon();
            }
        }

        private bool summoned;
        private uint monsterID;
        private uint[] Maps;
        private ushort X;
        private ushort Y;
        private int Minute;
        public uint[] cpsarray;
        public uint[] itemsarray;

        private IDisposable[] TimerSubscriptions;
        private object DisposalSyncRoot;
        public MatrixMob(uint monsterID, uint[] MapID, ushort X, ushort Y, int Minute)
        {
            this.monsterID = monsterID;
            this.Maps = MapID;
            this.X = X;
            this.Y = Y;
            this.Minute = Minute;
            TimerSubscriptions = new IDisposable[] 
            {
                Buffers.Add(this),               
            };
            DisposalSyncRoot = new object();
        }

        private void Summon()
        {
            var MapID = (ushort)RandFromGivingNums(Maps);
            if (Kernel.Maps.ContainsKey(MapID))
            {
                var Map = Kernel.Maps[MapID];
                if (Database.MonsterInformation.MonsterInformations.ContainsKey(monsterID))
                {
                    Database.MonsterInformation mt = Database.MonsterInformation.MonsterInformations[monsterID];
                    mt.BoundX = X;
                    mt.BoundY = Y;
                    mt.RespawnTime = 36000;
                    Entity entity = new Entity(EntityFlag.Monster, false);
                    entity.MapObjType = MapObjectType.Monster;
                    entity.MonsterInfo = mt.Copy();
                    entity.MonsterInfo.Owner = entity;
                    entity.Name = mt.Name;
                    entity.MinAttack = mt.MinAttack;
                    entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
                    entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
                    entity.Defence = mt.Defence;
                    entity.Body = mt.Mesh;
                    entity.Level = mt.Level;
                    entity.UID = Map.EntityUIDCounter.Next;
                    entity.MapID = MapID;
                    entity.SendUpdates = true;
                    if (X != 0)
                        entity.X = X;
                    if (Y != 0)
                        entity.Y = Y;
                    if (X == 0 || Y == 0)
                    {
                        var cord = Map.RandomCoordinates();
                        entity.X = cord.Item1;
                        entity.Y = cord.Item2;
                        do
                        {
                            cord = Map.RandomCoordinates();
                            entity.X = cord.Item1;
                            entity.Y = cord.Item2;
                        }
                        while (!Map.Floor[entity.X, entity.Y, MapObjectType.Monster]);
                    }

                    Map.AddEntity(entity);
                    if (!Mobs.ContainsKey(entity.MapID))
                        Mobs.Add(entity.MapID, new SafeDictionary<uint, MatrixMob>());

                    if (!Mobs[entity.MapID].ContainsKey(entity.UID))
                        Mobs[entity.MapID].Add(entity.UID, this);

                    Kernel.SendWorldMessage(new Message(entity.Name + " has shown up in " + ((Enums.Maps)entity.MapID).ToString() + " (" + entity.X + ", " + entity.Y + ")", Color.Red, Message.BroadcastMessage));
                    Console.WriteLine(entity.Name + " has shown up in " + ((Enums.Maps)entity.MapID).ToString() + " (" + entity.X + ", " + entity.Y + ")");
                    if (mt.SuperBoss || mt.Boss)
                        foreach (var client in Program.Values)
                            client.MessageBox(entity.Name + " has apeared , Who will Defeat it. !", p => { p.Entity.Teleport(entity.MapID, (ushort)(entity.X + 3), (ushort)(entity.Y + 3)); }, null);

                }
            }
        }
        ~MatrixMob()
        {
            DisposeTimers();
        }
        private void DisposeTimers()
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

        public static uint RandFromGivingNums(params uint[] nums)
        {
            return nums[Kernel.Random.Next(0, nums.Length)];
        }

        public static bool Drop(Entity Killer, Entity Monster)
        {
            if (Killer == null || Monster == null)
                return false;
            if (Mobs.ContainsKey(Monster.MapID))
            {
                if (Mobs[Monster.MapID].ContainsKey(Monster.UID))
                {
                    var mob = Mobs[Monster.MapID][Monster.UID];
                    if (mob != null)
                    {
                        if (mob.cpsarray != null)
                        {
                            if (mob.cpsarray.Length > 0)
                            {
                                var cp = RandFromGivingNums(mob.cpsarray);
                                if (cp != 0)
                                {
                                    Killer.ConquerPoints += 200000;
                                    Kernel.SendWorldMessage(new Message(Killer.Name + " has killed " + Monster.Name + " at " + ((Enums.Maps)Monster.MapID).ToString() + " and get 200,000 Cps .", Color.Red, Message.Center));
                                    //Program.AddMobLog(Monster.Name, Killer.Name, cp);

                                }
                            }
                        }
                        if (mob.itemsarray != null)
                        {
                            if (mob.itemsarray.Length > 0)
                            {
                                var item = RandFromGivingNums(mob.itemsarray);
                                if (item != 0 && Database.ConquerItemInformation.BaseInformations.ContainsKey(item))
                                {
                                    Killer.Owner.Inventory.Add(item, 0, 1);
                                    Kernel.SendWorldMessage(new Message(Killer.Name + " has killed " + Monster.Name + " at " + ((Enums.Maps)Monster.MapID).ToString() + " and get " + Database.ConquerItemInformation.BaseInformations[item].Name + " item .", Color.Red, Message.BroadcastMessage));
                                    Program.AddMobLog(Monster.Name, Killer.Name, 0, item);
                                }
                            }
                        }
                        mob.summoned = false;
                        Kernel.Maps[Monster.MapID].RemoveEntity(Monster);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
 