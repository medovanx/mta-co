using System;
using System.Collections.Generic;

namespace MTA.Game.Features.Kisses;

public class Kisses {
    public static List<ListKissRank> Kiss2 = [];
    public static List<ListKissRank> Wine2 = [];
    public static List<ListKissRank> Letters2 = [];
    public static List<ListKissRank> Jades2 = [];
    public static List<ListKissRank> KissesToday = [];
    public static List<ListKissRank> WineToday = [];
    public static List<ListKissRank> LetterToday = [];
    public static List<ListKissRank> JadeToday = [];

    public uint id;
    public bool Jadestoday = false;
    public bool Jadestoday2;
    public bool Jadestoday3;
    public bool Jadestoday4;
    public SafeDictionary<uint, Kisses> Kiss = new(1000);
    public bool kissestoday = false;
    public bool kissestoday2;
    public bool kissestoday3;
    public bool kissestoday4;
    public bool letterstoday = false;
    public bool letterstoday2;
    public bool letterstoday3;
    public bool letterstoday4;
    public bool winetoday = false;
    public bool winetoday2;
    public bool winetoday3;
    public bool winetoday4;

    public DateTime LastKissesSent { get; set; }

    public string name { get; set; }

    public uint Letters1 { get; set; }

    public uint LetterToday1 { get; set; }

    public uint Wine { get; set; }

    public uint Wine2day { get; set; }

    public uint Kisses2 { get; set; }

    public uint Kisses2day { get; set; }

    public uint Jades { get; set; }

    public uint Jades2day { get; set; }

    public struct ListKissRank {
        public string name;
        public uint Kisses;
        public uint Wine;
        public uint Letter;
        public uint Jade;
        public int rank;
        public short body;
        public uint uid;
    }
}