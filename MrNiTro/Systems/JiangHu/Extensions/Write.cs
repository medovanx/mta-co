using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MTA.Database
{
    public enum Mode : byte
    {
        Open = 1,
        Create = 2
    }
    public class Write : IDisposable
    {
        private string location = "";
        private string[] Items;
        private int Count = 0;

        private StreamWriter SW;



        public Write(string loc)
        {
            location = loc;

        }
        public void ChangeLocation(string loc)
        {
            location = loc;

        }
        public void Dispose()
        {
            Items = null;
            Count = 0;
            location = string.Empty;
            SW = null;
        }



        public Write Add(string[] data, int count)
        {
            Count = count;

            Items = new string[count];

            for (int x = 0; x < count; x++)
                Items[x] = data[x];

            return this;
        }

        public Write Execute(Mode mod)
        {
            using (SW = new StreamWriter(location, false, Encoding.Default))
            {
                SW.WriteLine("Count=" + Count);
                for (int x = 0; x < Count; x++)
                    SW.WriteLine(Items[x]);
            }
            return this;
        }

    }
}
