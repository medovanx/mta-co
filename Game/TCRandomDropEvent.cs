//using MTA.Network.GamePackets;
//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace MTA.Game
//{
//    public class TCRandomDropEvent
//    {
//        static Zoning.Zone area = new Zoning.Zone(new System.Drawing.Point(421, 361), new System.Drawing.Point(456, 361), new System.Drawing.Point(421, 395), new System.Drawing.Point(456, 395));
//        static Map map = Kernel.Maps[1002];
//        static DateTime lastdrop = DateTime.Now;

//        public static bool CanDrop()
//        {
//            if ((DateTime.Now.Hour == 7 || DateTime.Now.Hour == 11 || DateTime.Now.Hour == 15 || DateTime.Now.Hour == 19) && DateTime.Now.Minute >= 55)
//            {
//                int min = 60 - DateTime.Now.Minute;
//                Kernel.SendWorldMessage(new Message("TC Drop-Event will start in " + min.ToString() + " Minuts, Please come to the center of the TwinCity!", System.Drawing.Color.Red, Message.TopLeft));
//            }
//            if ((DateTime.Now.Hour == 8 || DateTime.Now.Hour == 12 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 18) && DateTime.Now.Minute >= 0 && DateTime.Now.Minute <= 5)
//            {
//                if (DateTime.Now >= lastdrop.AddSeconds(20))
//                    return true;
//            }
//            return false;
//        }

//        public static void Drop()
//        {

//            try
//            {
//                for (int j = 0; j < 20; j++)
//                {
//                    for (int i = 0; i < 30; i++)
//                    {
//                        ushort X = (ushort)Kernel.Random.Next(420, 460);
//                        ushort Y = (ushort)Kernel.Random.Next(360, 396);
//                        if (map.Floor[X, Y, MapObjectType.Item, null])
//                        {
//                            FloorItem floorItem = new FloorItem(true);

//                            floorItem.ItemID = 1088000;
//                            floorItem.Item = new ConquerItem(true);
//                            floorItem.Item.ID = 1088000;
//                            floorItem.Item.UID = FloorItem.FloorUID.Next;
//                            floorItem.UID = floorItem.Item.UID;
//                            floorItem.Item.MobDropped = true;
//                            while (map.Npcs.ContainsKey(floorItem.Item.UID))
//                            {
//                                floorItem.Item.UID = FloorItem.FloorUID.Next;
//                                floorItem.UID = FloorItem.FloorUID.Next;
//                            }

//                            floorItem.MapID = map.ID;
//                            floorItem.MapObjType = Game.MapObjectType.Item;
//                            floorItem.X = X;
//                            floorItem.Y = Y;
//                            floorItem.Type = FloorItem.Drop;
//                            floorItem.OnFloor = Time32.Now;

//                            map.AddFloorItem(floorItem);
//                            lastdrop = DateTime.Now;
//                            break;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("TC DROP \n" + ex.ToString());
//            }
//        }
//    }
//}
