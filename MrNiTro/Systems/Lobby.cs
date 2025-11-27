using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game;
using MTA.Game.ConquerStructures;
using MTA.Client;
using System.Threading.Generic;
using MTA.Network.GamePackets;
using MTA.Database;
using System.Collections.Concurrent;
using System.IO;
using MTA.Interfaces;

namespace MTA.MaTrix
{
    public class AI
    {
        #region Static Actions
        public static SafeConcurrentDictionary<uint, AI> Ais = new SafeConcurrentDictionary<uint, AI>();
        private static TimerRule<GameState> AIAction;
        public static void CreateTimerFactories()
        {
            AIAction = new TimerRule<GameState>(AisActionCallback, 500);
        }
        private static void AisActionCallback(GameState client, int time)
        {
            try
            {
                if (!client.Socket.Alive)
                {
                    client.AI.DisposeTimers();
                    return;
                }
                if (client.Entity == null)
                    return;
                if (client.Map == null)
                    return;
                Time32 Now = Time32.Now;
                foreach (var ai in Ais.Values)
                {
                    if (ai.Bot == null)
                        continue;
                    if (ai.Bot.Entity == null)
                        continue;
                    if (ai.Loaded)
                    {
                        if (client.Entity.MapID == ai.Bot.Entity.MapID || ai.selectFunc != null)
                        {
                            #region Check Target
                            if (ai.Target == null)
                            {
                                switch (ai.Type)
                                {
                                    case BotType.MyAi:
                                        {
                                            client.AI.Dispose(client);
                                        }
                                        break;
                                    case BotType.AI:
                                        {
                                            foreach (var obj in ai.Bot.Screen.Objects)
                                            {
                                                if (obj.MapObjType == MapObjectType.Monster || obj.MapObjType == MapObjectType.Player)
                                                {
                                                    if (Kernel.GetDistance(ai.Bot.Entity.X, ai.Bot.Entity.Y, obj.X, obj.Y) < 18)
                                                    {
                                                        var entity = obj as Entity;
                                                        if (entity.Dead) continue;
                                                        if (obj.MapObjType == MapObjectType.Player)
                                                            if (entity.Owner.Fake)
                                                                continue;
                                                        ai.Target = entity;
                                                        if (obj.MapObjType == MapObjectType.Player)
                                                            ai.Disguise(ai.Target.Owner);
                                                    }

                                                }
                                            }

                                        }
                                        break;
                                    case BotType.MatrixAI:
                                        {
                                            foreach (var obj in ai.Bot.Screen.Objects)
                                            {
                                                if (obj.MapObjType == MapObjectType.Player || obj.MapObjType == MapObjectType.Monster)
                                                {
                                                    var entity = obj as Entity;
                                                    if (entity.Dead)
                                                        continue;
                                                    if (entity.UID == ai.Bot.Entity.UID)
                                                        continue;
                                                    if (obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        if (entity.MapID == ai.Bot.Entity.MapID)
                                                        // if (Kernel.GetDistance(ai.Bot.Entity.X, ai.Bot.Entity.Y, obj.X, obj.Y) < 18)
                                                        {
                                                            if (ai.selectFunc(entity.Owner))
                                                            {
                                                                ai.Target = entity;
                                                                if (ai.Disguisefun)
                                                                {
                                                                    ai.Disguise(ai.Target.Owner);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (ai.Target == null)
                                            {
                                                var ais_in_map = GetAiinMap(ai);
                                                if (ais_in_map != null)
                                                {
                                                    ai.Target = ais_in_map.Bot.Entity;
                                                }
                                                else
                                                {
                                                    ai.Bot.Screen.Reload();
                                                }

                                            }
                                        }
                                        break;
                                }
                                if (Now >= ai.LastTalk.AddSeconds(60))
                                {
                                    ai.LastTalk = Time32.Now;
                                    if (ai.Target == null)
                                        ai.Bot.SendScreen(new Message("Idle Mode | No Target!", "ALL", ai.Bot.Entity.Name, System.Drawing.Color.White, Message.Talk), false);
                                    else
                                        ai.Bot.SendScreen(new Message("Found Target! : " + ai.Target.Name, "ALL", ai.Bot.Entity.Name, System.Drawing.Color.White, Message.Talk), false);
                                }

                            }
                            #endregion Check Target
                            else
                            {
                                if (ai.Type == BotType.MatrixAI)
                                {
                                    if (ai.selectFunc != null)
                                    {
                                        if (!ai.selectFunc(ai.Target.Owner))
                                        {
                                            ai.Target = null;
                                            return;
                                        }
                                    }
                                }
                                var Bot = ai.Bot;
                                var Target = ai.Target;
                                if (!Target.Dead)
                                {
                                    #region Death Actions
                                    if (Bot.Entity.Dead)
                                    {
                                        #region Die Delay

                                        if (Bot.Entity.Hitpoints == 0 && Bot.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Dead) && !Bot.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Ghost))
                                        {
                                            if (Now > Bot.Entity.DeathStamp.AddSeconds(2))
                                            {
                                                Bot.Entity.AddFlag(Network.GamePackets.Update.Flags.Ghost);
                                                if (Bot.Entity.Body % 10 < 3)
                                                    Bot.Entity.TransformationID = 99;
                                                else
                                                    Bot.Entity.TransformationID = 98;

                                                Bot.SendScreenSpawn(Bot.Entity, true);
                                            }
                                        }
                                        #endregion
                                        if (Now >= Bot.Entity.DeathStamp.AddSeconds(18))
                                        {
                                            Bot.Entity.BringToLife();
                                            SpellUse use = new SpellUse(true)
                                            {
                                                Attacker = Bot.Entity.UID,
                                                SpellID = 0x44c,
                                                X = Bot.Entity.X,
                                                Y = Bot.Entity.Y
                                            };
                                            Bot.SendScreen(use, true);
                                            Bot.SendScreenSpawn(Bot.Entity, true);
                                            Bot.SendScreen(new Message("Reviving!", Target.Name, Bot.Entity.Name, System.Drawing.Color.White, Message.Talk), false);
                                            return;
                                        }
                                    }
                                    #endregion
                                    else
                                    {
                                        var allspells = Bot.Spells.Values.ToArray();
                                        var rnd = Kernel.Random.Next(Bot.Spells.Count);
                                        var spell = allspells[rnd];
                                        switch (ai.Skill)
                                        {
                                            case BotSkill.FB:
                                                spell = Bot.Spells[1045];
                                                break;
                                            case BotSkill.SS:
                                                spell = Bot.Spells[1046];
                                                break;
                                            case BotSkill.FBSS:
                                                {
                                                    if (Kernel.Rate(50))
                                                        spell = Bot.Spells[1046];
                                                    else
                                                        spell = Bot.Spells[1045];
                                                }
                                                break;
                                        }

                                        if (SpellTable.SpellInformations.ContainsKey(spell.ID))
                                        {
                                            if (!SpellTable.SpellInformations[spell.ID].ContainsKey(spell.Level))
                                                return;
                                        }
                                        else
                                            return;

                                        if (Ais.ContainsKey(Target.UID))
                                        {
                                            if (Kernel.Rate((double)50.0))
                                            {
                                                ai.Bot.Entity.Die(Target);
                                            }
                                            else
                                            {
                                                Target.Die(ai.Bot.Entity);
                                            }
                                        }

                                        #region Stamina Check
                                        if (Bot.Entity.Action == Enums.ConquerAction.Sit)
                                        {
                                            Bot.Entity.Stamina += (byte)Kernel.Random.Next(10);
                                        }
                                        else
                                        {
                                            if (Bot.Entity.Stamina < SpellTable.SpellInformations[spell.ID][spell.Level].UseStamina)
                                            {
                                                Bot.Entity.Action = Enums.ConquerAction.Sit;
                                                MTA.Network.GamePackets.Data buffer = new MTA.Network.GamePackets.Data(true)
                                                {
                                                    UID = Bot.Entity.UID,
                                                    dwParam = Bot.Entity.Action
                                                };
                                                Bot.Entity.SendScreen(buffer);

                                                Bot.SendScreenSpawn(Bot.Entity, true);
                                                Target.Owner.SendScreenSpawn(Bot.Entity, true);
                                                return;
                                            }
                                        }
                                        #endregion Stamina Check
                                        #region Jump
                                        if (Now >= ai.LastBotJump.AddMilliseconds(ai.JumpSpeed))
                                        {                                            
                                            ushort X = Bot.Entity.X;
                                            ushort Y = Bot.Entity.Y;
                                            var dist = Kernel.GetDistance(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y);
                                            int count = (int)Math.Ceiling((double)dist / 16);
                                            var path = Pathfinding.AStar.Calculate.FindWay(Target.X, Target.Y, Bot.Entity.X, Bot.Entity.Y, Bot.Map);
                                            if (path != null)
                                            {

                                                var point = Math.Ceiling((double)path.Count / count);
                                                if (point > 0)
                                                {
                                                    point = Math.Min(path.Count - 1, point);
                                                    X = path[(int)point].X;
                                                    Y = path[(int)point].Y;
                                                }
                                                else
                                                {
                                                    X = path[path.Count - 1].X;
                                                    Y = path[path.Count - 1].Y;
                                                }
                                            }
                                            //  var angel = Kernel.GetAngle(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y);
                                            // Bot.Entity.Move(angel);
                                            Data buffer = new Data(true)
                                            {
                                                ID = MTA.Network.GamePackets.Data.Jump,
                                                dwParam = (uint)((Y << 0x10) | X),
                                                wParam1 = X,
                                                wParam2 = Y,
                                                UID = Bot.Entity.UID
                                            };
                                            Bot.Entity.SendScreen(buffer);
                                            ai.LastBotJump = Time32.Now;
                                        }
                                        #endregion
                                        #region Attack
                                       
                                            
                                            if (Now > client.Entity.AttackStamp.AddSeconds(1))
                                            {
                                                if (MTA.MyMath.Success(ai.ShootChance))
                                                {
                                                    var dist = Kernel.GetDistance(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y);
                                                    var spelldist = SpellTable.SpellInformations[spell.ID][spell.Level].Range;
                                                    if (dist < spelldist)
                                                    {
                                                        var interact = new Attack(true);
                                                        interact.AttackType = Attack.Magic;
                                                        interact.MagicType = spell.ID;
                                                        interact.Attacker = Bot.Entity.UID;
                                                        interact.Attacked = Target.UID;
                                                        interact.MagicLevel = spell.Level;
                                                        interact.Decoded = true;
                                                        if (MTA.MyMath.Success(ai.Accuracy))
                                                        {
                                                            interact.X = Target.X;
                                                            interact.Y = Target.Y;
                                                        }
                                                        else
                                                        {
                                                            interact.X = (ushort)(Target.X + 1);
                                                            interact.Y = (ushort)(Target.Y + 1);
                                                        }
                                                        Bot.Entity.AttackPacket = interact;
                                                        new MTA.Game.Attacking.Handle(interact, Bot.Entity, Target);

                                                    }
                                                    //if (dist < 2)
                                                    //{
                                                    //    var interact = new Attack(true);
                                                    //    interact.AttackType = Attack.Melee;
                                                    //    interact.Attacker = Bot.Entity.UID;
                                                    //    interact.Attacked = Target.UID;
                                                    //    if (Kernel.ChanceSuccess(ai.Accuracy))
                                                    //    {
                                                    //        interact.X = Target.X;
                                                    //        interact.Y = Target.Y;
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        interact.X = (ushort)(Target.X + 1);
                                                    //        interact.Y = (ushort)(Target.Y + 1);
                                                    //    }
                                                    //    Bot.Entity.AttackPacket = interact;
                                                    //    new MTA.Game.Attacking.Handle(interact, Bot.Entity, Target);
                                                    //}
                                                }
                                        }
                                        #endregion

                                    }
                                }
                                else
                                {
                                    if (Bot.Entity.Action != Enums.ConquerAction.Cool && Time32.Now >= Bot.CoolStamp.AddSeconds(10))
                                    {
                                        Bot.Entity.Action = Enums.ConquerAction.Cool;
                                        MTA.Network.GamePackets.Data generalData = new MTA.Network.GamePackets.Data(true);
                                        generalData.UID = Bot.Entity.UID;
                                        generalData.dwParam = Bot.Entity.Action;
                                        generalData.dwParam |= (uint)((Bot.Entity.Class * 0x10000) + 0x1000000);
                                        Bot.Entity.SendScreen(generalData);
                                        Bot.CoolStamp = Time32.Now;

                                        Bot.SendScreenSpawn(Bot.Entity, true);
                                        Target.Owner.SendScreenSpawn(Bot.Entity, true);
                                        Bot.SendScreen(new Message("Die Noob :P , HHHHHHHHHHH!", Target.Name, Bot.Entity.Name, System.Drawing.Color.White, Message.Talk), false);
                                        return;
                                    }
                                    if (ai.Type == BotType.AI || ai.Type == BotType.MatrixAI)
                                    {
                                        ai.Target = null;
                                        return;
                                    }
                                }
                            }
                        }
                        //else
                        else if (client.Entity.MapID != ai.Bot.Entity.MapID)
                        {
                            if (client.AI == ai)
                            {
                                client.AI.Dispose(client);
                            }
                        }
                    }
                }
                if (Ais.Count > 0)
                {
                    List<AI> array = new List<AI>();
                    foreach (var item in Ais.Values)
                    {
                        if (item.Bot == null)
                            array.Add(item);

                    }
                    // var array = Ais.Values.Where(ai => ai.Bot == null).ToArray();
                    if (array != null)
                    {
                        for (int i = 0; i < array.Count; i++)
                        {
                            var ai = array[i];
                            Ais.Remove(ai.UID);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion
        private static AI GetAiinMap(AI ai)
        {
            if (ai.Bot == null)
                return null;
            if (ai.Bot.Entity == null)
                return null;

            var array= Ais.Values.Where(i => i.UID != ai.UID && i.Bot.Entity.MapID == ai.Bot.Entity.MapID).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (ai.selectFunc != null)
                    if (ai.selectFunc(array[i].Bot))
                        return array[i];
            }
            return null;
        }
        #region Timers
        private IDisposable[] TimerSubscriptions;
        private object DisposalSyncRoot;
        ~AI()
        {
            DisposeTimers();
            Target = null;
            Bot = null;
        }
        public void Dispose(GameState client)
        {
            DisposeTimers();
            Bot = null;
            Target = null;
            // Ais.Remove(this);
            Join(client);
            client.Entity.OnDeath = null;
        }
        private void DisposeTimers()
        {
            if (DisposalSyncRoot == null)
                return;
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
        public enum BotSkill
        {
            FB,
            SS,
            FBSS,
            All
        }
        public enum BotType
        {
            MyAi,
            AI,
            MatrixAI
        }
        public enum BotLevel
        {
            Noob = 0,
            Easy = 1,
            Normal = 2,
            Medium = 3,
            Hard = 4,
            Insane = 5,
            MaTrix = 6
        }
        private BotSkill Skill;
        private BotType Type;
        public GameState Bot;
        private Entity Target;
        private int JumpSpeed = 0;
        private int ShootChance = 0;
        private int Accuracy = 0;
        private Time32 LastBotJump;
        private Time32 LastTalk;
        private bool Loaded;
        private Func<GameState, bool> selectFunc;
        private bool Disguisefun;
        public Counter UIDCounter = new MTA.Counter(60000000);
        private uint UID;

        public AI(ushort Map, ushort x, ushort y, BotLevel Level, BotSkill skill = BotSkill.SS)
        {
            Bot = new GameState(null);
            Type = BotType.AI;
            Skill = skill;
            SetLevel(Level);
            LoadAI(Map, x, y);
            do
            {
                Bot.Entity.UID = UIDCounter.Next;
            }
            while (Ais.ContainsKey(Bot.Entity.UID));
            UID = Bot.Entity.UID;
            Ais.Add(Bot.Entity.UID, this);
        }
        public AI(ushort Map, ushort x, ushort y, BotLevel Level, Func<GameState, bool> Func, bool Func2 = true)
        {
            Bot = new GameState(null);
            Type = BotType.MatrixAI;
            Skill = BotSkill.SS;
            selectFunc = Func;
            Disguisefun = Func2;
            SetLevel(Level);
            LoadAI(Map, x, y);
            do
            {
                Bot.Entity.UID = UIDCounter.Next;
            }
            while (Ais.ContainsKey(Bot.Entity.UID));
            Ais.Add(Bot.Entity.UID, this);
            UID = Bot.Entity.UID;
        }
        public AI(GameState client)
        {
            Join(client);
        }
        public AI(GameState client, BotLevel Level, BotSkill skill = BotSkill.SS)
        {
            Target = client.Entity;
            Bot = new GameState(null);
            Type = BotType.MyAi;
            Skill = skill;
            SetLevel(Level);
            LoadAI();
            Join(client);
            do
            {
                Bot.Entity.UID = UIDCounter.Next;
            }
            while (Ais.ContainsKey(Bot.Entity.UID));
            Ais.Add(Bot.Entity.UID, this);
            UID = Bot.Entity.UID;
        }
        public void Join(GameState client)
        {
            TimerSubscriptions = new IDisposable[] 
            {
                AIAction.Add(client)
            };
            DisposalSyncRoot = new object();
        }
        public void SetLevel(BotLevel Level)
        {
            switch (Level)
            {
                case BotLevel.Noob:
                    JumpSpeed = 3000;
                    ShootChance = 10;
                    Accuracy = 5;
                    break;

                case BotLevel.Easy:
                    JumpSpeed = 1500;
                    ShootChance = 25;
                    Accuracy = 10;
                    break;

                case BotLevel.Normal:
                    JumpSpeed = 1250;
                    ShootChance = 33;
                    Accuracy = 20;
                    break;

                case BotLevel.Medium:
                    JumpSpeed = 1000;
                    ShootChance = 50;
                    Accuracy = 33;
                    break;

                case BotLevel.Hard:
                    JumpSpeed = 1000;
                    ShootChance = 75;
                    Accuracy = 50;
                    break;

                case BotLevel.Insane:
                    JumpSpeed = 1000;
                    ShootChance = 90;
                    Accuracy = 80;
                    break;
                case BotLevel.MaTrix:
                    JumpSpeed = 1000;
                    ShootChance = 100;
                    Accuracy = 100;
                    break;
            }
        }
        public void LoadAI(ushort MapID = 0, ushort X = 0, ushort Y = 0)
        {
            Bot.ReadyToPlay();
            Bot.Entity = new Entity(EntityFlag.Player, false);
            Bot.Entity.Owner = Bot;
            Bot.Entity.MapObjType = MapObjectType.Player;
            Bot.Variables = new VariableVault();
            Bot.Friends = new SafeDictionary<uint, Game.ConquerStructures.Society.Friend>();
            Bot.Enemy = new SafeDictionary<uint, Game.ConquerStructures.Society.Enemy>();
            Bot.ChiData = new ChiTable.ChiData();
            Bot.ChiPowers = new List<ChiPowerStructure>();
            Bot.Entity.Vitality = 537;
            if (Target != null)
            {
                Target.OnDeath = p =>
                {
                    p.Owner.MessageBox("Do You Want To Quit?", c => { c.AI.Dispose(c); c.Entity.Teleport(1002, 301, 266); });
                };
            }
            Bot.Entity.OnDeath = p =>
            {
                if (p.Owner.Team != null)
                {
                    if (p.Owner.Team.EliteMatch != null)
                    {
                        p.Owner.Team.EliteMatch.End(p.Owner.Team);
                    }
                }
                p.Owner.SendScreen(new Message("Reviving in 18 seconds!", "ALL", p.Name, System.Drawing.Color.White, Message.Talk), false);
                foreach (var obj in p.Owner.Screen.Objects)
                    if (obj.MapObjType == MapObjectType.Player)
                    {
                        (obj as Entity).Owner.Time(10);
                        if ((obj as Entity).Owner.AI.Bot == p.Owner)
                        {
                            (obj as Entity).Owner.MessageBox("Do You Want To Quit?", c => { c.AI.Dispose(c); c.Entity.Teleport(1002, 301, 266); });
                        }
                    }
            };
            switch (Type)
            {
                case BotType.MyAi:
                    {
                        var client = Target.Owner;
                        Bot.Entity.Name = Target.Name + "[BoT]";
                        Bot.Entity.Face = client.Entity.Face;
                        Bot.Entity.Body = client.Entity.Body;
                        Bot.Entity.HairStyle = client.Entity.HairStyle;
                        Bot.Entity.Level = client.Entity.Level;
                        Bot.Entity.Class = client.Entity.Class;
                        Bot.Entity.Reborn = client.Entity.Reborn;
                        Bot.Entity.Level = client.Entity.Level;
                        Bot.Entity.MapID = client.Entity.MapID;
                        Bot.Entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(5));
                        Bot.Entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(5));
                        Bot.Entity.MinAttack = client.Entity.MinAttack;
                        Bot.Entity.MaxAttack = client.Entity.MagicAttack;
                        uint UID = 70000000;
                        UID += Target.UID;


                        Bot.Entity.MaxHitpoints = client.Entity.MaxHitpoints;
                        Bot.Entity.Hitpoints = Bot.Entity.MaxHitpoints;
                        Bot.Entity.Mana = Bot.Entity.MaxMana;

                        Bot.Entity.Agility = client.Entity.Agility;
                        Bot.Entity.Spirit = client.Entity.Spirit;
                        Bot.Entity.Strength = client.Entity.Strength;
                        Bot.Entity.Vitality = client.Entity.Vitality;

                        Bot.Entity.UID = UID;
                        Bot.Entity.Stamina = 150;
                        Bot.Equipment.ForceEquipments(Target.Owner.Equipment);

                        Bot.ChiData = client.ChiData;
                        Bot.ChiPowers = client.ChiPowers;

                        Bot.LoadItemStats();
                        Bot.Equipment.UpdateEntityPacket();
                        ClientEquip equips = new ClientEquip();
                        equips.DoEquips(Bot);
                        Bot.Send(equips);

                        Bot.Spells = client.Spells;
                        Bot.Proficiencies = client.Proficiencies;
                    }
                    break;
                case BotType.MatrixAI:
                case BotType.AI:
                    {
                        Bot.Entity.Name = "Lucky[" + Kernel.Random.Next(20) + "][BoT]";
                        Bot.Entity.Face = 37;
                        Bot.Entity.Body = 1003;
                        Bot.Entity.HairStyle = 630;
                        Bot.Entity.Level = 140;
                        Bot.Entity.Class = 15;
                        Bot.Entity.Reborn = 2;
                        Bot.Entity.MapID = MapID;
                        Bot.Entity.X = (ushort)(X + Kernel.Random.Next(5));
                        Bot.Entity.Y = (ushort)(Y + Kernel.Random.Next(5));
                        uint UID = UIDCounter.Next;

                        Bot.Entity.MaxHitpoints = 20000;
                        Bot.Entity.Hitpoints = Bot.Entity.MaxHitpoints;
                        Bot.Entity.Mana = 800;

                        Bot.Entity.UID = UID;
                        Bot.Entity.Stamina = 150;

                        Bot.Spells = new SafeDictionary<ushort, Interfaces.ISkill>();
                        Bot.Proficiencies = new SafeDictionary<ushort, Interfaces.IProf>();
                    }
                    break;
            }
            if (Skill == BotSkill.FBSS || Skill == BotSkill.FB || Skill == BotSkill.SS)
            {
                if (!Bot.Proficiencies.ContainsKey(410))
                    Bot.AddProficiency(new Proficiency(true) { ID = 410, Level = 20 });
                if (!Bot.Proficiencies.ContainsKey(420))
                    Bot.AddProficiency(new Proficiency(true) { ID = 420, Level = 20 });
                if (!Bot.Spells.ContainsKey(1045))
                    Bot.AddSpell(Npcs.LearnableSpell(1045, 4));
                if (!Bot.Spells.ContainsKey(1046))
                    Bot.AddSpell(Npcs.LearnableSpell(1046, 4));
                var weapons = Bot.Weapons;
                if (weapons.Item1 != null)
                {
                    if (weapons.Item1.ID / 1000 != 410 && weapons.Item1.ID / 1000 != 420)
                    {
                        if (weapons.Item2 != null)
                        {
                            if (weapons.Item2.ID / 1000 != 410 && weapons.Item2.ID / 1000 != 420)
                            {
                                weapons.Item1.ID = 420439;
                            }
                        }
                    }
                }
                else
                {
                    if (Bot.Equipment == null) Bot.Equipment = new Equipment(Bot);
                    Bot.Equipment.Add(new ConquerItem(true) { ID = 420439, Plus = 12, Position = 4 });
                }
            }
            this.LastBotJump = Time32.Now;
            Loaded = true;
        }
        public void Disguise(GameState client)
        {
            Bot.Entity.Face = client.Entity.Face;
            Bot.Entity.Body = client.Entity.Body;
            Bot.Entity.HairStyle = client.Entity.HairStyle;
            Bot.Entity.Level = client.Entity.Level;
            Bot.Entity.Class = client.Entity.Class;
            Bot.Entity.Reborn = client.Entity.Reborn;
            Bot.Entity.Level = client.Entity.Level;
            Bot.Entity.MapID = client.Entity.MapID;
            Bot.Entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(5));
            Bot.Entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(5));
            Bot.Entity.MinAttack = client.Entity.MinAttack;
            Bot.Entity.MaxAttack = client.Entity.MagicAttack;

            Bot.Entity.MaxHitpoints = client.Entity.MaxHitpoints;
            Bot.Entity.Hitpoints = Bot.Entity.MaxHitpoints;
            Bot.Entity.Mana = Bot.Entity.MaxMana;

            Bot.Entity.Agility = client.Entity.Agility;
            Bot.Entity.Spirit = client.Entity.Spirit;
            Bot.Entity.Strength = client.Entity.Strength;
            Bot.Entity.Vitality = client.Entity.Vitality;


            Bot.Entity.Stamina = 150;
            Bot.Equipment.ForceEquipments(client.Equipment);

            if (client.ChiData != null)
                Bot.ChiData = client.ChiData;
            Bot.ChiPowers = client.ChiPowers;
            Bot.Entity.MyJiang = client.Entity.MyJiang;
            Bot.Entity.SubClasses = client.Entity.SubClasses;

            Bot.LoadItemStats();

            Bot.Equipment.UpdateEntityPacket();
            ClientEquip equips = new ClientEquip();
            equips.DoEquips(Bot);
            Bot.Send(equips);

            Bot.Spells = client.Spells;
            Bot.Proficiencies = client.Proficiencies;
            if (Skill == BotSkill.FBSS || Skill == BotSkill.FB || Skill == BotSkill.SS)
            {
                if (!Bot.Proficiencies.ContainsKey(410))
                    Bot.AddProficiency(new Proficiency(true) { ID = 410, Level = 20 });
                if (!Bot.Proficiencies.ContainsKey(420))
                    Bot.AddProficiency(new Proficiency(true) { ID = 420, Level = 20 });
                if (!Bot.Spells.ContainsKey(1045))
                    Bot.AddSpell(Npcs.LearnableSpell(1045, 4));
                if (!Bot.Spells.ContainsKey(1046))
                    Bot.AddSpell(Npcs.LearnableSpell(1046, 4));
                var weapons = Bot.Weapons;
                if (weapons.Item1 != null)
                {
                    if (weapons.Item1.ID / 1000 != 410 && weapons.Item1.ID / 1000 != 420)
                    {
                        if (weapons.Item2 != null)
                        {
                            if (weapons.Item2.ID / 1000 != 410 && weapons.Item2.ID / 1000 != 420)
                            {
                                weapons.Item1.ID = 420439;
                            }
                        }
                    }
                }
            }
            this.LastBotJump = Time32.Now;
            Loaded = true;
            Bot.SendScreenSpawn(Bot.Entity, true);
        }

        public static void CheckScreen(GameState Owner, Interfaces.IPacket spawnWith = null)
        {
            // if (Owner.Fake) return;
            foreach (var ai in MaTrix.AI.Ais.Values)
            {
                if (ai.Bot == null)
                    continue;
                if (ai.Bot.Entity == null)
                    continue;

                if (Owner.Entity.MapID == ai.Bot.Entity.MapID)
                {
                    var pClient = ai.Bot;
                    short dist = Kernel.GetDistance(ai.Bot.Entity.X, ai.Bot.Entity.Y, Owner.Entity.X, Owner.Entity.Y);
                    if (dist <= Constants.pScreenDistance)
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
                    else if (Owner.Fake)
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
        }
    }
    public class Lobby
    {
        public enum MatchType
        {
            AllSkills,
            FBSS
        }
        public static uint cpsFee = 100000;
        public static INpc Npc;
        public static Map Map;
        public Lobby()
        {
            Map = Kernel.Maps[700].MakeDynamicMap();
            Npc = new NpcSpawn();
            Npc.UID = 3;
            Npc.X = 50;
            Npc.Y = 50;
            Npc.MapID = Map.ID;
            Npc.Mesh = 20880;
            Npc.Type = (Enums.NpcType)32;
            Npc.Name = "Lobby";
            Map.AddNpc(Npc);
            World.Subscribe(Functions, 1000);
        }

        private void Functions(int time)
        {
            EngagePlayers();
            CheckGroups();
        }
        public static void EngagePlayers()
        {
            var Players = Program.Values.Where(c => c.LobbySignup == true && c.Entity.MapID == Map.ID).ToArray();
            if (Players.Length < 2)
                return;
            int i, j;
            int iPlus, jPlus;
            int iEnd, jEnd;
            iPlus = Kernel.Random.Next(2);
            jPlus = Kernel.Random.Next(2);
            if (iPlus == 0) { i = 0; iPlus = 1; iEnd = Players.Length; } else { i = Players.Length - 1; iPlus = -1; iEnd = -1; }
            if (jPlus == 0) { j = 0; jPlus = 1; jEnd = Players.Length; } else { j = Players.Length - 1; jPlus = -1; jEnd = -1; }
            Time32 Now = Time32.Now;
            for (; i != iEnd; i += iPlus)
            {
                var Challanger = Players[i];

                if (Challanger.Entity.MapID == Map.ID && Challanger.LobbySignup == true)
                {
                    for (; j != jEnd; j += jPlus)
                    {
                        var Challanged = Players[j];
                        if (Challanged.Entity.MapID == Map.ID && Challanged.LobbySignup == true)
                        {
                            if (Challanger.MatchType == Challanged.MatchType)
                            {
                                if (Challanger.MatchType == MatchType.FBSS)
                                {
                                    var weapons = Challanger.Weapons;
                                    if (weapons.Item1 != null)
                                    {
                                        if (!SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(weapons.Item1.ID / 1000)) && !SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(Challanger.WeaponLook / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(weapons.Item1.ID / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(Challanger.WeaponLook / 1000)))
                                        {
                                            if (weapons.Item2 != null)
                                            {
                                                if (!SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(weapons.Item2.ID / 1000)) && !SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(Challanger.WeaponLook2 / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(weapons.Item2.ID / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(Challanger.WeaponLook2 / 1000)))
                                                {
                                                    Challanger.MessageBox("You Had to Wear Blade Or Sword.");
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Challanger.MessageBox("You Had to Wear Blade Or Sword.");
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Challanger.MessageBox("You Had to Wear Blade Or Sword.");
                                        return;
                                    }
                                }
                                if (Challanged.MatchType == MatchType.FBSS)
                                {
                                    var weapons = Challanged.Weapons;
                                    if (weapons.Item1 != null)
                                    {
                                        if (!SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(weapons.Item1.ID / 1000)) && !SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(Challanged.WeaponLook / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(weapons.Item1.ID / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(Challanged.WeaponLook / 1000)))
                                        {
                                            if (weapons.Item2 != null)
                                            {
                                                if (!SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(weapons.Item2.ID / 1000)) && !SpellTable.SpellInformations[1045][0].WeaponSubtype.Contains((ushort)(Challanged.WeaponLook2 / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(weapons.Item2.ID / 1000)) && !SpellTable.SpellInformations[1046][0].WeaponSubtype.Contains((ushort)(Challanged.WeaponLook2 / 1000)))
                                                {
                                                    Challanged.MessageBox("You Had to Wear Blade Or Sword.");
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Challanged.MessageBox("You Had to Wear Blade Or Sword.");
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Challanged.MessageBox("You Had to Wear Blade Or Sword.");
                                        return;
                                    }
                                }
                                if (Challanger.Entity.UID != Challanged.Entity.UID)
                                {
                                    if (Challanger.LobbyGroup == null && Challanged.LobbyGroup == null)
                                    {
                                        Challanger.MessageBox("You Will Enter The  Fight soon. Wanna Quit?", p =>
                                        {
                                            if (p.LobbyGroup != null)
                                            {
                                                p.LobbyGroup.End(p);
                                                var winner = p.LobbyGroup.Winner;
                                                var loser = p.LobbyGroup.Loser;
                                                p.LobbyGroup.Export();
                                                Win(winner, loser);
                                            }
                                        });
                                        Challanged.MessageBox("You Will Enter The  Fight soon. Wanna Quit?", p =>
                                        {
                                            if (p.LobbyGroup != null)
                                            {
                                                p.LobbyGroup.End(p);
                                                var winner = p.LobbyGroup.Winner;
                                                var loser = p.LobbyGroup.Loser;
                                                p.LobbyGroup.Export();
                                                Win(winner, loser);
                                            }
                                        });

                                        MaTrix.Lobby.QualifierGroup group = new MaTrix.Lobby.QualifierGroup(Challanger, Challanged);
                                        Program.World.DelayedTask.StartDelayedTask(() =>
                                        {
                                            if (Challanger.LobbyGroup == null || Challanged.LobbyGroup == null)
                                                return;
                                            if (!Challanger.LobbySignup || !Challanged.LobbySignup)
                                                return;
                                            if (group.Done)
                                                return;
                                            group.Import();

                                        }, 6000);
                                    }
                                    else
                                    {
                                        if (Challanger.LobbyGroup != null)
                                        {
                                            if (Challanger.LobbyGroup.Done)
                                            {
                                                Challanger.LobbyGroup.Export();
                                            }
                                        }
                                        if (Challanged.LobbyGroup != null)
                                        {
                                            if (Challanged.LobbyGroup.Done)
                                            {
                                                Challanger.LobbyGroup.Export();
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

        public static void CheckGroups()
        {
            if (Groups.Count > 0)
            {
                foreach (var group in Groups.Values)
                {
                    if (Time32.Now > group.CreateTime.AddSeconds(5))
                    {
                        if (!group.Done)
                        {
                            if (Time32.Now > group.CreateTime.AddMinutes(3))
                            {
                                group.End();
                            }
                        }
                        else
                        {
                            if (Time32.Now > group.DoneStamp.AddSeconds(4))
                            {
                                group.DoneStamp = Time32.Now.AddDays(1);
                                group.Export();
                                Win(group.Winner, group.Loser);
                            }
                        }
                    }
                }
            }
        }

        public static void Win(Client.GameState winner, Client.GameState loser)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(winner.Entity.Name);
            builder.Append(" has defeated ");
            builder.Append(loser.Entity.Name);
            builder.Append(" in the Lobby Challange");
            builder.Append(".");

            Kernel.SendWorldMessage(new Message(builder.ToString(), System.Drawing.Color.Red, Message.ArenaQualifier), Program.Values, Map.ID);

            winner.LobbyGroup = null;
            loser.LobbyGroup = null;

            if (winner.LobbyPlayWith == null)
                winner.Entity.ConquerPoints += cpsFee;

            loser.LobbySignup = false;

            QualifierGroup.ArenaSignup sign = new QualifierGroup.ArenaSignup();
            sign.Stats = loser.Entity;
            sign.DialogID = QualifierGroup.ArenaSignup.MainIDs.Dialog2;
            loser.Send(sign.BuildPacket());
            sign.Stats = winner.Entity;
            sign.OptionID = QualifierGroup.ArenaSignup.DialogButton.Win;
            winner.Send(sign.BuildPacket());

            loser.Send(loser.ArenaStatistic);
            winner.Send(winner.ArenaStatistic);

            if (winner.LobbyPlayWith != null)
                winner.LobbyPlayWith = null;
            if (loser.LobbyPlayWith != null)
                loser.LobbyPlayWith = null;

        }

        public static ConcurrentDictionary<uint, GameState> WaitingPlayerList = new ConcurrentDictionary<uint, GameState>();
        public static ConcurrentDictionary<uint, QualifierGroup> Groups = new ConcurrentDictionary<uint, QualifierGroup>();
        public static Counter GroupCounter = new MTA.Counter();
        public class QualifierGroup
        {
            public class GroupMatch
            {
                public ushort Type = 2210;
                public QualifierGroup Group;
                public byte[] BuildPacket()
                {
                    MemoryStream strm = new MemoryStream();
                    BinaryWriter wtr = new BinaryWriter(strm);
                    wtr.Write((ushort)56);
                    wtr.Write((ushort)Type);
                    wtr.Write((uint)Group.Player1.Entity.UID);
                    byte[] array = Encoding.Default.GetBytes(Group.Player1.Entity.Name);
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < Group.Player1.Entity.Name.Length)
                        {
                            wtr.Write(array[i]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)Group.Player1Damage);
                    wtr.Write((uint)Group.Player2.Entity.UID);
                    byte[] array2 = Encoding.Default.GetBytes(Group.Player2.Entity.Name);
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < Group.Player2.Entity.Name.Length)
                        {
                            wtr.Write(array2[i]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)Group.Player2Damage);
                    wtr.Write((uint)1);
                    wtr.Write(Encoding.Default.GetBytes("TQServer"));
                    strm.Position = 0;
                    byte[] buf = new byte[strm.Length];
                    strm.Read(buf, 0, buf.Length);
                    wtr.Close();
                    strm.Close();
                    return buf;
                }
            }
            public Time32 CreateTime;
            public Time32 DoneStamp;

            public uint Player1Damage, Player2Damage;
            public uint Player1Cheers, Player2Cheers;

            public bool Done;

            private Game.Enums.PkMode P1Mode, P2Mode;

            public uint ID;

            public GroupMatch match = new GroupMatch();

            public Client.GameState Winner, Loser;
            public Client.GameState Player1, Player2;

            public Map dynamicMap;
            public Time32 ImportTime;
            public MatchType MatchType = MatchType.AllSkills;
            public QualifierGroup(Client.GameState player1, Client.GameState player2, MatchType matchtype = MatchType.AllSkills)
            {
                Player1 = player1;
                Player2 = player2;
                CreateTime = Time32.Now;
                Player1Damage = 0;
                Player2Damage = 0;
                Done = false;
                ID = GroupCounter.Next;
                match.Group = this;
                Done = false;
                MatchType = matchtype;
                Groups.Add(ID, this);

                Player1.LobbyGroup = this;
                Player2.LobbyGroup = this;
            }

            public GameState OppositeClient(GameState client)
            {
                if (client == Player1)
                    return Player2;
                else
                    return Player1;
            }

            public void Import()
            {
                //Player1.LobbyGroup = this;
                //Player2.LobbyGroup = this;

                if (!Kernel.Maps.ContainsKey(700))
                    new Map(700, Database.DMaps.MapPaths[700]);
                Map origMap = Kernel.Maps[700];
                dynamicMap = origMap.MakeDynamicMap();
                Player1.Entity.Teleport(origMap.ID, dynamicMap.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                Player2.Entity.Teleport(origMap.ID, dynamicMap.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                ImportTime = Time32.Now;
                if (Player1.Map.ID == Player2.Map.ID)
                {
                    Player1.Send(match.BuildPacket());
                    Player2.Send(match.BuildPacket());
                    Player1.Entity.BringToLife();
                    Player2.Entity.BringToLife();
                    if (Player1.Entity.ContainsFlag(Update.Flags.Ride))
                        Player1.Entity.RemoveFlag(Update.Flags.Ride);
                    if (Player2.Entity.ContainsFlag(Update.Flags.Ride))
                        Player2.Entity.RemoveFlag(Update.Flags.Ride);
                    Player1.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                    Player2.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                    P1Mode = Player1.Entity.PKMode;
                    Player1.Entity.PKMode = MTA.Game.Enums.PkMode.PK;
                    Player1.Send(new Data(true) { UID = Player1.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player1.Entity.PKMode });
                    P2Mode = Player2.Entity.PKMode;
                    Player2.Entity.PKMode = MTA.Game.Enums.PkMode.PK;
                    Player2.Send(new Data(true) { UID = Player2.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player2.Entity.PKMode });
                    Player1.Screen.FullWipe();
                    Player1.Screen.Reload();
                    Player2.Screen.FullWipe();
                    Player2.Screen.Reload();
                }
                else
                    End();
            }

            public void Export()
            {
                Groups.Remove(ID);

                if (dynamicMap != null)
                    dynamicMap.Dispose();


                Player1.Entity.Teleport(Lobby.Map.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                Player2.Entity.Teleport(Lobby.Map.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));


                Player1.Entity.Ressurect();
                Player2.Entity.Ressurect();

                Player1.Entity.PKMode = P1Mode;
                Player1.Send(new Data(true) { UID = Player1.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player1.Entity.PKMode });
                Player2.Entity.PKMode = P2Mode;
                Player2.Send(new Data(true) { UID = Player2.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player2.Entity.PKMode });
                Player1.LobbyGroup = null;
                Player2.LobbyGroup = null;

                Player1.Entity.ToxicFogLeft = 0;
                Player1.Entity.NoDrugsTime = 1 - 0;
                Player1.Entity.RemoveFlag(Update.Flags.Poisoned);
                Player2.Entity.ToxicFogLeft = 0;
                Player2.Entity.NoDrugsTime = 1 - 0;
                Player2.Entity.RemoveFlag(Update.Flags.Poisoned);

                Player1.endarena = false;
                Player2.endarena = false;
            }

            public void End()
            {
                if (Done) return;
                Player1.endarena = true;
                Player2.endarena = true;
                if (Player1Damage > Player2Damage)
                {
                    Winner = Player1;
                    Loser = Player2;
                }
                else
                {
                    Winner = Player2;
                    Loser = Player1;
                }
                var sign = new ArenaSignup();
                sign.Stats = Loser.Entity;
                sign.DialogID = ArenaSignup.MainIDs.Dialog;
                sign.OptionID = ArenaSignup.DialogButton.Lose;
                Loser.Send(sign.BuildPacket());
                sign.OptionID = ArenaSignup.DialogButton.Win;
                sign.Stats = Winner.Entity;
                Winner.Send(sign.BuildPacket());
                Done = true;
                DoneStamp = Time32.Now;
            }

            public void End(GameState loser)
            {
                if (Done) return;
                Player1.endarena = true;
                Player2.endarena = true;
                if (Player1.Entity.UID == loser.Entity.UID)
                {
                    Winner = Player2;
                    Loser = Player1;
                }
                else
                {
                    Winner = Player1;
                    Loser = Player2;
                }
                var sign = new ArenaSignup();
                sign.Stats = Loser.Entity;
                sign.DialogID = ArenaSignup.MainIDs.Dialog;
                sign.OptionID = ArenaSignup.DialogButton.Lose;
                Loser.Send(sign.BuildPacket());
                sign.OptionID = ArenaSignup.DialogButton.Win;
                sign.Stats = Winner.Entity;
                Winner.Send(sign.BuildPacket());
                Done = true;
                DoneStamp = Time32.Now;
            }
            public class ArenaSignup
            {
                public abstract class MainIDs
                {
                    public const uint ArenaIconOn = 0,
                                        ArenaIconOff = 1,
                                        StartCountDown = 2,
                                        OpponentGaveUp = 4,
                                        Match = 6,
                                        YouAreKicked = 7,
                                        StartTheFight = 8,
                                        Dialog = 9,
                                        Dialog2 = 10;
                }
                public abstract class DialogButton
                {
                    public const uint Lose = 3,
                                        Win = 1,
                                        MatchOff = 3,
                                        MatchOn = 5;
                }


                public ushort Type = 2205;
                public uint DialogID;
                public uint OptionID;
                public Entity Stats;
                public byte[] BuildPacket()
                {
                    MemoryStream strm = new MemoryStream();
                    BinaryWriter wtr = new BinaryWriter(strm);
                    wtr.Write((ushort)0);
                    wtr.Write((ushort)Type);
                    wtr.Write((uint)DialogID);
                    wtr.Write((uint)OptionID);
                    wtr.Write((uint)Stats.UID);
                    byte[] array = Encoding.Default.GetBytes(Stats.Name);
                    for (int i = 0; i < 20; i++)
                    {
                        if (i < Stats.Name.Length)
                        {
                            wtr.Write(array[i]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)Stats.Class);
                    wtr.Write((uint)1);
                    wtr.Write((uint)1000);
                    wtr.Write((uint)Stats.Level);
                    int packetlength = (int)strm.Length;
                    strm.Position = 0;
                    wtr.Write((ushort)packetlength);
                    strm.Position = strm.Length;
                    wtr.Write(Encoding.Default.GetBytes("TQServer"));
                    strm.Position = 0;
                    byte[] buf = new byte[strm.Length];
                    strm.Read(buf, 0, buf.Length);
                    wtr.Close();
                    strm.Close();
                    return buf;
                }
            }
            public void UpdateDamage(GameState client, uint damage)
            {
                if (client != null && Player1 != null)
                {
                    if (client.Entity.UID == Player1.Entity.UID)
                    {
                        Player1Damage += damage;
                    }
                    else
                    {
                        Player2Damage += damage;
                    }
                    Player1.Send(match.BuildPacket());
                    Player2.Send(match.BuildPacket());
                }
            }
        }
    }
}
