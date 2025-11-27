using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game
{
    public class ElitePKTournament
    {
        public const byte
            EPK_Lvl100Minus = 0,
            EPK_Lvl100To119 = 1,
            EPK_Lvl120To129 = 2,
            EPK_Lvl130Plus = 3;

        public static ElitePK[] Tournaments;
        public const ushort WaitingAreaID = 2068;
        public static bool TimersRegistered;

        public static void Create()
        {
            Tournaments = new ElitePK[4];
            for (int i = 0; i < 4; i++)
                Tournaments[i] = new ElitePK(i);
            //ElitePK.LoadTop(ElitePKTournament.Tournaments, "ElitePkTop8");
        }
        //public static void GiveClientReward(Client.GameState client)
        //{
        //    DateTime timer = new DateTime();
        //    timer = DateTime.Now.AddDays(1);
        //    bool ReceiveHightRward = false;
        //    //for (int i = 0; i < EPK_Lvl130Plus+1; i++)
        //    //{
        //    //    var Tops = Tournaments[i].Top8;  
        //    //     if (Tops != null)
        //    //     {
        //    //         for (ushort x = 0; x < Tops.Length; x++)
        //    //         {
        //    //             if (Tops[x] != null)
        //    //             {
        //    //                 var obj_client = Tops[x];
        //    //                 if (obj_client.UID == client.Entity.UID)
        //    //                 { 
        //    //                 //الاكونت في التوب

        //    //                 }
        //    //             }
        //    //         }
        //    //     }
        //    //}
        //    var BigRewaurdDictionary = Tournaments[EPK_Lvl130Plus].Top8;
        //    if (BigRewaurdDictionary != null)
        //    {
        //        sbyte client_rank = -1;
        //        for (ushort x = 0; x < BigRewaurdDictionary.Length; x++)
        //        {
        //            if (BigRewaurdDictionary[x] != null)
        //            {
        //                var obj_client = BigRewaurdDictionary[x];
        //                if (obj_client.UID == client.Entity.UID)
        //                    client_rank = (sbyte)x;
        //            }
        //        }
        //        if (client_rank != -1)
        //        {
        //            ReceiveHightRward = true;

        //            if (client_rank == 0)
        //            {
        //                if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePKChamption_High))
        //                    client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePKChamption_High, timer, true);

        //                var EliteSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_High;
        //                var EliteThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_High;
        //                var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
        //                if (client.Entity.Titles.ContainsKey(EliteSecond))
        //                    client.Entity.RemoveTopStatus((ulong)EliteSecond);
        //                if (client.Entity.Titles.ContainsKey(EliteThird))
        //                    client.Entity.RemoveTopStatus((ulong)EliteThird);
        //                if (client.Entity.Titles.ContainsKey(EliteEight))
        //                    client.Entity.RemoveTopStatus((ulong)EliteEight);
        //            }
        //            else if (client_rank == 1)
        //            {
        //                if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_High))
        //                    client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_High, timer, true);

        //                var EliteChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_High;
        //                var EliteThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_High;
        //                var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
        //                if (client.Entity.Titles.ContainsKey(EliteChampion))
        //                    client.Entity.RemoveTopStatus((ulong)EliteChampion);
        //                if (client.Entity.Titles.ContainsKey(EliteThird))
        //                    client.Entity.RemoveTopStatus((ulong)EliteThird);
        //                if (client.Entity.Titles.ContainsKey(EliteEight))
        //                    client.Entity.RemoveTopStatus((ulong)EliteEight);
        //            }
        //            else if (client_rank == 2)
        //            {
        //                if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_High))
        //                    client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_High, timer, true);

        //                var EliteChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_High;
        //                var EliteSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_High;
        //                var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
        //                if (client.Entity.Titles.ContainsKey(EliteChampion))
        //                    client.Entity.RemoveTopStatus((ulong)EliteChampion);
        //                if (client.Entity.Titles.ContainsKey(EliteSecond))
        //                    client.Entity.RemoveTopStatus((ulong)EliteSecond);
        //                if (client.Entity.Titles.ContainsKey(EliteEight))
        //                    client.Entity.RemoveTopStatus((ulong)EliteEight);
        //            }
        //            else
        //            {
        //                if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_High))
        //                    client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_High, timer, true);

        //                var EliteChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_High;
        //                var EliteSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_High;
        //                var EliteThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_High;
        //                if (client.Entity.Titles.ContainsKey(EliteChampion))
        //                    client.Entity.RemoveTopStatus((ulong)EliteChampion);
        //                if (client.Entity.Titles.ContainsKey(EliteSecond))
        //                    client.Entity.RemoveTopStatus((ulong)EliteSecond);
        //                if (client.Entity.Titles.ContainsKey(EliteThird))
        //                    client.Entity.RemoveTopStatus((ulong)EliteThird);
        //            }
        //        }

        //    }
        //    if (!ReceiveHightRward)
        //    {
        //        sbyte client_rank = -1;
        //        for (byte i = 0; i < EPK_Lvl130Plus; i++)
        //        {
        //            var ditionaryLow = Tournaments[i].Top8;
        //            if (ditionaryLow != null)
        //            {
        //                for (byte x = 0; x < ditionaryLow.Length; x++)
        //                {
        //                    if (ditionaryLow[x] != null)
        //                    {
        //                        var obj = ditionaryLow[x];
        //                        if (obj.UID == client.Entity.UID)
        //                            client_rank = (sbyte)x;
        //                    }
        //                }
        //                if (client_rank != -1)
        //                {

        //                    if (client_rank == 0)
        //                    {
        //                        if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low))
        //                            client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low, timer, true);

        //                        var EliteEightSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_Low;
        //                        var EliteEightThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_Low;
        //                        var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_Low;
        //                        if (client.Entity.Titles.ContainsKey(EliteEightSecond))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightSecond);
        //                        if (client.Entity.Titles.ContainsKey(EliteEightThird))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightThird);
        //                        if (client.Entity.Titles.ContainsKey(EliteEight))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEight);
        //                    }
        //                    else if (client_rank == 1)
        //                    {
        //                        if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_Low))
        //                            client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_Low, timer, true);

        //                        var EliteEightChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
        //                        var EliteEightThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_Low;
        //                        var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_Low;
        //                        if (client.Entity.Titles.ContainsKey(EliteEightChampion))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightChampion);
        //                        if (client.Entity.Titles.ContainsKey(EliteEightThird))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightThird);
        //                        if (client.Entity.Titles.ContainsKey(EliteEight))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEight);
        //                    }
        //                    else if (client_rank == 2)
        //                    {
        //                        if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_Low))
        //                            client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_Low, timer, true);

        //                        var EliteEightChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
        //                        var EliteEightSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_Low;
        //                        var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_Low;
        //                        if (client.Entity.Titles.ContainsKey(EliteEightChampion))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightChampion);
        //                        if (client.Entity.Titles.ContainsKey(EliteEightSecond))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightSecond);
        //                        if (client.Entity.Titles.ContainsKey(EliteEight))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEight);
        //                    }
        //                    else
        //                    {
        //                        if (!client.Entity.Titles.ContainsKey(Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_Low))
        //                            client.Entity.AddTopStatus((ulong)Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_Low, timer, true);

        //                        var EliteEightChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
        //                        var EliteEightSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_Low;
        //                        var EliteEightThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_Low;
        //                        if (client.Entity.Titles.ContainsKey(EliteEightChampion))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightChampion);
        //                        if (client.Entity.Titles.ContainsKey(EliteEightSecond))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightSecond);
        //                        if (client.Entity.Titles.ContainsKey(EliteEightThird))
        //                            client.Entity.RemoveTopStatus((ulong)EliteEightThird);
        //                    } break;
        //                }
        //            }
        //        }
        //    }
        //}

        public static bool SignUp(GameState client)
        {
            DateTime Now = DateTime.Now;
            //if (Now.DayOfWeek != DayOfWeek.Friday) return false;

            byte tournament = 0;
            if (client.Entity.Level >= 130)
                tournament = EPK_Lvl130Plus;
            else if (client.Entity.Level >= 120)
                tournament = EPK_Lvl120To129;
            else if (client.Entity.Level >= 100)
                tournament = EPK_Lvl100To119;
            else
                tournament = EPK_Lvl100Minus;

            //  tournament = EPK_Lvl130Plus;

            int extraTime = 3 - tournament;
            extraTime *= 2;
            //if((Now.Hour == 19 && Now.Minute >= 55) || (Now.Hour == 20 && Now.Minute < extraTime))
            if (((Now.Hour == ElitePK.EventTime) && Now.Minute >= 1) || ((Now.Hour == ElitePK.EventTime + 1) && Now.Minute < extraTime))
            {
                Tournaments[tournament].SignUp(client);
                return true;
            }
            return false;
        }

        public static void RegisterTimers()
        {
            if (Tournaments == null) return;
            TimersRegistered = true;
            foreach (var epk in Tournaments)
                if (epk != null)
                    epk.SubscribeTimer();
        }
    }
}
