using Conquer_Online_Server.Database;
using System;
using System.Collections.Generic;
namespace Conquer_Online_Server.Game.Features.Flowers
{
    public class Flowers
    {
        public SafeDictionary<uint, Game.Features.Flowers.Flowers> flower = new SafeDictionary<uint, Game.Features.Flowers.Flowers>(1000);
        public static List<ListFlowerRank> Redrosse = new List<ListFlowerRank>();
        public static List<ListFlowerRank> Orchides = new List<ListFlowerRank>();
        public static List<ListFlowerRank> Lilise = new List<ListFlowerRank>();
        public static List<ListFlowerRank> Tuplise = new List<ListFlowerRank>();
        public static List<ListFlowerRank> RedrosseToday = new List<ListFlowerRank>();
        public static List<ListFlowerRank> OrchidesToday = new List<ListFlowerRank>();
        public static List<ListFlowerRank> LiliseToday = new List<ListFlowerRank>();
        public static List<ListFlowerRank> TupliseToday = new List<ListFlowerRank>();
        public bool liliestoday = false;
        public bool orchadstoday = false;
        public bool tulpistoday = false;
        public bool redroesstoday = false;
        public bool liliestoday2;
        public bool orchadstoday2;
        public bool tulpistoday2;
        public bool redroesstoday2;
        public bool liliestoday3;
        public bool orchadstoday3;
        public bool tulpistoday3;
        public bool redroesstoday3;
        public bool liliestoday4;
        public bool orchadstoday4;
        public bool tulpistoday4;
        public bool redroesstoday4;
        public struct ListFlowerRank
        {
            public string name;
            public uint redrosse;
            public uint orchides;
            public uint lilise;
            public uint tuplise;
            public int rank;
            public short body;
            public uint uid;
        }

        private DateTime _LastFlowerSent;
        private uint _Lilies;
        private uint _Lilies2day;
        private uint _Orchads;
        private uint _Orchads2day;
        private uint _RedRoses;
        private uint _RedRoses2day;
        private uint _Tulips;
        private uint _Tulips2day;
        public uint id;

        public DateTime LastFlowerSent
        {
            get
            {
                return this._LastFlowerSent;
            }
            set
            {
                this._LastFlowerSent = value;
            }
        }
        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }

        }

        public uint Lilies
        {
            get
            {
                return this._Lilies;
            }
            set
            {
                this._Lilies = value;
            }
        }

        public uint Lilies2day
        {
            get
            {
                return this._Lilies2day;
            }
            set
            {
                this._Lilies2day = value;
            }
        }

        public uint Orchads
        {
            get
            {
                return this._Orchads;
            }
            set
            {
                this._Orchads = value;
            }
        }

        public uint Orchads2day
        {
            get
            {
                return this._Orchads2day;
            }
            set
            {
                this._Orchads2day = value;
            }
        }

        public uint RedRoses
        {
            get
            {
                return this._RedRoses;
            }
            set
            {
                this._RedRoses = value;
            }
        }

        public uint RedRoses2day
        {
            get
            {
                return this._RedRoses2day;
            }
            set
            {
                this._RedRoses2day = value;
            }
        }

        public uint Tulips
        {
            get
            {
                return this._Tulips;
            }
            set
            {
                this._Tulips = value;
            }
        }

        public uint Tulips2day
        {
            get
            {
                return this._Tulips2day;
            }
            set
            {
                this._Tulips2day = value;
            }
        }
    }
}

