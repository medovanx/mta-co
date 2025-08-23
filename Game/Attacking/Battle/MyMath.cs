// * Created by CptSky
// * Copyright © 2010
// * COPS v6 Emulator - Project

using System;
using System.Drawing;
using System.Collections.Generic;
using CO2_CORE_DLL;

namespace MTA
{
    public partial class MyMath
    {
        //  private static SafeRandom Rand = new SafeRandom(Environment.TickCount);

        public const Int32 NORMAL_RANGE = 17;
        public const Int32 BIG_RANGE = 34;
        public const Int32 USERDROP_RANGE = 9;

        public static Boolean Success(Double Chance)
        {
            return ((Double)Generate(1, 1000000)) / 10000 >= 100 - Chance;
        }

        public static Int32 Generate(Int32 Min, Int32 Max)
        {
            if (Max != Int32.MaxValue)
                Max++;

            Int32 Value = 0;
            /*lock (Rand) { */
            Value = Kernel.Random.Next(Min, Max); /*}*/
            return Value;
        }

        public static Int32 Generate(UInt32 Min, UInt32 Max)
        {
            if (Max != Int32.MaxValue)
                Max++;

            Int32 Value = 0;
            /*lock (Rand) { */
            Value = Kernel.Random.Next((int)Min, (int)Max); /*}*/
            return Value;
        }

    }
}
