using System;
using System.Collections.Generic;
namespace MTA.Game.Features.Kisses
{
    public class Kisses
    {
        public SafeDictionary<uint, Kisses> Kiss = new SafeDictionary<uint, Kisses>(1000);
        public static List<ListKissRank> Kiss2 = [];
        public static List<ListKissRank> Wine2 = [];
        public static List<ListKissRank> Letters2 = [];
        public static List<ListKissRank> Jades2 = [];
        public static List<ListKissRank> KissesToday = [];
        public static List<ListKissRank> WineToday = [];
        public static List<ListKissRank> LetterToday = [];
        public static List<ListKissRank> JadeToday = [];
        public bool letterstoday = false;
        public bool winetoday = false;
        public bool Jadestoday = false;
        public bool kissestoday = false;
        public bool letterstoday2;
        public bool winetoday2;
        public bool Jadestoday2;
        public bool kissestoday2;
        public bool letterstoday3;
        public bool winetoday3;
        public bool Jadestoday3;
        public bool kissestoday3;
        public bool letterstoday4;
        public bool winetoday4;
        public bool Jadestoday4;
        public bool kissestoday4;
        public struct ListKissRank
        {
            public string name;
            public uint Kisses;
            public uint Wine;
            public uint Letter;
            public uint Jade;
            public int rank;
            public short body;
            public uint uid;
        }

        private DateTime _LastKissesSent;
        private uint _Letters;
        private uint _Letters2day;
        private uint _Wine;
        private uint _Wine2day;
        private uint _Kisses;
        private uint _Kisses2day;
        private uint _Jades;
        private uint _Jades2day;
        public uint id;

        public DateTime LastKissesSent
        {
            get
            {
                return _LastKissesSent;
            }
            set
            {
                _LastKissesSent = value;
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

        public uint Letters1
        {
            get
            {
                return _Letters;
            }
            set
            {
                _Letters = value;
            }
        }

        public uint LetterToday1
        {
            get
            {
                return _Letters2day;
            }
            set
            {
                _Letters2day = value;
            }
        }

        public uint Wine
        {
            get
            {
                return _Wine;
            }
            set
            {
                _Wine = value;
            }
        }

        public uint Wine2day
        {
            get
            {
                return _Wine2day;
            }
            set
            {
                _Wine2day = value;
            }
        }

        public uint Kisses2
        {
            get
            {
                return _Kisses;
            }
            set
            {
                _Kisses = value;
            }
        }

        public uint Kisses2day
        {
            get
            {
                return _Kisses2day;
            }
            set
            {
                _Kisses2day = value;
            }
        }

        public uint Jades
        {
            get
            {
                return _Jades;
            }
            set
            {
                _Jades = value;
            }
        }

        public uint Jades2day
        {
            get
            {
                return _Jades2day;
            }
            set
            {
                _Jades2day = value;
            }
        }
    }
}

