using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MTA.Network;
using MTA.Database;
using MTA.Network.Sockets;
using MTA.Network.AuthPackets;
using MTA.Game;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using MTA.Network.GamePackets;
using TrialModePlugin;
using MTA.Client;
using System.Diagnostics;
using System.Threading.Tasks;
using MTA.Franko;

namespace MTA
{
    class Program
    {
        #region Rooms
        public static uint Room1Price = 0;
        public static uint Room2Price = 0;
        public static uint Room3Price = 0;
        public static uint Room4Price = 0;
        public static uint Room5Price = 0;
        public static uint Room6Price = 0;
        public static bool Room1 = false;
        public static bool Room2 = false;
        public static bool Room3 = false;
        public static bool Room4 = false;
        public static bool Room5 = false;
        public static bool Room6 = false;
        public static bool Onctf = false;
        public static ServerBase.Thread GHRooms = new ServerBase.Thread(1000);
        #endregion Rooms
        public static uint _NextItemID;
        public static uint NextItemID
        {
            get
            {
                return _NextItemID++;
            }
        }
        public static Encoding Encoding = ASCIIEncoding.Default;//Encoding.GetEncoding("iso-8859-1");
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #region Cyclone War
        public static bool cycolne = false;
        public static bool cycolne1 = false;
        #endregion Cyclone War
        public static int PlayerCap = 800;
        public static bool NemesisTyrantSpanwed = false;
        public static long MaxOn = 0;
        public static ServerSocket[] AuthServer;
        public static ServerSocket GameServer;
        public static Counter EntityUID;
        public static string GameIP;
        public static bool SpookAlive = false;
        //  public static bool SpookSpawned = false;
        //  public static DateTime SpookTime;
        public static DayOfWeek Today;
        public static MemoryCompressor MCompressor = new MemoryCompressor();
        public static bool CpuUsageTimer = true;
        public static int CpuUse = 0;
        public static long Carnaval = 0;
        public static long Carnaval2 = 0;
        public static long Carnaval3 = 0;
        public static ushort GamePort;
        public static List<ushort> AuthPort;
        public static DateTime StartDate;
        public static bool reseted = false;
        public static uint ScreenColor = 0;
        public static DateTime RestartDate = DateTime.Now.AddHours(24);
        public static bool restarted = false;
        public static bool WarEnd = false;
        public static bool uniquepk = false;
        public static uint mess = 10;
        //public static Time32 messtime;
        public static World World;
        public static Client.GameState[] Values = new Client.GameState[0];
        public static VariableVault Vars;
        //public static string Password;
        public static long WeatherType = 0L;
        public static bool TestingMode = false;
        public static bool AllTest = false;
        public static int RandomSeed = 0;
        public static void BackUP()
        {
            //var program = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            //var Directories = Directory.GetDirectories(program + "/PremiumSoft/");
            //var path =  Directories.FirstOrDefault();
            //if (File.Exists(path + "/navicat.exe"))
            //{

            //    Console.WriteLine("Backuping.................");            
            //    var info = new ProcessStartInfo();

            //    info.WorkingDirectory = path;
            //    info.FileName = path+ "/navicat.exe";
            //    info.Arguments = " /schedule matrix";         
            //    info.UseShellExecute = true;            
            //    var p = Process.Start(info);
            //    p.WaitForExit(); 
            //    Console.WriteLine("Backup Done");
            //}

        }
        public static bool Transfer(GameState game)
        {
            byte[] CreateTransfer = new MTA.WebServer.Transfer(game, Constants.ServerName).GetArray();
            try
            {
                System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                socket.Connect(ServerIP, WebServerPort);
                socket.SendBufferSize = ushort.MaxValue;
                if (socket.Connected)
                {
                    socket.SendTo(CreateTransfer, socket.RemoteEndPoint);
                    return true;
                }
            }
            catch (System.Net.Sockets.SocketException e) { Console.WriteLine(e.Message); }

            return false;
        }
        public static ushort WebServerPort = 9700;
        public static ushort TransferServerPort = 9800;
        public static string ServerIP;
        public static ushort ServerGamePort;
        public static uint ServerKey = 10000000;
        public static bool MainServer = false;
        public static bool TransferServer = false;
        public static bool ServerTransfer = false;
        public static bool ALEXPC = false;

        #region Translate
        public static void SaveTranslate()
        {
            var file = Environment.CurrentDirectory + "Translate.txt";
            if (File.Exists(file))
                File.Delete(file);

            StreamWriter writer = new StreamWriter(File.Create(file), Encoding);
            writer.AutoFlush = true;
            foreach (var item in Kernel.Translateed)
                writer.WriteLine(item.Key + "@@" + item.Value);
            writer.Close();

        }
        public static void LoadTranslate()
        {
            var file = Environment.CurrentDirectory + "Translate.txt";

            if (File.Exists(file))
            {
                string[] text = File.ReadAllLines(file, Encoding);

                for (int x = 0; x < text.Length; x++)
                {
                    string line = text[x];
                    string[] split = line.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length < 1)
                        continue;
                    var key = split[0];
                    if (split.Length < 2)
                        continue;
                    var value = split[1];
                    if (!Kernel.Translateed.ContainsKey(key))
                        Kernel.Translateed.Add(key, value);
                }
            }

        }
        public static string Translate(string Text, string To)
        {
            if (To == "en")
                return Text;

            if (Kernel.Translateed.ContainsKey(Text))
                return Kernel.Translateed[Text];

            try
            {
                // this is the service root uri for the Microsoft Translator service  
                var serviceRootUri =
                               new Uri("https://api.datamarket.azure.com/Bing/MicrosoftTranslator/");

                // this is the Account Key I generated for this app 
                var accountKey = "1TGOJYN9IrRlIpYYQq4vVW0cfVyVC6sWMO34CsQyJKw";

                // the TranslatorContainer gives us access to the Microsoft Translator services 
                Microsoft.TranslatorContainer tc = new Microsoft.TranslatorContainer(serviceRootUri);

                // Give the TranslatorContainer access to your subscription 
                tc.Credentials = new System.Net.NetworkCredential(accountKey, accountKey);

                // Generate the query 
                var languagesForTranslationQuery = tc.GetLanguagesForTranslation();

                // Call the query to get the results as an Array 
                var availableLanguages = languagesForTranslationQuery.Execute().ToArray();

                // Generate the query 
                var translationQuery = tc.Translate(Text, To, null);

                // Call the query and get the results as a List 
                var translationResults = translationQuery.Execute().ToList();

                // Verify there was a result 
                if (translationResults.Count() <= 0)
                {
                    return " ";
                }

                // In case there were multiple results, pick the first one 
                var translationResult = translationResults.First();

                if (!Kernel.Translateed.ContainsKey(Text))
                    Kernel.Translateed.Add(Text, translationResult.Text);

                return translationResult.Text;
            }
            catch
            {
                return Text;
            }

        }

        #endregion
        public static void Members30Guilds_Save(string epk)
        {
            try
            {
                var array = Kernel.Members30Guilds.ToArray();
                int len = array.Length;
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                writer.Write(len);
                foreach (var item in array)
                    writer.Write(item);

                string SQL = "UPDATE `matrixvariable` SET data=@data where ID = '" + epk + "' ;";
                byte[] rawData = stream.ToArray();
                using (var conn = DataHolder.MySqlConnection)
                {
                    conn.Open();
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = SQL;
                        cmd.Parameters.AddWithValue("@data", rawData);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void Members30Guilds_Load(string epk)
        {
            try
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
                {
                    cmd.Select("matrixvariable").Where("ID", epk);
                    using (MySqlReader rdr = new MySqlReader(cmd))
                    {
                        if (rdr.Read())
                        {
                            byte[] data = rdr.ReadBlob("data");
                            if (data.Length > 0)
                            {
                                using (var stream = new MemoryStream(data))
                                using (var reader = new BinaryReader(stream))
                                {
                                    int len = reader.ReadInt32();
                                    for (int i = 0; i < len; i++)
                                    {
                                        var ID = reader.ReadUInt32();
                                        if (!Kernel.Members30Guilds.Contains(ID))
                                            Kernel.Members30Guilds.Add(ID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (var command = new MySqlCommand(MySqlCommandType.INSERT))
                            {
                                command.Insert("matrixvariable").Insert("ID", epk);
                                command.Execute();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
        }

        static void Main(string[] args)
        {
            Program.MCompressor.Optimize();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_ThreadException);
            ALEXPC = true;
            #region VersionChecker
            //if (VersionChecker.IsValidVersion() == false)
            //{
            //    MessageBox.Show("Your Trial Version has expired, Please get the Full version |Contact 011 42 72 19 32");
            //    MTA.Console.WriteLine("Enter Password:");
            //    Program.Password = Console.ReadLine();
            //    if (ClassExtensions.Get64HashCode(Program.Password) != 4586181316755855993)
            //    {
            //        MessageBox.Show("Your Trial Version has expired, Please get the Full version |Contact 011 42 72 19 32");
            //        MTA.Console.WriteLine("Thx. For Waiting...!");
            //        MTA.Console.WriteLine("Wrong Password!");
            //        MTA.Console.WriteLine("If Want To make this Source Work");
            //        MTA.Console.WriteLine("You need to put the right Password. !");
            //        MTA.Console.ReadLine();
            //        MessageBox.Show("You Will Get Hacked. | Contact 011 42 72 19 32");
            //        System.Console.ForegroundColor = System.ConsoleColor.DarkGreen;
            //        System.Console.WindowLeft = System.Console.WindowTop = 0;
            //        System.Console.WindowHeight = System.Console.BufferHeight = System.Console.LargestWindowHeight;
            //        System.Console.WindowWidth = System.Console.BufferWidth = System.Console.LargestWindowWidth;
            //        System.Console.CursorVisible = false;
            //        int width, height;
            //        int[] y;
            //        int[] l;
            //        Initialize(out width, out height, out y, out l);
            //        int ms;
            //        while (true)
            //        {
            //            DateTime t1 = DateTime.Now;
            //            MatrixStep(width, height, y, l);
            //            ms = 10 - (int)((TimeSpan)(DateTime.Now - t1)).TotalMilliseconds;

            //            if (ms > 0)
            //                System.Threading.Thread.Sleep(ms);

            //            if (System.Console.KeyAvailable)
            //                if (System.Console.ReadKey().Key == ConsoleKey.F5)
            //                    Initialize(out width, out height, out y, out l);
            //        }

            //        //   Environment.Exit(0);

            //    }
            //    else
            //    {
            //        VersionChecker.TrialEnd();
            //        MessageBox.Show("Your Trial Version has Renewed, To get the Full version");
            //    }
            //    return;


            //}
            //else
            //{
            //    VersionChecker chk = new VersionChecker();
            //}
            #endregion VersionChecker
            Time32 Start = Time32.Now;
            RandomSeed = Convert.ToInt32(DateTime.Now.Ticks.ToString().Remove(DateTime.Now.Ticks.ToString().Length / 2));
            Kernel.Random = new FastRandom(RandomSeed);
            StartDate = DateTime.Now;
            Console.Title = "MTA CO Server";
            IntPtr hWnd = FindWindow(null, Console.Title);
            Console.WriteLine("Loaded server configuration.");
            string ConfigFileName = "Config\\configuration.ini";
            IniFile IniFile = new IniFile(ConfigFileName);
            GameIP = IniFile.ReadString("configuration", "IP");
            GamePort = IniFile.ReadUInt16("configuration", "GamePort");

            AuthPort = new List<ushort>()
            {
                 IniFile.ReadUInt16("configuration", "AuthPort"),
            };

            Constants.ServerName = IniFile.ReadString("configuration", "ServerName");
            //TestingMode = IniFile.ReadString("configuration", "TestMode", "0") == "1" ? true : false;
            AllTest = IniFile.ReadString("configuration", "AllTest", "0") == "1" ? true : false;
            rates.Load(IniFile);

            Console.WriteLine("loading Transfer config.....");
            // TransferServer
            TransferServer = IniFile.ReadString("TransferServer", "TransferServer", "0") == "1" ? true : false;
            if (TransferServer)
            {
                TransferServerPort = IniFile.ReadUInt16("TransferServer", "Webport");
                var count = IniFile.ReadUInt16("TransferServer", "count");
                for (int i = 1; i < count + 1; i++)
                {
                    var serverline = IniFile.ReadString("TransferServer", "server" + i);
                    var array = serverline.Split(':');
                    TransferServer.Client.TranServer server = new TransferServer.Client.TranServer();
                    server.ID = byte.Parse(array[0]);
                    server.ip = array[1];
                    server.port = int.Parse(array[2]);
                    server.servername = array[3];

                    if (!MTA.TransferServer.Client.TranServers.ContainsKey(server.servername))
                        MTA.TransferServer.Client.TranServers.Add(server.servername, server);
                }
                Console.WriteLine(string.Format("TransferServerPort : {0} , ServersCount : {1}", TransferServerPort, count));
                foreach (var server in MTA.TransferServer.Client.TranServers.Values)
                    Console.WriteLine(string.Format("Server1 :  ID : {0} , IP : {1} , Port : {2}, Name {3} ", server.ID, server.ip, server.port, server.servername));
            }
            //Main
            MainServer = IniFile.ReadString("Transfers", "MainServer", "0") == "1" ? true : false;
            if (MainServer)
            {
                WebServerPort = IniFile.ReadUInt16("Transfers", "Webport");
                var count = IniFile.ReadUInt16("Transfers", "count");
                for (int i = 1; i < count + 1; i++)
                {
                    var serverline = IniFile.ReadString("Transfers", "server" + i);
                    var array = serverline.Split(':');
                    WebServer.Client.TranServer server = new WebServer.Client.TranServer();
                    server.ip = array[0];
                    server.servername = array[1];
                    server.Key = uint.Parse(array[2]);
                    server.ID = byte.Parse(array[3]);
                    if (!WebServer.Client.TranServers.ContainsKey(server.ip))
                        WebServer.Client.TranServers.Add(server.ip, server);
                }
                Console.WriteLine(string.Format("WebServerPort : {0} , ServersCount : {1}", WebServerPort, count));
                foreach (var server in WebServer.Client.TranServers.Values)
                    Console.WriteLine(string.Format("Server1 :  IP : {0} , Name : {1} , Key : {2}, ID {3} ", server.ip, server.servername, server.Key, server.ID));

            }
            else
            {
                ServerIP = IniFile.ReadString("Transfer", "IP");
                ServerGamePort = IniFile.ReadUInt16("Transfer", "GamePort");
                WebServerPort = IniFile.ReadUInt16("Transfer", "Webport");
                ServerKey = IniFile.ReadUInt32("Transfer", "Key");
                ServerTransfer = IniFile.ReadUInt16("Transfer", "Transfer") == 1;
                Console.WriteLine($"Server IP : {ServerIP}, Game Port {ServerGamePort}, Transfer Port {WebServerPort}, Auth Port : {string.Join(",", AuthPort)}");
            }
            Database.DataHolder.CreateConnection(
                IniFile.ReadString("MySql", "Host"),
                IniFile.ReadString("MySql", "Username"),
                 IniFile.ReadString("MySql", "Password"),
                 IniFile.ReadString("MySql", "Database")
                );
            BackUP();
            EntityUID = new Counter(0);
            using (MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("configuration").Where("Server", Constants.ServerName);
                using (MySqlReader r = new MySqlReader(cmd))
                {
                    if (r.Read())
                    {
                        EntityUID = new Counter(r.ReadUInt32("EntityID"));
                        Game.ConquerStructures.Society.Guild.GuildCounter = new MTA.Counter(r.ReadUInt32("GuildID"));
                        // Network.GamePackets.ConquerItem.ItemUID = new MTA.Counter(r.ReadUInt32("ItemUID"));
                        Constants.ExtraExperienceRate = r.ReadUInt32("ExperienceRate");
                        Constants.ExtraSpellRate = r.ReadUInt32("ProficiencyExperienceRate");
                        Constants.ExtraProficiencyRate = r.ReadUInt32("SpellExperienceRate");
                        Constants.MoneyDropRate = r.ReadUInt32("MoneyDropRate");
                        Constants.MoneyDropMultiple = r.ReadUInt32("MoneyDropMultiple");
                        Constants.ConquerPointsDropRate = r.ReadUInt32("ConquerPointsDropRate");
                        Constants.ConquerPointsDropMultiple = r.ReadUInt32("ConquerPointsDropMultiple");
                        Constants.ItemDropRate = r.ReadUInt32("ItemDropRate");
                        Constants.ItemDropQualityRates = r.ReadString("ItemDropQualityString").Split('~');
                        Constants.WebAccExt = r.ReadString("AccountWebExt");
                        Constants.WebVoteExt = r.ReadString("VoteWebExt");
                        Constants.WebDonateExt = r.ReadString("DonateWebExt");
                        Constants.ServerWebsite = r.ReadString("ServerWebsite");
                        Constants.ServerGMPass = r.ReadString("ServerGMPass");
                        PlayerCap = r.ReadInt32("PlayerCap");
                        Database.EntityVariableTable.Load(0, out Vars);
                    }
                }
            }
            if (EntityUID.Now == 0)
            {
                Console.Clear();
                Console.WriteLine("Database error. Please check your MySQL. Server will now close.");
                // Print error
                Console.WriteLine(EntityUID);
                Console.ReadLine();
                return;
            }
            else
                NextItemUID();
            Console.WriteLine("Initializing database.");
            World = new World();
            //  World.Init();           
            ProjectX_V3_Game.Database.ScriptDatabase.LoadSettings();
            ProjectX_V3_Game.Database.ScriptDatabase.LoadNPCScripts();
            Console.WriteLine("Checking LastItem UID.");

            Database.ConquerItemInformation.Load();
            Database.ConquerItemTable.ClearNulledItems();
            if (!ServerTransfer)
            {
                Database.MonsterInformation.Load();
                Database.MapsTable.Load();
                Map.CreateTimerFactories();
                Database.DMaps.Load();
                Database.ChampionTable.Load();
            }
            {
                if (!ServerTransfer)
                {
                    MaTrix.QuestInfo.Load();
                    Database.GameUpdatess.LoadRates();
                    Database.SpellTable.Load();
                    Database.ShopFile.Load();
                    Database.HonorShop.Load();
                    Database.RacePointShop.Load();
                    Database.ChampionShop.Load();
                    Database.EShopFile.Load();
                    Database.EShopV2File.Load();
                    StorageManager.Load();
                    new Map(2073, DMaps.MapPaths[1015]);
                    Game.PoleIslanD.PoleIslanDIni();// PoleIslanD
                    Console.WriteLine("PoleIslanD initializated.");
                    new Map(3990, DMaps.MapPaths[3990]);
                    Game.PoleRakion.PoleRakionIni();// PoleRakion
                    Console.WriteLine("PoleRakion initializated.");
                    new Map(3995, DMaps.MapPaths[3995]);
                    Game.PoleMagice.PoleMagiceIni();// PoleRakion
                    Console.WriteLine("PoleMagice initializated.");
                    Kernel.QuizShow = new Game.ConquerStructures.QuizShow();
                    Refinery.Load();
                    Values = new Client.GameState[0];
                    new Game.Map(1002, Database.DMaps.MapPaths[1002]);
                    new Game.Map(1038, Database.DMaps.MapPaths[1038]);
                    new Game.Map(2071, Database.DMaps.MapPaths[2071]);
                    Game.GuildWar.Initiate();
                    Game.SuperGuildWar.Initiate();
                    new Game.Map(1509, Database.DMaps.MapPaths[1509]);
                    new Game.Map(10002, 2021, Database.DMaps.MapPaths[2021]);
                    new Game.Map(8883, 1004, Database.DMaps.MapPaths[1004]);
                    Constants.PKFreeMaps.Add(8883);
                    Game.ClanWar.Initiate();
                    Console.WriteLine("Guild war initializated.");
                    Game.EliteGuildWar.EliteGwint();
                    Console.WriteLine("Elite Guild war initializated.");
                    Database.Furniture.Load();
                    MaTrix.House.LoadHouses();
                    Database.PokerTables.LoadTables();
                    Console.WriteLine("Poker [Money + CPs] Tables Loaded.");

                }
                Flowers.LoadFlowers();
                Database.DataHolder.ReadStats();
                Database.IPBan.Load();
                GHRooms.Execute += new Action(GHRooms_Execute);
                GHRooms.Start();
                Database.NobilityTable.Load();
                Database.ArenaTable.Load();
                Database.TeamArenaTable.Load();
                Database.GuildTable.Load();
                Game.guildtop.Load();
                Database.ChiTable.LoadAllChi();
                Console.WriteLine("Loading Game Clans.");
                Clan.LoadClans();
                Game.Screen.CreateTimerFactories();
                PerfectionTable.Load();
                Network.Cryptography.AuthCryptography.PrepareAuthCryptography();
                if (!ServerTransfer)
                    World.CreateTournaments();
                World.Init(ServerTransfer);
                new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set("Online", 0).Execute();
                Console.WriteLine("Initializing sockets.");
                AuthServer = new ServerSocket[AuthPort.Count];
                for (int i = 0; i < AuthServer.Length; i++)
                {
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
                        _handler += new EventHandler(Handler);
                    SetConsoleCtrlHandler(_handler, true);
                    MaTrix.Pet.CreateTimerFactories();
                    MaTrix.AI.CreateTimerFactories();
                    MaTrix.MatrixMob.CreateTimerFactories();
                    Console.WriteLine("Testing Npcs");
                    var client = new GameState(null);
                    client.Entity = new Entity(EntityFlag.Monster, false);
                    client.Entity.MapID = 1002;
                    Npcs npc = new Npcs(client);
                    var req = new NpcRequest();
                    req.Deserialize(new byte[28]);
                    Npcs.GetDialog(req, client);
                    client = null;
                    new PerfectionScore().GetRankingList();
                    new PerfectionRank().UpdateRanking();
                    Console.WriteLine("Loading Booths");
                    MaTrix.Booths.Load();
                }
                Console.WriteLine();
                Console.WriteLine(@"+-----------------------------Server:Online-----------------------------------+");
                Console.WriteLine("Server Loaded in " + (Time32.Now - Start) + " Milliseconds.");
                GC.Collect();
                WorkConsole();
            }
        }

        #region Closing Events
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;
        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        private static bool Handler(CtrlType sig)
        {
            if (MessageBox.Show("Are you sure you want to Exit  ?", "MTA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                Console.WriteLine("Saving Before Exiting ...");
                return !Save();
            }
            return true;
        }
        public static bool Save()
        {
            try
            {

                using (var conn = Database.DataHolder.MySqlConnection)
                {
                    conn.Open();
                    Parallel.ForEach(Program.Values, client =>//)
                    //   foreach (Client.GameState client in Program.Values)
                    {
                        // client.Account.Save();
                        Database.EntityTable.SaveEntity(client, conn);
                        Database.SkillTable.SaveProficiencies(client, conn);
                        Database.SkillTable.SaveSpells(client, conn);
                        Database.ArenaTable.SaveArenaStatistics(client.ArenaStatistic, conn);
                        Database.TeamArenaTable.SaveArenaStatistics(client.TeamArenaStatistic, conn);
                        //    Database.ChampionTable.SaveStatistics(client.ChampionStats, conn);
                    });
                }
                Flowers.SaveFlowers();
                new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                Game.ClanWarArena.Save();
                Console.WriteLine("Saving CMD Done Thanks ,");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
        #endregion

        #region Exceptions & Logs
        public static void AddVendorLog(String vendor, string buying, string moneyamount, ConquerItem Item)
        {
            String folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
                Path = "gmlogs\\VendorLogs\\",
                NewPath = System.IO.Path.Combine(Path, folderN);
            if (!File.Exists(NewPath + folderN))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Path, folderN));
            }
            if (!File.Exists(NewPath + "\\" + DateTime.Now.Day + ".txt"))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(NewPath + "\\" + DateTime.Now.Day + ".txt"))
                {
                    fs.Close();
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(NewPath + "\\" + DateTime.Now.Day + ".txt", true))
            {
                file.WriteLine("------------------------------------------------------------------------------------");
                file.WriteLine("{0} HAS BOUGHT AN ITEM : {2} FROM {1} SHOP - for {3}", vendor, buying, Item.ToLog(), moneyamount);
                file.WriteLine("------------------------------------------------------------------------------------");
            }
        }
        public static void SaveException(Exception e, bool dont = false)
        {
            if (e.TargetSite.Name == "ThrowInvalidOperationException") return;
            if (e.Message.Contains("String reference not set")) return;
            if (!dont)
                Console.WriteLine(e);
            var dt = DateTime.Now;
            string date = dt.Month + "-" + dt.Day + "//";
            if (!Directory.Exists(Application.StartupPath + Constants.UnhandledExceptionsPath))
                Directory.CreateDirectory(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath);
            if (!Directory.Exists(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date))
                Directory.CreateDirectory(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date);
            if (!Directory.Exists(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date + e.TargetSite.Name))
                Directory.CreateDirectory(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date + e.TargetSite.Name);
            string fullPath = Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date + e.TargetSite.Name + "\\";
            string date2 = dt.Hour + "-" + dt.Minute;
            List<string> Lines = new List<string>();
            Lines.Add("----Exception message----");
            Lines.Add(e.Message);
            Lines.Add("----End of exception message----\r\n");
            Lines.Add("----Stack trace----");
            Lines.Add(e.StackTrace);
            Lines.Add("----End of stack trace----\r\n");
            File.WriteAllLines(fullPath + date2 + ".txt", Lines.ToArray());
        }
        public static void AddDropLog(String Name, ConquerItem Item)
        {
            String folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
                Path = "gmlogs\\droplogs\\",
                NewPath = System.IO.Path.Combine(Path, folderN);
            if (!File.Exists(NewPath + folderN))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Path, folderN));
            }
            string path = NewPath + "\\" + DateTime.Now.Day + ".txt";
            if (!File.Exists(path)) File.AppendAllText(path, "");

            string text = "------------------------------------------------------------------------------------"
                + Environment.NewLine + string.Format("Player {0} HAS DROPPED AN ITEM : {1} -", Name, Item.ToLog())
                + Environment.NewLine + "------------------------------------------------------------------------------------";
            File.AppendAllText(path, text);
        }
        public static void AddTradeLog(MTA.Game.ConquerStructures.Trade first, String firstN, MTA.Game.ConquerStructures.Trade second, String secondN)
        {
            String folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
                Path = "gmlogs\\tradelogs\\",
                NewPath = System.IO.Path.Combine(Path, folderN);
            if (!File.Exists(NewPath + folderN))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Path, folderN));
            }
            if (!File.Exists(NewPath + "\\" + DateTime.Now.Day + ".txt"))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(NewPath + "\\" + DateTime.Now.Day + ".txt"))
                {
                    fs.Close();
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(NewPath + "\\" + DateTime.Now.Day + ".txt", true))
            {
                file.WriteLine("************************************************************************************");
                file.WriteLine("First Person TradeLog ( {0} ) -", firstN);
                file.WriteLine("Gold Traded: " + first.Money);
                file.WriteLine("Conquer Points Traded: " + first.ConquerPoints);

                for (int i = 0; i < first.Items.Count; i++)
                {
                    file.WriteLine("------------------------------------------------------------------------------------");
                    file.WriteLine("Item : " + first.Items[i].ToLog());
                    file.WriteLine("------------------------------------------------------------------------------------");
                }
                file.WriteLine("Second Person TradeLog ( {0} ) -", secondN);
                file.WriteLine("Gold Traded: " + second.Money);
                file.WriteLine("Conquer Points Traded: " + second.ConquerPoints);

                for (int i = 0; i < second.Items.Count; i++)
                {
                    file.WriteLine("------------------------------------------------------------------------------------");
                    file.WriteLine("Item : " + second.Items[i].ToLog());
                    file.WriteLine("------------------------------------------------------------------------------------");
                }
                file.WriteLine("************************************************************************************");
            }
        }
        public static void AddMobLog(string War, string name, uint CPs = 0, uint item = 0)
        {
            String folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
                    Path = "gmlogs\\MobLogs\\",
                    NewPath = System.IO.Path.Combine(Path, folderN);
            if (!File.Exists(NewPath + folderN))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Path, folderN));
            }
            if (!File.Exists(NewPath + "\\" + DateTime.Now.Day + ".txt"))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(NewPath + "\\" + DateTime.Now.Day + ".txt"))
                {
                    fs.Close();
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(NewPath + "\\" + DateTime.Now.Day + ".txt", true))
            {
                if (CPs != 0)
                    file.WriteLine(name + " got " + CPs + " CPs from the [" + War + "] as prize at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                else
                    file.WriteLine(name + " got " + item + " Item from the [" + War + "] as prize at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
            }
        }
        public static void AddWarLog(string War, string CPs, string name)
        {
            String folderN = DateTime.Now.Year + "-" + DateTime.Now.Month,
                    Path = "gmlogs\\Warlogs\\",
                    NewPath = System.IO.Path.Combine(Path, folderN);
            if (!File.Exists(NewPath + folderN))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Path, folderN));
            }
            if (!File.Exists(NewPath + "\\" + DateTime.Now.Day + ".txt"))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(NewPath + "\\" + DateTime.Now.Day + ".txt"))
                {
                    fs.Close();
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(NewPath + "\\" + DateTime.Now.Day + ".txt", true))
            {
                file.WriteLine(name + " got " + CPs + " CPs from the [" + War + "] as prize at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
            }
        }
        static void Application_ThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            SaveException(e.ExceptionObject as Exception);
        }

        public static void SaveException(Exception e)
        {
            if (e.TargetSite.Name == "ThrowInvalidOperationException")
                return;
            if (e.Message.Contains("String reference not set"))
                return;

            Console.WriteLine(e);

            var dt = DateTime.Now;
            string date = dt.Month + "-" + dt.Day + "//";

            if (!Directory.Exists(Application.StartupPath + Constants.UnhandledExceptionsPath))
                Directory.CreateDirectory(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath);
            if (!Directory.Exists(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date))
                Directory.CreateDirectory(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date);
            if (!Directory.Exists(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date + e.TargetSite.Name))
                Directory.CreateDirectory(Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date + e.TargetSite.Name);

            string fullPath = Application.StartupPath + "\\" + Constants.UnhandledExceptionsPath + date + e.TargetSite.Name + "\\";

            string date2 = dt.Hour + "-" + dt.Minute;
            List<string> Lines = new List<string>();

            Lines.Add("----Exception message----");
            Lines.Add(e.Message);
            Lines.Add("----End of exception message----\r\n");

            Lines.Add("----Stack trace----");
            Lines.Add(e.StackTrace);
            Lines.Add("----End of stack trace----\r\n");

            //Lines.Add("----Data from exception----");
            //foreach (KeyValuePair<object, object> data in e.Data)
            //    Lines.Add(data.Key.ToString() + "->" + data.Value.ToString());
            //Lines.Add("----End of data from exception----\r\n");

            File.WriteAllLines(fullPath + date2 + ".txt", Lines.ToArray());
        }
        #endregion

        private static void WorkConsole()
        {
            while (true)
            {
                try
                {
                    CommandsAI(Console.ReadLine());
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        public static DateTime LastRandomReset = DateTime.Now;
        public static Network.GamePackets.BlackSpotPacket BlackSpotPacket = new Network.GamePackets.BlackSpotPacket();
        public static bool MyPC = true;

        public static void CommandsAI(string command)
        {
            try
            {
                if (command == null)
                    return;
                string[] data = command.Split(' ');
                switch (data[0])
                {
                    case "@testmode":
                        {
                            if (TestingMode)
                            {
                                TestingMode = false;
                                Console.WriteLine("Test Mode Off");
                            }
                            else
                            {
                                TestingMode = true;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Server will exit for 5 min to fix some bugs, please be paitent" }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                CommandsAI("@save");

                                new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                                Database.EntityVariableTable.Save(0, Vars);


                                var WC = Program.Values.ToArray();
                                foreach (Client.GameState client in WC)
                                {
                                    if (client.Account.State != AccountTable.AccountState.GM)
                                    {
                                        client.Send("Server will exit for 5 min to fix some bugs, please be paitent !");
                                        client.Disconnect();
                                    }
                                }

                                if (GuildWar.IsWar)
                                    GuildWar.End();
                                new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                                Console.WriteLine("Test Mode On");
                            }
                            break;
                        }
                    case "@jsspell":
                        {
                            new MaTrix.GUI.SpellControl().ShowDialog();
                            break;
                        }
                    case "@king":
                        Console.WriteLine("Load Server Configuration By Legends !");
                        string ConfigFileName = "Config\\configuration.ini";
                        IniFile IniFile = new IniFile(ConfigFileName);
                        GameIP = IniFile.ReadString("configuration", "IP");
                        GamePort = IniFile.ReadUInt16("configuration", "GamePort");

                        AuthPort = new List<ushort>()
            {
                 IniFile.ReadUInt16("configuration", "AuthPort"),
            };
                        Constants.ServerName = IniFile.ReadString("configuration", "ServerName");
                        //TestingMode = IniFile.ReadString("configuration", "TestMode", "0") == "1" ? true : false;
                        AllTest = IniFile.ReadString("configuration", "AllTest", "0") == "1" ? true : false;
                        rates.Load(IniFile);
                        {
                            break;
                        }
                    case "@nob":
                        {
                            Database.NobilityTable.Load();
                            break;
                        }
                    case "@reloadnpc":// new npc script
                        {
                            World.ScriptEngine.Check_Updates();
                            Console.WriteLine("New System's Npc Reloaded.");
                            break;
                        }
                    case "@guildwaroff":
                        {
                            if (Game.GuildWar.IsWar)
                            {
                                Game.GuildWar.End();
                            }
                            break;
                        }
                    case "@char":
                    case "@cp":
                        {
                            Controlpanel cp = new Controlpanel();
                            cp.ShowDialog();
                            break;
                        }
                    case "@clear":
                        {
                            Console.Clear();
                            MTA.Console.WriteLine("Consle and program Cleared !!");
                            break;
                        }
                    case "@matrix":
                    case "@bigbos":
                        MTA.Console.WriteLine("Server will restart after 10 minutes.");
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 5 minute, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 4 minute 30 second, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 4 minute, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 3 minute 30 second, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 3 minute, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 2 minute 30 second, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 2 minute, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 1 minute 30 second, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 1 minute, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The server will be brought down for maintenance in 30 second, Please exit the game now.", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        MTA.Console.WriteLine("Server will exit after 1 minute.");
                        CommandsAI("@save");
                        System.Threading.Thread.Sleep(0x7530);
                        Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("The Server restarted, Please log in after 2 minutes! ", System.Drawing.Color.Orange, 0x7db), Program.Values);
                        try
                        {
                            CommandsAI("@restart");
                        }
                        catch
                        {
                            MTA.Console.WriteLine("Server cannot exit");
                        }
                        break;
                    case "@flushbans":
                        {
                            Database.IPBan.Load();
                            break;
                        }
                    case "@GUI":
                        {
                            Franko.GUI.GUI Franko = new Franko.GUI.GUI();
                            Franko.ShowDialog();
                            break;
                        }
                    case "@alivetime":
                        {
                            DateTime now = DateTime.Now;
                            TimeSpan t2 = new TimeSpan(StartDate.ToBinary());
                            TimeSpan t1 = new TimeSpan(now.ToBinary());
                            Console.WriteLine("The server has been online " + (int)(t1.TotalHours - t2.TotalHours) + " hours, " + (int)((t1.TotalMinutes - t2.TotalMinutes) % 60) + " minutes.");
                            break;
                        }
                    case "@online":
                        {
                            Console.WriteLine("Online Players Count : " + Kernel.GamePool.Count);
                            string line = "";
                            foreach (Client.GameState pClient in Program.Values)
                                line += pClient.Entity.Name + ",";
                            if (line != "")
                            {
                                line = line.Remove(line.Length - 1);
                                Console.WriteLine("Players : " + line);
                            }
                            break;
                        }
                    case "@memoryusage":
                        {
                            var proc = System.Diagnostics.Process.GetCurrentProcess();
                            Console.WriteLine("Thread count: " + proc.Threads.Count);
                            Console.WriteLine("Memory set(MB): " + ((double)((double)proc.WorkingSet64 / 1024)) / 1024);
                            proc.Close();
                            break;
                        }
                    case "@campion":
                        {
                            foreach (var client in Kernel.GamePool.Values)
                            {
                                if (client.ChampionStats.SignedUp)
                                    client.Send(Champion.ChampionKernel.SignUp().BuildPacket());
                                //   Game.Champion.QualifyEngine.DoSignup(client);
                            }
                            break;
                        }
                    case "@save":
                        {
                            Save();
                        }
                        break;
                    case "@playercap":
                        {
                            try
                            {
                                PlayerCap = int.Parse(data[1]);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    case "@skill":
                        {
                            Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Open();
                            foreach (var clien in Kernel.GamePool.Values)
                            {
                                if (clien.Team == null)
                                    clien.Team = new Game.ConquerStructures.Team(clien);
                                Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Join(clien, 3);
                            }
                            break;
                        }
                    case "@team":
                        {
                            Game.Features.Tournaments.TeamElitePk.TeamTournament.Open();
                            foreach (var clien in Kernel.GamePool.Values)
                            {
                                if (clien.Team == null)
                                    clien.Team = new Game.ConquerStructures.Team(clien);
                                Game.Features.Tournaments.TeamElitePk.TeamTournament.Join(clien, 3);
                            }
                            break;
                        }
                    case "@exit":
                        {
                            GameServer.Disable();
                            for (int i = 0; i < AuthServer.Length; i++)
                            {
                                AuthServer[i].Disable();
                            }

                            new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                            Database.EntityVariableTable.Save(0, Vars);
                            var WC = Program.Values.ToArray();
                            // foreach (Client.GameState client in WC)
                            Parallel.ForEach(Program.Values, client =>
                            {
                                client.Send("Server will exit for 5 min to fix some bugs, please be paitent !");
                                client.Disconnect();
                            });

                            Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Server will exit for 5 min to fix some bugs, please be paitent" }), System.Drawing.Color.Black, 0x7db), Program.Values);
                            CommandsAI("@save");

                            if (GuildWar.IsWar)
                                GuildWar.End();

                            new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                            //  new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", Program._NextItemID).Where("Server", Constants.ServerName).Execute();
                            Environment.Exit(0);
                        }
                        break;

                    case "serverpass":
                        {
                            using (MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT))
                            {
                                cmd.Select("configuration").Where("Server", Constants.ServerName);
                                using (MySqlReader r = new MySqlReader(cmd))
                                {
                                    if (r.Read())
                                        Constants.ServerGMPass = r.ReadString("ServerGMPass");
                                }

                            }
                            break;
                        }
                    case "@pressure":
                        {
                            Console.WriteLine("Genr: " + World.GenericThreadPool.ToString());
                            Console.WriteLine("Send: " + World.SendPool.ToString());
                            Console.WriteLine("Recv: " + World.ReceivePool.ToString());
                            break;
                        }
                    case "@restart":
                        {
                            try
                            {
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Server Will Be Restart Now !" }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                CommandsAI("@save");
                                new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                                //new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", Program._NextItemID).Where("Server", Constants.ServerName).Execute();

                                var WC = Program.Values.ToArray();
                                foreach (Client.GameState client in WC)
                                {
                                    client.Send("Server Will Be Restart Now !");
                                    client.Disconnect();
                                }
                                GameServer.Disable();
                                for (int i = 0; i < AuthServer.Length; i++)
                                {
                                    AuthServer[i].Disable();
                                }
                                if (GuildWar.IsWar)
                                    GuildWar.End();
                                new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", ConquerItem.ItemUID.Now).Where("Server", Constants.ServerName).Execute();
                                Application.Restart();
                                Environment.Exit(0);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Console.ReadLine();
                            }
                        }
                        break;
                    case "@account":
                        {
                            Database.AccountTable account = new AccountTable(data[1]);
                            account.Password = data[2];
                            account.State = AccountTable.AccountState.Player;
                            account.Save();
                        }
                        break;
                    case "@process":
                        {
                            HandleClipboardPacket(command);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void HandleClipboardPacket(string cmd)
        {
            string[] pData = cmd.Split(' ');
            long off = 0, type = 0, val = 0;
            if (pData.Length > 1)
            {
                //@process a:b:c
                //a: offset to modify
                //b: type: 1,2,4,8,u
                //c: value
                string[] oData = pData[1].Split(':');
                if (oData.Length == 3)
                {
                    off = long.Parse(oData[0]);
                    type = long.Parse(oData[1]);
                    if (oData[2] == "u")
                        val = 1337;
                    else
                        val = long.Parse(oData[2]);
                }
            }
            string Data = OSClipboard.GetText();
            //Data = Data.Substring(Data.IndexOf('{') + 1);
            //Data = Data.Replace("};", "").Replace(",", "").Replace("\r", "").Replace("\n", "");
            string[] num = Data.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            byte[] packet = new byte[num.Length + 8];
            for (int i = 0; i < num.Length; i++)
                packet[i] = byte.Parse(num[i], System.Globalization.NumberStyles.HexNumber);
            Writer.WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            if (off != 0)
            {
                switch (type)
                {
                    case 1:
                        {
                            packet[(int)off] = (byte)val;
                            break;
                        }
                    case 2:
                        {
                            Writer.WriteUInt16((ushort)val, (int)off, packet);
                            break;
                        }
                    case 4:
                        {
                            Writer.WriteUInt32((uint)val, (int)off, packet);
                            break;
                        }
                    case 8:
                        {
                            Writer.WriteUInt64((ulong)val, (int)off, packet);
                            break;
                        }
                }
            }
            foreach (var client in Program.Values)
            {
                if (val == 1337 && type == 4)
                    Writer.WriteUInt32(client.Entity.UID, (int)off, packet);
                client.Send(packet);
            }
        }

        static void GameServer_OnClientReceive(byte[] buffer, int length, ClientWrapper obj)
        {
            if (obj.Connector == null)
            {
                obj.Disconnect();
                return;
            }
            Client.GameState Client = obj.Connector as Client.GameState;
            if (Client.Exchange)
            {
                Client.Exchange = false;
                Client.Action = 1;
                var crypto = new Network.Cryptography.GameCryptography(Program.Encoding.GetBytes(Constants.GameCryptographyKey));
                if (Program.TestingMode)
                    crypto = new Network.Cryptography.GameCryptography(Program.Encoding.GetBytes(Constants.GameCryptographyKey));
                byte[] otherData = new byte[length];
                Array.Copy(buffer, otherData, length);
                crypto.Decrypt(otherData, length);

                bool extra = false;
                int pos = 0;
                if (BitConverter.ToInt32(otherData, length - 140) == 128)//no extra packet
                {
                    pos = length - 140;
                    Client.Cryptography.Decrypt(buffer, length);
                }
                else if (BitConverter.ToInt32(otherData, length - 176) == 128)//extra packet
                {
                    pos = length - 176;
                    extra = true;
                    Client.Cryptography.Decrypt(buffer, length - 36);
                }
                int len = BitConverter.ToInt32(buffer, pos); pos += 4;
                if (len != 128)
                {
                    Client.Disconnect();
                    return;
                }
                byte[] pubKey = new byte[128];
                for (int x = 0; x < len; x++, pos++) pubKey[x] = buffer[pos];

                string PubKey = Program.Encoding.GetString(pubKey);
                Client.Cryptography = Client.DHKeyExchange.HandleClientKeyPacket(PubKey, Client.Cryptography);

                if (extra)
                {
                    byte[] data = new byte[36];
                    Buffer.BlockCopy(buffer, length - 36, data, 0, 36);
                    processData(data, 36, Client);
                }
            }
            else
            {
                processData(buffer, length, Client);
            }
        }

        private static void processData(byte[] buffer, int length, Client.GameState Client)
        {
            Client.Cryptography.Decrypt(buffer, length);
            Client.Queue.Enqueue(buffer, length);
            while (Client.Queue.CanDequeue())
            {
                byte[] data = Client.Queue.Dequeue();
                Task.Factory.StartNew(() => Network.PacketHandler.HandlePacket(data, Client));
            }
        }

        static void GameServer_OnClientConnect(ClientWrapper obj)
        {
            Client.GameState client = new Client.GameState(obj);
            client.Send(client.DHKeyExchange.CreateServerKeyPacket());
            obj.Connector = client;
        }

        static void GameServer_OnClientDisconnect(ClientWrapper obj)
        {
            if (obj.Connector != null)
                (obj.Connector as Client.GameState).Disconnect();
            else
                obj.Disconnect();
        }
        static void GHRooms_Execute()
        {
            #region Rooms FBandSS
            #region Room1
            if (Room1 == false)
            {
                int entered1 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1543 && (!Player.Entity.Dead))
                    {
                        entered1++;
                    }
                }
                if (entered1 > 1)
                {
                    Room1 = true;
                }
                else if (entered1 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1543 && (!Player.Entity.Dead))
                        {
                            if (Time32.Now > Player.Entity.WaitingTimeFB.AddSeconds(20))
                            {
                                Player.Entity.ConquerPoints += Room1Price;
                                Room1Price = 0;
                                Player.Entity.Teleport(1002, 311, 290);
                            }
                        }
                    }
                }
            }
            else
            {
                int alive1 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1543 && (!Player.Entity.Dead))
                    {
                        alive1++;
                    }
                }
                if (alive1 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1543)
                        {
                            if (!Player.Entity.Dead)//winner 
                            {
                                Player.Entity.ConquerPoints += Room1Price * 2;
                                Player.Entity.WaitingTimeFB = Time32.Now;
                                Room1 = false;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Congratulations! ", Player.Entity.Name, " has won ", Room1Price * 2, "  CPs FB/SS in Room 1." }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                Room1Price = 0;
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_victory");
                                Player.SendScreen(str, true);
                                Player.Entity.WinnerWaiting = Time32.Now;
                                Player.Entity.aWinner = true;
                            }
                            else//loser 
                            {
                                Player.Entity.Teleport(1002, 311, 290);

                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_failure");
                                Player.SendScreen(str, true);
                                Player.Entity.Action = Game.Enums.ConquerAction.None;
                                Player.ReviveStamp = Time32.Now;
                                Player.Attackable = false;

                                Player.Entity.TransformationID = 0;
                                Player.Entity.RemoveFlag(Update.Flags.Dead);
                                Player.Entity.RemoveFlag(Update.Flags.Ghost);
                                Player.Entity.Hitpoints = Player.Entity.MaxHitpoints;

                                Player.Entity.Ressurect();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Room2
            if (Room2 == false)
            {
                int entered2 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1544 && (!Player.Entity.Dead))
                    {
                        entered2++;
                    }
                }
                if (entered2 > 1)
                {
                    Room2 = true;
                }
                else if (entered2 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1544 && (!Player.Entity.Dead))
                        {
                            if (Time32.Now > Player.Entity.WaitingTimeFB.AddSeconds(20))
                            {
                                Player.Entity.ConquerPoints += Room2Price;
                                Room2Price = 0;
                                Player.Entity.Teleport(1002, 311, 290);
                            }
                        }
                    }
                }
            }
            else
            {
                int alive2 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1544 && (!Player.Entity.Dead))
                    {
                        alive2++;
                    }
                }
                if (alive2 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1544)
                        {
                            if (!Player.Entity.Dead)//winner 
                            {
                                Player.Entity.ConquerPoints += Room2Price * 2;
                                Player.Entity.WaitingTimeFB = Time32.Now;
                                Room2 = false;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Congratulations! ", Player.Entity.Name, " has won ", Room2Price * 2, "  CPs FB/SS in Room 2." }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                Room2Price = 0;
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_victory");
                                Player.SendScreen(str, true);
                                Player.Entity.WinnerWaiting = Time32.Now;
                                Player.Entity.aWinner = true;
                            }
                            else//loser 
                            {
                                Player.Entity.Teleport(1002, 311, 290);
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_failure");
                                Player.SendScreen(str, true);
                                Player.Entity.Action = Game.Enums.ConquerAction.None;
                                Player.ReviveStamp = Time32.Now;
                                Player.Attackable = false;

                                Player.Entity.TransformationID = 0;
                                Player.Entity.RemoveFlag(Update.Flags.Dead);
                                Player.Entity.RemoveFlag(Update.Flags.Ghost);
                                Player.Entity.Hitpoints = Player.Entity.MaxHitpoints;

                                Player.Entity.Ressurect();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Room3
            if (Room3 == false)
            {
                int entered3 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1545 && (!Player.Entity.Dead))
                    {
                        entered3++;
                    }
                }
                if (entered3 > 1)
                {
                    Room3 = true;
                }
                else if (entered3 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1545 && (!Player.Entity.Dead))
                        {
                            if (Time32.Now > Player.Entity.WaitingTimeFB.AddSeconds(20))
                            {
                                Player.Entity.ConquerPoints += Room3Price;
                                Room3Price = 0;
                                Player.Entity.Teleport(1002, 299, 281);
                            }
                        }
                    }
                }
            }
            else
            {
                int alive3 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1545 && (!Player.Entity.Dead))
                    {
                        alive3++;
                    }
                }
                if (alive3 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1545)
                        {
                            if (!Player.Entity.Dead)//winner 
                            {
                                Player.Entity.ConquerPoints += Room3Price * 2;
                                Player.Entity.WaitingTimeFB = Time32.Now;
                                Room3 = false;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Congratulations! ", Player.Entity.Name, " has won ", Room3Price * 2, "  CPs FB/SS in Room 3." }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                Room3Price = 0;
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_victory");
                                Player.SendScreen(str, true);
                                Player.Entity.WinnerWaiting = Time32.Now;
                                Player.Entity.aWinner = true;
                            }
                            else//loser 
                            {
                                Player.Entity.Teleport(1002, 311, 290);
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_failure");
                                Player.SendScreen(str, true);
                                Player.Entity.Action = Game.Enums.ConquerAction.None;
                                Player.ReviveStamp = Time32.Now;
                                Player.Attackable = false;

                                Player.Entity.TransformationID = 0;
                                Player.Entity.RemoveFlag(Update.Flags.Dead);
                                Player.Entity.RemoveFlag(Update.Flags.Ghost);
                                Player.Entity.Hitpoints = Player.Entity.MaxHitpoints;

                                Player.Entity.Ressurect();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Room4
            if (Room4 == false)
            {
                int entered4 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1546 && (!Player.Entity.Dead))
                    {
                        entered4++;
                    }
                }
                if (entered4 > 1)
                {
                    Room4 = true;
                }
                else if (entered4 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1546 && (!Player.Entity.Dead))
                        {
                            if (Time32.Now > Player.Entity.WaitingTimeFB.AddSeconds(20))
                            {
                                Player.Entity.ConquerPoints += Room4Price;
                                Room4Price = 0;
                                Player.Entity.Teleport(1002, 311, 290);
                            }
                        }
                    }
                }
            }
            else
            {
                int alive4 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1546 && (!Player.Entity.Dead))
                    {
                        alive4++;
                    }
                }
                if (alive4 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1546)
                        {
                            if (!Player.Entity.Dead)//winner 
                            {
                                Player.Entity.ConquerPoints += Room4Price * 2;
                                Player.Entity.WaitingTimeFB = Time32.Now;
                                Room4 = false;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Congratulations! ", Player.Entity.Name, " has won ", Room4Price * 2, "  CPs FB/SS in Room 4." }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                Room4Price = 0;
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_victory");
                                Player.SendScreen(str, true);
                                Player.Entity.WinnerWaiting = Time32.Now;
                                Player.Entity.aWinner = true;
                            }
                            else//loser 
                            {
                                Player.Entity.Teleport(1002, 311, 290);
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_failure");
                                Player.SendScreen(str, true);
                                Player.Entity.Action = Game.Enums.ConquerAction.None;
                                Player.ReviveStamp = Time32.Now;
                                Player.Attackable = false;

                                Player.Entity.TransformationID = 0;
                                Player.Entity.RemoveFlag(Update.Flags.Dead);
                                Player.Entity.RemoveFlag(Update.Flags.Ghost);
                                Player.Entity.Hitpoints = Player.Entity.MaxHitpoints;

                                Player.Entity.Ressurect();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Room5
            if (Room5 == false)
            {
                int entered5 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1547 && (!Player.Entity.Dead))
                    {
                        entered5++;
                    }
                }
                if (entered5 > 1)
                {
                    Room5 = true;
                }
                else if (entered5 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1547 && (!Player.Entity.Dead))
                        {
                            if (Time32.Now > Player.Entity.WaitingTimeFB.AddSeconds(20))
                            {
                                Player.Entity.ConquerPoints += Room5Price;
                                Room5Price = 0;
                                Player.Entity.Teleport(1002, 311, 290);
                            }
                        }
                    }
                }
            }
            else
            {
                int alive5 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1547 && (!Player.Entity.Dead))
                    {
                        alive5++;
                    }
                }
                if (alive5 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1547)
                        {
                            if (!Player.Entity.Dead)//winner 
                            {
                                Player.Entity.ConquerPoints += Room5Price * 2;
                                Player.Entity.WaitingTimeFB = Time32.Now;
                                Room5 = false;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Congratulations! ", Player.Entity.Name, " has won ", Room5Price * 2, "  CPs FB/SS in Room 5." }), System.Drawing.Color.Black, 0x7db), Program.Values);
                                Room5Price = 0;
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_victory");
                                Player.SendScreen(str, true);
                                Player.Entity.WinnerWaiting = Time32.Now;
                                Player.Entity.aWinner = true;
                            }
                            else//loser 
                            {
                                Player.Entity.Teleport(1002, 311, 290);
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_failure");
                                Player.SendScreen(str, true);
                                Player.Entity.Action = Game.Enums.ConquerAction.None;
                                Player.ReviveStamp = Time32.Now;
                                Player.Attackable = false;

                                Player.Entity.TransformationID = 0;
                                Player.Entity.RemoveFlag(Update.Flags.Dead);
                                Player.Entity.RemoveFlag(Update.Flags.Ghost);
                                Player.Entity.Hitpoints = Player.Entity.MaxHitpoints;

                                Player.Entity.Ressurect();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Room6
            if (Room6 == false)
            {
                int entered6 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1548 && (!Player.Entity.Dead))
                    {
                        entered6++;
                    }
                }
                if (entered6 > 1)
                {
                    Room6 = true;
                }
                else if (entered6 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1548 && (!Player.Entity.Dead))
                        {
                            if (Time32.Now > Player.Entity.WaitingTimeFB.AddSeconds(20))
                            {
                                Player.Entity.ConquerPoints += Room6Price;
                                Room6Price = 0;
                                Player.Entity.Teleport(1002, 311, 290);
                            }
                        }
                    }
                }
            }
            else
            {
                int alive6 = 0;
                foreach (Client.GameState Player in Kernel.GamePool.Values)
                {
                    if (Player.Entity.MapID == 1548 && (!Player.Entity.Dead))
                    {
                        alive6++;
                    }
                }
                if (alive6 == 1)
                {
                    foreach (Client.GameState Player in Kernel.GamePool.Values)
                    {
                        if (Player.Entity.MapID == 1548)
                        {
                            if (!Player.Entity.Dead)//winner 
                            {
                                Player.Entity.ConquerPoints += Room6Price * 2;
                                Player.Entity.WaitingTimeFB = Time32.Now;
                                Room6 = false;
                                Kernel.SendWorldMessage(new Network.GamePackets.Message(string.Concat(new object[] { "Congratulations! ", Player.Entity.Name, " has won ", Room6Price * 2, "  CPs FB/SS in Room 6." }), System.Drawing.Color.White, 0x7db), Program.Values);
                                Room6Price = 0;
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_victory");
                                Player.SendScreen(str, true);
                                Player.Entity.WinnerWaiting = Time32.Now;
                                Player.Entity.aWinner = true;
                            }
                            else//loser 
                            {
                                Player.Entity.Teleport(1002, 311, 290);
                                _String str = new _String(true)
                                {
                                    UID = Player.Entity.UID,
                                    TextsCount = 1,
                                    Type = 10
                                };
                                str.Texts.Add("sports_failure");
                                Player.SendScreen(str, true);
                                Player.Entity.Action = Game.Enums.ConquerAction.None;
                                Player.ReviveStamp = Time32.Now;
                                Player.Attackable = false;

                                Player.Entity.TransformationID = 0;
                                Player.Entity.RemoveFlag(Update.Flags.Dead);
                                Player.Entity.RemoveFlag(Update.Flags.Ghost);
                                Player.Entity.Hitpoints = Player.Entity.MaxHitpoints;

                                Player.Entity.Ressurect();
                            }
                        }
                    }
                }
            }
            #endregion
            #endregion
        }
        static void AuthServer_OnClientReceive(byte[] buffer, int length, ClientWrapper arg3)
        {
            var player = arg3.Connector as Client.AuthClient;

            player.Cryptographer.Decrypt(buffer, length);

            player.Queue.Enqueue(buffer, length);
            while (player.Queue.CanDequeue())
            {
                byte[] packet = player.Queue.Dequeue();

                ushort len = BitConverter.ToUInt16(packet, 0);
                ushort id = BitConverter.ToUInt16(packet, 2);
                if (len == 312)
                {

                    player.Info = new Authentication();
                    player.Info.Deserialize(packet);
                    string accounts = "accounts";
                    switch (player.Info.Server)
                    {
                        default:
                        case "FastBlade":
                            {
                                accounts = "accounts";
                                break;
                            }
                            //default:
                            //    {
                            //        Console.WriteLine("Invaild Server Name : " + player.Info.Server);
                            //        return;
                            //    }
                    }
                    player.Account = new AccountTable(player.Info.Username, accounts);
                    msvcrt.msvcrt.srand(player.PasswordSeed);

                    Forward Fw = new Forward();

                    if (player.Account.Password == player.Info.Password && player.Account.exists)
                        Fw.Type = Forward.ForwardType.Ready;
                    else
                        Fw.Type = Forward.ForwardType.InvalidInfo;

                    if (IPBan.IsBanned(arg3.IP))
                    {
                        Fw.Type = Forward.ForwardType.Banned;
                        player.Send(Fw);
                        return;
                    }
                    if (!MainServer)
                    {
                        if (ServerTransfer && (Kernel.TransferdPlayers.Contains(player.Account.EntityID)) || GameServer == null)
                        {
                            if (Fw.Type == Network.AuthPackets.Forward.ForwardType.Ready)
                            {
                                var fClient = new GameState(null);
                                fClient.Fake = false;
                                fClient.FakeLoad(player.Account.EntityID, false);
                                fClient.Account = player.Account;
                                if (fClient.FakeLoaded)
                                {
                                    if (Transfer(fClient))
                                    {
                                        // if (Program.World.DelayedTask == null)
                                        //     Program.World.DelayedTask = new MaTrix.DelayedTask();
                                        // Program.World.DelayedTask.StartDelayedTask(() =>
                                        // {
                                        Fw.Identifier = (uint)(player.Account.EntityID + ServerKey);
                                        Fw.IP = ServerIP;
                                        Fw.Port = ServerGamePort;
                                        player.Send(Fw);
                                        if (Kernel.TransferdPlayers.Contains(player.Account.EntityID))
                                            Kernel.TransferdPlayers.Remove(player.Account.EntityID);
                                        Console.WriteLine("[" + (player.Account.EntityID + ServerKey) + "] " + player.Account.Username + " has been redirected to " + ServerIP + " : " + ServerGamePort + " .");
                                        // }, 100);

                                        return;
                                    }
                                    else
                                    {
                                        Fw.Type = (Forward.ForwardType)56;
                                    }
                                }
                                else
                                {
                                    Fw.Type = (Forward.ForwardType)56;
                                }
                            }
                        }
                        else
                        {
                            if (Fw.Type == Network.AuthPackets.Forward.ForwardType.Ready)
                            {
                                Fw.Identifier = player.Account.GenerateKey();
                                Kernel.AwaitingPool[Fw.Identifier] = player.Account;
                                Fw.IP = GameIP;
                                Fw.Port = GamePort;
                            }
                        }
                    }
                    else
                    {
                        if (Fw.Type == Network.AuthPackets.Forward.ForwardType.Ready)
                        {
                            Fw.Identifier = player.Account.GenerateKey();
                            Kernel.AwaitingPool[Fw.Identifier] = player.Account;
                            Fw.IP = GameIP;
                            Fw.Port = GamePort;
                        }
                    }
                    player.Send(Fw);
                }
            }
        }

        static void AuthServer_OnClientDisconnect(ClientWrapper obj)
        {
            obj.Disconnect();
        }

        static void AuthServer_OnClientConnect(ClientWrapper obj)
        {
            Client.AuthClient authState;
            obj.Connector = (authState = new Client.AuthClient(obj));
            authState.Cryptographer = new Network.Cryptography.AuthCryptography();
            Network.AuthPackets.PasswordCryptographySeed pcs = new PasswordCryptographySeed();
            pcs.Seed = Kernel.Random.Next();
            authState.PasswordSeed = pcs.Seed;
            authState.Send(pcs);
        }

        internal static Client.GameState FindClient(string name)
        {
            return Values.FirstOrDefault(p => p.Entity.Name == name);
        }

        #region Matrix Style
        static bool thistime = false;
        private static void MatrixStep(int width, int height, int[] y, int[] l)
        {
            int x;
            thistime = !thistime;
            for (x = 0; x < width; ++x)
            {
                if (x % 11 == 10)
                {
                    if (!thistime)
                        continue;
                }
                else
                {
                    System.Console.SetCursorPosition(x, inBoxY(y[x] - 2 - (l[x] / 40 * 2), height));
                    System.Console.Write(R);
                }
                System.Console.SetCursorPosition(x, y[x]);
                System.Console.Write(R);
                y[x] = inBoxY(y[x] + 1, height);
                System.Console.SetCursorPosition(x, inBoxY(y[x] - l[x], height));
                System.Console.Write(' ');
            }
        }
        private static void Initialize(out int width, out int height, out int[] y, out int[] l)
        {
            int h1;
            int h2 = (h1 = (height = System.Console.WindowHeight) / 2) / 2;
            width = System.Console.WindowWidth - 1;
            y = new int[width];
            l = new int[width];
            int x;
            System.Console.Clear();
            for (x = 0; x < width; ++x)
            {
                y[x] = r.Next(height);
                l[x] = r.Next(h2 * ((x % 11 != 10) ? 2 : 1), h1 * ((x % 11 != 10) ? 2 : 1));
            }
        }
        static Random r = new Random();
        public static DateTime KingsTime;

        static char R
        {
            get
            {
                int t = r.Next(10);
                if (t <= 2)
                    return (char)('0' + r.Next(10));
                else if (t <= 4)
                    return (char)('a' + r.Next(27));
                else if (t <= 6)
                    return (char)('A' + r.Next(27));
                else
                    return (char)(r.Next(32, 255));
            }
        }
        public static int inBoxY(int n, int height)
        {
            n = n % height;
            if (n < 0)
                return n + height;
            else
                return n;
        }
        #endregion Matrix Style



        public static void NextItemUID()
        {
            // Console.Write("Check Last Item UID... ");
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("items"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    //  Console.Write("\b{0}", Loading.NextChar());
                    uint UID = reader.ReadUInt32("UID");
                    if ((UID > 0) && (UID > _NextItemID))
                    {
                        _NextItemID = UID;
                    }
                }
            }
            Console.WriteLine("Ok!");
        }
    }

    public class Matrix_Times
    {
        public static DateTime now
        {
            get
            {
                return DateTime.Now;
            }
        }

        public class Start
        {


            public static bool TheTeam
            {
                get
                {
                    return (now.Hour == 14 || now.Hour == 2) && now.Minute == 10 && now.Second == 1;
                }
            }


            public static bool ClanWar
            {
                get
                {
                    return now.Hour == 14 || now.Hour == 2;
                }
            }
            public static bool EliteGW
            {
                get
                {
                    return (now.Minute == 14);
                }
            }
            public static bool SkillTeam
            {
                get
                {
                    return (now.Hour == 21) && now.Minute == 1;
                }
            }
            public static bool TeamPK
            {
                get
                {
                    return (now.Hour == 20) && now.Minute == 1;
                }
            }

            public static bool CTF
            {
                get
                {
                    return now.Hour == 15 || now.Hour == 3;
                }
            }


            public static bool CrossServer
            {
                get
                {
                    return now.Hour == 22 || now.Hour == 10;
                }
            }
            public static bool PoleDomnation
            {
                get
                {
                    return (now.Hour == 5 || now.Hour == 17);
                }
            }

            public static bool ClanWarArena2
            {
                get
                {
                    return (now.Hour == 22 && now.Minute == 25) || (now.Hour == 10 && now.Minute == 25);
                }
            }
            public static bool ClanWarArena
            {
                get
                {
                    return (now.Hour == 22 && now.Minute == 30) || (now.Hour == 10 && now.Minute == 30);
                }
            }
            public static bool Flashwar
            {
                get
                {
                    return (now.Hour == 13 && now.Minute == 4) || (now.Hour == 19 && now.Minute == 4);
                }
            }
            public static bool ClassWar
            {
                get
                {
                    return (now.Hour == 21 && now.Minute == 3) || (now.Hour == 9 && now.Minute == 0);
                }
            }



            ///////////////////////////////////////////////

            public static bool HeroOfGame
            {
                get
                {
                    return now.Minute == 30;
                }
            }
            public static bool FBSS
            {
                get
                {
                    return now.Minute == 5;
                }
            }
            public static bool FBSS2
            {
                get
                {
                    return now.Minute >= 21 && now.Minute < 23;
                }
            }
            public static bool Cyclone
            {
                get
                {
                    return now.Minute == 57;
                }
            }
            public static bool Cyclone1
            {
                get
                {
                    return now.Minute == 58;
                }
            }
            public static int hunterthief = 42;
            public static int dashbash = 46;

            public static int chase = 45;

            public static int dizzy = 49;

            public static bool Nobilty
            {
                get
                {
                    return now.Minute >= 20 && now.Minute <= 23;
                }
            }


        }
        public class End
        {
            public static bool FBSS
            {
                get
                {
                    return now.Minute >= 23;
                }
            }
            public static int hunterthief = 45;
            public static int dashbash = 48;

            public static bool Cyclone
            {
                get
                {
                    return now.Minute == 59;
                }
            }

            public static int chase = 45;

            public static int dizzy2 = 50;

            public static bool Nobilty
            {
                get
                {
                    return now.Minute >= 24 && now.Minute <= 30;
                }
            }
            /////////////////////

            public static bool EliteGW
            {
                get
                {
                    return now.Minute == 30;
                }
            }
            public static bool ClanWar
            {
                get
                {
                    return now.Hour == 15 || now.Hour == 3;
                }
            }

        }

    }

    public class rates
    {
        public static uint GuildWar;
        public static uint ChangeName;
        public static uint king;
        public static uint prince;
        public static uint duke;

        public static uint EliteGw;
        public static uint SkillTeam1;
        public static uint SkillTeam2;
        public static uint SkillTeam3;
        public static uint SkillTeam4;
        public static uint WeeklyPk;
        public static uint topguild;
        public static uint mrconquer;
        public static uint uniquepk;
        public static uint Portals;
        public static uint heroofgame;
        public static uint NobilityPrize;
        public static uint lastman;
        public static uint Daily;
        public static uint fbss;
        public static uint Poles;
        public static uint Clanwarday;
        public static uint soulp6;
        public static uint soulp7;
        public static uint changebody;
        public static uint ref6;
        public static uint Twar;
        public static uint stwar;
        public static uint ctf;
        public static uint cps;
        public static uint ClanwarCity;
        public static uint ClassPk;
        public static uint DeathMatchs;
        public static uint lobby;
        public static uint hunter;
        public static uint thief;
        public static uint housepromete;
        public static uint itembox;
        public static uint houseupgrade;
        public static uint MonthlyPk;
        public static uint TopSpouse;
        public static uint Bosses;
        public static uint Night;
        public static uint Broadcast;
        public static uint GuildFee;
        public static uint TeleportFee;
        public static uint DragonBall;
        public static uint Meteor;
        public static string VoteUrl;
        public static string coder = "HeMa";
        public static uint Reincarnation;
        public static uint donationrate;
        public static string servername { get { return Constants.ServerName; } }

        public static void Load(IniFile IniFile)
        {
            DragonBall = IniFile.ReadUInt32("Rates", "DragonBall");
            Meteor = IniFile.ReadUInt32("Rates", "Meteor");
            GuildWar = IniFile.ReadUInt32("Rates", "GuildWar");
            EliteGw = IniFile.ReadUInt32("Rates", "questday");
            Bosses = IniFile.ReadUInt32("Rates", "Bosses");
            Broadcast = IniFile.ReadUInt32("Rates", "Broadcast");
            TeleportFee = IniFile.ReadUInt32("Rates", "TeleportFee");
            GuildFee = IniFile.ReadUInt32("Rates", "GuildFee");
            king = IniFile.ReadUInt32("Rates", "king");
            prince = IniFile.ReadUInt32("Rates", "prince");
            duke = IniFile.ReadUInt32("Rates", "duke");
            Reincarnation = IniFile.ReadUInt32("Rates", "Reincarnation");
            MonthlyPk = IniFile.ReadUInt32("Rates", "MonthlyPk");
            TopSpouse = IniFile.ReadUInt32("Rates", "TopSpouse");
            ChangeName = IniFile.ReadUInt32("Rates", "ChangeName");
            housepromete = IniFile.ReadUInt32("Rates", "housepromete");
            itembox = IniFile.ReadUInt32("Rates", "itembox");
            Night = IniFile.ReadUInt32("Rates", "Night");
            VoteUrl = IniFile.ReadString("Rates", "VoteUrl");
            Portals = IniFile.ReadUInt32("Rates", "Portals");
            coder = IniFile.ReadString("Rates", "coder");
            SkillTeam1 = IniFile.ReadUInt32("Rates", "SkillTeam1");
            SkillTeam2 = IniFile.ReadUInt32("Rates", "SkillTeam2");
            SkillTeam3 = IniFile.ReadUInt32("Rates", "SkillTeam3");
            SkillTeam4 = IniFile.ReadUInt32("Rates", "SkillTeam4");
            soulp6 = IniFile.ReadUInt32("Rates", "soulp6");
            soulp7 = IniFile.ReadUInt32("Rates", "soulp7");
            ref6 = IniFile.ReadUInt32("Rates", "ref6");
            changebody = IniFile.ReadUInt32("Rates", "changebody");
            uniquepk = IniFile.ReadUInt32("Rates", "uniquepk");
            WeeklyPk = IniFile.ReadUInt32("Rates", "WeeklyPk");
            fbss = IniFile.ReadUInt32("Rates", "fbss");
            Poles = IniFile.ReadUInt32("Rates", "Poles");
            Clanwarday = IniFile.ReadUInt32("Rates", "Clanwarday");
            lastman = IniFile.ReadUInt32("Rates", "lastman");
            Daily = IniFile.ReadUInt32("Rates", "Daily");
            topguild = IniFile.ReadUInt32("Rates", "topguild");
            mrconquer = IniFile.ReadUInt32("Rates", "mrconquer");
            NobilityPrize = IniFile.ReadUInt32("Rates", "NobilityPrize");
            heroofgame = IniFile.ReadUInt32("Rates", "heroofgame");
            Twar = IniFile.ReadUInt32("Rates", "Twar");
            stwar = IniFile.ReadUInt32("Rates", "stwar");
            ctf = IniFile.ReadUInt32("Rates", "ctf");
            cps = IniFile.ReadUInt32("Rates", "cps");
            ClanwarCity = IniFile.ReadUInt32("Rates", "ClanwarCity");
            ClassPk = IniFile.ReadUInt32("Rates", "ClassPk");
            DeathMatchs = IniFile.ReadUInt32("Rates", "DeathMatchs");
            lobby = IniFile.ReadUInt32("Rates", "lobby");
            hunter = IniFile.ReadUInt32("Rates", "hunter");
            thief = IniFile.ReadUInt32("Rates", "thief");
            donationrate = IniFile.ReadUInt32("Rates", "donationrate");
        }

    }


    public class Kernel32
    {
        public delegate bool ConsoleEventHandler(CtrlType sig);

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(ConsoleEventHandler handler, bool add);
    }
}