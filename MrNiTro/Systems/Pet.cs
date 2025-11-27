using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game;
using System.Threading.Generic;
using MTA.Database;
using MTA.Network.GamePackets;
using MTA.Network;
using MTA.Game.ConquerStructures;
using System.Collections.Concurrent;

namespace MTA.MaTrix
{
    public class Pet
    {
        public enum PetType
        {
            Normal = 0,
            Looter,
            Stiger,
            Attacker,
            defender

        }
        public class PetInfo
        {
            public Entity Entity;
            public PetType Type;
            public GameState Owner;

        }
        #region Static Actions
        private static TimerRule<GameState> PetsAction;
        public static void CreateTimerFactories()
        {
            PetsAction = new TimerRule<GameState>(PetsActionCallback, 500);
        }
        private static void PetsActionCallback(GameState client, int time)
        {
            if (!client.Socket.Alive)
            {
                client.Pet.DisposeTimers();
                return;
            }
            if (client.Entity == null)
                return;
            if (client.Map == null)
                return;
            if (client.Pet == null)
                return;
            if (client.Pet.Pets == null)
            {
                client.Pet.DisposeTimers();
                return;
            }
            Time32 Now = new Time32(time);
            var pets = client.Pet.Pets.Values;
            foreach (var pet in pets)
            {

                if (pet != null)
                {

                    #region Back To Owner
                    short distance = Kernel.GetDistance(pet.Entity.X, pet.Entity.Y, client.Entity.X, client.Entity.Y);
                    if (distance >= 8)
                    {
                        ushort X = (ushort)(client.Entity.X + Kernel.Random.Next(2));
                        ushort Y = (ushort)(client.Entity.Y + Kernel.Random.Next(2));
                        if (!client.Map.SelectCoordonates(ref X, ref Y))
                        {
                            X = client.Entity.X;
                            Y = client.Entity.Y;
                        }
                        pet.Entity.X = X;
                        pet.Entity.Y = Y;
                        Network.GamePackets.Data data = new MTA.Network.GamePackets.Data(true);
                        data.ID = Network.GamePackets.Data.Jump;
                        data.dwParam = (uint)((Y << 16) | X);
                        data.wParam1 = X;
                        data.wParam2 = Y;
                        data.UID = pet.Entity.UID;
                        pet.Entity.MonsterInfo.SendScreen(data);
                        client.SendScreenSpawn(pet.Entity, true);
                    }
                    else if (distance > 4)
                    {
                        Enums.ConquerAngle facing = Kernel.GetAngle(pet.Entity.X, pet.Entity.Y, pet.Entity.Owner.Entity.X, pet.Entity.Owner.Entity.Y);
                        if (!pet.Entity.Move(facing))
                        {
                            facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                            if (pet.Entity.Move(facing))
                            {
                                pet.Entity.Facing = facing;
                                Network.GamePackets.GroundMovement move = new MTA.Network.GamePackets.GroundMovement(true);
                                move.Direction = facing;
                                move.UID = pet.Entity.UID;
                                move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                                pet.Entity.MonsterInfo.SendScreen(move);
                            }
                        }
                        else
                        {
                            pet.Entity.Facing = facing;
                            Network.GamePackets.GroundMovement move = new MTA.Network.GamePackets.GroundMovement(true);
                            move.Direction = facing;
                            move.UID = pet.Entity.UID;
                            move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                            pet.Entity.MonsterInfo.SendScreen(move);
                        }
                        client.SendScreenSpawn(pet.Entity, true);
                    }

                    #endregion
                    switch (pet.Type)
                    {
                        case PetType.Normal:
                            {
                                #region Normal Attack Guard
                                {
                                    var monster = pet.Entity;
                                    if (monster.MonsterInfo.InSight == 0)
                                    {
                                        if (client.Entity.AttackPacket != null)
                                        {
                                            if (client.Entity.AttackPacket.AttackType == Network.GamePackets.Attack.Magic)
                                            {
                                                if (client.Entity.AttackPacket.Decoded)
                                                {
                                                    if (Database.SpellTable.SpellInformations.ContainsKey((ushort)client.Entity.AttackPacket.Damage))
                                                    {
                                                        var info = Database.SpellTable.SpellInformations[(ushort)client.Entity.AttackPacket.Damage].Values.ToArray()[client.Spells[(ushort)client.Entity.AttackPacket.Damage].Level];
                                                        if (info.CanKill)
                                                        {
                                                            monster.MonsterInfo.InSight = client.Entity.AttackPacket.Attacked;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                monster.MonsterInfo.InSight = client.Entity.AttackPacket.Attacked;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (monster.MonsterInfo.InSight > 400000 && monster.MonsterInfo.InSight < 600000 || monster.MonsterInfo.InSight > 800000 && monster.MonsterInfo.InSight != monster.UID)
                                        {
                                            Entity attacked = null;

                                            if (client.Screen.TryGetValue(monster.MonsterInfo.InSight, out attacked))
                                            {
                                                if (Now > monster.AttackStamp.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                                {
                                                    monster.AttackStamp = Now;
                                                    if (attacked.Dead)
                                                    {
                                                        monster.MonsterInfo.InSight = 0;
                                                    }
                                                    else
                                                        new Game.Attacking.Handle(null, monster, attacked);
                                                }
                                            }
                                            else
                                                monster.MonsterInfo.InSight = 0;
                                        }
                                    }
                                }
                                #endregion
                                break;
                            }
                        case PetType.Stiger:
                            {
                                #region Stiger Guard
                                if (!client.Entity.ContainsFlag(Update.Flags.Stigma))
                                {
                                    SpellUse suse = new SpellUse(true);
                                    suse.Attacker = pet.Entity.UID;
                                    suse.SpellID = 1095;
                                    suse.SpellLevel = 4;
                                    suse.X = client.Entity.X;
                                    suse.Y = client.Entity.Y;

                                    suse.AddTarget(client.Entity, 0, null);

                                    client.Entity.AddFlag(Update.Flags.Stigma);
                                    client.Entity.StigmaStamp = Time32.Now;
                                    client.Entity.StigmaIncrease = 50;
                                    client.Entity.StigmaTime = (byte)60;
                                    if (client.Entity.EntityFlag == EntityFlag.Player)
                                        client.Entity.Owner.Send(Constants.Stigma(50, 60));
                                }
                                #endregion
                                break;
                            }
                        case PetType.Looter:
                            {
                                #region Shield Guard
                                if (!client.Entity.ContainsFlag(Update.Flags.MagicShield))
                                {
                                    SpellUse suse = new SpellUse(true);
                                    suse.Attacker = pet.Entity.UID;
                                    suse.SpellID = 1090;
                                    suse.SpellLevel = 4;
                                    suse.X = client.Entity.X;
                                    suse.Y = client.Entity.Y;

                                    suse.AddTarget(client.Entity, 0, null);

                                    client.Entity.AddFlag(Update.Flags.MagicShield);
                                    client.Entity.ShieldStamp = Time32.Now;
                                    client.Entity.ShieldIncrease = 1.1f;
                                    client.Entity.ShieldTime = (byte)60;
                                    if (client.Entity.EntityFlag == EntityFlag.Player)
                                        client.Entity.Owner.Send(Constants.Shield(50, 60));
                                }
                                #endregion
                                break;
                            }
                        case PetType.Attacker:
                            {
                                #region Attacker
                                foreach (var obj in client.Screen.Objects)
                                {
                                    if (client.Entity.Dead)
                                        return;
                                    if (obj.MapObjType == MapObjectType.Monster)
                                    {
                                        var attacked = obj as Entity;
                                        if (attacked.Companion || attacked.MonsterInfo.Guard)
                                            continue;
                                        //   if (Kernel.GetDistance(pet.Entity.X, pet.Entity.Y, attacked.X, attacked.Y) <= 15)
                                        {
                                            if (Now > pet.Entity.AttackStamp.AddMilliseconds(1000 - client.Entity.Agility))
                                            {
                                                pet.Entity.AttackStamp = Now;
                                                if (!attacked.Dead)
                                                    new Game.Attacking.Handle(null, pet.Entity, attacked);
                                            }
                                        }
                                    }
                                    else if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        var attacked = obj as Entity;
                                        if (attacked.Dead)
                                            continue;
                                        if (Game.Attacking.Handle.CanAttack(client.Entity, attacked, null, true))
                                        {
                                            //  if (Kernel.GetDistance(pet.Entity.X, pet.Entity.Y, attacked.X, attacked.Y) <= 15)
                                            {
                                                if (Now > pet.Entity.AttackStamp.AddMilliseconds(1000 - client.Entity.Agility))
                                                {
                                                    pet.Entity.AttackStamp = Now;
                                                    if (!attacked.Dead)
                                                        new Game.Attacking.Handle(null, pet.Entity, attacked);
                                                }
                                            }
                                        }

                                    }
                                    else if (obj.MapObjType == MapObjectType.SobNpc)
                                    {
                                        var attackedSobNpc = obj as SobNpcSpawn;
                                        if (Game.Attacking.Handle.CanAttack(client.Entity, attackedSobNpc, null))
                                        {
                                            //   if (Kernel.GetDistance(pet.Entity.X, pet.Entity.Y, attackedSobNpc.X, attackedSobNpc.Y) <= 15)
                                            {
                                                if (Now > pet.Entity.AttackStamp.AddMilliseconds(1000 - client.Entity.Agility))
                                                {
                                                    pet.Entity.AttackStamp = Now;
                                                    SpellUse suse = new SpellUse(true);
                                                    Attack attack = new Attack(true);
                                                    attack.Effect1 = Attack.AttackEffects1.None;
                                                    uint damage = Game.Attacking.Calculate.Melee(client.Entity, attackedSobNpc, ref attack);
                                                    suse.Effect1 = attack.Effect1;
                                                    Game.Attacking.Handle.ReceiveAttack(pet.Entity, attackedSobNpc, attack, damage, null);
                                                    suse.Attacker = pet.Entity.UID;
                                                    suse.SpellID = pet.Entity.MonsterInfo.SpellID;
                                                    suse.X = attackedSobNpc.X;
                                                    suse.Y = attackedSobNpc.Y;
                                                    suse.AddTarget(attackedSobNpc, damage, attack);
                                                    pet.Entity.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                                break;
                            }
                        case PetType.defender:
                            {
                                #region defender
                                foreach (var obj in client.Screen.Objects)
                                {
                                    if (obj.MapObjType == MapObjectType.Monster)
                                    {
                                        var attacked = obj as Entity;
                                        if (attacked.MonsterInfo != null)
                                        {
                                            if (attacked.MonsterInfo.InSight == client.Entity.UID || attacked.MonsterInfo.InSight == pet.Entity.UID)
                                            {
                                                if (Now > pet.Entity.AttackStamp.AddMilliseconds(1000 - client.Entity.Agility))
                                                {
                                                    pet.Entity.AttackStamp = Now;
                                                    if (!attacked.Dead)
                                                        new Game.Attacking.Handle(null, pet.Entity, attacked);
                                                }
                                            }
                                        }
                                    }
                                    else if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        var attacked = obj as Entity;
                                        if (attacked.AttackPacket != null)
                                        {
                                            if (attacked.AttackPacket.Attacked == client.Entity.UID || attacked.AttackPacket.Attacked == pet.Entity.UID)
                                            {
                                                if (Now > pet.Entity.AttackStamp.AddMilliseconds(1000 - client.Entity.Agility))
                                                {
                                                    pet.Entity.AttackStamp = Now;
                                                    if (!attacked.Dead)
                                                        new Game.Attacking.Handle(null, pet.Entity, attacked);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                                break;
                            }
                    }
                }
                else
                    break;
            }
        }
        #endregion
        #region Timers
        private IDisposable[] TimerSubscriptions;
        private object DisposalSyncRoot;
        ~Pet()
        {
            DisposeTimers();
            Owner = null;
            Pets = null;
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
        #endregion

        public uint MaxAllowed = 2;
        public GameState Owner;
        public SafeConcurrentDictionary<PetType, PetInfo> Pets;
        public Pet(GameState client)
        {
            Owner = client;
            Pets = new SafeConcurrentDictionary<PetType, PetInfo>();
            TimerSubscriptions = new IDisposable[]
            {
               PetsAction.Add(client)
            };
            DisposalSyncRoot = new object();
        }

        public void AddPet(MonsterInformation Mob, PetType Type = PetType.Normal)
        {
            if (Pets.Count == MaxAllowed)
                ClearAll();
            if (Mob.Mesh == 847)
                Type = PetType.Stiger;
            if (Mob.Mesh == 850)
                Type = PetType.Attacker;
            if (Mob.Mesh == 848 || Mob.Mesh == 849)
                Type = PetType.defender;
            if (Mob.Mesh == 846)
                Type = PetType.Looter;
            if (Mob.SpellID == 0)
                Mob.SpellID = 1002;

            if (Type != PetType.Normal)
            {
                var mesh = Mob.Mesh;
                var Name = Mob.Name;

                MonsterInformation.MonsterInformations.TryGetValue(9003, out Mob);

                Mob.Mesh = mesh;
                Mob.Name = Name;
            }

            if (Pets.ContainsKey(Type))
            {
                Data data = new Data(true);
                data.UID = Pets[Type].Entity.UID;
                data.ID = Data.RemoveEntity;
                Pets[Type].Entity.MonsterInfo.SendScreen(data);
                Pets[Type].Entity = null;
                Pets.Remove(Type);
            }
            PetInfo pet = new PetInfo();
            pet.Type = Type;
            pet.Owner = Owner;
            pet.Entity = new Entity(EntityFlag.Monster, true);
            pet.Entity.MonsterInfo = new MonsterInformation();
            pet.Entity.Owner = Owner;
            pet.Entity.MapObjType = MapObjectType.Monster;
            pet.Entity.MonsterInfo = Mob.Copy();
            pet.Entity.MonsterInfo.Owner = pet.Entity;

            pet.Entity.Name = Mob.Name;
            if (Type != PetType.Normal)
                pet.Entity.Name = Mob.Name + "(" + Owner.Entity.Name + ")";

            pet.Entity.MinAttack = Mob.MinAttack;
            pet.Entity.MaxAttack = pet.Entity.MagicAttack = Math.Max(Mob.MinAttack, Mob.MaxAttack);
            pet.Entity.Hitpoints = pet.Entity.MaxHitpoints = Mob.Hitpoints;
            pet.Entity.Body = Mob.Mesh;
            pet.Entity.Level = Mob.Level;
            pet.Entity.UID = (uint)(Owner.Entity.UID - (200000 + Pets.Count));
            pet.Entity.MapID = Owner.Map.ID;
            pet.Entity.SendUpdates = true;
            pet.Entity.X = Owner.Entity.X;
            pet.Entity.Y = Owner.Entity.Y;
            pet.Entity.pettype = Type;
            Pets.Add(pet.Type, pet);

            Owner.SendScreenSpawn(pet.Entity, true);
            //  pet.Entity.SendSpawn(Owner);

        }


        public void RemovePet(PetType Type)
        {
            if (Pets.Count == 0)
                return;
            if (Pets[Type] == null) return;
            Data data = new Data(true);
            data.UID = Pets[Type].Entity.UID;
            data.ID = Data.RemoveEntity;
            Pets[Type].Entity.MonsterInfo.SendScreen(data);
            Pets.Remove(Type);
        }
        public void ClearAll()
        {
            if (Pets.Count > 0)
            {
                foreach (var pet in Pets.Values)
                {
                    Data data = new Data(true);
                    data.UID = pet.Entity.UID;
                    data.ID = Data.RemoveEntity;
                    pet.Entity.MonsterInfo.SendScreen(data);
                    pet.Entity = null;
                }
                Pets.Clear();
            }
        }
    }
}
