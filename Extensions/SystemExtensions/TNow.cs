using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            this.int_0 = Value;
        }

        public TTime(uint Value)
        {
            this.int_0 = (int)Value;
        }

        public TTime(long Value)
        {
            this.int_0 = (int)Value;
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
                return this.int_0;
            }
        }
        public int Value
        {
            get
            {
                return this.int_0;
            }
        }
        public TTime AddMilliseconds(int Amount)
        {
            return new TTime(this.int_0 + Amount);
        }

        public int AllMilliseconds()
        {
            return this.GetHashCode();
        }

        public TTime AddSeconds(int Amount)
        {
            return this.AddMilliseconds(Amount * 0x3e8);
        }

        public int AllSeconds()
        {
            return (this.AllMilliseconds() / 0x3e8);
        }

        public TTime AddMinutes(int Amount)
        {
            return this.AddSeconds(Amount * 60);
        }

        public int AllMinutes()
        {
            return (this.AllSeconds() / 60);
        }

        public TTime AddHours(int Amount)
        {
            return this.AddMinutes(Amount * 60);
        }

        public int AllHours()
        {
            return (this.AllMinutes() / 60);
        }

        public TTime AddDays(int Amount)
        {
            return this.AddHours(Amount * 0x18);
        }

        public int AllDays()
        {
            return (this.AllHours() / 0x18);
        }

        public bool Next(int due = 0, int time = 0)
        {
            if (time == 0)
            {
                time = timeGetTime().int_0;
            }
            return ((this.int_0 + due) <= time);
        }

        public void Set(int due, int time = 0)
        {
            if (time == 0)
            {
                time = timeGetTime().int_0;
            }
            this.int_0 = time + due;
        }

        public void SetSeconds(int due, int time = 0)
        {
            this.Set(due * 0x3e8, time);
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
            return this.int_0.ToString();
        }

        public override int GetHashCode()
        {
            return this.int_0;
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
