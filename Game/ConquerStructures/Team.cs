using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using MTA.Game.Features.Tournaments;
using MTA.Network.GamePackets;
using MTA.Game.Attacking;

namespace MTA.Game.ConquerStructures
{
    public class Team
    {
        public class Member : IDisposable
        {
            public Client.GameState entry;
            public bool Lider = false;
            public byte[] Create()
            {
                Network.GamePackets.AddToTeam temate = new Network.GamePackets.AddToTeam();
                temate.UID = entry.Entity.UID;
                temate.Hitpoints = (ushort)entry.Entity.Hitpoints;
                temate.MaxHitpoints = (ushort)entry.Entity.MaxHitpoints;
                temate.Mesh = entry.Entity.Mesh;
                temate.Name = entry.Entity.Name;

                return temate.ToArray();
            }
            public void Dispose()
            {
                GC.SuppressFinalize(this);
                entry.Team = null;
            }
        }
        public bool OnPopulates
        {
            get { return Temates.Length > 0; }
        }

        public bool RightTeamElite()
        {
            SearchForLowest();
            int highestLvl = 0;
            foreach (var teamMate in Teammates)
                if (teamMate.Entity.Level > highestLvl)
                    highestLvl = teamMate.Entity.Level;
            if (highestLvl >= 130 && LowestLevel >= 130) // 130 team
                return true;
            if (LowestLevel >= 120 && highestLvl <= 129) // 120-129 team
                return true;
            if (LowestLevel >= 100 && highestLvl <= 119)// 100-119 team
                return true;
            if (highestLvl < 99) //99 team
                return true;
            return false;
        }

        public byte GetEliteGroup(byte p)
        {
            if (p <= 99)
                return 0;
            if (p >= 100 && p <= 119)
                return 1;
            if (p >= 120 && p <= 130)
                return 2;
            return 3;
        }
        public TeamElitePk.FighterStats EliteFighterStats;
        public TeamElitePk.Match EliteMatch;
        public void SetEliteFighterStats(bool Create)
        {
            if (Create)
                EliteFighterStats = new TeamElitePk.FighterStats(Lider.Entity.UID, Lider.Entity.Name + "`s team", Lider.Entity.Mesh, this);
            else
            {
                if (EliteFighterStats != null)
                {
                    EliteFighterStats.LeaderUID = Lider.Entity.UID;
                    EliteFighterStats.LeaderMesh = Lider.Entity.Mesh;
                    EliteFighterStats.Name = Lider.Entity.Name + "`s team";
                }
            }
        }

        public byte LowestLevel;
        public uint LowestLevelsUID;

        public bool ForbidJoin = false;
        public bool PickupMoney = true;
        public bool PickupItems = false;
        public bool AutoInvite = false;
        public bool TeamLeader = false;
        public uint UID;

        public Client.GameState Lider;
        public Dictionary<uint, Client.GameState> m_Team;
        private ConcurrentDictionary<uint, Member> Members;

        public bool TeamLider(Client.GameState client) { return client.Entity.UID == Lider.Entity.UID; }

        public static Counter TeamCounter = new Counter(1);

        public Member[] Temates { get { return Members.Values.ToArray(); } }
        public Client.GameState[] Teammates
        {
            get
            {
                return Players.ToArray();
            }
        }
        public void GetClanShareBp(Client.GameState Target)
        {
            if (Lider == null) return;
            var LeaderClan = Lider.Entity.GetClan;
            var TargetClan = Target.Entity.GetClan;
            if (LeaderClan != null && TargetClan != null)
            {
                if (Target.Team == null)
                {
                    Target.Entity.ClanSharedBp = 0;
                    TargetClan.SendClanShareBp(0, 0, Target);
                    return;
                }
                if (Lider.Entity.MapID != Target.Entity.MapID)
                {
                    Target.Entity.ClanSharedBp = 0;
                    TargetClan.SendClanShareBp(0, 0, Target);
                    return;
                }
                if (LeaderClan.ID == TargetClan.ID)
                {
                    if (Lider.Entity.ClanRank != Clan.Ranks.ClanLeader)
                    {
                        Target.Entity.ClanSharedBp = 0;
                        TargetClan.SendClanShareBp(0, 0, Target);
                        return;
                    }
                    if (Lider.Entity.BattlePower > Target.Entity.BattlePower)
                    {
                        uint Bp = (uint)(Lider.Entity.BattlePower - Target.Entity.BattlePower);
                        Bp = (uint)Math.Ceiling((double)((Bp * ProcentClanSharedBp(LeaderClan.BPTower)) / 100));
                        Target.Entity.ClanSharedBp = Bp;
                        TargetClan.SendClanShareBp(Lider.Entity.UID, Bp, Target);
                    }
                    else
                    {
                        Target.Entity.ClanSharedBp = 0;
                        TargetClan.SendClanShareBp(0, 0, Target);
                    }
                }
                else
                {
                    Target.Entity.ClanSharedBp = 0;
                    TargetClan.SendClanShareBp(0, 0, Target);
                }
            }
        }
        public uint ProcentClanSharedBp(uint Bp)
        {
            if (Bp == 1)
                return 40;
            if (Bp == 2)
                return 50;
            if (Bp == 3)
                return 60;
            if (Bp == 4)
                return 70;
            return 30;
        }


        public IEnumerable<Client.GameState> Players
        {
            get
            {
                foreach (var mem in Members.Values)
                    yield return mem.entry;
            }
        }
        public bool Alive
        {
            get
            {

                bool alive = false;
                foreach (var player in Players)
                    if (player.Entity.Hitpoints > 0)
                        alive = true;

                return alive;
            }
        }
        public bool Contain(uint UID)
        {
            foreach (Member memb in Temates)
                if (UID == memb.entry.Entity.UID)
                    return true;

            return false;
        }

        public Team(Client.GameState owner)
        {
            UID = TeamCounter.Next;
            Lider = owner;
            Members = new ConcurrentDictionary<uint, Member>();
            Member member = new Member();
            member.entry = owner;
            Members.TryAdd(owner.Entity.UID, member);
            AddLider();
            TeamLeader = true;
        }
        public bool TryGetMember(uint UID, out Client.GameState client)
        {
            Member memb = null;
            if (!Members.TryGetValue(UID, out memb))
            {
                client = null;
                return false;
            }
            client = memb.entry;
            return true;
        }
        public void Add(Client.GameState client)
        {
            if (AllowADD())
            {
                if (LowestLevel == 0)
                {
                    LowestLevel = client.Entity.Level;
                    LowestLevelsUID = client.Entity.UID;
                }
                else
                {
                    if (client.Entity.Level < LowestLevel)
                    {
                        LowestLevel = client.Entity.Level;
                        LowestLevelsUID = client.Entity.UID;
                    }
                }
                Member member = new Member();
                member.entry = client;
                member.entry.Team = this;
                Members.TryAdd(client.Entity.UID, member);
                Network.GamePackets.Leadership lider = new Network.GamePackets.Leadership();
                lider.Type = Network.GamePackets.Leadership.Teammate;
                client.Send(lider.ToArray());

                lider.Type = Network.GamePackets.Leadership.Leader;
                lider.UID = client.Entity.UID;
                lider.LeaderUID = Lider.Entity.UID;
                lider.Count = (ushort)(Members.Count);
                client.Send(lider.ToArray());

                lider.UID = lider.LeaderUID = Lider.Entity.UID;
                Lider.Send(lider.ToArray());
                client.Send(lider.ToArray());

                Network.GamePackets.AddToTeam addme = new Network.GamePackets.AddToTeam();

                addme.UID = client.Entity.UID;
                addme.Hitpoints = (ushort)client.Entity.Hitpoints;
                addme.Mesh = client.Entity.Mesh;
                addme.Name = client.Entity.Name;
                addme.MaxHitpoints = (ushort)client.Entity.MaxHitpoints;
                foreach (var noob in Temates)
                {
                    noob.entry.Send(addme.ToArray());
                    client.Send(noob.Create());
                }

                GetClanShareBp(client);
            }
        }
        public void Remove(Client.GameState client, bool mode = true)
        {
            Member memb = null;
            if (Members.TryGetValue(client.Entity.UID, out memb))
            {
                if (LowestLevelsUID == UID)
                {
                    LowestLevelsUID = 0;
                    LowestLevel = 0;
                    SearchForLowest();
                }
                foreach (var noob in Temates)
                {
                    Network.GamePackets.Team tem = new Network.GamePackets.Team();

                    tem.UID = memb.entry.Entity.UID;
                    if (mode)
                        tem.Action = Network.GamePackets.Team.Mode.ExitTeam;
                    else
                        tem.Action = Network.GamePackets.Team.Mode.Kick;
                    noob.entry.Send(tem.ToArray());
                }

                Members.TryRemove(client.Entity.UID, out memb);

                Network.GamePackets.Leadership lider = new Network.GamePackets.Leadership();

                lider.UID = memb.entry.Entity.UID;
                lider.Count = (ushort)Members.Count;
                lider.Type = Network.GamePackets.Leadership.Leader;
                memb.entry.Send(lider.ToArray());

                if (memb.Lider)
                {
                    memb.entry.Entity.RemoveFlag(Network.GamePackets.Update.Flags.TeamLeader);
                    AddLider();
                }

                memb.Dispose();

            }
        }
        public void AddLider()
        {
            if (Temates.Length >= 1)
            {
                var mem = Temates[0];
                if (mem.entry != null)
                {

                    Lider = mem.entry;
                    mem.Lider = true;
                    mem.entry.Entity.AddFlag(Network.GamePackets.Update.Flags.TeamLeader);

                    Network.GamePackets.Leadership lider = new Network.GamePackets.Leadership();

                    lider.UID = lider.LeaderUID = mem.entry.Entity.UID;
                    lider.Count = (ushort)Members.Count;
                    lider.Type = Network.GamePackets.Leadership.Leader;
                    foreach (var amem in Temates)
                    {
                        if (amem.entry != null)
                        {
                            amem.entry.Send(amem.Create());
                            amem.entry.Send(lider.ToArray());
                        }
                    }


                    var pMembers = Members.Values.ToArray();
                    foreach (var remover_members in pMembers)
                    {
                        if (remover_members.entry.Entity.UID != mem.entry.Entity.UID)
                            Remove(remover_members.entry, true);
                    }

                    foreach (var add_members in pMembers)
                        Add(add_members.entry);

                    SetEliteFighterStats(false);
                }
            }
            else
                TeamLeader = false;
        }
        public bool SpouseWarFull
        {
            get
            {
                if (Teammates != null)
                {
                    if (Teammates.Length != 2) return false;
                    if (Teammates[0].Entity.Spouse == Teammates[1].Entity.Name)
                        return true;
                }
                return false;
            }
        }
        public void SearchForLowest()
        {
            foreach (Client.GameState client in Players)
            {
                if (LowestLevel == 0)
                {
                    LowestLevel = client.Entity.Level;
                    LowestLevelsUID = client.Entity.UID;
                }
                else
                {
                    if (client.Entity.Level < LowestLevel)
                    {
                        LowestLevel = client.Entity.Level;
                        LowestLevelsUID = client.Entity.UID;
                    }
                }
            }
        }
        public bool CanGetNoobExperience(Client.GameState Teammate)
        {
            return Teammate.Entity.Level > LowestLevel && LowestLevel < 70;
        }
        public bool IsTeammate(uint UID)
        {
            return Members.ContainsKey(UID);
        }
        public void SendTeam(byte[] packet, bool me)
        {
            foreach (Member memb in Temates)
            {
                if (me && TeamLider(memb.entry))
                    continue;
                memb.entry.Send(packet);
            }
        }
        public void SendMesageTeam(byte[] packet, uint mee)
        {
            foreach (Member memb in Temates)
            {
                if (memb.entry.Entity.UID == mee)
                    continue;
                memb.entry.Send(packet);
            }
        }
        public void SendMessage(byte[] data)
        {
            foreach (var teammate in Temates)
                teammate.entry.Send(data);
        }
        public bool AllowADD()
        {
            return Members.Count < 5;
        }
        public void SendMessage(Interfaces.IPacket message)
        {
            foreach (var teammate in Teammates)
                teammate.Send(message);
        }
    }
}

