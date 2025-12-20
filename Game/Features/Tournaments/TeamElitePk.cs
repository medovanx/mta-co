using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Database;
using MTA.MaTrix;
using MTA.Network.GamePackets;
using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using Team = MTA.Game.ConquerStructures.Team;

namespace MTA.Game.Features.Tournaments {
    public class ElitePkStatus {
        public enum top_typ {
            Elite_PK_Champion__Low_ = 12,
            Elite_PK_2nd_Place_Low_ = 13,
            Elite_PK_3rd_Place_Low_ = 14,
            Elite_PK_Top_8__Low_ = 15,

            Elite_PK_Champion_High_ = 16,
            Elite_PK_2nd_Place_High_ = 17,
            Elite_PK_3rd_Place__High_ = 18,
            Elite_PK_Top_8_High_ = 19
        }

        public static byte[] BlockOpenArena = new byte[] {
            20, 0, 86, 4, 255, 87, 14, 0, 160, 187, 13, 0, 103, 0, 21, 64, 204, 17, 0, 0, 84, 81, 83, 101, 114, 118,
            101, 114
        };

        public void GetSkillElitePkReward(GameState client, byte Rank) {
            uint ITEM_ID = 0;
            switch (Rank) {
                case 0:
                    ITEM_ID = (uint)(721300 + GetEliteGroup(client.Entity.Level));
                    break;
                case 1:
                    ITEM_ID = (uint)(721304 + GetEliteGroup(client.Entity.Level));
                    break;
                case 2:
                    ITEM_ID = (uint)(721308 + GetEliteGroup(client.Entity.Level));
                    break;
                default:
                    ITEM_ID = (uint)(721312 + GetEliteGroup(client.Entity.Level));
                    break;
            }

            client.Inventory.Add(ITEM_ID, 0, 1);
        }

        public void GetTeamElitePkReward(GameState client, byte Rank) {
            uint ITEM_ID = 0;
            switch (Rank) {
                case 0:
                    ITEM_ID = (uint)(720794 + GetEliteGroup(client.Entity.Level));
                    break;
                case 1:
                    ITEM_ID = (uint)(720798 + GetEliteGroup(client.Entity.Level));
                    break;
                case 2:
                    ITEM_ID = (uint)(720802 + GetEliteGroup(client.Entity.Level));
                    break;
                default:
                    ITEM_ID = (uint)(720806 + GetEliteGroup(client.Entity.Level));
                    break;
            }

            client.Inventory.Add(ITEM_ID, 0, 1);
        }

        public byte GetEliteGroup(byte level) {
            if (level >= 130)
                return 3;
            if (level >= 120)
                return 3;
            if (level >= 100)
                return 1;
            else
                return 0;
        }

        public class States {
            public const byte
                GUI_Top8Ranking = 0,
                GUI_Knockout = 3,
                GUI_Top8Qualifier = 4,
                GUI_Top4Qualifier = 5,
                GUI_Top2Qualifier = 6,
                GUI_Top3 = 7,
                GUI_Top1 = 8,
                GUI_ReconstructTop = 9;

            public const byte
                T_Organize = 0,
                T_CreateMatches = 1,
                T_Wait = 2,
                T_Fights = 3,
                T_Finished = 4,
                T_ReOrganize = 5;
        }
    }

    public class TeamElitePk : ElitePkStatus {
        public enum GamePackets : ushort {
            SkillEliteSetTeamName = 2260,
            SkillElitePkTop = 2253,
            SkillElitePkBrackets = 2252,
            SkillElitePKMatchStats = 2251,
            SkillElitePKMatchUI = 2250,
            TeamEliteSetTeamName = 2240,
            TeamElitePkTop = 2233,
            TeamElitePkBrackets = 2232,
            TeamElitePKMatchStats = 2231,
            TeamElitePKMatchUI = 2230,
        }

        public static List<AI> Ais = new List<AI>();

        public bool Alive = false;
        private bool AllowJoin = false;
        public DateTime ConstructTop8 = new DateTime();
        private Match[] ExtendedMatchArray;
        public int GroupID;
        public GamePackets ID;
        private Match[] MatchArray;
        private Counter MatchCounter;
        public ConcurrentDictionary<uint, Match> Matches;
        private ushort MatchIndex;

        private Time32 pStamp;
        private int pState = States.T_Organize;

        DateTime StarTimer = new DateTime();
        public int State;
        private IDisposable Subscriber;

        public ConcurrentDictionary<uint, Team> Teams;
        private Match[] Top4MatchArray;

        public FighterStats[] Top8 = new FighterStats[0];
        public Map WaitingArea;
        private bool willAdvance;


        public TeamElitePk(int group, GamePackets T_ID) {
            if (!Alive) {
                ID = T_ID;
                Alive = true;
                Teams = new ConcurrentDictionary<uint, Team>();
                Matches = new ConcurrentDictionary<uint, Match>();

                GroupID = group;

                var name = "TeamPk";
                if (ID == GamePackets.SkillElitePkBrackets)
                    name = "SkillPk";
                LoadTop8(string.Format("{0}{1}", name, GroupID));

                MatchCounter = new Counter((uint)(GroupID * 100000 + 100000));

                WaitingArea = Kernel.Maps[ElitePKTournament.WaitingAreaID].MakeDynamicMap();
                Constants.PKForbiddenMaps.Add(WaitingArea.ID);

                State = States.GUI_Top8Ranking;
                pState = States.T_Organize;
            }
        }

        public ushort TimeLeft {
            get {
                int value = (pStamp.TotalMilliseconds - Time32.Now.TotalMilliseconds) / 1000;
                if (value < 0) return 0;
                return (ushort)value;
            }
        }

        private void SaveTop8(string epk) {
            try {
                int len = Top8.Count(p => p.LeaderUID != 0);
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                writer.Write(len);
                for (int i = 0; i < len; i++) {
                    writer.Write(Top8[i].LeaderUID);
                    writer.Write(Top8[i].Name);
                    writer.Write(Top8[i].LeaderMesh);
                    writer.Write(Top8[i].Rank);
                    writer.Write(Top8[i].Title);
                    writer.Write(Top8[i].RceiveReward);
                }

                string SQL = "UPDATE `matrixvariable` SET data=@data where ID = '" + epk + "' ;";
                byte[] rawData = stream.ToArray();
                using (var conn = DataHolder.MySqlConnection) {
                    conn.Open();
                    using (var cmd = new MySqlCommand()) {
                        cmd.Connection = conn;
                        cmd.CommandText = SQL;
                        cmd.Parameters.AddWithValue("@data", rawData);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private void LoadTop8(string epk) {
            try {
                Top8 = new FighterStats[8];
                using (var cmd = new Database.MySqlCommand(MySqlCommandType.SELECT)) {
                    cmd.Select("matrixvariable").Where("ID", epk);
                    using (MySqlReader rdr = new MySqlReader(cmd)) {
                        if (rdr.Read()) {
                            byte[] data = rdr.ReadBlob("data");
                            if (data != null && data.Length > 0) {
                                try {
                                    using (var stream = new MemoryStream(data))
                                    using (var reader = new BinaryReader(stream)) {
                                        if (stream.Length >= 4) {
                                            int len = reader.ReadInt32();

                                            int maxLen = Math.Min(len, Top8.Length);
                                            for (int i = 0; i < maxLen; i++) {
                                                try {
                                                    if (stream.Position + 4 > stream.Length)
                                                        break;

                                                    uint id = reader.ReadUInt32();

                                                    string name = "";
                                                    if (stream.Position + 4 <= stream.Length) {
                                                        try {
                                                            name = reader.ReadString();
                                                        }
                                                        catch (EndOfStreamException) {
                                                            break;
                                                        }
                                                    }

                                                    if (stream.Position + 4 > stream.Length)
                                                        break;

                                                    uint score = reader.ReadUInt32();
                                                    Top8[i] = new FighterStats(id, name, score, null);

                                                    if (stream.Position + 3 <= stream.Length) {
                                                        Top8[i].Rank = reader.ReadByte();
                                                        Top8[i].Title = reader.ReadByte();
                                                        Top8[i].RceiveReward = reader.ReadByte();
                                                    }
                                                }
                                                catch (EndOfStreamException) {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (EndOfStreamException) {
                                    // Data is incomplete, skip loading
                                }
                                catch (Exception ex) {
                                    Console.WriteLine($"Error loading TeamElitePk data: {ex.Message}");
                                }
                            }
                        }

                        else {
                            using (var command = new Database.MySqlCommand(MySqlCommandType.INSERT)) {
                                command.Insert("matrixvariable").Insert("ID", epk);
                                command.Execute();
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public bool SignUp(GameState client) {
            if (AllowJoin || client.Fake) {
                var coords = WaitingArea.RandomCoordinates();
                client.Entity.Teleport(WaitingArea.ID, coords.Item1, coords.Item2);


                if (!Teams.ContainsKey(client.Team.UID))
                    Teams.TryAdd(client.Team.UID, client.Team);

                TeamElitePKMatchUI ui = new TeamElitePKMatchUI(ID);
                {
                    ui.Type = TeamElitePKMatchUI.Information;
                    ui.Append(client.Team);
                    client.Send(ui.ToArray());
                    ui.Type = 5;
                    client.Send(ui.ToArray());
                }
                if (client.Team.TeamLider(client)) {
                    SkillEliteSetTeamName teamname = new SkillEliteSetTeamName(ID);
                    {
                        teamname.TeamID = client.Team.UID;
                        teamname.TeamName = client.Team.EliteFighterStats.Name;
                        client.Send(teamname.ToArray());
                    }
                }

                return true;
            }

            return false;
        }

        public void SubscribeTimer(int minutes) {
            StarTimer = DateTime.Now;
            StarTimer = StarTimer.AddMinutes(5);
            AllowJoin = true;
            if (Subscriber == null)
                Subscriber = World.Subscribe(timerCallback, 1000);
        }

        public void Purge() {
            foreach (var team in Teams.Values) {
                if (State == States.GUI_Top8Qualifier) {
                    if (Teams.Count > 8) {
                        if (team.EliteFighterStats.Lost ||
                            !team.OnPopulates) // && player.Player.MyLocation.DinamicID != WaitingArea)
                        {
                            Team remover;
                            Teams.TryRemove(team.UID, out remover);
                        }
                    }
                }
                else if (team.EliteFighterStats.Lost) {
                    if (Teams.Count > 8) {
                        Team remover;
                        Teams.TryRemove(team.UID, out remover);
                    }
                }
            }
        }

        private void timerCallback(int tim) {
            try {
                if (!Alive) {
                    if (ID == GamePackets.SkillElitePkBrackets)
                        SkillTeamTournament.Opened = false;
                    else
                        TeamTournament.Opened = false;
                    Subscriber.Dispose();
                    return;
                }

                #region ReconstructTop

                if (State == States.GUI_ReconstructTop) {
                    if (DateTime.Now > ConstructTop8.AddMinutes(2)) {
                        State = States.GUI_Top8Ranking;
                        Kernel.SendWorldMessage(new TeamElitePkBrackets(ID)
                            { Type = 6, OnGoing = false }.ToArray());
                        Alive = false;
                    }

                    return;
                }

                #endregion

                DateTime Now64 = DateTime.Now;
                if (Now64 > StarTimer) {
                    SkillTeamTournament.Opened = false;
                    TeamTournament.Opened = false;

                    #region GetState

                    if (State == States.GUI_Top8Ranking) {
                        willAdvance = Teams.Count > 8;
                        Kernel.SendWorldMessage(new TeamElitePkBrackets(ID) { Type = 6, OnGoing = true }
                            .ToArray());
                        foreach (var team in Teams.Values) {
                            foreach (var player in team.Players) {
                                if (player != null) {
                                    if (player.Team != null) {
                                        SkillEliteSetTeamName teamname =
                                            new SkillEliteSetTeamName(ID);
                                        {
                                            teamname.Type = SkillEliteSetTeamName.SuccessfulName;
                                            teamname.TeamID = player.Team.UID;
                                            teamname.TeamName = player.Team.EliteFighterStats.Name;
                                            player.Send(teamname.ToArray());

                                            if (player.Team.TeamLider(player)) {
                                                teamname.Type = SkillEliteSetTeamName.Remove;
                                                player.Send(teamname.ToArray());
                                            }
                                        }
                                        //TeamPK A~prize~for~every~participant~in~the~Team~PK~Tournament.~Right~click~it~to~get~1~hour`s~double~EXP.
                                        if (ID == GamePackets.TeamElitePkBrackets)
                                            player.Inventory.Add(720793, 0, 1);
                                    }
                                }
                            }
                        }

                        if (Teams.Count < 8) {
                            if (Teams.Count < 1) {
                                foreach (var team in Teams.Values)
                                foreach (var player in team.Players)
                                    player.Entity.Teleport(1002, 439, 390);
                                Teams.Clear();
                                Alive = false;
                                Kernel.SendWorldMessage(new TeamElitePkBrackets(ID)
                                    { Type = 6, OnGoing = false }.ToArray());
                                return;
                            }

                            if (GroupID == 3) {
                                Ais = new List<AI>();
                                int count = 8 - Teams.Count;
                                for (int i = 0; i < count; i++) {
                                    var ai = new AI(WaitingArea.ID, 50,
                                        50, AI.BotLevel.MaTrix,
                                        (po) => { return GameState.IsVaildForTeamPk(po); });
                                    if (ai.Bot.Team == null)
                                        ai.Bot.Team = new Team(ai.Bot);
                                    ai.Bot.Team.SetEliteFighterStats(true);
                                    if (!Teams.ContainsKey(ai.Bot.Team.UID))
                                        Teams.TryAdd(ai.Bot.Team.UID, ai.Bot.Team);
                                    // Game.Features.Tournaments.TeamElitePk.TeamTournament.Join(ai.Bot, 3);
                                    Ais.Add(ai);
                                }
                            }
                        }

                        Kernel.SendWorldMessage(new TeamElitePkBrackets(ID) { Type = 6, OnGoing = true }
                            .ToArray());
                        Top8 = new FighterStats[8];
                        if (Teams.Count == 8)
                            State = States.GUI_Top4Qualifier;
                        else if (willAdvance && Teams.Count <= 24)
                            State = States.GUI_Top8Qualifier;
                        else
                            State = States.GUI_Knockout;
                    }

                    #endregion

                    #region Knockout

                    if (State == States.GUI_Knockout) {
                        AllowJoin = false;
                        if (pState == States.T_Organize) {
                            if (willAdvance && Teams.Count <= 24) {
                                State = States.GUI_Top8Qualifier;
                            }
                            else {
                                MatchIndex = 0;
                                var array = Teams.Values.ToArray();
                                ushort counter = 0;
                                bool NoPar = array.Length % 2 == 0;
                                if (NoPar) {
                                    for (ushort x = 0; x < array.Length; x++) {
                                        int r = counter;
                                        counter++;
                                        int t = counter;
                                        counter++;
                                        if (counter <= array.Length) {
                                            Match match = new Match(ID, array[r].EliteFighterStats,
                                                array[t].EliteFighterStats);
                                            match.Index = MatchIndex++;
                                            match.ID = MatchCounter.Next;
                                            Matches.TryAdd(match.ID, match);
                                        }
                                    }
                                }
                                else {
                                    for (ushort x = 0; x < array.Length - 1; x++) {
                                        int r = counter;
                                        counter++;
                                        int t = counter;
                                        counter++;
                                        if (counter <= array.Length) {
                                            Match match = new Match(ID, array[r].EliteFighterStats,
                                                array[t].EliteFighterStats);
                                            match.Index = MatchIndex++;
                                            match.ID = MatchCounter.Next;
                                            Matches.TryAdd(match.ID, match);
                                        }
                                    }

                                    Match math_bye = new Match(ID, array[array.Length - 1].EliteFighterStats);
                                    math_bye.Index = MatchIndex++;
                                    math_bye.ID = MatchCounter.Next;
                                    Matches.TryAdd(math_bye.ID, math_bye);
                                }

                                pStamp = Time32.Now.AddSeconds(60);
                                pState = States.T_Wait;
                                MatchArray = (from match in Matches.Values
                                    orderby match.MatchStats.Length descending
                                    select match).ToArray();
                                var brackets = CreateBrackets(MatchArray, 0);
                                foreach (var packet in brackets) {
                                    Kernel.SendWorldMessage(packet.ToArray());
                                }
                            }
                        }
                        else if (pState == States.T_Wait) {
                            if (TimeLeft == 0) {
                                int doneMatches = 0;

                                foreach (var match in MatchArray) {
                                    match.AliveMatch();
                                    if (!match.Inside) {
                                        match.Import(Match.StatusFlag.Watchable);
                                    }
                                    else {
                                        if (!match.Exported && (match.Done || match.TimeLeft == 0)) {
                                            match.Export();
                                        }

                                        if (match.Exported || match.MatchStats.Length == 1)
                                            doneMatches++;
                                    }
                                }

                                if (doneMatches == Matches.Count) {
                                    Matches.Clear();
                                    Purge();
                                    pState = States.T_Organize;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Top8Qualifier

                    if (State == States.GUI_Top8Qualifier) {
                        if (pState == States.T_Organize) {
                            if (Teams.Count == 8) {
                                State = States.GUI_Top4Qualifier;
                            }
                            else {
                                MatchIndex = 0;
                                var array = Teams.Values.ToArray();
                                int[] taken = new int[array.Length];

                                if (array.Length <= 16) {
                                    ushort counter = 0;
                                    int t1Group = array.Length - 8;
                                    int lim = taken.Length / 2;
                                    for (int i = 0; i < t1Group; i++) {
                                        int r = counter;
                                        counter++;
                                        int t = counter;
                                        counter++;
                                        Match match = new Match(ID, array[r].EliteFighterStats,
                                            array[t].EliteFighterStats);
                                        match.Index = MatchIndex++;
                                        match.ID = MatchCounter.Next;
                                        Matches.TryAdd(match.ID, match);
                                    }

                                    for (int i = 0; i < 8 - t1Group; i++) {
                                        ushort r = counter;
                                        counter++;
                                        Match match = new Match(ID, array[r].EliteFighterStats);
                                        match.Index = MatchIndex++;
                                        match.ID = MatchCounter.Next;
                                        Matches.TryAdd(match.ID, match);
                                    }
                                }
                                else {
                                    ushort counter = 0;
                                    int t3GroupCount = array.Length - 16;
                                    for (int i = 0; i < t3GroupCount; i++) {
                                        int r = counter;
                                        counter++;
                                        int t = counter;
                                        counter++;
                                        int y = counter;
                                        counter++;
                                        Match match = new Match(ID, array[r].EliteFighterStats,
                                            array[t].EliteFighterStats,
                                            array[y].EliteFighterStats);
                                        match.Index = MatchIndex++;
                                        match.ID = MatchCounter.Next;
                                        Matches.TryAdd(match.ID, match);
                                    }

                                    int t2GroupCount = array.Length - counter;
                                    for (int i = 0; i < t2GroupCount / 2; i++) {
                                        int r = counter;
                                        counter++;
                                        int t = counter;
                                        counter++;
                                        Match match = new Match(ID, array[r].EliteFighterStats,
                                            array[t].EliteFighterStats);
                                        match.Index = MatchIndex++;
                                        match.ID = MatchCounter.Next;
                                        Matches.TryAdd(match.ID, match);
                                    }
                                }

                                pStamp = Time32.Now.AddSeconds(60);
                                pState = States.T_Wait;
                                MatchArray = (from match in Matches.Values
                                    orderby match.MatchStats.Length descending
                                    select match).ToArray();

                                var brackets = CreateBrackets(MatchArray, 0,
                                    TeamElitePkBrackets.UpdateList);
                                foreach (var packet in brackets) {
                                    Kernel.SendWorldMessage(packet.ToArray());
                                }

                                TeamElitePkBrackets brea =
                                    new TeamElitePkBrackets(0);
                                brea.Type = TeamElitePkBrackets.EPK_State;
                                brea.OnGoing = true;
                                Kernel.SendWorldMessage(brea.ToArray());
                            }
                        }
                        else if (pState == States.T_Wait) {
                            if (TimeLeft == 0) {
                                int doneMatches = 0;
                                foreach (var match in MatchArray) {
                                    match.AliveMatch();
                                    if (!match.Inside) {
                                        if (match.MatchStats.Length > 1) {
                                            match.Import(Match.StatusFlag.Watchable);
                                        }
                                        else {
                                            match.Inside = true;
                                            match.Completed = true;
                                            match.Done = true;
                                            match.MatchStats
                                                .First(p => p != null).Team
                                                .EliteFighterStats.Flag = FighterStats.StatusFlag.Qualified;
                                        }
                                    }
                                    else {
                                        if (!match.Exported && (match.Done || match.TimeLeft == 0)) {
                                            match.Export();
                                            if (match.MatchStats.Length == 3)
                                                match.SwitchBetween();
                                            else
                                                match.Flag = Match.StatusFlag.OK;
                                        }

                                        if (match.Exported || match.MatchStats.Length == 1)
                                            doneMatches++;
                                    }
                                }

                                if (doneMatches == Matches.Count) {
                                    bool finishedRound = true;
                                    foreach (var match in MatchArray) {
                                        if (!match.Completed) {
                                            finishedRound = false;
                                            break;
                                        }
                                    }

                                    if (!finishedRound) {
                                        foreach (var match in MatchArray) {
                                            if (!match.Completed) {
                                                foreach (var stats in match.MatchStats)
                                                    stats.Reset(true);
                                                match.Inside = false;
                                                match.Done = false;
                                                match.Exported = false;
                                                if (match.MatchStats.Length != 1)
                                                    match.Flag = Match.StatusFlag.Waiting;
                                            }
                                        }

                                        pStamp = Time32.Now.AddSeconds(60);
                                        pState = States.T_Wait;
                                        var brakets = CreateBrackets(MatchArray, 0,
                                            TeamElitePkBrackets.UpdateList);
                                        foreach (var packet in brakets) {
                                            Kernel.SendWorldMessage(packet.ToArray());
                                        }
                                    }
                                    else {
                                        Matches.Clear();
                                        Purge();
                                        pState = States.T_Organize;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Top4Qualifier

                    if (State == States.GUI_Top4Qualifier) {
                        if (pState == States.T_Organize) {
                            try {
                                MatchIndex = 0;
                                var array = Teams.Values.ToArray();
                                int[] taken = new int[array.Length];
                                int lim = taken.Length / 2;
                                for (int i = 0; i < taken.Length; i += 2) {
                                    taken[i] = taken[i + 1] = 1;
                                    Match match = new Match(ID, array[i].EliteFighterStats,
                                        array[i + 1].EliteFighterStats);
                                    match.Index = MatchIndex++;
                                    //match.Flag = Match.StatusFlag.FinalMatch;
                                    match.ID = MatchCounter.Next;
                                    Matches.TryAdd(match.ID, match);
                                }

                                pStamp = Time32.Now.AddSeconds(60);
                                pState = States.T_Wait;
                                MatchArray = Matches.Values.ToArray();
                                var brackets = CreateBrackets(MatchArray, 0,
                                    TeamElitePkBrackets.UpdateList);
                                foreach (var packet in brackets)
                                    Kernel.SendWorldMessage(packet.ToArray());
                            }
                            catch (Exception e) {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else if (pState == States.T_Wait) {
                            if (TimeLeft == 0) {
                                try {
                                    int doneMatches = 0;

                                    foreach (var match in MatchArray) {
                                        match.AliveMatch();
                                        if (!match.Inside) {
                                            match.Import(Match.StatusFlag.Watchable);
                                        }
                                        else {
                                            if (!match.Exported && (match.Done || match.TimeLeft == 0))
                                                match.Export();
                                            if (match.Exported)
                                                doneMatches++;
                                        }
                                    }

                                    if (doneMatches == MatchArray.Length) {
                                        int i = 7;
                                        foreach (var match in MatchArray) {
                                            foreach (var stats in match.MatchStats)
                                                if (stats.Flag == FighterStats.StatusFlag.Lost)
                                                    Top8[i--] = stats.Clone(); //fara clone;
                                            match.Commit();
                                        }

                                        State = States.GUI_Top2Qualifier;
                                        pState = States.T_Organize;
                                    }
                                }
                                catch (Exception e) {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                    }

                    #endregion

                    #region Top2Qualifier

                    if (State == States.GUI_Top2Qualifier) {
                        if (pState == States.T_Organize) {
                            try {
                                MatchIndex = 0;
                                Top4MatchArray = new Match[2];

                                for (int i = 0; i < Top4MatchArray.Length; i++) {
                                    Match match = new Match(ID, //p.winer este
                                        MatchArray[i]
                                            .Return(p =>
                                                p.Team.EliteFighterStats
                                                    .Winner), //.MatchStats.First(p => p.Team.EliteFighterStats.Winner),
                                        MatchArray[i + 2]
                                            .Return(p =>
                                                p.Team.EliteFighterStats
                                                    .Winner)); //.MatchStats.First(p => p.Team.EliteFighterStats.Winner));
                                    match.Index = MatchIndex++;
                                    match.ID = MatchCounter.Next;
                                    Top4MatchArray[i] = match;
                                    Matches.TryAdd(match.ID, match);
                                }

                                pStamp = Time32.Now.AddSeconds(60);
                                pState = States.T_Wait;
                                var brackets = CreateBrackets(Top4MatchArray, 0,
                                    TeamElitePkBrackets.UpdateList);
                                foreach (var packet in brackets)
                                    Kernel.SendWorldMessage(packet.ToArray());
                            }
                            catch (Exception e) {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else if (pState == States.T_Wait) {
                            if (TimeLeft == 0) {
                                try {
                                    int doneMatches = 0;
                                    foreach (var match in Top4MatchArray) {
                                        match.AliveMatch();
                                        if (!match.Inside) {
                                            match.Import(Match.StatusFlag.Watchable);
                                        }
                                        else {
                                            if (!match.Exported && (match.Done || match.TimeLeft == 0))
                                                match.Export();
                                            if (match.Exported)
                                                doneMatches++;
                                        }
                                    }

                                    if (doneMatches == Top4MatchArray.Length) {
                                        foreach (var match in Top4MatchArray)
                                            match.Commit();

                                        pState = States.T_Organize;
                                        State = States.GUI_Top3;
                                    }
                                }
                                catch (Exception e) {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                    }

                    #endregion

                    #region Top3

                    if (State == States.GUI_Top3) {
                        if (pState == States.T_Organize) {
                            try {
                                MatchIndex = 0;

                                ExtendedMatchArray = new Match[2];
                                Match match = new Match(ID,
                                    Top4MatchArray[0]
                                        .Return(p =>
                                            !p.Team.EliteFighterStats
                                                .Winner), //.MatchStats.First(p => !p.Team.EliteFighterStats.Winner),
                                    Top4MatchArray[1]
                                        .Return(p =>
                                            !p.Team.EliteFighterStats
                                                .Winner)); //.MatchStats.First(p => !p.Team.EliteFighterStats.Winner));
                                match.Index = MatchIndex++;
                                match.ID = MatchCounter.Next;
                                ExtendedMatchArray[0] = match;

                                Match finalmatch = new Match(ID,
                                    Top4MatchArray[0].Return(p => p.Winner)
                                        .Clone(), //.MatchStats.First(p => p.Team.EliteFighterStats.Winner).Clone(),
                                    Top4MatchArray[1].Return(p => p.Winner)
                                        .Clone()); //.MatchStats.First(p => p.Team.EliteFighterStats.Winner).Clone());
                                finalmatch.Flag = Match.StatusFlag.SwitchOut;
                                ExtendedMatchArray[1] = finalmatch;

                                Matches.TryAdd(match.ID, match);
                                pStamp = Time32.Now.AddSeconds(60);
                                pState = States.T_Wait;
                                var brackets = CreateBrackets(ExtendedMatchArray, 0,
                                    TeamElitePkBrackets.UpdateList);
                                foreach (var packet in brackets)
                                    Kernel.SendWorldMessage(packet.ToArray());

                                Kernel.SendWorldMessage(new TeamElitePkBrackets(ID)
                                    { Type = 6, OnGoing = true }.ToArray());
                            }
                            catch (Exception e) {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else if (pState == States.T_Wait) {
                            if (TimeLeft == 0) {
                                try {
                                    var match = ExtendedMatchArray[0];
                                    match.AliveMatch();
                                    if (!match.Inside) {
                                        match.Import(Match.StatusFlag.Watchable);
                                    }
                                    else {
                                        if (!match.Exported && (match.Done || match.TimeLeft == 0))
                                            match.Export();
                                        if (match.Exported) {
                                            //var a_math =ExtendedMatchArray[0].MatchStats;
                                            var top3 = ExtendedMatchArray[0]
                                                .Return(p => p.Team.EliteFighterStats.Winner);
                                            //.First(p => p.Team.EliteFighterStats.Winner).Team.EliteFighterStats;
                                            var top4 = ExtendedMatchArray[0]
                                                .Return(p => !p.Team.EliteFighterStats.Winner);
                                            //ExtendedMatchArray[0].MatchStats
                                            //.First(p => !p.Team.EliteFighterStats.Winner).Team.EliteFighterStats;

                                            Top8[2] = top3;
                                            Top8[3] = top4;
                                            top3.Flag = FighterStats.StatusFlag.Lost;

                                            pState = States.T_Organize;
                                            State = States.GUI_Top1;
                                            foreach (var client in Kernel.GamePool.Values)
                                                Rankings(client);
                                            var brackets = CreateBrackets(ExtendedMatchArray, 0,
                                                TeamElitePkBrackets.UpdateList);
                                            foreach (var packet in brackets)
                                                Kernel.SendWorldMessage(packet.ToArray());
                                        }
                                    }
                                }
                                catch (Exception e) {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                    }

                    #endregion

                    #region Top1

                    if (State == States.GUI_Top1) {
                        if (pState == States.T_Organize) {
                            try {
                                MatchIndex = 0;
                                //  var brackets = CreateBrackets(MatchArray, 0, Client.GamePackets.SkillElitePkBrackets.GUIEdit);
                                //  foreach (var packet in brackets)
                                //       CMD.MapPackets.Enqueue(new CMD.MapPackets.Models() { data = packet.ToArray() });
                                Match match = new Match(ID, ExtendedMatchArray[1].MatchStats);
                                ExtendedMatchArray = new Match[1];
                                match.Index = MatchIndex++;
                                match.ID = MatchCounter.Next;
                                ExtendedMatchArray[0] = match;
                                Matches.TryAdd(match.ID, match);
                                pStamp = Time32.Now.AddSeconds(60);
                                pState = States.T_Wait;
                                var brackets = CreateBrackets(ExtendedMatchArray, 0,
                                    TeamElitePkBrackets.UpdateList);
                                foreach (var packet in brackets)
                                    Kernel.SendWorldMessage(packet.ToArray());

                                Kernel.SendWorldMessage(new TeamElitePkBrackets(ID)
                                    { Type = 6, OnGoing = true }.ToArray());
                            }
                            catch (Exception e) {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else if (pState == States.T_Wait) {
                            if (TimeLeft == 0) {
                                try {
                                    int doneMatches = 0;
                                    foreach (var match in ExtendedMatchArray) {
                                        match.AliveMatch();
                                        if (!match.Inside) {
                                            match.Import(Match.StatusFlag.Watchable);
                                        }
                                        else {
                                            if (!match.Exported && (match.Done || match.TimeLeft == 0))
                                                match.Export();
                                            if (match.Exported)
                                                doneMatches++;
                                        }
                                    }

                                    if (doneMatches == ExtendedMatchArray.Length) {
                                        try {
                                            var top1 = ExtendedMatchArray[0].MatchStats
                                                .First(p => p.Team.EliteFighterStats.Winner).Team.EliteFighterStats;
                                            var top2 = ExtendedMatchArray[0].MatchStats
                                                .First(p => !p.Team.EliteFighterStats.Winner).Team.EliteFighterStats;
                                            Top8[0] = top1;
                                            Top8[1] = top2;
                                            ConstructTop8 = DateTime.Now;
                                            var name = "TeamPk";
                                            if (ID == GamePackets.SkillElitePkBrackets)
                                                name = "SkillPk";
                                            SaveTop8(string.Format("{0}{1}", name, GroupID));

                                            foreach (var ai in Ais)
                                                ai.Bot = null;
                                            foreach (var team in Teams.Values)
                                            foreach (var player in team.Players)
                                                player.Entity.Teleport(1002, 439, 390);
                                            State = States.GUI_ReconstructTop;
                                            foreach (var client in Kernel.GamePool.Values)
                                                Rankings(client);
                                            var brackets = CreateBrackets(ExtendedMatchArray, 0,
                                                TeamElitePkBrackets.UpdateList);
                                            foreach (var packet in brackets)
                                                Kernel.SendWorldMessage(packet.ToArray());
                                            Kernel.SendWorldMessage(new TeamElitePkBrackets(ID)
                                                { Type = 6, OnGoing = false }.ToArray());
                                        }
                                        catch (Exception e) {
                                            Console.WriteLine(e.ToString());
                                        }
                                    }
                                }
                                catch (Exception e) {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                    }

                    #endregion
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void Update(GameState client, int page = 0) {
            try {
                if (State == States.GUI_Top8Ranking) {
                    Rankings(client);
                }
                else {
                    if (State >= States.GUI_Top4Qualifier) {
                        var brackets = CreateBrackets(MatchArray, 0, TeamElitePkBrackets.GUIEdit);
                        foreach (var packet in brackets)
                            client.Send(packet.ToArray());
                    }
                    else {
                        var brackets = CreateBrackets(MatchArray, page);
                        foreach (var packet in brackets)
                            client.Send(packet.ToArray());
                    }

                    if (State >= States.GUI_Top1)
                        Rankings(client);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void Rankings(GameState client) {
            TeamElitePkTop ranks = new TeamElitePkTop(ID);
            {
                ranks.Type = TeamElitePkTop.Top8;
                ranks.Group = (uint)GroupID;
                ranks.GroupStatus = (uint)State;
                //ranks.UID = client.Player.UID;
                if (State >= States.GUI_Top1) {
                    ranks.Type = TeamElitePkTop.Top3;
                    if (State == States.GUI_Top1) {
                        ranks.Count = 1;
                        ranks.Append(Top8[2], 3);
                    }
                    else {
                        ranks.Count = 3;
                        for (int i = 0; i < 3; i++)
                            ranks.Append(Top8[i], i + 1);
                    }

                    client.Send(ranks.ToArray());
                }
                else {
                    ranks.Count = (uint)Top8.Length;
                    for (int i = 0; i < ranks.Count; i++)
                        if (Top8[i] != null)
                            ranks.Append(Top8[i], i);
                    client.Send(ranks.ToArray());
                }
            }
        }

        private TeamElitePkBrackets[] CreateBrackets(Match[] matches, int page = 0, byte type = 0) {
            try {
                int lim = 0, count = 0;
                if (matches == null) return new TeamElitePkBrackets[0];
                if (State == States.GUI_Knockout) {
                    const int max = 5;
                    int offset = page * max;

                    int Do_count = matches.Length - page * max;

                    if (page * max > matches.Length)
                        Do_count = 0;
                    uint ccount = (uint)Math.Min(max, Do_count);

                    TeamElitePkBrackets brackets =
                        new TeamElitePkBrackets(ID, (int)ccount);
                    brackets.Group = (ushort)GroupID;
                    brackets.GUIType = (ushort)State;
                    brackets.TotalMatchesOnRoom = (ushort)ccount;
                    brackets.Page = (byte)page;
                    brackets.TimeLeft = TimeLeft;
                    brackets.MatchCount = (ushort)matches.Length;
                    brackets.Type = type;
                    for (int i = 0; i < ccount; i++) {
                        var element = matches[i + offset];
                        brackets.Append(element);
                    }

                    TeamElitePkBrackets[] buffer = new TeamElitePkBrackets[1];
                    buffer[0] = brackets;
                    return buffer;
                }
                else if (State == States.GUI_Top8Qualifier) {
                    const int max = 5;
                    int offset = page * max;

                    int Do_count = matches.Length - page * max;

                    if (page * max > matches.Length)
                        Do_count = 0;
                    uint ccount = (uint)Math.Min(max, Do_count);

                    TeamElitePkBrackets[] buffer;
                    if (matches.Length > ccount)
                        buffer = new TeamElitePkBrackets[2];
                    else
                        buffer = new TeamElitePkBrackets[1];

                    TeamElitePkBrackets brackets =
                        new TeamElitePkBrackets(ID, (int)ccount);
                    brackets.Group = (ushort)GroupID;
                    brackets.GUIType = (ushort)State;
                    brackets.TotalMatchesOnRoom = (ushort)ccount;
                    brackets.Page = (byte)page;
                    brackets.TimeLeft = TimeLeft;
                    brackets.MatchCount = (ushort)matches.Length;
                    brackets.Type = type;
                    for (int i = 0; i < ccount; i++) {
                        var element = matches[i + offset];
                        brackets.Append(element);
                    }

                    buffer[0] = brackets;
                    if (matches.Length > ccount) {
                        ushort towcount = (ushort)(matches.Length - ccount);
                        TeamElitePkBrackets twobrackets =
                            new TeamElitePkBrackets(ID, towcount);
                        twobrackets.Group = (ushort)GroupID;
                        twobrackets.GUIType = (ushort)State;
                        twobrackets.TotalMatchesOnRoom = towcount;
                        twobrackets.Page = (byte)page;
                        twobrackets.TimeLeft = TimeLeft;
                        twobrackets.ListCount = 1;
                        twobrackets.MatchCount = (ushort)matches.Length;
                        twobrackets.Type = type;
                        for (int i = 0; i < towcount; i++) {
                            var element = matches[i + ccount];
                            twobrackets.Append(element);
                        }

                        buffer[1] = twobrackets;
                        return buffer;
                    }

                    return buffer;
                }

                lim = matches.Length;
                count = Math.Min(lim, matches.Length - page * lim);

                if (State >= States.GUI_Top2Qualifier) {
                    lim = MatchArray.Length;
                    count = Math.Min(lim, MatchArray.Length - page * lim);
                    TeamElitePkBrackets[] buffer = new TeamElitePkBrackets[2];
                    if (Top4MatchArray == null)
                        return new TeamElitePkBrackets[0];
                    TeamElitePkBrackets qbrackets =
                        new TeamElitePkBrackets(ID, count);
                    {
                        qbrackets.Group = (ushort)GroupID;
                        qbrackets.GUIType = (ushort)State;
                        qbrackets.TotalMatchesOnRoom = (ushort)(count);
                        qbrackets.Page = (byte)page;
                        qbrackets.TimeLeft = TimeLeft;
                        qbrackets.MatchCount = (byte)(MatchArray.Length * 2);
                        qbrackets.Type = type;
                        for (int i = page * lim; i < page * lim + count; i++)
                            qbrackets.Append(MatchArray[i]);
                        buffer[0] = qbrackets;
                    }
                    if (State >= States.GUI_Top3) {
                        lim = 4;
                        count = (byte)(Top4MatchArray.Length + ExtendedMatchArray.Length);
                        TeamElitePkBrackets Twoqbrackets =
                            new TeamElitePkBrackets(ID, count);
                        {
                            Twoqbrackets.Group = (ushort)GroupID;
                            Twoqbrackets.GUIType = (ushort)State;
                            Twoqbrackets.TotalMatchesOnRoom = (ushort)count;
                            Twoqbrackets.Page = 1;
                            Twoqbrackets.TimeLeft = TimeLeft;
                            Twoqbrackets.MatchCount = (byte)((Top4MatchArray.Length + ExtendedMatchArray.Length) * 2);
                            Twoqbrackets.Type = type;
                            for (int i = 0; i < 2; i++)
                                Twoqbrackets.Append(Top4MatchArray[i]);
                            for (int x = 0; x < ExtendedMatchArray.Length; x++)
                                Twoqbrackets.Append(ExtendedMatchArray[x]);
                            buffer[1] = Twoqbrackets;
                        }
                        buffer[1] = Twoqbrackets;
                    }
                    else {
                        lim = 2;
                        count = Top4MatchArray.Length;
                        TeamElitePkBrackets Twoqbrackets =
                            new TeamElitePkBrackets(ID, count);
                        {
                            Twoqbrackets.Group = (ushort)GroupID;
                            Twoqbrackets.GUIType = (ushort)State;
                            // Twoqbrackets.ListCount = 1;
                            Twoqbrackets.TotalMatchesOnRoom = (ushort)count;
                            Twoqbrackets.Page = 1;
                            Twoqbrackets.TimeLeft = TimeLeft;
                            Twoqbrackets.MatchCount = (byte)MatchArray.Length;
                            Twoqbrackets.Type = type;
                            for (int i = page * lim; i < page * lim + count; i++)
                                Twoqbrackets.Append(Top4MatchArray[i]);
                            buffer[1] = Twoqbrackets;
                        }
                    }

                    return buffer;
                }

                TeamElitePkBrackets aqbrackets = new TeamElitePkBrackets(ID, count);
                {
                    aqbrackets.Group = (ushort)GroupID;
                    aqbrackets.GUIType = (ushort)State;
                    aqbrackets.TotalMatchesOnRoom = (ushort)count;
                    aqbrackets.Page = (byte)page;
                    aqbrackets.TimeLeft = TimeLeft;
                    aqbrackets.MatchCount = (ushort)matches.Length;
                    aqbrackets.Type = type;
                    for (int i = page * lim; i < page * lim + count; i++)
                        aqbrackets.Append(matches[i]);
                    TeamElitePkBrackets[] buffer = new TeamElitePkBrackets[1];
                    buffer[0] = aqbrackets;
                    return buffer;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                return new TeamElitePkBrackets[0];
            }
        }

        public class TeamTournament {
            public static uint WaitingAreaID = 2068;
            public static TeamElitePk[] Tournaments;
            public static bool Opened = false;

            public static void Create() {
                try {
                    Tournaments = new TeamElitePk[4];
                    for (byte x = 0; x < Tournaments.Length; x++)
                        Tournaments[x] = new TeamElitePk(x, GamePackets.TeamElitePkBrackets);

                    //   LoadTop(Tournaments, "TeamElitePkTop8");
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }

            public static void Save() {
                // SaveTop(Tournaments, "TeamElitePkTop8");
            }

            public static bool GetReward(GameState client, out byte rank, out byte elitestage) {
                if (Tournaments != null)

                    for (byte x = 0; x < Tournaments.Length; x++) {
                        if (Tournaments[x] != null) {
                            if (Tournaments[x].Top8 != null) {
                                for (byte i = 0; i < Tournaments[x].Top8.Length; i++) {
                                    if (Tournaments[x].Top8[i] != null) {
                                        if (Tournaments[x].Top8[i].LeaderUID == client.Entity.UID) {
                                            if (Tournaments[x].Top8[i].RceiveReward == 0) {
                                                rank = (byte)(i + 1);
                                                elitestage = x;
                                                Tournaments[x].Top8[i].RceiveReward = 1;
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                rank = 0;
                elitestage = 0;
                return false;
            }

            public static void Open() {
                if (!Opened) {
                    Opened = true;
                    for (byte x = 0; x < Tournaments.Length; x++) {
                        Tournaments[x].SubscribeTimer(1);
                        Tournaments[x].AllowJoin = true;
                    }
                    //The Team PK Tournament has start at 20:00. Prepare yourself and sign up for it as a team!");
                    //Program.World.SendServerMessaj("The Team PK Tournament has start at 20:00. Prepare yourself and sign up for it as a team!");
                }
            }

            public static bool Join(GameState client, byte Grope) {
                if (client.Team == null)
                    return false;

                if (Grope <= 3) {
                    client.Team.SetEliteFighterStats(true);
                    return Tournaments[Grope].SignUp(client);
                }

                return false;
            }
        }

        public class SkillTeamTournament {
            public static uint WaitingAreaID = 2068;
            public static TeamElitePk[] Tournaments;
            public static bool Opened = false;

            public static bool GetReward(GameState client, out byte rank, out byte elitestage) {
                if (Tournaments != null)

                    for (byte x = 0; x < Tournaments.Length; x++) {
                        if (Tournaments[x] != null) {
                            if (Tournaments[x].Top8 != null) {
                                for (byte i = 0; i < Tournaments[x].Top8.Length; i++) {
                                    if (Tournaments[x].Top8[i] != null) {
                                        if (Tournaments[x].Top8[i].LeaderUID == client.Entity.UID) {
                                            if (Tournaments[x].Top8[i].RceiveReward == 0) {
                                                rank = (byte)(i + 1);
                                                elitestage = x;
                                                Tournaments[x].Top8[i].RceiveReward = 1;
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                rank = 0;
                elitestage = 0;
                return false;
            }

            public static void Create() {
                Tournaments = new TeamElitePk[4];
                for (byte x = 0; x < Tournaments.Length; x++)
                    Tournaments[x] = new TeamElitePk(x, GamePackets.SkillElitePkBrackets);
                //LoadTop(Tournaments, "SkillTeamElitePkTop8");
            }

            public static void Save() {
                //  SaveTop(Tournaments, "SkillTeamElitePkTop8");
            }

            public static void Open() {
                if (!Opened) {
                    Opened = true;
                    for (byte x = 0; x < Tournaments.Length; x++) {
                        Tournaments[x].SubscribeTimer(1);
                        Tournaments[x].AllowJoin = true;
                    }
                    //Program.World.SendServerMessaj("The Skill Team PK Tournament will start at 20:00. Prepare yourself and sign up for it as a team!");
                }
            }

            public static bool Join(GameState client, byte Grope) {
                if (client.Team == null)
                    return false;

                if (Grope <= 3) {
                    client.Team.SetEliteFighterStats(true);
                    return Tournaments[Grope].SignUp(client);
                }

                return false;
            }
        }
        //public static void SaveTop(TeamElitePk[] elit, string Ini_file)
        //{
        //    if (elit == null) return;
        //    using (Database.Write _wr = new Database.Write("database//" + Ini_file + ".ini"))
        //    {
        //        uint count = 0;
        //        for (byte x = 0; x < elit.Length; x++)
        //            if (elit[x].Top8 != null)
        //                count += (byte)elit[x].Top8.Length;
        //        string[] lines = new string[count];
        //        uint amount = 0;
        //        for (byte x = 0; x < elit.Length; x++)
        //        {
        //            if (elit[x].Top8 != null)
        //                for (byte i = 0; i < elit[x].Top8.Length; i++)
        //                {
        //                    if (elit[x].Top8[i] != null)
        //                        lines[amount] = x + "^" + elit[x].Top8[i].ToString();
        //                    amount++;
        //                }
        //        }

        //        _wr.Add(lines, lines.Length).Execute(Database.Mode.Open);
        //    }
        //}
        //public static void LoadTop(TeamElitePk[] elit, string Ini_file)
        //{
        //    try
        //    {
        //        using (Database.Read read = new Database.Read("database//" + Ini_file + ".ini"))
        //        {
        //            if (read.Reader())
        //            {
        //                byte[] counts = new byte[4];
        //                byte group = 0;
        //                while (read.UseRead())
        //                {
        //                    string[] line = read.ReadString("").Split('^');
        //                    group = byte.Parse(line[0]);
        //                    if (elit[group].Top8 == null)
        //                        elit[group].Top8 = new FighterStats[8];
        //                    if (elit[group].Top8.Length < 8)
        //                        elit[group].Top8 = new FighterStats[8];

        //                    FighterStats fight = new FighterStats(0, "", 0, null);
        //                    fight.Load(line[1]);
        //                    elit[group].Top8[counts[group]] = fight;
        //                    counts[group]++;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //        Console.WriteLine(e.ToString());
        //    }
        //}
        public class FighterStats {
            public enum StatusFlag : int {
                None = 0,
                Fighting = 2,
                Lost = 4,
                Qualified = 3,
                Waiting = 1,
            }

            public uint Cheers;
            public StatusFlag Flag;
            public uint LeaderMesh;
            public uint LeaderUID;

            public string Name;

            public bool OnNextMatch = false;
            public uint Points;
            public byte Rank;

            public byte RceiveReward;

            public Team Team;
            public bool Teleported;
            public byte Title;


            public FighterStats(uint id, string name, uint mesh, Team temate) {
                LeaderUID = id;
                Name = name;
                LeaderMesh = mesh;
                Team = temate;
            }

            public bool Waiting {
                get { return Flag == StatusFlag.Waiting; }
            }

            public bool NoFlag {
                get { return Flag == StatusFlag.None; }
            }

            public bool Fighting {
                get { return Flag == StatusFlag.Fighting; }
            }

            public bool Winner {
                get { return Flag == StatusFlag.Qualified; }
            }

            public bool Lost {
                get { return Flag == StatusFlag.Lost; }
            }

            public void Reset(bool noflag = false) {
                Points = 0;
                Cheers = 0;
                if (!noflag) {
                    if (!Lost)
                        Flag = StatusFlag.Waiting;
                }
            }

            public FighterStats Clone() {
                FighterStats stats = new FighterStats(LeaderUID, Name, LeaderMesh, Team);
                stats.Points = this.Points;
                stats.Flag = this.Flag;

                return stats;
            }

            public override string ToString() {
                if (Name.Contains('^'))
                    Name.Replace("^", "");
                if (Name.Contains('#'))
                    Name.Replace('#', ' ');
                StringBuilder build = new StringBuilder();
                build.Append(LeaderUID + "#" + Name + "#" + LeaderMesh + "#" + Rank + "#" + Title + "#" + RceiveReward +
                             "");
                return build.ToString();
            }

            public void Load(string Line) {
                string[] data = Line.Split('#');
                LeaderUID = uint.Parse(data[0]);
                Name = data[1];
                LeaderMesh = uint.Parse(data[2]);
                Rank = byte.Parse(data[3]);
                Title = byte.Parse(data[4]);
                RceiveReward = byte.Parse(data[5]);
            }
        }

        public class Match {
            public enum StatusFlag : int {
                SwitchOut = 0,
                Watchable = 1,
                Waiting = 3,
                OK = 2,
            }

            public bool Completed;
            public bool Done;
            private Time32 DoneStamp;
            public bool Exported;

            public StatusFlag Flag;

            public uint ID;

            private int Imports;

            public Time32 ImportTime;
            public ushort Index;
            public bool Inside;
            public Map Map;

            public FighterStats[] MatchStats;
            public GamePackets T_ID;

            public ConcurrentDictionary<uint, GameState> Watchers;

            public Match(GamePackets ID, params FighterStats[] teamates) {
                try {
                    Map = Kernel.Maps[700].MakeDynamicMap();
                    T_ID = ID;
                    if (teamates.Length == 1) {
                        Flag = StatusFlag.OK;
                        MatchStats = new FighterStats[1];
                        MatchStats[0] = teamates[0].Team.EliteFighterStats;
                        MatchStats[0].Team.EliteMatch = this;
                        MatchStats[0].Points = 0;
                        MatchStats[0].Cheers = 0;
                        MatchStats[0].Flag = FighterStats.StatusFlag.Qualified;
                        Flag = StatusFlag.OK;
                    }
                    else {
                        Watchers = new ConcurrentDictionary<uint, GameState>();
                        MatchStats = new FighterStats[teamates.Length];
                        for (int i = 0; i < teamates.Length; i++) {
                            MatchStats[i] = teamates[i].Team.EliteFighterStats;
                            MatchStats[i].Team.EliteMatch = this;
                            MatchStats[i].Flag = FighterStats.StatusFlag.Waiting;
                            MatchStats[i].Points = 0;
                            MatchStats[i].Cheers = 0;
                            MatchStats[i].Teleported = false;
                        }

                        if (MatchStats.Length == 3)
                            MatchStats[0].Flag = FighterStats.StatusFlag.None;

                        Imports = 0;
                        Flag = StatusFlag.Waiting;
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }

            public IEnumerable<GameState> Players {
                get {
                    foreach (var team in MatchStats)
                    foreach (var player in team.Team.Players)
                        yield return player;
                }
            }

            public bool OnGoing {
                get { return Flag == StatusFlag.Watchable; }
            }

            public int GroupID {
                get { return (int)ID / 100000 - 1; }
            }

            public uint TimeLeft {
                get {
                    int val = 0;
                    if (MatchStats.Length == 1)
                        val = 0;
                    else
                        val = (ImportTime.AddMinutes(4).TotalMilliseconds - Time32.Now.TotalMilliseconds) / 1000;
                    if (val < 0)
                        val = 0;
                    return (uint)val;
                }
            }

            public void Commit() {
                var matchStats = new FighterStats[MatchStats.Length];
                for (int i = 0; i < matchStats.Length; i++) {
                    matchStats[i] = MatchStats[i].Clone();
                }

                MatchStats = matchStats;
            }

            public FighterStats Return(Func<FighterStats, bool> fn) {
                foreach (var stat in MatchStats)
                    if (stat != null)
                        if (fn(stat)) {
                            if (T_ID == GamePackets.SkillElitePkBrackets)
                                return SkillTeamTournament.Tournaments[GroupID].Teams[stat.Team.UID].EliteFighterStats;
                            else
                                return TeamTournament.Tournaments[GroupID].Teams[stat.Team.UID].EliteFighterStats;
                            // return ElitePKTournament.Tournaments[GroupID].Players[stat.UID];
                        }

                MatchStats[0].Flag = FighterStats.StatusFlag.Qualified;
                return MatchStats[0];
                // return null;
            }


            public void AliveMatch() {
                if (Flag != StatusFlag.OK) {
                    if (Inside && !Done) {
                        foreach (var dismised in MatchStats) {
                            if (Flag != StatusFlag.OK) {
                                if (MatchStats.Length == 2) {
                                    if (MatchStats[0].Winner)
                                        return;
                                    if (MatchStats[1].Winner)
                                        return;
                                }

                                if (!dismised.Team.OnPopulates) {
                                    if (dismised.Waiting) {
                                        dismised.Flag = FighterStats.StatusFlag.Lost;
                                        dismised.Team.EliteFighterStats.Flag = FighterStats.StatusFlag.Lost;
                                    }

                                    if (!dismised.Lost) {
                                        Console.WriteLine("team " + dismised.Team.EliteFighterStats.Name + " has lost");
                                        this.End(dismised.Team);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void Import(StatusFlag flag = StatusFlag.OK) {
                Imports++;
                Exported = Done = false;
                Inside = true;
                Flag = flag;
                ImportTime = Time32.Now;

                if (MatchStats.Length > 1) {
                    Team team1 = null, team2 = null;
                    for (int i = 0; i < MatchStats.Length; i++)
                        if (!MatchStats[i].NoFlag && !MatchStats[i].Lost)
                            if (team1 == null)
                                team1 = MatchStats[i].Team;
                            else
                                team2 = MatchStats[i].Team;

                    team1.EliteFighterStats.Flag = FighterStats.StatusFlag.Fighting;
                    team2.EliteFighterStats.Flag = FighterStats.StatusFlag.Fighting;
                    importPlayer(team1, team2);
                    importPlayer(team2, team1);

                    TeamElitePkBrackets brackets = new TeamElitePkBrackets(T_ID, 1);
                    {
                        brackets.Group = (ushort)GroupID;
                        brackets.GUIType = (ushort)ElitePKTournament.Tournaments[GroupID].State;
                        brackets.TotalMatchesOnRoom = brackets.MatchCount = 1;
                        brackets.Type = TeamElitePkBrackets.StaticUpdate;
                        brackets.Append(this);
                        Kernel.SendWorldMessage(brackets.ToArray());
                    }
                }
                else {
                    Done = true;
                    Flag = StatusFlag.OK;
                    Exported = true;
                    Completed = true;
                    MatchStats[0].Flag = FighterStats.StatusFlag.Qualified;
                }
            }

            private void importPlayer(Team teamone, Team opponent) {
                if (MatchStats.Length > 1) {
                    teamone.EliteFighterStats.Teleported = true;
                    opponent.EliteFighterStats.Teleported = true;
                    //   ushort x = 0; ushort y = 0;
                    foreach (var player in teamone.Players) {
                        if (player.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Ride)) {
                            player.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                        }

                        if (T_ID == GamePackets.SkillElitePkBrackets) {
                            //  player.LoadItemStats();
                            player.Entity.InSkillPk = true;
                        }

                        var coords = Map.RandomCoordinates();
                        player.Entity.Teleport(Map.BaseID, Map.ID, coords.Item1, coords.Item2);
                        player.Entity.BringToLife();
                        player.Entity.SkillTeamWatchingElitePKMatch = null;

                        if (T_ID == GamePackets.SkillElitePkBrackets)
                            player.LoadItemStats(); //.MyEquip.SendAllItems(player.Player.MyEquip.aleternante);
                        TeamElitePKMatchUI ui = new TeamElitePKMatchUI(T_ID);
                        {
                            ui.Append(opponent);
                            ui.TimeLeft = TimeLeft;
                            ui.Type = TeamElitePKMatchUI.BeginMatch;
                            player.Send(ui.ToArray());
                        }
                        player.Send(CreateUpdate().ToArray());
                        player.CantAttack = Time32.Now.AddSeconds(11);
                        player.Entity.PrevPKMode = player.Entity.PKMode;
                        player.Entity.PKMode = Enums.PkMode.Team;

                        Data dat = new Data(true);
                        {
                            dat.UID = player.Entity.UID;
                            dat.ID = Data.ChangePKMode;
                            dat.dwParam = (uint)player.Entity.PKMode;
                            player.Send(dat.ToArray());
                        }
                    }
                }
                else {
                    MatchStats[0].Flag = FighterStats.StatusFlag.Qualified;
                    Done = true;
                    Flag = StatusFlag.OK;
                    Exported = true;
                    Completed = true;
                }
            }

            public void End(Team team) {
                try {
                    if (MatchStats.Length == 1) {
                        MatchStats[0].Flag = FighterStats.StatusFlag.Qualified;
                        Flag = StatusFlag.OK;
                        return;
                    }

                    if (Done) return;
                    Done = true;
                    DoneStamp = Time32.Now;
                    Flag = StatusFlag.OK;
                    team.EliteFighterStats.Flag = FighterStats.StatusFlag.Lost;
                    TeamElitePKMatchUI ui = new TeamElitePKMatchUI(T_ID);
                    {
                        ui.Type = ElitePKMatchUI.Effect;
                        ui.dwParam = ElitePKMatchUI.Effect_Lose;
                        team.SendMesageTeam(ui.ToArray(), 0);

                        var team_win = targetOfWin(team);
                        if (team_win != null) {
                            ui.Append(team_win.Team);
                            ui.dwParam = TeamElitePKMatchUI.Effect_Win;

                            foreach (var target in team_win.Team.Players) {
                                target.Entity.InSkillPk = false;
                                target.Send(ui.ToArray());
                                ui.Type = TeamElitePKMatchUI.EndMatch;
                                team.SendMesageTeam(ui.ToArray(), 0);
                                target.Send(ui.ToArray());

                                if (T_ID == GamePackets.SkillElitePkBrackets)
                                    target.Inventory.Add(720981, 0, 1);

                                ui.Type = TeamElitePKMatchUI.Reward;
                                target.Send(ui.ToArray());

                                if (Imports == 2 || MatchStats.Length != 3 || MatchStats[0] == null) {
                                    team_win.Flag = FighterStats.StatusFlag.Qualified;
                                }
                                else {
                                    var dictionar = MatchStats.Where(p => !p.Lost).ToArray();
                                    if (dictionar.Length == 2) {
                                        Flag = StatusFlag.SwitchOut;
                                        team_win.Flag = FighterStats.StatusFlag.Waiting;
                                        team_win.OnNextMatch = true;
                                    }
                                    else
                                        team_win.Flag = FighterStats.StatusFlag.Qualified;
                                }
                            }
                        }
                    }
                    TeamElitePkBrackets brackets = new TeamElitePkBrackets(T_ID, 1);
                    {
                        brackets.Group = (ushort)GroupID;
                        brackets.GUIType = (ushort)ElitePKTournament.Tournaments[GroupID].State;
                        brackets.TotalMatchesOnRoom = brackets.MatchCount = 1;
                        brackets.Type = TeamElitePkBrackets.StaticUpdate;
                        brackets.Append(this);
                        Kernel.SendWorldMessage(brackets.ToArray());
                    }
                    foreach (var clien in Watchers.Values)
                        if (clien.Entity.MapID == Map.ID)
                            LeaveWatch(clien);
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }

            public void Export() {
                try {
                    if (Time32.Now > DoneStamp.AddSeconds(3) || TimeLeft == 0) {
                        #region !Done

                        if (!Done) {
                            if (MatchStats.Length == 1) {
                                MatchStats[0].Flag = FighterStats.StatusFlag.Qualified;
                                Flag = StatusFlag.OK;
                                Exported = true;

                                Exported = true;
                                Completed = true;
                                return;
                            }
                            else {
                                var fighters = MatchStats.Where(p => p.Fighting).OrderBy(p => p.Points).ToArray();
                                for (int i = 0; i < fighters.Length; i++) {
                                    var team = fighters[i];
                                    End(team.Team);
                                }
                                //var fighters = MatchStats.Where(p => p.Fighting).ToArray();
                                //if (fighters.Length > 1)
                                //{
                                //    if (fighters[0].Fighting && fighters[1].Fighting)
                                //    {
                                //        if (fighters[0].Points == fighters[1].Points)
                                //            End(fighters.Last(p => p.Team.UID == fighters[0].Team.UID).Team);
                                //        else
                                //        {
                                //            if (fighters[0].Points > fighters[1].Points)
                                //                End(fighters.Last(p => p.Team.UID == fighters[0].Team.UID).Team);
                                //            else
                                //                End(fighters.Last(p => p.Team.UID == fighters[1].Team.UID).Team);
                                //        }
                                //    }
                                //}
                                //else if (fighters.Length == 1)
                                //    End(fighters.First(p => p.Team.UID == fighters[0].Team.UID).Team);
                            }
                        }

                        #endregion

                        foreach (var teams in MatchStats) {
                            if (teams != null) {
                                if (!teams.Waiting && teams.Teleported || teams.OnNextMatch) {
                                    foreach (var player in teams.Team.Players) {
                                        if (player.Entity.MapID == Map.ID) {
                                            player.Entity.InSkillPk = false;
                                            if (T_ID == GamePackets.TeamElitePkBrackets) {
                                                var map = TeamTournament.Tournaments[GroupID].WaitingArea;
                                                var coords = map.RandomCoordinates();
                                                player.Entity.Teleport(map.ID, coords.Item1, coords.Item2);
                                            }

                                            else {
                                                player.LoadItemStats();
                                                var map = SkillTeamTournament.Tournaments[GroupID].WaitingArea;
                                                var coords = map.RandomCoordinates();
                                                player.Entity.Teleport(map.ID, coords.Item1, coords.Item2);
                                            }

                                            if (player.Entity.Hitpoints == 0)
                                                player.Entity.BringToLife();
                                        }
                                    }

                                    teams.Teleported = true;
                                }
                            }
                        }

                        Exported = true;
                        Completed = true;
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }

            public void SwitchBetween() {
                if (Imports == 1 && MatchStats.Length == 3 && MatchStats[0] != null && !MatchStats[0].Lost) {
                    MatchStats[0].Flag = FighterStats.StatusFlag.Waiting;
                    if (MatchStats[1].Winner) {
                        MatchStats[1].Teleported = false;
                        MatchStats[1].Flag = FighterStats.StatusFlag.Waiting;
                    }

                    if (MatchStats[2].Winner) {
                        MatchStats[2].Teleported = false;
                        MatchStats[2].Flag = FighterStats.StatusFlag.Waiting;
                    }

                    Completed = false;
                }
            }

            public FighterStats targetOfWin(Team team) {
                var dictionar = MatchStats.Where(p => p.Fighting).ToArray();
                for (int i = 0; i < dictionar.Length; i++)
                    if (dictionar[i] != null)
                        if (dictionar[i].Team.UID != team.UID)
                            return dictionar[i];
                return null;
            }

            public TeamElitePKMatchStats CreateUpdate() {
                TeamElitePKMatchStats stats = new TeamElitePKMatchStats(T_ID);
                stats.Append(this);
                return stats;
            }

            public void BeginWatch(GameState client) {
                if (Watchers.ContainsKey(client.Entity.UID)) return;
                Watchers.TryAdd(client.Entity.UID, client);
                client.Entity.SkillTeamWatchingElitePKMatch = this;
                var coords = Map.RandomCoordinates();
                client.Entity.Teleport(Map.BaseID, Map.ID, coords.Item1, coords.Item2);

                ElitePKWatch watch = new ElitePKWatch(true, MatchStats.Length);

                watch.ID = ID;
                watch.Type = ElitePKWatch.Fighters;
                watch.dwCheers1 = MatchStats[0].Cheers;
                watch.dwCheers2 = MatchStats[1].Cheers;
                watch.Append(MatchStats[0].Name);
                watch.Append(MatchStats[1].Name);
                client.Send(watch.ToArray());
                watch = new ElitePKWatch(true, Watchers.Count);
                watch.ID = ID;
                watch.dwCheers1 = MatchStats[0].Cheers;
                watch.dwCheers2 = MatchStats[1].Cheers;
                foreach (var pClient in Watchers.Values)
                    watch.Append(pClient.Entity.Mesh, pClient.Entity.Name);
                client.Send(watch.ToArray());
                client.Send(CreateUpdate().ToArray());

                UpdateWatchers();
            }

            public void Update() {
                var update = CreateUpdate();
                foreach (var player in Players)
                    if (player != null)
                        if (player.Team.EliteFighterStats.Fighting)
                            player.Send(update.ToArray());
            }

            public void UpdateWatchers() {
                ElitePKWatch watch = new ElitePKWatch(true, Watchers.Count);
                {
                    watch.ID = ID;
                    watch.Type = ElitePKWatch.Watchers;
                    watch.dwCheers1 = MatchStats[0].Cheers;
                    watch.dwCheers2 = MatchStats[1].Cheers;
                    foreach (var pClient in Watchers.Values)
                        watch.Append(pClient.Entity.Mesh, pClient.Entity.Name);
                    foreach (var pClient in Watchers.Values)
                        pClient.Send(watch.ToArray());
                    foreach (var pClient in Players)
                        if (pClient != null)
                            pClient.Send(watch.ToArray());
                }
            }

            public void LeaveWatch(GameState client) {
                if (Watchers.TryRemove(client.Entity.UID, out client)) {
                    client.Entity.SkillTeamWatchingElitePKMatch = null;
                    ElitePKWatch watch = new ElitePKWatch(true);
                    {
                        watch.ID = ID;
                        watch.Type = ElitePKWatch.Leave;
                        client.Send(watch.ToArray());
                        client.Entity.PreviousTeleport();
                    }
                }
            }

            public void Cheer(GameState client, uint fighter) {
                MatchStats.First(p => p != null && p.Team.Contain(fighter)).Team
                    .EliteFighterStats.Cheers++;
                UpdateWatchers();
            }
        }
    }
}