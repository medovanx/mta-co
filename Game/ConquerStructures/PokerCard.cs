using System;

namespace MTA.Game.ConquerStructures
{
    public class PokerCard
    {
        public PokerCard(Enums.PokerCardsType type, Enums.PokerCardsValue value)
        {
            Type = type;
            Value = value;
        }
        public byte ID
        {
            get { return (byte)((byte)Value + (13 * (byte)Type)); }
        }
        public Enums.PokerCardsValue Value;
        public Enums.PokerCardsType Type;
        public override string ToString()
        {
            return String.Format("{0}{1}", ValueChar(Value), KindChar(Type));
        }
        public static char ValueChar(Enums.PokerCardsValue v)
        {
            var chars = "23456789TJQKA";
            return chars[(int)v];
        }
        public static char KindChar(Enums.PokerCardsType k)
        {
            var chars = "cdhs";
            return chars[(int)k];
        }
    }
}