using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    public class Flowers
    {
        public static void SaveFlowers()
        {
            using (Write write = new Write(Constants.DatabaseBasePath + "flowers.txt"))
            {
                MTA.Game.Features.Flowers.Flowers[] array = MTA.Game.Features.Flowers.Flowers.Flowers_Poll.Values.ToArray<MTA.Game.Features.Flowers.Flowers>();
                uint count = (uint)MTA.Game.Features.Flowers.Flowers.Flowers_Poll.Count;
                string[] array2 = new string[count];
                for (uint num = 0u; num < count; num += 1u)
                {
                    array2[(int)((UIntPtr)num)] = array[(int)((UIntPtr)num)].ToString();
                }
                write.Add(array2, array2.Length).Execute(Mode.Open);
            }
            using (Write write2 = new Write(Constants.DatabaseBasePath + "boyflowers.txt"))
            {
                MTA.Game.Features.Flowers.Flowers[] array3 = MTA.Game.Features.Flowers.Flowers.BoyFlowers.Values.ToArray<MTA.Game.Features.Flowers.Flowers>();
                uint count2 = (uint)MTA.Game.Features.Flowers.Flowers.BoyFlowers.Count;
                string[] array4 = new string[count2];
                for (uint num2 = 0u; num2 < count2; num2 += 1u)
                {
                    array4[(int)((UIntPtr)num2)] = array3[(int)((UIntPtr)num2)].ToString();
                }
                write2.Add(array4, array4.Length).Execute(Mode.Open);
            }
        }
        public static void LoadFlowers()
        {
            using (Read read = new Read(Constants.DatabaseBasePath + "flowers.txt"))
            {
                if (read.Reader(true))
                {
                    int count = read.Count;
                    uint num = 0u;
                    while ((ulong)num < (ulong)((long)count))
                    {
                        string text = read.ReadString("");
                        if (text != null)
                        {
                            MTA.Game.Features.Flowers.Flowers flowers = new MTA.Game.Features.Flowers.Flowers();
                            flowers.Read(text);
                            if (!MTA.Game.Features.Flowers.Flowers.Flowers_Poll.ContainsKey(flowers.UID))
                            {
                                MTA.Game.Features.Flowers.Flowers.Flowers_Poll.TryAdd(flowers.UID, flowers);
                            }
                            else
                            {
                                MTA.Game.Features.Flowers.Flowers.Flowers_Poll[flowers.UID] = flowers;
                            }
                            MTA.Game.Features.Flowers.Flowers.CulculateRankRouse(flowers);
                            MTA.Game.Features.Flowers.Flowers.CulculateRankLilies(flowers);
                            MTA.Game.Features.Flowers.Flowers.CulculateRankOrchids(flowers);
                            MTA.Game.Features.Flowers.Flowers.CulculateRankTulips(flowers);
                        }
                        num += 1u;
                    }
                }
            }
            using (Read read2 = new Read(Constants.DatabaseBasePath + "boyflowers.txt"))
            {
                if (read2.Reader(true))
                {
                    int count2 = read2.Count;
                    uint num2 = 0u;
                    while ((ulong)num2 < (ulong)((long)count2))
                    {
                        string text2 = read2.ReadString("");
                        if (text2 != null)
                        {
                            MTA.Game.Features.Flowers.Flowers flowers2 = new MTA.Game.Features.Flowers.Flowers();
                            flowers2.Read(text2);
                            if (!MTA.Game.Features.Flowers.Flowers.BoyFlowers.ContainsKey(flowers2.UID))
                            {
                                MTA.Game.Features.Flowers.Flowers.BoyFlowers.TryAdd(flowers2.UID, flowers2);
                            }
                            else
                            {
                                MTA.Game.Features.Flowers.Flowers.BoyFlowers[flowers2.UID] = flowers2;
                            }
                            MTA.Game.Features.Flowers.Flowers.CulculateRankKiss(flowers2);
                            MTA.Game.Features.Flowers.Flowers.CulculateRankLove(flowers2);
                            MTA.Game.Features.Flowers.Flowers.CulculateRankTine(flowers2);
                            MTA.Game.Features.Flowers.Flowers.CulculateRankJade(flowers2);
                        }
                        num2 += 1u;
                    }
                }
            }
            GC.Collect();
        }
    }
}
