using System;

namespace MTA
{// a da ya 3m :D timestamp da :V wala nta gayb omo mninne? eh dah asln :D a7a :D ma 3lenii 
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TTime
    {
        private int int_0;
        private static uint uint_0;
        public static readonly TTime NULL;
        public TTime(int Value)
        {
            int_0 = Value;
        }

        public TTime(uint Value)
        {
            int_0 = (int)Value;
        }

        public TTime(long Value)
        {
            int_0 = (int)Value;
        }

        static TTime()
        {
            NULL = new TTime(0);
        }

        public static TTime Now
        {
            get
            {
                return new TTime((uint)Environment.TickCount);
            }
        }
        public int TotalMilliseconds
        {
            get
            {
                return int_0;
            }
        }
        public int Value
        {
            get
            {
                return int_0;
            }
        }
        public TTime AddMilliseconds(int Amount)
        {
            return new TTime(int_0 + Amount);
        }

        public int AllMilliseconds()
        {
            return GetHashCode();
        }

        public TTime AddSeconds(int Amount)
        {
            return AddMilliseconds(Amount * 0x3e8);
        }

        public int AllSeconds()
        {
            return (AllMilliseconds() / 0x3e8);
        }

        public TTime AddMinutes(int Amount)
        {
            return AddSeconds(Amount * 60);
        }

        public int AllMinutes()
        {
            return (AllSeconds() / 60);
        }

        public TTime AddHours(int Amount)
        {
            return AddMinutes(Amount * 60);
        }

        public int AllHours()
        {
            return (AllMinutes() / 60);
        }

        public TTime AddDays(int Amount)
        {
            return AddHours(Amount * 0x18);
        }

        public int AllDays()
        {
            return (AllHours() / 0x18);
        }

        public bool Next(int due = 0, int time = 0)
        {
            if (time == 0)
            {
                time = timeGetTime().int_0;
            }
            return ((int_0 + due) <= time);
        }

        public void Set(int due, int time = 0)
        {
            if (time == 0)
            {
                time = timeGetTime().int_0;
            }
            int_0 = time + due;
        }

        public void SetSeconds(int due, int time = 0)
        {
            Set(due * 0x3e8, time);
        }

        public override bool Equals(object obj)
        {
            if (obj is TTime)
            {
                return (((TTime)obj) == this);
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return int_0.ToString();
        }

        public override int GetHashCode()
        {
            return int_0;
        }

        public static bool operator ==(TTime t1, TTime t2)
        {
            return (t1.int_0 == t2.int_0);
        }

        public static bool operator !=(TTime t1, TTime t2)
        {
            return (t1.int_0 != t2.int_0);
        }

        public static bool operator >(TTime t1, TTime t2)
        {
            return (t1.int_0 > t2.int_0);
        }

        public static bool operator <(TTime t1, TTime t2)
        {
            return (t1.int_0 < t2.int_0);
        }

        public static bool operator >=(TTime t1, TTime t2)
        {
            return (t1.int_0 >= t2.int_0);
        }

        public static bool operator <=(TTime t1, TTime t2)
        {
            return (t1.int_0 <= t2.int_0);
        }

        public static TTime operator -(TTime t1, TTime t2)
        {
            return new TTime(t1.int_0 - t2.int_0);
        }

        [DllImport("winmm.dll")]
        public static extern TTime timeGetTime();
    }
}
