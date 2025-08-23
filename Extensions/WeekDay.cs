using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA
{
    public class WeekDay
    {
        private uint day;

        public const uint
            // Summary:
            //     Indicates Sunday.
            Sunday = 1 << 0,
            //
            // Summary:
            //     Indicates Monday.
            Monday = 1 << 1,
            //
            // Summary:
            //     Indicates Tuesday.
            Tuesday = 1 << 2,
            //
            // Summary:
            //     Indicates Wednesday.
            Wednesday = 1 << 3,
            //
            // Summary:
            //     Indicates Thursday.
            Thursday = 1 << 4,
            //
            // Summary:
            //     Indicates Friday.
            Friday = 1 << 5,
            //
            // Summary:
            //     Indicates Saturday.
            Saturday = 1 << 6,

            Everyday = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday;

        public static implicit operator WeekDay(uint day)
        {
            return new WeekDay() { day = day };
        }

        public bool Contains(DayOfWeek day)
        {
            uint flag = 0;
            if (day == DayOfWeek.Friday) flag = Friday;
            if (day == DayOfWeek.Monday) flag = Monday;
            if (day == DayOfWeek.Saturday) flag = Saturday;
            if (day == DayOfWeek.Sunday) flag = Sunday;
            if (day == DayOfWeek.Thursday) flag = Thursday;
            if (day == DayOfWeek.Tuesday) flag = Tuesday;
            if (day == DayOfWeek.Wednesday) flag = Wednesday;
            return ((this.day & flag) == flag);
        }
    }
}
