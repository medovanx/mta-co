
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA
{
    public static class Console
    {
        private static DateTime NOW = DateTime.Now;
        private static Time32 NOW32 = Time32.Now;
        public static string Title
        {
            get
            {
                return System.Console.Title;
            }
            set
            {
                System.Console.Title = value;
            }
        }

        public static int WindowWidth
        {
            get
            {
                return System.Console.WindowWidth;
            }
            set
            {
                System.Console.WindowWidth = value; ;
            }
        }

        public static int WindowHeight
        {
            get
            {
                return System.Console.WindowHeight;
            }
            set
            {
                System.Console.WindowHeight = value; ;
            }
        }


        public static void WriteLine(object line, ConsoleColor color = ConsoleColor.Red)
        {

            if (line.ToString() == "" || line.ToString() == " ")
                System.Console.WriteLine();
            else
            {
                System.Console.Write(TimeStamp() + " ");
                System.Console.Write(line + "\n");
            }
        }

        public static void WriteLine()
        {
            System.Console.WriteLine();
        }

        internal static void WriteLine(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public static string ReadLine()
        {
            return System.Console.ReadLine();
        }

        public static ConsoleColor BackgroundColor
        {
            get
            {
                return System.Console.BackgroundColor;
            }
            set
            {
                System.Console.BackgroundColor = value;
            }
        }

        public static void Clear()
        {
            System.Console.Clear();
        }
        private static Random _random = new Random();
        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return (ConsoleColor)consoleColors.GetValue(_random.Next(consoleColors.Length));
        }

        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.White)
        {
            System.Console.Write(TimeStamp() + " ");
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(text);
            System.Console.ResetColor();
        }

        public static ConsoleColor ForegroundColor
        {
            get
            {
                return System.Console.ForegroundColor;
            }
            set
            {
                System.Console.ForegroundColor = value;
            }
        }

        public static string TimeStamp()
        {
            return "[" + NOW.AddMilliseconds((Time32.Now - NOW32).GetHashCode()).ToString("yyyy-MM-dd HH:mm:ss") + "]";
        }
    }
}