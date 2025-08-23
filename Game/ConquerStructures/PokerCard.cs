using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game.ConquerStructures
{
    public class PokerCard
    {
        public PokerCard(Game.Enums.PokerCardsType type, Game.Enums.PokerCardsValue value)
        {
            Type = type;
            Value = value;
        }
        public byte ID
        {
            get { return (byte)((byte)Value + (13 * (byte)Type)); }
        }
        public Game.Enums.PokerCardsValue Value;
        public Game.Enums.PokerCardsType Type;
        public override string ToString()
        {
            return String.Format("{0}{1}", ValueChar(Value), KindChar(Type));
        }
        public static char ValueChar(Game.Enums.PokerCardsValue v)
        {
            var chars = "23456789TJQKA";
            return chars[(int)v];
        }
        public static char KindChar(Game.Enums.PokerCardsType k)
        {
            var chars = "cdhs";
            return chars[(int)k];
        }
    }
}