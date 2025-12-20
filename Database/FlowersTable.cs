using System;
using System.Linq;

namespace MTA.Database {
    public class Flowers {
        public static void SaveFlowers() {
            using (Write write = new Write(Constants.FlowersPath)) {
                Game.Features.Flowers.Flowers[] array = Game.Features.Flowers.Flowers.Flowers_Poll.Values
                    .ToArray<Game.Features.Flowers.Flowers>();
                uint count = (uint)Game.Features.Flowers.Flowers.Flowers_Poll.Count;
                string[] array2 = new string[count];
                for (uint num = 0u; num < count; num += 1u) {
                    array2[(int)num] = array[(int)num].ToString();
                }

                write.Add(array2, array2.Length).Execute(Mode.Open);
            }

            using (Write write2 = new Write(Constants.BoyFlowersPath)) {
                Game.Features.Flowers.Flowers[] array3 = Game.Features.Flowers.Flowers.BoyFlowers.Values
                    .ToArray<Game.Features.Flowers.Flowers>();
                uint count2 = (uint)Game.Features.Flowers.Flowers.BoyFlowers.Count;
                string[] array4 = new string[count2];
                for (uint num2 = 0u; num2 < count2; num2 += 1u) {
                    array4[(int)num2] = array3[(int)num2].ToString();
                }

                write2.Add(array4, array4.Length).Execute(Mode.Open);
            }
        }

        public static void LoadFlowers() {
            using (Read read = new Read(Constants.FlowersPath)) {
                if (read.Reader(true)) {
                    int count = read.Count;
                    uint num = 0u;
                    while (num < (ulong)count) {
                        string text = read.ReadString("");
                        if (text != null) {
                            Game.Features.Flowers.Flowers flowers = new Game.Features.Flowers.Flowers();
                            flowers.Read(text);
                            if (!Game.Features.Flowers.Flowers.Flowers_Poll.ContainsKey(flowers.UID)) {
                                Game.Features.Flowers.Flowers.Flowers_Poll.TryAdd(flowers.UID, flowers);
                            }
                            else {
                                Game.Features.Flowers.Flowers.Flowers_Poll[flowers.UID] = flowers;
                            }

                            Game.Features.Flowers.Flowers.CulculateRankRouse(flowers);
                            Game.Features.Flowers.Flowers.CulculateRankLilies(flowers);
                            Game.Features.Flowers.Flowers.CulculateRankOrchids(flowers);
                            Game.Features.Flowers.Flowers.CulculateRankTulips(flowers);
                        }

                        num += 1u;
                    }
                }
            }

            using (Read read2 = new Read(Constants.BoyFlowersPath)) {
                if (read2.Reader(true)) {
                    int count2 = read2.Count;
                    uint num2 = 0u;
                    while (num2 < (ulong)count2) {
                        string text2 = read2.ReadString("");
                        if (text2 != null) {
                            Game.Features.Flowers.Flowers flowers2 = new Game.Features.Flowers.Flowers();
                            flowers2.Read(text2);
                            if (!Game.Features.Flowers.Flowers.BoyFlowers.ContainsKey(flowers2.UID)) {
                                Game.Features.Flowers.Flowers.BoyFlowers.TryAdd(flowers2.UID, flowers2);
                            }
                            else {
                                Game.Features.Flowers.Flowers.BoyFlowers[flowers2.UID] = flowers2;
                            }

                            Game.Features.Flowers.Flowers.CulculateRankKiss(flowers2);
                            Game.Features.Flowers.Flowers.CulculateRankLove(flowers2);
                            Game.Features.Flowers.Flowers.CulculateRankTine(flowers2);
                            Game.Features.Flowers.Flowers.CulculateRankJade(flowers2);
                        }

                        num2 += 1u;
                    }
                }
            }

            GC.Collect();
        }
    }
}