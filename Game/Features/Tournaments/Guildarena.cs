using System;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Models;
using MTA.Game.Constants;
using MTA.Game.Features.Guilds.Constants;
using MTA.Network.GamePackets;

namespace MTA.Game {
    public class Guildarena {
        public static ushort t1X = (ushort)Kernel.Random.Next(35, 70), t1Y = (ushort)Kernel.Random.Next(35, 70);
        public Map dynamicMap;
        public Guild Guild1;
        public Guild Guild2;
        public GameState Guildleader1;
        public GameState Guildleader2;
        public uint ValueFight;

        public Guildarena(GameState c1, GameState c2) {
            Guildleader1 = c1;
            Guildleader2 = c2;
            Guild1 = Guildleader1.Guild;
            Guild2 = Guildleader2.Guild;
        }

        public void import() {
            if (!Kernel.Maps.ContainsKey(700))
                new Map(700, DMaps.MapPaths[700]);
            Map origMap = Kernel.Maps[700];
            dynamicMap = origMap.MakeDynamicMap();
            foreach (var Member in Guild1.Members.Values) {
                var import = Member.Client;
                if (import != null) {
                    if (CanFight(import)) {
                        import.MessageBox(
                            "Your Guild Will Fight In Guild Arena For [" + ValueFight + "]CPS You Like To Join", p => {
                                p.Entity.GuildArenaBattle = this;
                                p.PrevPK = p.Entity.PKMode;
                                p.Entity.PKMode = Enums.PkMode.Team;
                                p.Send(new Data(true)
                                    { UID = p.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)p.Entity.PKMode });
                                p.Entity.Teleport(p.Entity.GuildArenaBattle.dynamicMap.ID,
                                    (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                                p.Entity.GuildArenaBattleFight = this;
                            });
                    }
                }
            }

            foreach (var Member in Guild2.Members.Values) {
                var import = Member.Client;
                if (import != null) {
                    if (CanFight(import)) {
                        import.MessageBox(
                            "Your Guild Will Fight In Guild Arena For [" + ValueFight + "]CPS You Like To Join", p => {
                                p.Entity.GuildArenaBattle = this;
                                p.PrevPK = p.Entity.PKMode;
                                p.Entity.PKMode = Enums.PkMode.Team;
                                p.Send(new Data(true)
                                    { UID = p.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)p.Entity.PKMode });
                                p.Entity.Teleport(p.Entity.GuildArenaBattle.dynamicMap.ID,
                                    (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                                p.Entity.GuildArenaBattleFight = this;
                            });
                    }
                }
            }
        }

        private bool CanFight(GameState client) {
            if (client.Entity.ContainsFlag2(Update.Flags2.SoulShackle)) return false;
            if (client.Map.BaseID == 1038) return false;
            if (client.Map.BaseID == 700) return false;
            if (client.Entity.MapID is >= 1090 and <= 1094) return false;
            if (client.Entity.MapID is >= 1505 and <= 1509) return false;
            if (client.Entity.MapID == 1081) return false;
            return (!GameConstants.PKFreeMaps.Contains(client.Map.ID) || client.Map.ID == 1005);
        }

        private void KickAll() {
            var allinmap = Program.Values.ToArray().Where(p => p.Entity.MapID == dynamicMap.ID).ToArray();
            if (allinmap != null) {
                foreach (var p in allinmap)
                    p.Entity.Teleport(1002, 303, 278);
            }
        }

        private void End(Guild winner, Guild Loser) {
            if (winner == null || Loser == null)
                KickAll();
            if (winner.Members == null || Loser.Members == null)
                KickAll();
            if (winner.Members.Count == 0 || Loser.Members.Count == 0)
                KickAll();
            var count = winner.Members.Values.Where(mem => mem.Client != null).Count(mem =>
                !mem.Client.Entity.Dead && mem.Client.Entity.MapID == dynamicMap.ID);
            count = Math.Max(1, count);
            foreach (var mem in winner.Members.Values) {
                var Client = mem.Client;
                if (Client != null) {
                    Client.Entity.ConquerPoints += 20000000;
                    Client.Entity.Teleport(1002, 303, 278);
                }
            }

            foreach (var mem in Loser.Members.Values) {
                var Client = mem.Client;
                if (Client != null) {
                    Client.Entity.BringToLife();
                    Client.Entity.Teleport(1002, 303, 278);
                }
            }
        }

        public void CheakToEnd(GameState gameClient, bool dc = false) {
            bool GuildSurive1 = false;
            bool GuildSurive2 = false;
            if (dc && gameClient.Entity.GuildRank == (ushort)MemberRank.GuildLeader) {
                if (Guildleader1 == gameClient)
                    End(Guild2, Guild1);
                else if (Guildleader2 == gameClient)
                    End(Guild1, Guild2);
            }

            GuildSurive1 = Guild1.Members.Values.Where(mem => mem.Client != null).Any(mem =>
                !mem.Client.Entity.Dead && mem.Client.Entity.MapID == dynamicMap.ID);

            GuildSurive2 = Guild2.Members.Values.Where(mem => mem.Client != null).Any(mem =>
                !mem.Client.Entity.Dead && mem.Client.Entity.MapID == dynamicMap.ID);
            if (!GuildSurive1)
                End(Guild2, Guild1);
            else if (!GuildSurive2)
                End(Guild1, Guild2);
        }
    }
}
