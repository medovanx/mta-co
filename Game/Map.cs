using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using MTA.Interfaces;
using System.Text;
using System.Linq;
using MTA.Network.GamePackets;
using System.Threading.Generic;
using MTA.Client;
using System.Collections.Concurrent;
using MTA.Database;

namespace MTA.Game
{
    public class Map
    {
        public DMapPortal[] portals;
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct DMapPortal
        {
            private ushort xCord;
            private ushort yCord;
            public ushort XCord
            {
                get
                {
                    return this.xCord;
                }
                set
                {
                    this.xCord = value;
                }
            }
            public ushort YCord
            {
                get
                {
                    return this.yCord;
                }
                set
                {
                    this.yCord = value;
                }
            }
        }
        public void PopulatePortals(uint amount)
        {
            this.portals = new DMapPortal[amount];
        }
        public void SetPortal(int Position, DMapPortal portal)
        {
            this.portals[Position] = portal;
        }

        public static Counter DynamicIDs = new MTA.Counter(11000)
        {
            Finish =
                0
        };

        public static Enums.ConquerAngle[] Angles = new Enums.ConquerAngle[] {
            Enums.ConquerAngle.SouthWest,
            Enums.ConquerAngle.West,
            Enums.ConquerAngle.NorthWest,
            Enums.ConquerAngle.North,
            Enums.ConquerAngle.NorthEast,
            Enums.ConquerAngle.East,
            Enums.ConquerAngle.SouthEast,
            Enums.ConquerAngle.South };
        public static Floor ArenaBaseFloor = null;
        public Counter EntityUIDCounter = new MTA.Counter(400000);
        public Counter EntityUIDCounter2 = new MTA.Counter(100000);
        public Counter CloneCounter = new MTA.Counter(0);
        public List<Zoning.Zone> Zones = new List<Zoning.Zone>();
        public ushort ID;
        public ushort BaseID;
        public bool WasPKFree = false;
        public Floor Floor;
        public string Path;
        public bool IsDynamic()
        {
            return BaseID != ID;
        }
        public SafeDictionary<uint, Entity> Entities;
      //  public SafeDictionary<uint, Entity> Companions;
        public Dictionary<uint, INpc> Npcs;
        public Dictionary<uint, INpc> Statues = new Dictionary<uint, INpc>();
        public Dictionary<uint, INpc> TempNpcs = new Dictionary<uint, INpc>();
        //   public Dictionary<uint, SobNpcSpawn> Furniture = new Dictionary<uint, SobNpcSpawn>();  
        public ConcurrentDictionary<uint, FloorItem> FloorItems;
        public void AddPole(INpc npc)
        {
            Npcs[npc.UID] = npc;
            Floor[npc.X, npc.Y, MapObjectType.InvalidCast, npc] = false;
        }
        public void AddMonesterTimer()
        {
            Timer = MonsterTimers.Add(this);
        }
        public void RemovePole(INpc npc)
        {
            Npcs.Remove(npc.UID);
            Floor[npc.X, npc.Y, MapObjectType.InvalidCast, null] = true;
        }
        public void AddNpc(INpc npc, bool addquery = false)
        {
            if (Npcs.ContainsKey(npc.UID) == false || addquery)
            {
                if (!addquery)
                    Npcs.Add(npc.UID, npc);
                #region Setting the near coords invalid to avoid unpickable items.
                Floor[npc.X, npc.Y, MapObjectType.InvalidCast, npc] = false;
                if (npc.Mesh / 10 != 108 && (byte)npc.Type < 10)
                {
                    ushort X = npc.X, Y = npc.Y;
                    foreach (Enums.ConquerAngle angle in Angles)
                    {
                        ushort xX = X, yY = Y;
                        UpdateCoordonatesForAngle(ref xX, ref yY, angle);
                        Floor[xX, yY, MapObjectType.InvalidCast, null] = false;
                    }
                }
                #endregion
            }
        }
        public void AddEntity(Entity entity)
        {
            if (entity.UID < 800000 || entity.Body == 1003)
            {
                if (Entities.ContainsKey(entity.UID) == false)
                {
                    Entities.Add(entity.UID, entity);
                    Floor[entity.X, entity.Y, MapObjectType.Monster, entity] = false;
                }
            }
            //else
            //{
            //    if (Companions.ContainsKey(entity.UID) == false)
            //    {
            //        Companions.Add(entity.UID, entity);
            //        Floor[entity.X, entity.Y, MapObjectType.Monster, entity] = false;
            //    }
            //}
        }
        public void RemoveEntity(Entity entity)
        {
            if (Entities.ContainsKey(entity.UID) == true)
            {
                Entities.Remove(entity.UID);
                Floor[entity.X, entity.Y, MapObjectType.Monster, entity] = true;
            }
            //if (Companions.ContainsKey(entity.UID) == true)
            //{
            //    Companions.Remove(entity.UID);
            //    Floor[entity.X, entity.Y, MapObjectType.Monster, entity] = true;
            //}
        }
        public void AddFloorItem(Network.GamePackets.FloorItem floorItem)
        {
            FloorItems.Add(floorItem.UID, floorItem);
            Floor[floorItem.X, floorItem.Y, MapObjectType.Item, floorItem] = false;
        }
        public void RemoveFloorItem(Network.GamePackets.FloorItem floorItem)
        {
            FloorItems.Remove(floorItem.UID);
            Floor[floorItem.X, floorItem.Y, MapObjectType.Item, floorItem] = true;
        }

        public bool SelectCoordonates(ref ushort X, ref ushort Y)
        {
            if (Floor[X, Y, MapObjectType.Item, null])
            {
                bool can = true;
                if (Zones.Count != 0)
                {
                    foreach (Zoning.Zone z in Zones)
                    {
                        if (z.IsPartOfRectangle(new Point() { X = X, Y = Y }))
                        {
                            can = false;
                            break;
                        }
                    }
                }
                if (can)
                    return true;
            }

            foreach (Enums.ConquerAngle angle in Angles)
            {
                ushort xX = X, yY = Y;
                UpdateCoordonatesForAngle(ref xX, ref yY, angle);
                if (Floor[xX, yY, MapObjectType.Item, null])
                {
                    if (Zones.Count != 0)
                    {
                        bool can = true;
                        foreach (Zoning.Zone z in Zones)
                        {
                            if (z.IsPartOfRectangle(new Point() { X = xX, Y = yY })) { can = false; break; }
                        }
                        if (!can)
                            continue;
                    }
                    X = xX;
                    Y = yY;
                    return true;
                }
            }
            return false;
        }
        public static void UpdateCoordonatesForAngle(ref ushort X, ref ushort Y, Enums.ConquerAngle angle)
        {
            sbyte xi = 0, yi = 0;
            switch (angle)
            {
                case Enums.ConquerAngle.North: xi = -1; yi = -1; break;
                case Enums.ConquerAngle.South: xi = 1; yi = 1; break;
                case Enums.ConquerAngle.East: xi = 1; yi = -1; break;
                case Enums.ConquerAngle.West: xi = -1; yi = 1; break;
                case Enums.ConquerAngle.NorthWest: xi = -1; break;
                case Enums.ConquerAngle.SouthWest: yi = 1; break;
                case Enums.ConquerAngle.NorthEast: yi = -1; break;
                case Enums.ConquerAngle.SouthEast: xi = 1; break;
            }
            X = (ushort)(X + xi);
            Y = (ushort)(Y + yi);
        }

        public static void Pushback(ref ushort x, ref ushort y, Enums.ConquerAngle angle, int paces)
        {
            sbyte xi = 0, yi = 0;
            for (int i = 0; i < paces; i++)
            {
                switch (angle)
                {
                    case Enums.ConquerAngle.North: xi = -1; yi = -1; break;
                    case Enums.ConquerAngle.South: xi = 1; yi = 1; break;
                    case Enums.ConquerAngle.East: xi = 1; yi = -1; break;
                    case Enums.ConquerAngle.West: xi = -1; yi = 1; break;
                    case Enums.ConquerAngle.NorthWest: xi = -1; break;
                    case Enums.ConquerAngle.SouthWest: yi = 1; break;
                    case Enums.ConquerAngle.NorthEast: yi = -1; break;
                    case Enums.ConquerAngle.SouthEast: xi = 1; break;
                }
                x = (ushort)(x + xi);
                y = (ushort)(y + yi);
            }
        }
        #region Scenes
        private SceneFile[] Scenes;
        private static string NTString(string value)
        {
            value = value.Remove(value.IndexOf("\0"));
            return value;
        }
        private SceneFile CreateSceneFile(BinaryReader Reader)
        {
            SceneFile file = new SceneFile();
            file.SceneFileName = NTString(Program.Encoding.GetString(Reader.ReadBytes(260)));
            file.Location = new Point(Reader.ReadInt32(), Reader.ReadInt32());
            using (BinaryReader reader = new BinaryReader(new FileStream(Constants.DataHolderPath + file.SceneFileName, FileMode.Open)))
            {
                ScenePart[] partArray = new ScenePart[reader.ReadInt32()];
                for (int i = 0; i < partArray.Length; i++)
                {
                    reader.BaseStream.Seek(0x14cL, SeekOrigin.Current);
                    partArray[i].Size = new Size(reader.ReadInt32(), reader.ReadInt32());
                    reader.BaseStream.Seek(4L, SeekOrigin.Current);
                    partArray[i].StartPosition = new Point(reader.ReadInt32(), reader.ReadInt32());
                    reader.BaseStream.Seek(4L, SeekOrigin.Current);
                    partArray[i].NoAccess = new bool[partArray[i].Size.Width, partArray[i].Size.Height];
                    for (int j = 0; j < partArray[i].Size.Height; j++)
                    {
                        for (int k = 0; k < partArray[i].Size.Width; k++)
                        {
                            partArray[i].NoAccess[k, j] = reader.ReadInt32() == 0;
                            reader.BaseStream.Seek(8L, SeekOrigin.Current);
                        }
                    }
                }
                file.Parts = partArray;
            }
            return file;
        }
        public struct SceneFile
        {
            public string SceneFileName
            {
                get;
                set;
            }
            public Point Location
            {
                get;
                set;
            }
            public ScenePart[] Parts
            {
                get;
                set;
            }
        }
        public struct ScenePart
        {
            public string Animation;
            public string PartFile;
            public Point Offset;
            public int aniInterval;
            public System.Drawing.Size Size;
            public int Thickness;
            public Point StartPosition;
            public bool[,] NoAccess;
        }
        #endregion

        public Map(ushort id, string path)
        {
            if (!Kernel.Maps.ContainsKey(id))
                Kernel.Maps.Add(id, this);
            Npcs = new Dictionary<uint, INpc>();
            Entities = new SafeDictionary<uint, Entity>();
            FloorItems = new ConcurrentDictionary<uint, FloorItem>();
            Floor = new Floor(0, 0, id);
           // Companions = new SafeDictionary<uint, Entity>();
            ID = id;
            BaseID = id;
            if (path == "") path = Database.DMaps.MapPaths[id];
            Path = path;
            #region Loading floor.
            if (File.Exists(Constants.DMapsPath + "\\maps\\" + id.ToString() + ".map"))
            {
             //   Console.WriteLine("Loading " + ID + " DMap : maps\\" + id.ToString() + ".map");
                byte[] buff = File.ReadAllBytes(Constants.DMapsPath + "\\maps\\" + id.ToString() + ".map");
                MemoryStream FS = new MemoryStream(buff);
                BinaryReader BR = new BinaryReader(FS);
                int Width = BR.ReadInt32();
                int Height = BR.ReadInt32();
                Floor = new Game.Floor(Width, Height, ID);
                if (id == 700)
                    ArenaBaseFloor = new Game.Floor(Width, Height, ID);
                for (ushort y = 0; y < Height; y = (ushort)(y + 1))
                {
                    for (ushort x = 0; x < Width; x = (ushort)(x + 1))
                    {
                        bool walkable = !(BR.ReadByte() == 1 ? true : false);
                        Floor[x, y, MapObjectType.InvalidCast, null] = walkable;
                        if (id == 700)
                            ArenaBaseFloor[x, y, MapObjectType.InvalidCast, null] = walkable;
                    }
                }

                BR.Close();
                FS.Close();
            }
            else
            {
                if (File.Exists(Constants.DMapsPath + Path))
                {
              //      Console.WriteLine("Loading " + ID + " DMap : " + Path);
                    byte[] buff = File.ReadAllBytes(Constants.DMapsPath + Path);
                    MemoryStream FS = new MemoryStream(buff);
                    BinaryReader BR = new BinaryReader(FS);
                    BR.ReadBytes(268);
                    int Width = BR.ReadInt32();
                    int Height = BR.ReadInt32();
                    Floor = new Game.Floor(Width, Height, ID);
                    if (id == 700)
                        ArenaBaseFloor = new Game.Floor(Width, Height, ID);
                    for (ushort y = 0; y < Height; y = (ushort)(y + 1))
                    {
                        for (ushort x = 0; x < Width; x = (ushort)(x + 1))
                        {
                            bool walkable = !Convert.ToBoolean(BR.ReadUInt16());
                            Floor[x, y, MapObjectType.InvalidCast, null] = walkable;
                            if (id == 700)
                                ArenaBaseFloor[x, y, MapObjectType.InvalidCast, null] = walkable;
                            BR.BaseStream.Seek(4L, SeekOrigin.Current);
                        }
                        BR.BaseStream.Seek(4L, SeekOrigin.Current);
                    }
                    uint amount = BR.ReadUInt32();
                    PopulatePortals(amount);
                    for (ushort j = 0; j < amount; j = (ushort)(j + 1))
                    {
                        DMapPortal portal = new DMapPortal
                        {
                            XCord = (ushort)BR.ReadUInt32(),
                            YCord = (ushort)BR.ReadUInt32()
                        };
                        SetPortal(j, portal);
                        BR.BaseStream.Seek(4L, SeekOrigin.Current);
                    }
                    // BR.BaseStream.Seek(amount * 12, SeekOrigin.Current);
                    LoadMapObjects(BR);
                    MergeSceneToTextureArea();
                    BR.Close();
                    FS.Close();
                    SaveMap();
                    SavePortals();
                }
            }
            #endregion
            LoadNpcs();
           
            LoadMonsters();
            LoadPortals();

        }
        public Map(ushort id, ushort baseid, string path)
        {
            if (!Kernel.Maps.ContainsKey(id))
                Kernel.Maps.Add(id, this);
            else
                Kernel.Maps[id] = this;
            Npcs = new Dictionary<uint, INpc>();
            Entities = new SafeDictionary<uint, Entity>();
        //    Companions = new SafeDictionary<uint, Entity>();
            FloorItems = new ConcurrentDictionary<uint, FloorItem>();
            ID = id;
            BaseID = baseid;
            Path = path;
            if (String.IsNullOrEmpty(path))
                Path = path = Database.DMaps.MapPaths[baseid];
            Floor = new Floor(0, 0, id);

            #region Loading floor.
            if (id != baseid && baseid == 700 && ArenaBaseFloor != null)
            {
                Floor = new Game.Floor(ArenaBaseFloor.Bounds.Width, ArenaBaseFloor.Bounds.Height, ID);
                for (ushort y = 0; y < ArenaBaseFloor.Bounds.Height; y = (ushort)(y + 1))
                {
                    for (ushort x = 0; x < ArenaBaseFloor.Bounds.Width; x = (ushort)(x + 1))
                    {
                        Floor[x, y, MapObjectType.InvalidCast, null] = !ArenaBaseFloor[x, y, MapObjectType.InvalidCast, null];
                    }
                }
            }
            else
            {
                if (File.Exists(Constants.DMapsPath + "\\maps\\" + baseid.ToString() + ".map"))
                {
                   // Console.WriteLine("Loading " + ID + " DMap : maps\\" + id.ToString() + ".map");
                    byte[] buff = File.ReadAllBytes(Constants.DMapsPath + "\\maps\\" + baseid.ToString() + ".map");
                    MemoryStream FS = new MemoryStream(buff);
                    BinaryReader BR = new BinaryReader(FS);
                    int Width = BR.ReadInt32();
                    int Height = BR.ReadInt32();

                    Floor = new Game.Floor(Width, Height, ID);

                    for (ushort y = 0; y < Height; y = (ushort)(y + 1))
                    {
                        for (ushort x = 0; x < Width; x = (ushort)(x + 1))
                        {
                            Floor[x, y, MapObjectType.InvalidCast, null] = !(BR.ReadByte() == 1 ? true : false);
                        }
                    }
                    BR.Close();
                    FS.Close();
                }
                else
                {
                    if (File.Exists(Constants.DMapsPath + Path))
                    {
                   //     Console.WriteLine("Loading "+ID+" DMap : " + Path);
                        FileStream FS = new FileStream(Constants.DMapsPath + Path, FileMode.Open);
                        BinaryReader BR = new BinaryReader(FS);
                        BR.ReadBytes(268);
                        int Width = BR.ReadInt32();
                        int Height = BR.ReadInt32();

                        Floor = new Game.Floor(Width, Height, ID);

                        for (ushort y = 0; y < Height; y = (ushort)(y + 1))
                        {
                            for (ushort x = 0; x < Width; x = (ushort)(x + 1))
                            {
                                Floor[x, y, MapObjectType.InvalidCast, null] = !Convert.ToBoolean(BR.ReadUInt16());

                                BR.BaseStream.Seek(4L, SeekOrigin.Current);
                            }
                            BR.BaseStream.Seek(4L, SeekOrigin.Current);
                        }
                        uint amount = BR.ReadUInt32();
                        PopulatePortals(amount);
                        for (ushort j = 0; j < amount; j = (ushort)(j + 1))
                        {
                            DMapPortal portal = new DMapPortal
                            {
                                XCord = (ushort)BR.ReadUInt32(),
                                YCord = (ushort)BR.ReadUInt32()
                            };
                            SetPortal(j, portal);
                            BR.BaseStream.Seek(4L, SeekOrigin.Current);
                        }
                        //    BR.BaseStream.Seek(amount * 12, SeekOrigin.Current);

                        int num = BR.ReadInt32();
                        List<SceneFile> list = new List<SceneFile>();
                        for (int i = 0; i < num; i++)
                        {
                            switch (BR.ReadInt32())
                            {
                                case 10:
                                    BR.BaseStream.Seek(0x48L, SeekOrigin.Current);
                                    break;

                                case 15:
                                    BR.BaseStream.Seek(0x114L, SeekOrigin.Current);
                                    break;

                                case 1:
                                    list.Add(this.CreateSceneFile(BR));
                                    break;

                                case 4:
                                    BR.BaseStream.Seek(0x1a0L, SeekOrigin.Current);
                                    break;
                            }
                        }
                        Scenes = list.ToArray();

                        for (int i = 0; i < Scenes.Length; i++)
                        {
                            foreach (ScenePart part in Scenes[i].Parts)
                            {
                                for (int j = 0; j < part.Size.Width; j++)
                                {
                                    for (int k = 0; k < part.Size.Height; k++)
                                    {
                                        Point point = new Point();
                                        point.X = ((Scenes[i].Location.X + part.StartPosition.X) + j) - part.Size.Width;
                                        point.Y = ((Scenes[i].Location.Y + part.StartPosition.Y) + k) - part.Size.Height;
                                        Floor[(ushort)point.X, (ushort)point.Y, MapObjectType.InvalidCast, null] = part.NoAccess[j, k];
                                    }
                                }
                            }
                        }

                        BR.Close();
                        FS.Close();
                        SaveMap();
                        SavePortals();
                    }
                }
            }
            #endregion
            LoadNpcs();
       
            LoadMonsters();
            LoadPortals();


        }

        private void MergeSceneToTextureArea()
        {
            for (int i = 0; i < Scenes.Length; i++)
            {
                if (Scenes[i].Parts == null) return;
                foreach (ScenePart part in Scenes[i].Parts)
                {
                    for (int j = 0; j < part.Size.Width; j++)
                    {
                        for (int k = 0; k < part.Size.Height; k++)
                        {
                            Point point = new Point
                            {
                                X = ((Scenes[i].Location.X + part.StartPosition.X) - j),
                                Y = ((Scenes[i].Location.Y + part.StartPosition.Y) - k)
                            };
                            Floor[(ushort)point.X, (ushort)point.Y, MapObjectType.InvalidCast] = part.NoAccess[j, k];
                        }
                    }
                }
            }
        }
        private void LoadMapObjects(BinaryReader Reader)
        {
            int num = Reader.ReadInt32();
            List<SceneFile> list = new List<SceneFile>();
            for (int i = 0; i < num; i++)
            {
                int id = Reader.ReadInt32();
                id = (byte)id;
                switch (id)
                {
                    case 10:
                        Reader.BaseStream.Seek(0x48L, SeekOrigin.Current);
                        break;

                    case 15:
                        Reader.BaseStream.Seek(0x114L, SeekOrigin.Current);
                        break;

                    case 1:
                        list.Add(this.CreateSceneFile(Reader));
                        break;

                    case 4:
                        Reader.BaseStream.Seek(0x1a0L, SeekOrigin.Current);
                        break;
                }
            }
            Scenes = list.ToArray();
        }
        private void LoadPortals()
        {
            IniFile file = new MTA.IniFile(Constants.PortalsPath);
            ushort portalCount = file.ReadUInt16(BaseID.ToString(), "Count");

            for (int i = 0; i < portalCount; i++)
            {
                string _PortalEnter = file.ReadString(BaseID.ToString(), "PortalEnter" + i.ToString());
                string _PortalExit = file.ReadString(BaseID.ToString(), "PortalExit" + i.ToString());
                string[] PortalEnter = _PortalEnter.Split(' ');
                string[] PortalExit = _PortalExit.Split(' ');
                Game.Portal portal = new MTA.Game.Portal();
                portal.CurrentMapID = Convert.ToUInt16(PortalEnter[0]);
                portal.CurrentX = Convert.ToUInt16(PortalEnter[1]);
                portal.CurrentY = Convert.ToUInt16(PortalEnter[2]);

                if (PortalExit.Length == 3)
                {
                    portal.DestinationMapID = Convert.ToUInt16(PortalExit[0]);
                    portal.DestinationX = Convert.ToUInt16(PortalExit[1]);
                    portal.DestinationY = Convert.ToUInt16(PortalExit[2]);
                }
                Portals.Add(portal);
            }
        }
        public List<Game.Portal> Portals = new List<Game.Portal>();
        private IDisposable Timer;

        public static sbyte[] XDir = new sbyte[] 
        { 
            -1, -2, -2, -1, 1, 2, 2, 1,
             0, -2, -2, -2, 0, 2, 2, 2, 
            -1, -2, -2, -1, 1, 2, 2, 1,
             0, -1, -1, -1, 0, 1, 1, 1,
        };
        public static sbyte[] YDir = new sbyte[] 
        {
            2,  1, -1, -2, -2, -1, 1, 2,
            2,  2,  0, -2, -2, -2, 0, 2, 
            2,  1, -1, -2, -2, -1, 1, 2, 
            1,  1,  0, -1, -1, -1, 0, 1
        };
        public SafeConcurrentDictionary<uint, StaticEntity> StaticEntities = new SafeConcurrentDictionary<uint, StaticEntity>();
        public void AddStaticEntity(StaticEntity item)
        {
            Floor[item.X, item.Y, MapObjectType.StaticEntity, null] = false;
            StaticEntities[item.UID] = item;
        }
        public void RemoveStaticItem(StaticEntity item)
        {
            Floor[item.X, item.Y, MapObjectType.StaticEntity, null] = true;
            StaticEntities.Remove(item.UID);
        }
        private void SaveMap()
        {
            if (!File.Exists(Constants.DMapsPath + "\\maps\\" + BaseID.ToString() + ".map"))
            {
                FileStream stream = new FileStream(Constants.DMapsPath + "\\maps\\" + BaseID.ToString() + ".map", FileMode.Create);
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((uint)Floor.Bounds.Width);
                writer.Write((uint)Floor.Bounds.Height);
                for (int y = 0; y < Floor.Bounds.Height; y++)
                {
                    for (int x = 0; x < Floor.Bounds.Width; x++)
                    {
                        writer.Write((byte)(Floor[x, y, MapObjectType.InvalidCast, null] == true ? 1 : 0));
                    }
                }
                writer.Close();
                stream.Close();
            }
        }
        private void SavePortals()
        {
            if (!File.Exists("portals.txt"))
            {
                File.Create("portals.txt");
            }
            string ConfigFileName = "portals.txt";
            IniFile IniFile = new IniFile(ConfigFileName);
            var id = ID.ToString();
            for (int i = 0; i < portals.Length; i++)
            {
                IniFile.Write(id, "Count", portals.Length.ToString());
                IniFile.Write(id, i.ToString(), string.Format("{0} {1} {2}", id, portals[i].XCord.ToString(), portals[i].YCord.ToString()));
            }

        }
       
        private void LoadNpcs()
        {
            using (var command = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
            {
                command.Select("npcs").Where("mapid", ID);
                using (var reader = new Database.MySqlReader(command))
                {
                    while (reader.Read())
                    {
                        INpc npc = new Network.GamePackets.NpcSpawn();
                        npc.UID = reader.ReadUInt32("id");
                        npc.Name = reader.ReadString("name");
                        npc.Mesh = reader.ReadUInt16("lookface");
                        npc.Type = (Enums.NpcType)reader.ReadByte("type");
                        npc.X = reader.ReadUInt16("cellx");
                        npc.Y = reader.ReadUInt16("celly");
                        npc.effect = reader.ReadString("effect");
                        npc.MapID = ID;
                       
                        AddNpc(npc);
                    }
                }
            }
            using (var command = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
            {
                command.Select("sobnpcs").Where("mapid", ID);
                using (var reader = new Database.MySqlReader(command))
                {
                    while (reader.Read())
                    {
                        Network.GamePackets.SobNpcSpawn npc = new Network.GamePackets.SobNpcSpawn();
                        npc.UID = reader.ReadUInt32("id");
                        npc.Mesh = reader.ReadUInt16("lookface");
                        if (ID == 1039)
                            npc.Mesh = (ushort)(npc.Mesh - npc.Mesh % 10 + 7);
                        npc.Type = (Enums.NpcType)reader.ReadByte("type");
                        npc.X = reader.ReadUInt16("cellx"); ;
                        npc.Y = reader.ReadUInt16("celly");
                        npc.MapID = reader.ReadUInt16("mapid");
                        npc.Sort = reader.ReadUInt16("sort");
                        npc.ShowName = true;
                        npc.Name = reader.ReadString("name");
                        npc.Hitpoints = reader.ReadUInt32("life");
                        npc.MaxHitpoints = reader.ReadUInt32("maxlife");
                        npc._isprize = reader.ReadBoolean("prize");
                        if (npc.UID >= 9994 && npc.UID <= 9997)
                        {
                            Database.GuildCondutors.GuildConductors.Add(npc.UID, new Database.GuildCondutors.Conductor() { npc = npc });
                            Database.GuildCondutors.MoveNpc(npc.UID, npc.MapID, npc.X, npc.Y);
                            AddNpc(npc, true);
                        }
                        else
                            AddNpc(npc);
                    }
                }
            }
            uint nextid = 100000;
            EntityUIDCounter2 = new Counter(nextid);
        }
        public bool FreezeMonsters = false;
        public void LoadMonsters()
        {
            //Companions = new SafeDictionary<uint, Entity>();
            using (var command = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
            {
                command.Select("monsterspawns").Where("mapid", ID);
                using (var reader = new Database.MySqlReader(command))
                {
                    int mycount = 0;
                    try
                    {
                        while (reader.Read())
                        {
                            uint monsterID = reader.ReadUInt32("npctype");
                            ushort CircleDiameter = reader.ReadUInt16("maxnpc");
                            ushort X = reader.ReadUInt16("bound_x");
                            ushort Y = reader.ReadUInt16("bound_y");
                            ushort XPlus = reader.ReadUInt16("bound_cx");
                            ushort YPlus = reader.ReadUInt16("bound_cy");
                            ushort Amount = reader.ReadUInt16("max_per_gen");
                            int respawn = reader.ReadInt32("rest_secs");
                            if (Database.MonsterInformation.MonsterInformations.ContainsKey(monsterID))
                            {
                                Database.MonsterInformation mt = Database.MonsterInformation.MonsterInformations[monsterID];
                                mt.RespawnTime = respawn + 5;
                                mt.BoundX = X;
                                mt.BoundY = Y;
                                mt.BoundCX = XPlus;
                                mt.BoundCY = YPlus;

                                bool more = true;
                                for (int count = 0; count < Amount; count++)
                                {
                                    if (!more)
                                        break;
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
                                    entity.UID = EntityUIDCounter.Next;
                                    entity.MapID = ID;
                                    entity.SendUpdates = true;
                                    //if (mt.Type == 2)
                                    //{

                                    //    entity.UID += 0xc3500 + 0xc3500;
                                    //   // Network.Writer.WriteUInt32(mt.helmet_type, 44 + 4, entity.SpawnPacket);// head.
                                    //  //  Network.Writer.WriteUInt16((byte)Enums.Color.Black, 139 + 4, entity.SpawnPacket);//  head color.

                                    //   // Network.Writer.WriteUInt32(mt.armor_type, 52 + 4, entity.SpawnPacket);// Armor.
                                    //   // Network.Writer.WriteUInt16((byte)Enums.Color.Black, 139 + 4, entity.SpawnPacket);// Armor color.

                                    //   // Network.Writer.WriteUInt32(mt.weaponr_type, 60 + 4, entity.SpawnPacket);// right wep.
                                    //   // Network.Writer.WriteUInt32(mt.weaponl_type, 56 + 4, entity.SpawnPacket);//left wep.                                

                                    //   // entity.NobilityRank = Game.ConquerStructures.NobilityRank.Baron;
                                    //   // entity.CountryID = Enums.CountryID.Egypt;
                                    //   // var value = Update.Flags.BlackName;
                                    //    //Network.Writer.WriteUInt64(value, 0x16 + 4, entity.SpawnPacket);

                                    //}
                                    //if (mt.Name == "Guard1")
                                    //{
                                    //    entity.UID += 0xc3500 + 0xc3500;
                                    //    //Network.Writer.WriteUInt32(mt.helmet_type, 44 + 4, entity.SpawnPacket);//head.
                                    //    //Network.Writer.WriteUInt16((byte)Enums.Color.Black, 139 + 4, entity.SpawnPacket);//head color.
                                    //    entity.Body = 1003;
                                    //    //Network.Writer.WriteUInt32(mt.armor_type, 52 + 4, entity.SpawnPacket);//Armor.
                                    //    //Network.Writer.WriteUInt16((byte)Enums.Color.Black, 139 + 4, entity.SpawnPacket);//Armor color.

                                    //    Network.Writer.WriteUInt32(420439, 60 + 4, entity.SpawnPacket);//right wep.
                                    //    Network.Writer.WriteUInt32(420439, 56 + 4, entity.SpawnPacket);//left wep.                                

                                    //    Network.Writer.WriteUInt32(188265, 52, entity.SpawnPacket);//Garment.          

                                    //    //Network.Writer.WriteUInt32(entity.Owner.Spells[mt.SpellID].LevelHu, 16, entity.SpawnPacket);//SkillSoulType.

                                    //    entity.TalentStaus = 5;
                                    //    Network.Writer.WriteByte(entity.TalentStaus, 0xf1, entity.SpawnPacket);

                                    //    entity.NobilityRank = Game.ConquerStructures.NobilityRank.Knight;
                                    //    entity.CountryID = Enums.CountryID.Egypt;
                                    //    entity.Action = Enums.ConquerAction.Dance;

                                    //    Network.Writer.WriteUshort(1000, 20, entity.SpawnPacket);
                                    //    //var value = Update.Flags.Ride;
                                    //    //Network.Writer.WriteUInt64(value, 0x16 + 4, entity.SpawnPacket);
                                    //}
                                    entity.X = (ushort)(X + Kernel.Random.Next(0, XPlus));
                                    entity.Y = (ushort)(Y + Kernel.Random.Next(0, YPlus));
                                    for (int count2 = 0; count2 < 50; count2++)
                                    {
                                        if (!Floor[entity.X, entity.Y, MapObjectType.Monster, null])
                                        {
                                            entity.X = (ushort)(X + Kernel.Random.Next(0, XPlus));
                                            entity.Y = (ushort)(Y + Kernel.Random.Next(0, YPlus));
                                            if (count2 == 50)
                                                more = false;
                                        }
                                        else
                                            break;
                                    }
                                    if (more)
                                    {
                                        if (Floor[entity.X, entity.Y, MapObjectType.Monster, null])
                                        {
                                            mycount++;
                                            AddEntity(entity);
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                    catch (Exception e) { Program.SaveException(e); }
                    if (mycount != 0)
                        Timer = MonsterTimers.Add(this);
                }
            }
        }

        public Tuple<ushort, ushort> RandomCoordinates()
        {
            int times = 10000;
            int x = Kernel.Random.Next(Floor.Bounds.Width), y = Kernel.Random.Next(Floor.Bounds.Height);
            while (times-- > 0)
            {
                if (!Floor[x, y, MapObjectType.Player, null])
                {
                    x = Kernel.Random.Next(Floor.Bounds.Width);
                    y = Kernel.Random.Next(Floor.Bounds.Height);
                }
                else break;
            }
            return new Tuple<ushort, ushort>((ushort)x, (ushort)y);
        }
        public Tuple<ushort, ushort> RandomCoordinates(int _x, int _y, int radius)
        {
            int times = 10000;
            int x = _x + Kernel.Random.Sign() * Kernel.Random.Next(radius),
                y = _y + Kernel.Random.Sign() * Kernel.Random.Next(radius);
            while (times-- > 0)
            {
                if (!Floor[x, y, MapObjectType.Player, null])
                {
                    x = _x + Kernel.Random.Sign() * Kernel.Random.Next(radius);
                    y = _y + Kernel.Random.Sign() * Kernel.Random.Next(radius);
                }
                else break;
            }
            return new Tuple<ushort, ushort>((ushort)x, (ushort)y);
        }

        private static TimerRule<Map> MonsterTimers;
        public static void CreateTimerFactories()
        {
            MonsterTimers = new TimerRule<Map>(_timerCallBack, 500);
        }

        public Time32 LastReload = Time32.Now;
        private static void _timerCallBack(Map map, int time)
        {
            //foreach (Entity monster in map.Companions.Values)
            //{
            //    if (!monster.Owner.Socket.Alive)
            //    {
            //        map.RemoveEntity(monster);
            //        break;
            //    }
            //}
            Time32 now = new Time32(time);
            foreach (Entity monster in map.Entities.Values)
            {
                if (monster.Dead)
                {
                    if (now > monster.DeathStamp.AddSeconds(monster.MonsterInfo.RespawnTime))
                    {
                        monster.X = (ushort)(monster.MonsterInfo.BoundX + Kernel.Random.Next(0, monster.MonsterInfo.BoundCX));
                        monster.Y = (ushort)(monster.MonsterInfo.BoundY + Kernel.Random.Next(0, monster.MonsterInfo.BoundCY));
                        for (int count = 0; count < monster.MonsterInfo.BoundCX * monster.MonsterInfo.BoundCY; count++)
                        {
                            if (!map.Floor[monster.X, monster.Y, MapObjectType.Monster])
                            {
                                monster.X = (ushort)(monster.MonsterInfo.BoundX + Kernel.Random.Next(0, monster.MonsterInfo.BoundCX));
                                monster.Y = (ushort)(monster.MonsterInfo.BoundY + Kernel.Random.Next(0, monster.MonsterInfo.BoundCY));
                            }
                            else
                                break;
                        }
                        if (map.Floor[monster.X, monster.Y, MapObjectType.Monster] || monster.X == monster.MonsterInfo.BoundX && monster.Y == monster.MonsterInfo.BoundY)
                        {
                            monster.Hitpoints = monster.MonsterInfo.Hitpoints;
                            monster.RemoveFlag(monster.StatusFlag);
                            var stringPacket = new _String(true);
                            stringPacket.UID = monster.UID;
                            stringPacket.Type = _String.Effect;
                            stringPacket.Texts.Add("MBStandard");
                            monster.StatusFlag = 0;
                            //if (monster.MonsterInfo.Type == 2)
                            //{
                            //    var value = Update.Flags.BlackName;
                            //    Network.Writer.WriteUInt64(value, 0x16 + 4, monster.SpawnPacket);

                            //}
                            //if (monster.MonsterInfo.Name == "Guard1")
                            //{
                            //    monster.Action = (ushort)Kernel.Random.Next(1, 15);
                            //}
                            /*if (monster.Name == "SnowbansheeSoul" || monster.Name == "SnowBanshee" || monster.Name == "EvilMonkMisery" || monster.Name == "FlameDevastator" || monster.Name == "NemesisTyrant" || monster.Name == "TeratoDragon")
                            {
                                foreach (Client.GameState client in Program.Values)
                                {
                                    client.MessageBox(monster.Name + " has apeared , Who will Defeat it and get 500 MonsterPoints !", (p) => { p.Entity.Teleport(monster.MapID, (ushort)(monster.X + 3), (ushort)(monster.Y + 3), false); }, null);
                                }
                                MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Warrning " + monster.Name + " has Apeared in [" + monster.MapID.ToString() + "] at " + monster.X + ", " + monster.Y + " Who will Defeat it and get 50 MonsterPoints !", System.Drawing.Color.White, MTA.Network.GamePackets.Message.Center), Program.Values);
                            }*/                            
                            foreach (Client.GameState client in Program.Values)
                            {
                                if (client.Map.ID == map.ID)
                                {
                                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, monster.X, monster.Y) < Constants.nScreenDistance)
                                    {
                                        monster.CauseOfDeathIsMagic = false;
                                        monster.SendSpawn(client, false);
                                        client.Send(stringPacket);
                                        if (monster.MaxHitpoints > 65535)
                                        {
                                            Update upd = new Update(true) { UID = monster.UID };
                                            // upd.Append(Update.MaxHitpoints, monster.MaxHitpoints);
                                            upd.Append(Update.Hitpoints, monster.Hitpoints);
                                            client.Send(upd);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (monster.ToxicFogLeft > 0)
                    {
                        if (monster.MonsterInfo.Boss)
                        {
                            monster.ToxicFogLeft = 0;
                            continue;
                        }
                        if (now > monster.ToxicFogStamp.AddSeconds(2))
                        {
                            monster.ToxicFogLeft--;
                            monster.ToxicFogStamp = now;
                            if (monster.Hitpoints > 1)
                            {
                                uint damage = Attacking.Calculate.Percent(monster, monster.ToxicFogPercent);
                                monster.Hitpoints -= damage;
                                var suse = new SpellUse(true);
                                suse.Attacker = monster.UID;
                                suse.SpellID = 10010;
                                suse.AddTarget(monster, damage, null);
                                monster.MonsterInfo.SendScreen(suse);
                            }
                        }
                    }
                }
            }
        }
        public void SpawnMonsterNearToHero(MonsterInformation mt, GameState client)
        {
            if (mt == null) return;
            mt.RespawnTime = 36000;
            MTA.Game.Entity entity = new MTA.Game.Entity(EntityFlag.Monster, false);
            entity.MapObjType = MapObjectType.Monster;
            entity.MonsterInfo = mt.Copy();
            entity.MonsterInfo.Owner = entity;
            entity.Name = mt.Name;
            entity.MinAttack = mt.MinAttack;
            entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
            entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
            entity.Body = mt.Mesh;
            entity.Level = mt.Level;
            entity.UID = EntityUIDCounter.Next;
            entity.MapID = ID;
            entity.SendUpdates = true;
            entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(5));
            entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(5));
            AddEntity(entity);
            entity.SendSpawn(client);
        }
        public void Spawnthis(MonsterInformation mt, GameState client, ushort ID, ushort x, ushort y)
        {
            if (mt == null)
            {
                return;
            }
            mt.RespawnTime = 36000;
            Entity entity = new Entity(EntityFlag.Monster, false);
            entity.MapObjType = MapObjectType.Monster;
            entity.MonsterInfo = mt.Copy();
            entity.MonsterInfo.Owner = entity;
            entity.Name = mt.Name;
            entity.MinAttack = mt.MinAttack;
            entity.MaxAttack = (entity.MagicAttack = mt.MaxAttack);
            entity.Hitpoints = (entity.MaxHitpoints = mt.Hitpoints);
            entity.Body = mt.Mesh;
            entity.Level = mt.Level;
            entity.UID = this.EntityUIDCounter.Next;
            entity.MapID = ID;
            entity.X = x;
            entity.Y = y;
            entity.SendUpdates = true;
            this.AddEntity(entity);
            entity.SendSpawn(client);
        }
        public Map MakeDynamicMap()
        {
            ushort id = (ushort)DynamicIDs.Next;
            Map myDynamic = new Map(id, this.ID, this.Path);
            return myDynamic;
        }
        bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
                Kernel.Maps.Remove(ID);

            disposed = true;
        }

        public void RemoveNpc(INpc npc, bool query = false)
        {
            if (Npcs.ContainsKey(npc.UID) || query)
            {
                if (!query)
                    Npcs.Remove(npc.UID);
                #region Setting the near coords invalid to avoid unpickable items.
                Floor[npc.X, npc.Y, MapObjectType.InvalidCast] = true;
                if (npc.Mesh / 10 != 108 && (byte)npc.Type < 10)
                {
                    ushort x = npc.X, Y = npc.Y;
                    foreach (Enums.ConquerAngle angle in Angles)
                    {
                        ushort xX = x, yY = Y;
                        UpdateCoordonatesForAngle(ref xX, ref yY, angle);
                        Floor[xX, yY, MapObjectType.InvalidCast] = true;
                    }
                }
                #endregion
            }
        }

    }
    public class Floor
    {
        [Flags]
        public enum DMapPointFlag : byte
        {
            Invalid = 1,
            Monster = 2,
            Npc,
            Item = 4,
            RaceItem = 8
        }
        public class Size
        {
            public int Width, Height;
            public Size(int width, int height)
            {
                Width = width;
                Height = height;
            }
            public Size()
            {
                Width = 0;
                Height = 0;
            }
        }
        public Size Bounds;
        public DMapPointFlag[,] Locations;
        public uint FloorMapId;
        public Floor(int width, int height, uint mapId)
        {
            FloorMapId = mapId;
            Bounds = new Size(width, height);
            Locations = new DMapPointFlag[width, height];
        }
        public bool this[int x, int y, MapObjectType type, object obj = null]
        {
            get
            {
                if (y >= Bounds.Height || x >= Bounds.Width || x < 0 || y < 0)
                    return false;

                DMapPointFlag filltype = Locations[x, y];

                if (type == MapObjectType.InvalidCast)
                    return (filltype & DMapPointFlag.Invalid) == DMapPointFlag.Invalid;
                if ((filltype & DMapPointFlag.Invalid) == DMapPointFlag.Invalid)
                    return false;
                if (type == MapObjectType.Player)
                    return true;
                else
                {
                    if (type == MapObjectType.Npc)
                        return (filltype & DMapPointFlag.Npc) != DMapPointFlag.Npc;
                    if (type == MapObjectType.Monster)
                        return (filltype & DMapPointFlag.Monster) != DMapPointFlag.Monster;
                    if (type == MapObjectType.Item)
                        return (filltype & DMapPointFlag.Item) != DMapPointFlag.Item;
                    if (type == MapObjectType.StaticEntity)
                        return (filltype & DMapPointFlag.RaceItem) != DMapPointFlag.RaceItem;
                }
                return false;
            }
            set
            {
                if (y >= Bounds.Height || x >= Bounds.Width || x < 0 || y < 0)
                    return;
                DMapPointFlag filltype = Locations[x, y];

                if (value)
                {
                    if (type == MapObjectType.InvalidCast)
                        TakeFlag(x, y, DMapPointFlag.Invalid);
                    if (type == MapObjectType.Item)
                        TakeFlag(x, y, DMapPointFlag.Item);
                    if (type == MapObjectType.Npc)
                        TakeFlag(x, y, DMapPointFlag.Npc);
                    if (type == MapObjectType.Monster)
                        TakeFlag(x, y, DMapPointFlag.Monster);
                    if (type == MapObjectType.StaticEntity)
                        TakeFlag(x, y, DMapPointFlag.RaceItem);
                }
                else
                {
                    if (type == MapObjectType.InvalidCast)
                        AddFlag(x, y, DMapPointFlag.Invalid);
                    if (type == MapObjectType.Npc)
                        TakeFlag(x, y, DMapPointFlag.Npc);
                    if (type == MapObjectType.Item)
                        AddFlag(x, y, DMapPointFlag.Item);
                    if (type == MapObjectType.Monster)
                        AddFlag(x, y, DMapPointFlag.Monster);
                    if (type == MapObjectType.StaticEntity)
                        AddFlag(x, y, DMapPointFlag.RaceItem);
                }
            }
        }
        public DMapPointFlag AddFlag(int x, int y, DMapPointFlag extraFlag)
        {
            Locations[x, y] |= extraFlag;
            return Locations[x, y];
        }
        public DMapPointFlag TakeFlag(int x, int y, DMapPointFlag extraFlag)
        {
            Locations[x, y] &= ~extraFlag;
            return Locations[x, y];
        }
    }
    public enum MapObjectType
    {
        SobNpc, Npc, Item, Monster, Player, Nothing, InvalidCast, StaticEntity
    }
    public class Portal
    {
        public Portal(ushort currentMapId, ushort currentX, ushort currentY, ushort destinationMapId, ushort destinationX, ushort destinationY)
        {
            CurrentMapID = currentMapId;
            CurrentX = currentX;
            CurrentY = currentY;
            DestinationMapID = destinationMapId;
            DestinationX = destinationX;
            DestinationY = destinationY;
        }
        public Portal()
        {

        }
        public ushort CurrentMapID
        {
            get;
            set;
        }
        public ushort CurrentX
        {
            get;
            set;
        }
        public ushort CurrentY
        {
            get;
            set;
        }
        public ushort DestinationMapID
        {
            get;
            set;
        }
        public ushort DestinationX
        {
            get;
            set;
        }
        public ushort DestinationY
        {
            get;
            set;
        }
    }
}