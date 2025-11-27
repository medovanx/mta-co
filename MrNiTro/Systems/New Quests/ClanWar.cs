using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Interfaces;
using MTA.Network.GamePackets;
using System.Drawing;
using MTA.Network;
using MTA.Game.ConquerStructures;
using System.Threading.Generic;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Game
{
    public class ClanArena
    {
        public GameState Clanleader1;
        public GameState Clanleader2;
        public Clan Clan1;
        public Clan Clan2;
        public Map dynamicMap;
        public uint ValueFight;
        public static ushort t1X = (ushort)Kernel.Random.Next(35, 70), t1Y = (ushort)Kernel.Random.Next(35, 70);

        public ClanArena(GameState c1, GameState c2)
        {
            Clanleader1 = c1;
            Clanleader2 = c2;
            Clan1 = Clanleader1.Entity.GetClan;
            Clan2 = Clanleader2.Entity.GetClan;
        }
        public void import()
        {
            if (!Kernel.Maps.ContainsKey(700))
                new Map(700, Database.DMaps.MapPaths[700]);
            Map origMap = Kernel.Maps[700];
            dynamicMap = origMap.MakeDynamicMap();
            foreach (var Member in Clan1.Members.Values)
            {
                var import = Member.Client;
                if (import != null)
                {
                    if (CanFight(import))
                    {
                        import.MessageBox("Your Clan Will Fight In Clan Arena For [" + ValueFight + "]CPS You Like To Join", p =>
                        {
                            p.Entity.CLanArenaBattle = this;
                            p.PrevPK = p.Entity.PKMode;
                            p.Entity.PKMode = Game.Enums.PkMode.Team;
                            p.Send(new Data(true) { UID = p.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)p.Entity.PKMode });
                            p.Entity.Teleport(p.Entity.CLanArenaBattle.dynamicMap.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                            p.Entity.CLanArenaBattleFight = this;
                        }, null);
                    }
                }
            }
            foreach (var Member in Clan2.Members.Values)
            {
                var import = Member.Client;
                if (import != null)
                {
                    if (CanFight(import))
                    {
                        import.MessageBox("Your Clan Will Fight In Clan Arena For [" + ValueFight + "]CPS You Like To Join", p =>
                        {
                            p.Entity.CLanArenaBattle = this;
                            p.PrevPK = p.Entity.PKMode;
                            p.Entity.PKMode = Game.Enums.PkMode.Team;
                            p.Send(new Data(true) { UID = p.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)p.Entity.PKMode });
                            p.Entity.Teleport(p.Entity.CLanArenaBattle.dynamicMap.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                            p.Entity.CLanArenaBattleFight = this;
                        }, null);
                    }
                }
            }
        }
        private bool CanFight(Client.GameState client)
        {
            if (client.Entity.ContainsFlag2(Update.Flags2.SoulShackle)) return false;
            if (client.Map.BaseID == 1038) return false;
            if (client.Map.BaseID == 700) return false;
            if (client.Entity.MapID >= 1090 && client.Entity.MapID <= 1094) return false;
            if (client.Entity.MapID >= 1505 && client.Entity.MapID <= 1509) return false;
            if (client.Entity.MapID == 1081) return false;
            return (!Constants.PKFreeMaps.Contains(client.Map.ID) || client.Map.ID == 1005);
        }
        private void KickAll()
        {
            var allinmap = Program.Values.ToArray().Where(p => p.Entity.MapID == dynamicMap.ID).ToArray();
            if (allinmap != null)
            {
                foreach (var p in allinmap)
                    p.Entity.Teleport(1002, 303, 278);
            }
        }

        private void End(Clan winner, Clan Loser)
        {
            if (winner == null || Loser == null)
                KickAll();
            if (winner.Members == null || Loser.Members == null)
                KickAll();
            if (winner.Members.Count == 0 || Loser.Members.Count == 0)
                KickAll();
            var count = winner.Members.Values.Where(mem => mem.Client != null).Count(mem => !mem.Client.Entity.Dead && mem.Client.Entity.MapID == dynamicMap.ID);
            count = Math.Max(1, count);
            foreach (var mem in winner.Members.Values)
            {
                var Client = mem.Client;
                if (Client != null)
                {
                    Client.Entity.ConquerPoints += 20000000;
                    Client.Entity.Teleport(1002, 303, 278);
                }
            }
            foreach (var mem in Loser.Members.Values)
            {
                var Client = mem.Client;
                if (Client != null)
                {
                    Client.Entity.BringToLife();
                    Client.Entity.Teleport(1002, 303, 278);
                }
            }
        }
        public void CheakToEnd(GameState gameClient, bool dc = false)
        {
            bool ClanSurive1 = false;
            bool ClanSurive2 = false;
            if (dc && gameClient.Entity.ClanRank == Clan.Ranks.ClanLeader)
            {
                if (Clanleader1 == gameClient)
                    End(Clan2, Clan1);
               else if (Clanleader2 == gameClient)                
                    End(Clan1, Clan2);
            }
            ClanSurive1 = Clan1.Members.Values.Where(mem => mem.Client != null).Any(mem => !mem.Client.Entity.Dead && mem.Client.Entity.MapID == dynamicMap.ID);

            ClanSurive2 = Clan2.Members.Values.Where(mem => mem.Client != null).Any(mem => !mem.Client.Entity.Dead && mem.Client.Entity.MapID == dynamicMap.ID);
            if (!ClanSurive1)
                End(Clan2, Clan1);
            else if (!ClanSurive2)
                End(Clan1, Clan2);
        }
    }
}
        