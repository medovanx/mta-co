using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Database;
using MTA.Extensions;
using MTA.Game;
using MTA.Game.Features;
using MTA.Game.ConquerStructures;
using MTA.Game.ConquerStructures.Society;
using MTA.Game.Features.Guilds.Models;
using MTA.Game.Features.House;
using MTA.Game.Items;
using MTA.Game.Npcs;
using MTA.Game.Npcs.ScriptEngine;
using MTA.MaTrix;
using MTA.MrNiTro.Systems;
using MTA.Network;
using MTA.Network.AuthPackets;
using MTA.Network.Cryptography;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using MTA.Network.Sockets;
using MTA.ServerBase;
using MTA.WebServer;
using static MTA.Game.Constants.GameConstants;
using Message = MTA.Network.GamePackets.Message;
using Screen = MTA.Game.Screen;
using Trade = MTA.Game.ConquerStructures.Trade;

namespace MTA;

internal class InClassName(Exception e, bool dont) {
    public Exception E { get; } = e;
    public bool Dont { get; } = dont;
}

internal abstract class Program {
    public static uint NextItemId;
    public static readonly Encoding Encoding = Encoding.Default; //Encoding.GetEncoding("iso-8859-1");
    public static int PlayerCap = 800;
    public static long MaxOn;
    public static ServerSocket[]? AuthServer;
    public static ServerSocket? GameServer;
    public static Counter? EntityUid;
    public static string? GameIp;

    //  public static bool SpookSpawned = false;
    //  public static DateTime SpookTime;
    public static DayOfWeek Today;
    public static ushort GamePort;
    public static List<ushort>? AuthPort;
    public static uint ScreenColor = 0;

    //public static Time32 messtime;
    public static World? World;
    public static GameState[] Values = [];

    public static VariableVault? Vars;

    //public static string Password;
    public static long WeatherType = 0L;
    public static int RandomSeed;
    public static ushort WebServerPort = 9700;
    public static ushort TransferServerPort = 9800;
    public static string? ServerIp;
    public static ushort ServerGamePort;
    public static uint ServerKey = 10000000;
    public static bool MainServer;
    public static bool TransferServer;
    public static bool ServerTransfer;
    public static bool UniquePk;
    public static DateTime LastRandomReset = DateTime.Now;
    public static BlackSpotPacket BlackSpotPacket = new();

    public static uint GetNextItemId() {
        return ++NextItemId;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    public static bool Transfer(GameState game) {
        if (ServerName == null) return false;
        var createTransfer = new Transfer(game, ServerName).GetArray();
        try {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (ServerIp != null) socket.Connect(ServerIp, WebServerPort);
            socket.SendBufferSize = ushort.MaxValue;
            if (socket.Connected) {
                if (socket.RemoteEndPoint != null) socket.SendTo(createTransfer, socket.RemoteEndPoint);
                return true;
            }
        }
        catch (SocketException e) {
            Console.WriteLine(e.Message);
        }

        return false;
    }


    private static void Main() {
        AppDomain.CurrentDomain.UnhandledException +=
            Application_ThreadException;
        var start = Time32.Now;
        RandomSeed =
            Convert.ToInt32(DateTime.Now.Ticks.ToString().Remove(DateTime.Now.Ticks.ToString().Length / 2));
        Kernel.Random = new FastRandom(RandomSeed);
        Console.Title = "MTA Server";
        FindWindow(null, Console.Title);
        Console.WriteLine("Loaded server configuration.");
        const string configFileName = "configuration.ini";
        var iniFile = new IniFile(configFileName);
        GameIp = iniFile.ReadString("configuration", "IP");
        GamePort = iniFile.ReadUInt16("configuration", "GamePort");

        AuthPort = [
            iniFile.ReadUInt16("configuration", "AuthPort")
        ];

        ServerName = iniFile.ReadString("configuration", "ServerName");
        Rates.Load(iniFile);

        Console.WriteLine("loading Transfer config.....");
        // TransferServer
        TransferServer = iniFile.ReadString("TransferServer", "TransferServer", "0") == "1";
        if (TransferServer) {
            TransferServerPort = iniFile.ReadUInt16("TransferServer", "Webport");
            var count = iniFile.ReadUInt16("TransferServer", "count");
            for (var i = 1; i < count + 1; i++) {
                var serverline = iniFile.ReadString("TransferServer", "server" + i);
                var array = serverline.Split(':');
                var server = new TransferServer.Client.TranServer {
                    ID = byte.Parse(array[0]),
                    ip = array[1],
                    port = int.Parse(array[2]),
                    servername = array[3]
                };

                if (!MTA.TransferServer.Client.TranServers.ContainsKey(server.servername))
                    MTA.TransferServer.Client.TranServers.Add(server.servername, server);
            }

            Console.WriteLine(string.Format("TransferServerPort : {0} , ServersCount : {1}", TransferServerPort,
                count));
            foreach (var server in MTA.TransferServer.Client.TranServers.Values)
                Console.WriteLine(string.Format("Server1 :  ID : {0} , IP : {1} , Port : {2}, Name {3} ", server.ID,
                    server.ip, server.port, server.servername));
        }

        //Main
        MainServer = iniFile.ReadString("Transfers", "MainServer", "0") == "1";
        if (MainServer) {
            WebServerPort = iniFile.ReadUInt16("Transfers", "Webport");
            var count = iniFile.ReadUInt16("Transfers", "count");
            for (var i = 1; i < count + 1; i++) {
                var serverline = iniFile.ReadString("Transfers", "server" + i);
                var array = serverline.Split(':');
                var server = new WebServer.Client.TranServer {
                    ip = array[0],
                    servername = array[1],
                    Key = uint.Parse(array[2]),
                    ID = byte.Parse(array[3])
                };
                if (!WebServer.Client.TranServers.ContainsKey(server.ip))
                    WebServer.Client.TranServers.Add(server.ip, server);
            }

            Console.WriteLine($"WebServerPort : {WebServerPort} , ServersCount : {count}");
            foreach (var server in WebServer.Client.TranServers.Values)
                Console.WriteLine(
                    $"Server1 :  IP : {server.ip} , Name : {server.servername} , Key : {server.Key}, ID {server.ID} ");
        }
        else {
            ServerIp = iniFile.ReadString("Transfer", "IP");
            ServerGamePort = iniFile.ReadUInt16("Transfer", "GamePort");
            WebServerPort = iniFile.ReadUInt16("Transfer", "Webport");
            ServerKey = iniFile.ReadUInt32("Transfer", "Key");
            ServerTransfer = iniFile.ReadUInt16("Transfer", "Transfer") == 1;
            Console.WriteLine(
                $"Server IP : {ServerIp}, Game Port {ServerGamePort}, Transfer Port {WebServerPort}, Auth Port : {string.Join(",", AuthPort)}");
        }

        DataHolder.CreateConnection(
            iniFile.ReadString("MySql", "Host"),
            iniFile.ReadString("MySql", "Username"),
            iniFile.ReadString("MySql", "Password"),
            iniFile.ReadString("MySql", "Database")
        );
        EntityUid = new Counter(0);
        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT)) {
            cmd.Select("configuration").Where("Server", ServerName);
            using (var mySqlReader = new MySqlReader(cmd)) {
                if (mySqlReader.Read()) {
                    EntityUid = new Counter(mySqlReader.ReadUInt32("EntityID"));
                    Guild.GuildCounter = new Counter(mySqlReader.ReadUInt32("GuildID"));
                    // Network.GamePackets.ConquerItem.ItemUID = new MTA.Counter(r.ReadUInt32("ItemUID"));
                    ExtraExperienceRate = mySqlReader.ReadUInt32("ExperienceRate");
                    ExtraSpellRate = mySqlReader.ReadUInt32("ProficiencyExperienceRate");
                    ExtraProficiencyRate = mySqlReader.ReadUInt32("SpellExperienceRate");
                    PlayerCap = mySqlReader.ReadInt32("PlayerCap");
                    MaxOn = mySqlReader.ReadInt64("MaxOnline");
                    EntityVariableTable.Load(0, out Vars);
                }
            }
        }

        if (EntityUid.Now == 0) {
            Console.Clear();
            Console.WriteLine("Database error. Please check your MySQL. Server will now close.");
            Console.WriteLine(EntityUid);
            Console.ReadLine();
            return;
        }

        NextItemUid();

        Console.WriteLine("Initializing database.");
        World = new World();
        //  World.Init();           
        ScriptDatabase.LoadSettings();
        ScriptDatabase.LoadNPCScripts();
        Console.WriteLine("Checking LastItem UID.");

        ConquerItemInformation.Load();
        ConquerItemTable.ClearNulledItems();
        if (!ServerTransfer) {
            MonsterInformation.Load();
            MapsTable.Load();
            Map.CreateTimerFactories();
            DMaps.Load();
            ChampionTable.Load();
        }

        {
            if (!ServerTransfer) {
                QuestInfo.Load();
                WelcomeMessage.Load();
                SpellTable.Load();
                ShopFile.Load();
                HonorShop.Load();
                RacePointShop.Load();
                ChampionShop.Load();
                EShopFile.Load();
                EShopV2File.Load();
                StorageManager.Load();
                _ = new Map(2073, DMaps.MapPaths[1015]);
                _ = new Map(2075, DMaps.MapPaths[2075]);
                _ = new Map(3990, DMaps.MapPaths[3990]);
                _ = new Map(3995, DMaps.MapPaths[3995]);
                Kernel.QuizShow = new QuizShow();
                Refinery.Load();
                Values = [];
                _ = new Map(1002, DMaps.MapPaths[1002]);
                _ = new Map(1038, DMaps.MapPaths[1038]);
                _ = new Map(2071, DMaps.MapPaths[2071]);
                _ = new Map(10380, DMaps.MapPaths[10380]);
                SuperGuildWar.Initiate();
                _ = new Map(1509, DMaps.MapPaths[1509]);
                _ = new Map(10002, 2021, DMaps.MapPaths[2021]);
                _ = new Map(8883, 1004, DMaps.MapPaths[1004]);
                PKFreeMaps.Add(8883);
                ClanWar.Initiate();
                EliteGuildWar.EliteGwint();
                Console.WriteLine("Elite Guild war initializated.");
                Furniture.Load();
                House.LoadHouses();
                PokerTables.LoadTables();
                Console.WriteLine("Poker [Money + CPs] Tables Loaded.");
            }

            Flowers.LoadFlowers();
            DataHolder.ReadStats();
            GhRooms.Execute += GHRooms_Execute;
            GhRooms.Start();
            NobilityTable.Load();
            ArenaTable.Load();
            TeamArenaTable.Load();
            GuildTable.Load();
            ChiTable.LoadAllChi();
            Console.WriteLine("Loading Game Clans.");
            Clan.LoadClans();
            Screen.CreateTimerFactories();
            PerfectionTable.Load();
            AuthCryptography.PrepareAuthCryptography();
            Console.WriteLine("Initializing NPC handlers...");
            NpcHandlerRegistry.Initialize();
            NpcHandlerRegistry.RegisterFurnitureHandlers();
            Console.WriteLine("Initializing Item handlers...");
            ItemHandlerRegistry.Initialize();
            Console.WriteLine("Initializing Packet handlers...");
            PacketHandlerRegistry.Initialize();
            _ = new Map(700, DMaps.MapPaths[700]);
            _ = new Map(1730, DMaps.MapPaths[1730]);
            _ = new Map(2068, DMaps.MapPaths[2068]);
            if (!ServerTransfer)
                World.CreateTournaments();
            World.Init(ServerTransfer);
            new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set("Online", 0).Execute();
            Console.WriteLine("Initializing sockets.");
            AuthServer = new ServerSocket[AuthPort.Count];
            for (var i = 0; i < AuthServer.Length; i++) {
                AuthServer[i] = new ServerSocket();
                AuthServer[i].OnClientConnect += AuthServer_OnClientConnect;
                AuthServer[i].OnClientReceive += AuthServer_OnClientReceive;
                AuthServer[i].OnClientDisconnect += AuthServer_OnClientDisconnect;
                AuthServer[i].Enable(AuthPort[i], "0.0.0.0");
                Console.WriteLine("Auth " + i + " server  online.");
            }

            {
                GameServer = new ServerSocket();
                GameServer.OnClientConnect += GameServer_OnClientConnect;
                GameServer.OnClientReceive += GameServer_OnClientReceive;
                GameServer.OnClientDisconnect += GameServer_OnClientDisconnect;
                GameServer.Enable(GamePort, "0.0.0.0");
                Console.WriteLine("Game server online.");
                Console.WriteLine("Web server online.");
                if (MainServer)
                    WebServer.Client.Create();
                if (TransferServer)
                    //MTA.TransferServer.Client.Create();
                    _handler += Handler;
                if (_handler != null) SetConsoleCtrlHandler(_handler, true);
                Pet.CreateTimerFactories();
                AI.CreateTimerFactories();
                MatrixMob.CreateTimerFactories();
                Console.WriteLine("Testing Npcs");
                var client = new GameState(null) {
                    Entity = new Entity(EntityFlag.Monster, false) {
                        MapID = 1002
                    }
                };
                var req = new NpcRequest();
                req.Deserialize(new byte[28]);
                Npcs.GetDialog(req, client);
                new PerfectionScore().GetRankingList();
                new PerfectionRank().UpdateRanking();
                Console.WriteLine("Loading Booths");
                Booths.Load();
            }
            Console.WriteLine(
                $"Server has been loaded in {(Time32.Now - start).Value / 1000.0:F2} seconds and is now online and ready to accept players.",
                ConsoleColor.Green);
            GC.Collect();
            WorkConsole();
        }
    }

    /// <summary>
    /// Intentional infinite loop - server console runs until process termination.
    /// Continuously reads commands from the console and dispatches them to the command handler.
    /// </summary>
    private static void WorkConsole() {
        while (true)
            try {
                CommandsAi(Console.ReadLine());
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        // ReSharper disable once FunctionNeverReturns
    }

    /// <summary>
    /// Handles console commands.
    /// Parses the command, identifies the command type, and executes the appropriate action.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public static void CommandsAi(string command) {
        try {
            var data = command.Split(' ');
            switch (data[0]) {
                case "@nob": {
                    NobilityTable.Load();
                    break;
                }
                case "@reloadnpc": {
                    World.ScriptEngine.Check_Updates();
                    Console.WriteLine("New System's Npc Reloaded.");
                    break;
                }
                case "@campion": {
                    foreach (var client in Kernel.GamePool.Values)
                        if (client.ChampionStats.SignedUp)
                            client.Send(Champion.ChampionKernel.SignUp().BuildPacket());
                    //   Game.Champion.QualifyEngine.DoSignup(client);
                    break;
                }
                case "@save": {
                    Save();
                }
                    break;
                case "@exit": {
                    GameServer?.Disable();
                    if (AuthServer != null)
                        foreach (var t in AuthServer)
                            t.Disable();

                    if (ServerName != null) {
                        new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                            .Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", ServerName).Execute();
                        if (Vars != null) EntityVariableTable.Save(0, Vars);
                        Parallel.ForEach(Values, client => {
                            client.Send("Server will exit for 5 min to fix some bugs, please be paitent !");
                            client.Disconnect();
                        });

                        Kernel.SendWorldMessage(
                            new Message(
                                string.Concat(new object[]
                                    { "Server will exit for 5 min to fix some bugs, please be paitent" }),
                                Color.Black,
                                0x7db), Values);
                        CommandsAi("@save");

                        new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                            .Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", ServerName).Execute();
                    }
                    Environment.Exit(0);
                }
                    break;
                case "@restart": {
                    try {
                        Kernel.SendWorldMessage(
                            new Message(string.Concat(new object[] { "Server Will Be Restart Now !" }), Color.Black,
                                0x7db), Values);
                        CommandsAi("@save");
                        if (ServerName != null) {
                            new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                                .Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", ServerName)
                                .Execute();
                            var wc = Values.ToArray();
                            foreach (var client in wc) {
                                client.Send("Server Will Be Restart Now !");
                                client.Disconnect();
                            }
                            GameServer?.Disable();
                            if (AuthServer != null)
                                foreach (var t in AuthServer)
                                    t.Disable();

                            new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                                .Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", ServerName)
                                .Execute();
                        }

                        Application.Restart();
                        Environment.Exit(0);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        Console.ReadLine();
                    }
                }
                    break;
            }
        }
        catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }

    private static void GameServer_OnClientReceive(byte[] buffer, int length, ClientWrapper obj) {
        var client = obj.Connector as GameState;
        if (client is { Exchange: true }) {
            client.Exchange = false;
            client.Action = 1;
            var crypto = new GameCryptography(Encoding.GetBytes(GameCryptographyKey));
            var otherData = new byte[length];
            Array.Copy(buffer, otherData, length);
            crypto.Decrypt(otherData, length);

            var extra = false;
            var pos = 0;
            if (BitConverter.ToInt32(otherData, length - 140) == 128) //no extra packet
            {
                pos = length - 140;
                client.Cryptography.Decrypt(buffer, length);
            }
            else if (BitConverter.ToInt32(otherData, length - 176) == 128) //extra packet
            {
                pos = length - 176;
                extra = true;
                client.Cryptography.Decrypt(buffer, length - 36);
            }

            var len = BitConverter.ToInt32(buffer, pos);
            pos += 4;
            if (len != 128) {
                client.Disconnect();
                return;
            }

            var pubKey = new byte[128];
            for (var x = 0; x < len; x++, pos++) pubKey[x] = buffer[pos];

            var pubKeyStr = Encoding.GetString(pubKey);
            client.Cryptography = client.DHKeyExchange.HandleClientKeyPacket(pubKeyStr, client.Cryptography);

            if (extra) {
                var data = new byte[36];
                Buffer.BlockCopy(buffer, length - 36, data, 0, 36);
                ProcessData(data, 36, client);
            }
        }
        else {
            if (client != null) ProcessData(buffer, length, client);
        }
    }

    private static void ProcessData(byte[] buffer, int length, GameState client) {
        client.Cryptography.Decrypt(buffer, length);
        client.Queue.Enqueue(buffer, length);
        while (client.Queue.CanDequeue()) {
            var data = client.Queue.Dequeue();
            Task.Factory.StartNew(() => PacketHandler.HandlePacket(data, client));
        }
    }

    private static void GameServer_OnClientConnect(ClientWrapper obj) {
        var client = new GameState(obj);
        client.Send(client.DHKeyExchange.CreateServerKeyPacket());
        obj.Connector = client;
    }

    private static void GameServer_OnClientDisconnect(ClientWrapper obj) {
        (obj.Connector as GameState)?.Disconnect();
    }

    private static void GHRooms_Execute() {
        #region Rooms FBandSS

        #region Room1

        if (!Room1) {
            var entered1 = 0;
            foreach (var player in Kernel.GamePool.Values) {
                if (player.Entity is { MapID: 1543, Dead: false })
                    entered1++;
            }

            switch (entered1) {
                case > 1:
                    Room1 = true;
                    break;
                case 1: {
                    foreach (var player in Kernel.GamePool.Values)
                        if (player.Entity is { MapID: 1543, Dead: false })
                            if (Time32.Now > player.Entity.WaitingTimeFB.AddSeconds(20)) {
                                player.Entity.ConquerPoints += Room1Price;
                                Room1Price = 0;
                                player.Entity.Teleport(1002, 311, 290);
                            }

                    break;
                }
            }
        }
        else {
            var alive1 = Kernel.GamePool.Values.Count(player => player.Entity is { MapID: 1543, Dead: false });

            if (alive1 == 1)
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity.MapID == 1543) {
                        if (!player.Entity.Dead) //winner 
                        {
                            player.Entity.ConquerPoints += Room1Price * 2;
                            player.Entity.WaitingTimeFB = Time32.Now;
                            Room1 = false;
                            Kernel.SendWorldMessage(
                                new Message(
                                    string.Concat(new object[] {
                                        "Congratulations! ", player.Entity.Name, " has won ", Room1Price * 2,
                                        "  CPs FB/SS in Room 1."
                                    }), Color.Black, 0x7db), Values);
                            Room1Price = 0;
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_victory");
                            player.SendScreen(str);
                            player.Entity.WinnerWaiting = Time32.Now;
                            player.Entity.aWinner = true;
                        }
                        else //loser 
                        {
                            player.Entity.Teleport(1002, 311, 290);

                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_failure");
                            player.SendScreen(str);
                            player.Entity.Action = Enums.ConquerAction.None;
                            player.ReviveStamp = Time32.Now;
                            player.Attackable = false;

                            player.Entity.TransformationID = 0;
                            player.Entity.RemoveFlag(Update.Flags.Dead);
                            player.Entity.RemoveFlag(Update.Flags.Ghost);
                            player.Entity.Hitpoints = player.Entity.MaxHitpoints;

                            player.Entity.Ressurect();
                        }
                    }
        }

        #endregion

        #region Room2

        if (!Room2) {
            var entered2 = Kernel.GamePool.Values.Count(player => player.Entity is { MapID: 1544, Dead: false });

            switch (entered2) {
                case > 1:
                    Room2 = true;
                    break;
                case 1: {
                    foreach (var player in Kernel.GamePool.Values)
                        if (player.Entity is { MapID: 1544, Dead: false })
                            if (Time32.Now > player.Entity.WaitingTimeFB.AddSeconds(20)) {
                                player.Entity.ConquerPoints += Room2Price;
                                Room2Price = 0;
                                player.Entity.Teleport(1002, 311, 290);
                            }

                    break;
                }
            }
        }
        else {
            var alive2 = 0;
            foreach (var player in Kernel.GamePool.Values)
                if (player.Entity is { MapID: 1544, Dead: false })
                    alive2++;

            if (alive2 == 1)
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity.MapID == 1544) {
                        if (!player.Entity.Dead) //winner 
                        {
                            player.Entity.ConquerPoints += Room2Price * 2;
                            player.Entity.WaitingTimeFB = Time32.Now;
                            Room2 = false;
                            Kernel.SendWorldMessage(
                                new Message(
                                    string.Concat(new object[] {
                                        "Congratulations! ", player.Entity.Name, " has won ", Room2Price * 2,
                                        "  CPs FB/SS in Room 2."
                                    }), Color.Black, 0x7db), Values);
                            Room2Price = 0;
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_victory");
                            player.SendScreen(str);
                            player.Entity.WinnerWaiting = Time32.Now;
                            player.Entity.aWinner = true;
                        }
                        else //loser 
                        {
                            player.Entity.Teleport(1002, 311, 290);
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_failure");
                            player.SendScreen(str);
                            player.Entity.Action = Enums.ConquerAction.None;
                            player.ReviveStamp = Time32.Now;
                            player.Attackable = false;

                            player.Entity.TransformationID = 0;
                            player.Entity.RemoveFlag(Update.Flags.Dead);
                            player.Entity.RemoveFlag(Update.Flags.Ghost);
                            player.Entity.Hitpoints = player.Entity.MaxHitpoints;

                            player.Entity.Ressurect();
                        }
                    }
        }

        #endregion

        #region Room3

        if (!Room3) {
            var entered3 = 0;
            foreach (var player in Kernel.GamePool.Values)
                if (player.Entity is { MapID: 1545, Dead: false })
                    entered3++;

            if (entered3 > 1)
                Room3 = true;
            else if (entered3 == 1)
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity is { MapID: 1545, Dead: false })
                        if (Time32.Now > player.Entity.WaitingTimeFB.AddSeconds(20)) {
                            player.Entity.ConquerPoints += Room3Price;
                            Room3Price = 0;
                            player.Entity.Teleport(1002, 299, 281);
                        }
        }
        else {
            var alive3 = 0;
            foreach (var player in Kernel.GamePool.Values) {
                if (player.Entity is { MapID: 1545, Dead: false })
                    alive3++;
            }

            if (alive3 == 1)
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity.MapID == 1545) {
                        if (!player.Entity.Dead) //winner 
                        {
                            player.Entity.ConquerPoints += Room3Price * 2;
                            player.Entity.WaitingTimeFB = Time32.Now;
                            Room3 = false;
                            Kernel.SendWorldMessage(
                                new Message(
                                    string.Concat(new object[] {
                                        "Congratulations! ", player.Entity.Name, " has won ", Room3Price * 2,
                                        "  CPs FB/SS in Room 3."
                                    }), Color.Black, 0x7db), Values);
                            Room3Price = 0;
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_victory");
                            player.SendScreen(str);
                            player.Entity.WinnerWaiting = Time32.Now;
                            player.Entity.aWinner = true;
                        }
                        else //loser 
                        {
                            player.Entity.Teleport(1002, 311, 290);
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_failure");
                            player.SendScreen(str);
                            player.Entity.Action = Enums.ConquerAction.None;
                            player.ReviveStamp = Time32.Now;
                            player.Attackable = false;

                            player.Entity.TransformationID = 0;
                            player.Entity.RemoveFlag(Update.Flags.Dead);
                            player.Entity.RemoveFlag(Update.Flags.Ghost);
                            player.Entity.Hitpoints = player.Entity.MaxHitpoints;

                            player.Entity.Ressurect();
                        }
                    }
        }

        #endregion

        #region Room4

        if (!Room4) {
            var entered4 = Kernel.GamePool.Values.Count(player => player.Entity is { MapID: 1546, Dead: false });

            switch (entered4) {
                case > 1:
                    Room4 = true;
                    break;
                case 1: {
                    foreach (var player in Kernel.GamePool.Values)
                        if (player.Entity is { MapID: 1546, Dead: false })
                            if (Time32.Now > player.Entity.WaitingTimeFB.AddSeconds(20)) {
                                player.Entity.ConquerPoints += Room4Price;
                                Room4Price = 0;
                                player.Entity.Teleport(1002, 311, 290);
                            }

                    break;
                }
            }
        }
        else {
            var alive4 = 0;
            foreach (var player in Kernel.GamePool.Values)
                if (player.Entity is { MapID: 1546, Dead: false })
                    alive4++;

            if (alive4 == 1)
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity.MapID == 1546) {
                        if (!player.Entity.Dead) //winner 
                        {
                            player.Entity.ConquerPoints += Room4Price * 2;
                            player.Entity.WaitingTimeFB = Time32.Now;
                            Room4 = false;
                            Kernel.SendWorldMessage(
                                new Message(
                                    string.Concat(new object[] {
                                        "Congratulations! ", player.Entity.Name, " has won ", Room4Price * 2,
                                        "  CPs FB/SS in Room 4."
                                    }), Color.Black, 0x7db), Values);
                            Room4Price = 0;
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_victory");
                            player.SendScreen(str);
                            player.Entity.WinnerWaiting = Time32.Now;
                            player.Entity.aWinner = true;
                        }
                        else //loser 
                        {
                            player.Entity.Teleport(1002, 311, 290);
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_failure");
                            player.SendScreen(str);
                            player.Entity.Action = Enums.ConquerAction.None;
                            player.ReviveStamp = Time32.Now;
                            player.Attackable = false;

                            player.Entity.TransformationID = 0;
                            player.Entity.RemoveFlag(Update.Flags.Dead);
                            player.Entity.RemoveFlag(Update.Flags.Ghost);
                            player.Entity.Hitpoints = player.Entity.MaxHitpoints;

                            player.Entity.Ressurect();
                        }
                    }
        }

        #endregion

        #region Room5

        if (!Room5) {
            var entered5 = 0;
            foreach (var player in Kernel.GamePool.Values)
                if (player.Entity is { MapID: 1547, Dead: false })
                    entered5++;

            switch (entered5) {
                case > 1:
                    Room5 = true;
                    break;
                case 1: {
                    foreach (var player in Kernel.GamePool.Values)
                        if (player.Entity is { MapID: 1547, Dead: false })
                            if (Time32.Now > player.Entity.WaitingTimeFB.AddSeconds(20)) {
                                player.Entity.ConquerPoints += Room5Price;
                                Room5Price = 0;
                                player.Entity.Teleport(1002, 311, 290);
                            }

                    break;
                }
            }
        }
        else {
            var alive5 = Kernel.GamePool.Values.Count(player => player.Entity is { MapID: 1547, Dead: false });

            if (alive5 == 1)
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity.MapID == 1547) {
                        if (!player.Entity.Dead) //winner 
                        {
                            player.Entity.ConquerPoints += Room5Price * 2;
                            player.Entity.WaitingTimeFB = Time32.Now;
                            Room5 = false;
                            Kernel.SendWorldMessage(
                                new Message(
                                    string.Concat(new object[] {
                                        "Congratulations! ", player.Entity.Name, " has won ", Room5Price * 2,
                                        "  CPs FB/SS in Room 5."
                                    }), Color.Black, 0x7db), Values);
                            Room5Price = 0;
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_victory");
                            player.SendScreen(str);
                            player.Entity.WinnerWaiting = Time32.Now;
                            player.Entity.aWinner = true;
                        }
                        else //loser 
                        {
                            player.Entity.Teleport(1002, 311, 290);
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_failure");
                            player.SendScreen(str);
                            player.Entity.Action = Enums.ConquerAction.None;
                            player.ReviveStamp = Time32.Now;
                            player.Attackable = false;

                            player.Entity.TransformationID = 0;
                            player.Entity.RemoveFlag(Update.Flags.Dead);
                            player.Entity.RemoveFlag(Update.Flags.Ghost);
                            player.Entity.Hitpoints = player.Entity.MaxHitpoints;

                            player.Entity.Ressurect();
                        }
                    }
        }

        #endregion

        #region Room6

        if (!Room6) {
            var entered6 = 0;
            foreach (var player in Kernel.GamePool.Values) {
                if (player.Entity is { MapID: 1548, Dead: false })
                    entered6++;
            }

            switch (entered6) {
                case > 1:
                    Room6 = true;
                    break;
                case 1: {
                    foreach (var player in Kernel.GamePool.Values)
                        if (player.Entity is { MapID: 1548, Dead: false })
                            if (Time32.Now > player.Entity.WaitingTimeFB.AddSeconds(20)) {
                                player.Entity.ConquerPoints += Room6Price;
                                Room6Price = 0;
                                player.Entity.Teleport(1002, 311, 290);
                            }

                    break;
                }
            }
        }
        else {
            var alive6 = Kernel.GamePool.Values.Count(player => player.Entity is { MapID: 1548, Dead: false });

            if (alive6 != 1) return;
            {
                foreach (var player in Kernel.GamePool.Values)
                    if (player.Entity.MapID == 1548) {
                        if (!player.Entity.Dead) //winner 
                        {
                            player.Entity.ConquerPoints += Room6Price * 2;
                            player.Entity.WaitingTimeFB = Time32.Now;
                            Room6 = false;
                            Kernel.SendWorldMessage(
                                new Message(
                                    string.Concat(new object[] {
                                        "Congratulations! ", player.Entity.Name, " has won ", Room6Price * 2,
                                        "  CPs FB/SS in Room 6."
                                    }), Color.White, 0x7db), Values);
                            Room6Price = 0;
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_victory");
                            player.SendScreen(str);
                            player.Entity.WinnerWaiting = Time32.Now;
                            player.Entity.aWinner = true;
                        }
                        else //loser 
                        {
                            player.Entity.Teleport(1002, 311, 290);
                            var str = new _String(true) {
                                UID = player.Entity.UID,
                                TextsCount = 1,
                                Type = 10
                            };
                            str.Texts.Add("sports_failure");
                            player.SendScreen(str);
                            player.Entity.Action = Enums.ConquerAction.None;
                            player.ReviveStamp = Time32.Now;
                            player.Attackable = false;

                            player.Entity.TransformationID = 0;
                            player.Entity.RemoveFlag(Update.Flags.Dead);
                            player.Entity.RemoveFlag(Update.Flags.Ghost);
                            player.Entity.Hitpoints = player.Entity.MaxHitpoints;

                            player.Entity.Ressurect();
                        }
                    }
            }
        }

        #endregion

        #endregion
    }

    private static void AuthServer_OnClientReceive(byte[] buffer, int length, ClientWrapper arg3) {
        var player = arg3.Connector as AuthClient;

        player?.Cryptographer.Decrypt(buffer, length);
        player?.Queue.Enqueue(buffer, length);
        while (player != null && player.Queue.CanDequeue()) {
            var packet = player.Queue.Dequeue();

            var len = BitConverter.ToUInt16(packet, 0);
            BitConverter.ToUInt16(packet, 2);
            if (len != 312) continue;
            player.Info = new Authentication();
            player.Info.Deserialize(packet);

            player.Account = new AccountTable(player.Info.Username);
            msvcrt.msvcrt.srand(player.PasswordSeed);

            var fw = new Forward();
            Console.WriteLine("[LOGIN] Username: " + player.Info.Username + ", Password: " +
                              player.Info.Password);
            if (player.Info.Password == player.Account.Password && player.Account.exists)
                fw.Type = Forward.ForwardType.Ready;
            else
                fw.Type = Forward.ForwardType.InvalidInfo;

            if (!MainServer) {
                if ((ServerTransfer && Kernel.TransferredPlayers.Contains(player.Account.EntityID)) ||
                    GameServer == null) {
                    if (fw.Type == Forward.ForwardType.Ready) {
                        var fClient = new GameState(null) {
                            Fake = false
                        };
                        fClient.FakeLoad(player.Account.EntityID, false);
                        fClient.Account = player.Account;
                        if (fClient.FakeLoaded) {
                            if (Transfer(fClient)) {
                                // if (Program.World.DelayedTask == null)
                                //     Program.World.DelayedTask = new MaTrix.DelayedTask();
                                // Program.World.DelayedTask.StartDelayedTask(() =>
                                // {
                                fw.Identifier = player.Account.EntityID + ServerKey;
                                fw.IP = ServerIp;
                                fw.Port = ServerGamePort;
                                player.Send(fw);
                                if (Kernel.TransferredPlayers.Contains(player.Account.EntityID))
                                    Kernel.TransferredPlayers.Remove(player.Account.EntityID);
                                Console.WriteLine("[" + (player.Account.EntityID + ServerKey) + "] " +
                                                  player.Account.Username + " has been redirected to " +
                                                  ServerIp + " : " + ServerGamePort + " .");
                                // }, 100);

                                return;
                            }

                            fw.Type = (Forward.ForwardType)56;
                        }
                        else {
                            fw.Type = (Forward.ForwardType)56;
                        }
                    }
                }
                else {
                    if (fw.Type == Forward.ForwardType.Ready) {
                        fw.Identifier = player.Account.GenerateKey();
                        Kernel.AwaitingPool[fw.Identifier] = player.Account;
                        fw.IP = GameIp;
                        fw.Port = GamePort;
                    }
                }
            }
            else {
                if (fw.Type == Forward.ForwardType.Ready) {
                    fw.Identifier = player.Account.GenerateKey();
                    Kernel.AwaitingPool[fw.Identifier] = player.Account;
                    fw.IP = GameIp;
                    fw.Port = GamePort;
                }
            }

            player.Send(fw);
        }
    }

    private static void AuthServer_OnClientDisconnect(ClientWrapper obj) {
        obj.Disconnect();
    }

    private static void AuthServer_OnClientConnect(ClientWrapper obj) {
        AuthClient authState;
        obj.Connector = authState = new AuthClient(obj);
        authState.Cryptographer = new AuthCryptography();
        var pcs = new PasswordCryptographySeed {
            Seed = Kernel.Random.Next()
        };
        authState.PasswordSeed = pcs.Seed;
        authState.Send(pcs);
    }

    internal static GameState? FindClient(string name) {
        return Values.FirstOrDefault(p => p.Entity.Name == name);
    }


    public static void NextItemUid() {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("items");
        using var reader = new MySqlReader(cmd);
        while (reader.Read()) {
            var uid = reader.ReadUInt32("UID");
            if (uid > 0 && uid > NextItemId) NextItemId = uid;
        }
    }

    #region Rooms

    public static uint Room1Price;
    public static uint Room2Price;
    public static uint Room3Price;
    public static uint Room4Price;
    public static uint Room5Price;
    public static uint Room6Price;
    public static bool Room1;
    public static bool Room2;
    public static bool Room3;
    public static bool Room4;
    public static bool Room5;
    public static bool Room6;
    public static Thread GhRooms = new(1000);

    #endregion Rooms


    #region Closing Events

    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);

    private static EventHandler? _handler;

    // Enum values are required by Windows API SetConsoleCtrlHandler contract
    private enum CtrlType { }

    private static bool Handler(CtrlType sig) {
        // Suppress unused parameter warning - sig is required by API delegate signature
        _ = sig;
        if (MessageBox.Show("Are you sure you want to Exit  ?", "MTA", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return true;
        Console.WriteLine("Saving Before Exiting ...");
        return !Save();
    }

    public static bool Save() {
        try {
            using (var conn = DataHolder.MySqlConnection) {
                conn.Open();
                var connection = conn; // Capture connection in local variable to avoid disposal warning
                Parallel.ForEach(Values, client => {
                    EntityTable.SaveEntity(client, connection);
                    SkillTable.SaveProficiencies(client);
                    SkillTable.SaveSpells(client);
                    ArenaTable.SaveArenaStatistics(client.ArenaStatistic, connection);
                    TeamArenaTable.SaveArenaStatistics(client.TeamArenaStatistic, connection);
                });
            }

            Flowers.SaveFlowers();
            if (ServerName != null)
                new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                    .Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", ServerName).Execute();
            ClanWarArena.Save();
            Console.WriteLine("Saving CMD Done Thanks ,");
        }
        catch (Exception e) {
            Console.WriteLine(e.ToString());
            return false;
        }

        return true;
    }

    #endregion

    #region Exceptions & Logs

    public static void AddVendorLog(string vendor, string buying, string moneyamount, ConquerItem item) {
        string folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
            path = "gmlogs\\VendorLogs\\",
            newPath = Path.Combine(path, folderN);
        if (!File.Exists(newPath + folderN)) Directory.CreateDirectory(Path.Combine(path, folderN));

        if (!File.Exists(newPath + "\\" + DateTime.Now.Day + ".txt")) {
            using var fs = File.Create(newPath + "\\" + DateTime.Now.Day + ".txt");
            fs.Close();
        }

        using var file = new StreamWriter(newPath + "\\" + DateTime.Now.Day + ".txt", true);
        file.WriteLine("------------------------------------------------------------------------------------");
        file.WriteLine("{0} HAS BOUGHT AN ITEM : {2} FROM {1} SHOP - for {3}", vendor, buying, item.ToLog(),
            moneyamount);
        file.WriteLine("------------------------------------------------------------------------------------");
    }

    public static void SaveException(InClassName inClassName) {
        var e = inClassName.E;
        var dont = inClassName.Dont;
        if (e.TargetSite?.Name == "ThrowInvalidOperationException") return;
        if (e.Message.Contains("String reference not set")) return;
        if (!dont)
            Console.WriteLine(e);
        var dt = DateTime.Now;
        var date = dt.Month + "-" + dt.Day + "//";
        if (!Directory.Exists(Application.StartupPath + UnhandledExceptionsPath))
            Directory.CreateDirectory(Application.StartupPath + "\\" + UnhandledExceptionsPath);
        if (!Directory.Exists(Application.StartupPath + "\\" + UnhandledExceptionsPath + date))
            Directory.CreateDirectory(Application.StartupPath + "\\" + UnhandledExceptionsPath + date);
        if (!Directory.Exists(Application.StartupPath + "\\" + UnhandledExceptionsPath + date +
                              e.TargetSite?.Name))
            Directory.CreateDirectory(Application.StartupPath + "\\" + UnhandledExceptionsPath + date +
                                      e.TargetSite?.Name);
        var fullPath = Application.StartupPath + "\\" + UnhandledExceptionsPath + date +
                       e.TargetSite?.Name + "\\";
        var date2 = dt.Hour + "-" + dt.Minute;
        var lines = new List<string> {
            "----Exception message----",
            e.Message,
            "----End of exception message----\r\n",
            "----Stack trace----"
        };
        if (e.StackTrace != null) lines.Add(e.StackTrace);
        lines.Add("----End of stack trace----\r\n");
        File.WriteAllLines(fullPath + date2 + ".txt", lines.ToArray());
    }

    public static void AddDropLog(string name, ConquerItem item) {
        string folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
            path = "gmlogs\\droplogs\\",
            newPath = Path.Combine(path, folderN);
        if (!File.Exists(newPath + folderN)) Directory.CreateDirectory(Path.Combine(path, folderN));

        path = newPath + "\\" + DateTime.Now.Day + ".txt";
        if (!File.Exists(path)) File.AppendAllText(path, "");

        var text = "------------------------------------------------------------------------------------"
                   + Environment.NewLine +
                   $"Player {name} HAS DROPPED AN ITEM : {item.ToLog()} -"
                   + Environment.NewLine +
                   "------------------------------------------------------------------------------------";
        File.AppendAllText(path, text);
    }

    public static void AddTradeLog(Trade first, string firstN, Trade second, string secondN) {
        var folderN = DateTime.Now.Year + "-" + DateTime.Now.Month;
        const string path = @"gmlogs\tradelogs\";
        var newPath = Path.Combine(path, folderN);
        if (!File.Exists(newPath + folderN)) Directory.CreateDirectory(Path.Combine(path, folderN));

        if (!File.Exists(newPath + "\\" + DateTime.Now.Day + ".txt")) {
            using var fs = File.Create(newPath + "\\" + DateTime.Now.Day + ".txt");
            fs.Close();
        }

        using var file = new StreamWriter(newPath + "\\" + DateTime.Now.Day + ".txt", true);
        file.WriteLine("************************************************************************************");
        file.WriteLine("First Person TradeLog ( {0} ) -", firstN);
        file.WriteLine("Gold Traded: " + first.Money);
        file.WriteLine("Conquer Points Traded: " + first.ConquerPoints);

        foreach (var t in first.Items) {
            file.WriteLine(
                "------------------------------------------------------------------------------------");
            file.WriteLine("Item : " + t.ToLog());
            file.WriteLine(
                "------------------------------------------------------------------------------------");
        }

        file.WriteLine("Second Person TradeLog ( {0} ) -", secondN);
        file.WriteLine("Gold Traded: " + second.Money);
        file.WriteLine("Conquer Points Traded: " + second.ConquerPoints);

        foreach (var t in second.Items) {
            file.WriteLine(
                "------------------------------------------------------------------------------------");
            file.WriteLine("Item : " + t.ToLog());
            file.WriteLine(
                "------------------------------------------------------------------------------------");
        }

        file.WriteLine("************************************************************************************");
    }

    public static void AddMobLog(string war, string name, uint cPs = 0, uint item = 0) {
        string folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
            path = "gmlogs\\MobLogs\\",
            newPath = Path.Combine(path, folderN);
        if (!File.Exists(newPath + folderN)) Directory.CreateDirectory(Path.Combine(path, folderN));

        if (!File.Exists(newPath + "\\" + DateTime.Now.Day + ".txt")) {
            using var fs = File.Create(newPath + "\\" + DateTime.Now.Day + ".txt");
            fs.Close();
        }

        using var file = new StreamWriter(newPath + "\\" + DateTime.Now.Day + ".txt", true);
        if (cPs != 0)
            file.WriteLine(name + " got " + cPs + " CPs from the [" + war + "] as prize at " +
                           DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
        else
            file.WriteLine(name + " got " + item + " Item from the [" + war + "] as prize at " +
                           DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
    }

    public static void AddWarLog(string war, string cPs, string name) {
        var folderN = DateTime.Now.Year + "-" + DateTime.Now.Month;
        const string path = @"gmlogs\Warlogs\";
        var newPath = Path.Combine(path, folderN);
        if (!File.Exists(newPath + folderN)) Directory.CreateDirectory(Path.Combine(path, folderN));

        if (!File.Exists(newPath + "\\" + DateTime.Now.Day + ".txt")) {
            using var fs = File.Create(newPath + "\\" + DateTime.Now.Day + ".txt");
            fs.Close();
        }

        using var file = new StreamWriter(newPath + "\\" + DateTime.Now.Day + ".txt", true);
        file.WriteLine(name + " got " + cPs + " CPs from the [" + war + "] as prize at " + DateTime.Now.Hour +
                       ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
    }

    private static void Application_ThreadException(object sender, UnhandledExceptionEventArgs e) {
        SaveException(e.ExceptionObject as Exception);
    }

    public static void SaveException(Exception? e) {
        if (e?.TargetSite?.Name == "ThrowInvalidOperationException")
            return;
        if (e != null && e.Message.Contains("String reference not set"))
            return;

        if (e == null) return;
        Console.WriteLine(e);

        var dt = DateTime.Now;
        var date = dt.Month + "-" + dt.Day + "//";

        if (!Directory.Exists(Application.StartupPath + UnhandledExceptionsPath))
            Directory.CreateDirectory(Application.StartupPath + "\\" + UnhandledExceptionsPath);
        if (!Directory.Exists(Application.StartupPath + "\\" + UnhandledExceptionsPath + date))
            Directory.CreateDirectory(Application.StartupPath + "\\" + UnhandledExceptionsPath + date);
        if (!Directory.Exists(Application.StartupPath + "\\" + UnhandledExceptionsPath + date +
                              e.TargetSite?.Name))
            Directory.CreateDirectory(Application.StartupPath + "\\" + UnhandledExceptionsPath + date +
                                      e.TargetSite?.Name);

        var fullPath = Application.StartupPath + "\\" + UnhandledExceptionsPath + date +
                       e.TargetSite?.Name + "\\";

        var date2 = dt.Hour + "-" + dt.Minute;
        var lines = new List<string> {
            "----Exception message----",
            e.Message,
            "----End of exception message----\r\n",
            "----Stack trace----"
        };

        if (e.StackTrace != null) lines.Add(e.StackTrace);
        lines.Add("----End of stack trace----\r\n");

        //Lines.Add("----Data from exception----");
        //foreach (KeyValuePair<object, object> data in e.Data)
        //    Lines.Add(data.Key.ToString() + "->" + data.Value.ToString());
        //Lines.Add("----End of data from exception----\r\n");

        File.WriteAllLines(fullPath + date2 + ".txt", lines.ToArray());
    }

    #endregion
}

internal class MatrixTimes {
    public static DateTime Now => DateTime.Now;

    public class Start {
        public static int Dizzy = 49;

        public static bool SkillTeam => Now is { Hour: 21, Minute: 1 };

        public static bool TeamPk => Now is { Hour: 20, Minute: 1 };

        public static bool PoleDomination => Now.Hour is 5 or 17;

        public static bool ClanWarArena2 => Now is { Hour: 22, Minute: 25 } or { Hour: 10, Minute: 25 };

        public static bool ClanWarArena => Now is { Hour: 22, Minute: 30 } || Now is { Hour: 10, Minute: 30 };

        public static bool HeroOfGame => Now.Minute == 30;

        public static bool Fbss2 => Now.Minute is >= 21 and < 23;

        public static bool Nobilty => Now.Minute is >= 20 and <= 23;
    }

    public class End {
        public static bool Fbss => Now.Minute >= 23;

        public static bool Nobility => Now.Minute is >= 24 and <= 30;
    }
}

public class Rates {
    public static uint GuildWar;
    public static uint ChangeName;
    public static uint King;
    public static uint Prince;
    public static uint Duke;
    public static uint EliteGw;
    public static uint SkillTeam1;
    public static uint SkillTeam2;
    public static uint SkillTeam3;
    public static uint SkillTeam4;
    public static uint WeeklyPk;
    public static uint TopGuild;
    public static uint MrConquer;
    public static uint UniquePk;
    public static uint Portals;
    public static uint HeroOfGame;
    public static uint NobilityPrize;
    public static uint LastMan;
    public static uint Daily;
    public static uint Fbss;
    public static uint Poles;
    public static uint ClanWarDay;
    public static uint SoulP6;
    public static uint SoulP7;
    public static uint ChangeBody;
    public static uint Refinery6;
    public static uint Twar;
    public static uint StWar;
    public static uint Ctf;
    public static uint Cps;
    public static uint ClanWarCity;
    public static uint ClassPk;
    public static uint DeathMatches;
    public static uint Lobby;
    public static uint Hunter;
    public static uint Thief;
    public static uint HousePromote;
    public static uint ItemBox;
    public static uint HouseUpgrade;
    public static uint MonthlyPk;
    public static uint TopSpouse;
    public static uint Bosses;
    public static uint Night;
    public static uint Broadcast;
    public static uint GuildFee;
    public static uint TeleportFee;
    public static uint DragonBall;
    public static uint Meteor;
    public static string? VoteUrl;
    public static uint Reincarnation;
    public static uint DonationRate;

    public static string? Servername => ServerName;

    public static void Load(IniFile iniFile) {
        DragonBall = iniFile.ReadUInt32("Rates", "DragonBall");
        Meteor = iniFile.ReadUInt32("Rates", "Meteor");
        GuildWar = iniFile.ReadUInt32("Rates", "GuildWar");
        EliteGw = iniFile.ReadUInt32("Rates", "EliteGw");
        Bosses = iniFile.ReadUInt32("Rates", "Bosses");
        Broadcast = iniFile.ReadUInt32("Rates", "Broadcast");
        TeleportFee = iniFile.ReadUInt32("Rates", "TeleportFee");
        GuildFee = iniFile.ReadUInt32("Rates", "GuildFee");
        King = iniFile.ReadUInt32("Rates", "King");
        Prince = iniFile.ReadUInt32("Rates", "Prince");
        Duke = iniFile.ReadUInt32("Rates", "Duke");
        Reincarnation = iniFile.ReadUInt32("Rates", "Reincarnation");
        MonthlyPk = iniFile.ReadUInt32("Rates", "MonthlyPk");
        TopSpouse = iniFile.ReadUInt32("Rates", "TopSpouse");
        ChangeName = iniFile.ReadUInt32("Rates", "ChangeName");
        HousePromote = iniFile.ReadUInt32("Rates", "HousePromote");
        ItemBox = iniFile.ReadUInt32("Rates", "ItemBox");
        Night = iniFile.ReadUInt32("Rates", "Night");
        VoteUrl = iniFile.ReadString("Rates", "VoteUrl");
        Portals = iniFile.ReadUInt32("Rates", "Portals");
        SkillTeam1 = iniFile.ReadUInt32("Rates", "SkillTeam1");
        SkillTeam2 = iniFile.ReadUInt32("Rates", "SkillTeam2");
        SkillTeam3 = iniFile.ReadUInt32("Rates", "SkillTeam3");
        SkillTeam4 = iniFile.ReadUInt32("Rates", "SkillTeam4");
        SoulP6 = iniFile.ReadUInt32("Rates", "SoulP6");
        SoulP7 = iniFile.ReadUInt32("Rates", "SoulP7");
        Refinery6 = iniFile.ReadUInt32("Rates", "Refinery6");
        ChangeBody = iniFile.ReadUInt32("Rates", "ChangeBody");
        UniquePk = iniFile.ReadUInt32("Rates", "UniquePk");
        WeeklyPk = iniFile.ReadUInt32("Rates", "WeeklyPk");
        Fbss = iniFile.ReadUInt32("Rates", "Fbss");
        Poles = iniFile.ReadUInt32("Rates", "Poles");
        ClanWarDay = iniFile.ReadUInt32("Rates", "ClanWarDay");
        LastMan = iniFile.ReadUInt32("Rates", "LastMan");
        Daily = iniFile.ReadUInt32("Rates", "Daily");
        TopGuild = iniFile.ReadUInt32("Rates", "TopGuild");
        MrConquer = iniFile.ReadUInt32("Rates", "MrConquer");
        NobilityPrize = iniFile.ReadUInt32("Rates", "NobilityPrize");
        HeroOfGame = iniFile.ReadUInt32("Rates", "HeroOfGame");
        Twar = iniFile.ReadUInt32("Rates", "Twar");
        StWar = iniFile.ReadUInt32("Rates", "StWar");
        Ctf = iniFile.ReadUInt32("Rates", "Ctf");
        Cps = iniFile.ReadUInt32("Rates", "Cps");
        ClanWarCity = iniFile.ReadUInt32("Rates", "ClanWarCity");
        ClassPk = iniFile.ReadUInt32("Rates", "ClassPk");
        DeathMatches = iniFile.ReadUInt32("Rates", "DeathMatches");
        Lobby = iniFile.ReadUInt32("Rates", "Lobby");
        Hunter = iniFile.ReadUInt32("Rates", "Hunter");
        Thief = iniFile.ReadUInt32("Rates", "Thief");
        DonationRate = iniFile.ReadUInt32("Rates", "DonationRate");
    }
}