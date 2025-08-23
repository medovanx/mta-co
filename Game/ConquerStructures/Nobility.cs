// ☺ Created by jiMMy
// ☺ Copyright © 2010 - 2016 TQ Digital
// ☺ Franko - Project

using System;
using System.IO;
using System.Linq;
using MTA.Client;
using System.Collections.Generic;
using MTA.Network.GamePackets;
using System.Drawing;

namespace MTA.Game.ConquerStructures
{
    public unsafe class Nobility : Network.Writer
    {
        public static ulong MaxDonation = 0;
        public static ulong MaxDonation1 = 0;
        public static ulong MaxDonation2 = 0;
        public static SafeDictionary<uint, NobilityInformation> Board = new SafeDictionary<uint, NobilityInformation>(10000);
        public static List<NobilityInformation> BoardList = new List<NobilityInformation>(10000);
        public static void Handle(NobilityInfo information, Client.GameState client)
        {
            switch (information.Type)
            {
                case NobilityInfo.Donate:
                    {
                        if (client.Trade.InTrade) return;
                        ulong silvers = information.dwParam;
                        bool newDonator = false;
                        client.NobilityInformation.Gender = (byte)(client.Entity.Body % 10);
                        if (client.NobilityInformation.Donation == 0)
                            newDonator = true;
                        if (information.MoneyType == 1)
                        {
                            ulong cps = silvers;
                            if (client.Entity.ConquerPoints >= cps)
                            {
                                client.Entity.ConquerPoints -= (uint)cps;
                                client.NobilityInformation.Donation += silvers;
                            }
                        }
                        //else if (client.Entity.Money >= silvers)
                        //{
                        //    client.Entity.Money -= (ulong)silvers;
                        //    client.NobilityInformation.Donation += silvers;
                        //}
                        if (!Board.ContainsKey(client.Entity.UID) && client.NobilityInformation.Donation == silvers && newDonator)
                        {
                            Board.Add(client.Entity.UID, client.NobilityInformation);
                            try
                            {
                                Database.NobilityTable.InsertNobilityInformation(client.NobilityInformation);
                            }
                            catch
                            {
                                Database.NobilityTable.UpdateNobilityInformation(client.NobilityInformation);
                            }
                        }
                        else
                        {
                            Database.NobilityTable.UpdateNobilityInformation(client.NobilityInformation);
                        }
                        Sort(client.Entity.UID);
                        break;
                    }
                case NobilityInfo.NextRank:
                    {
                        try
                        {

                            if (BoardList == null)
                                return;
                            if (client.NobilityInformation == null)
                                return;
                            ulong duke = 0, price = 0, king = 0;
                            var last = BoardList.Where(x => x.Rank == NobilityRank.Duke && x.Donation >= client.NobilityInformation.Donation).LastOrDefault();
                            if (last != null)
                                duke = last.Donation;
                            last = BoardList.Where(x => x.Rank == NobilityRank.Prince && x.Donation >= client.NobilityInformation.Donation).LastOrDefault();
                            if (last != null)
                                price = last.Donation;
                            last = BoardList.Where(x => x.Rank == NobilityRank.King && x.Donation >= client.NobilityInformation.Donation).LastOrDefault();
                            if (last != null)
                                king = last.Donation;
                            ulong[] Contributions = new ulong[]
                              {
                                0,
                                30000000,
                                0,
                                100000000,
                                0,
                                200000000,
                                0,
                                (byte)client.NobilityInformation.Rank > 7  || !BoardList.Any(x => x.Rank == NobilityRank.Duke) ? 0 : duke - client.NobilityInformation.Donation,
                                0,
                                (byte)client.NobilityInformation.Rank > 9 || !BoardList.Any(x => x.Rank == NobilityRank.Prince) ? 0 : price - client.NobilityInformation.Donation,
                                0,
                                0,
                                (byte)client.NobilityInformation.Position == 1 || !BoardList.Any(x => x.Rank == NobilityRank.King) ? 0 : king - client.NobilityInformation.Donation,
                              };
                            MemoryStream strm = new MemoryStream();
                            BinaryWriter wtr = new BinaryWriter(strm);
                            wtr.Write((ushort)0);
                            wtr.Write((ushort)2064);
                            wtr.Write((uint)4);
                            byte[] Ranks = { 1, 3, 5, 7, 9, 12 };
                            foreach (var Rank in Ranks)
                            {
                                //Console.WriteLine(Rank);
                                wtr.Write((ulong)Contributions[Rank]);
                                wtr.Write((uint)4294967295);
                                wtr.Write((uint)Rank);
                            }
                            int packetlength = (int)strm.Length;
                            strm.Position = 0;
                            wtr.Write((ushort)packetlength);
                            strm.Position = strm.Length;
                            wtr.Write(Program.Encoding.GetBytes("TQServer"));
                            strm.Position = 0;
                            byte[] buf = new byte[strm.Length];
                            strm.Read(buf, 0, buf.Length);
                            client.Send(buf);
                            wtr.Close();
                            strm.Close();
                        }
                        catch (Exception e)
                        {
                            MTA.Console.WriteLine(e); Program.SaveException(e);
                        }
                        break;
                    }
                case NobilityInfo.List:
                    {
                        byte[] packet = new byte[600 + 8];
                        WriteUInt16(600, 0, packet);
                        WriteUInt16(2064, 2, packet);
                        WriteUInt16(2, 4, packet);
                        WriteUInt16((ushort)(BoardList.Count > 50 ? 5 : (BoardList.Count / 10) + 1), 10, packet);
                        ushort Count = 0;
                        int offset = 120;
                        for (int i = (int)(information.wParam1 * 10); i < information.wParam1 * 10 + 10 && i < BoardList.Count; i++)
                        {
                            var nob = BoardList[i];
                            WriteUInt32(nob.EntityUID, offset, packet);
                            Count++;
                            offset += 4;
                            if (Kernel.GamePool.ContainsKey(nob.EntityUID))
                            {
                                WriteUInt32(1, offset, packet);
                                offset += 4;
                                WriteUInt32(nob.Mesh, offset, packet);
                                offset += 4;
                            }
                            else
                            {
                                offset += 8;
                            }
                            WriteString(nob.Name, offset, packet);
                            offset += 20;
                            WriteUInt64(nob.Donation, offset, packet);
                            offset += 8;
                            WriteUInt32((uint)nob.Rank, offset, packet);
                            offset += 4;
                            WriteUInt32((uint)i, offset, packet);
                            offset += 4;
                        }
                        WriteUInt16(Count, 12, packet);
                        client.Send(packet);
                        break;
                    }
            }
        }
        public static void Donate(ulong silvers, Client.GameState client)
        {
            bool newDonator = false;
            client.NobilityInformation.Gender = (byte)(client.Entity.Body % 10);
            if (client.NobilityInformation.Donation == 0) newDonator = true;
            client.NobilityInformation.Donation += silvers;

            if (!Board.ContainsKey(client.Entity.UID) && client.NobilityInformation.Donation == silvers && newDonator)
            {
                Board.Add(client.Entity.UID, client.NobilityInformation);
                try
                {
                    Database.NobilityTable.InsertNobilityInformation(client.NobilityInformation);
                }
                catch
                {
                    Database.NobilityTable.UpdateNobilityInformation(client.NobilityInformation);
                }
            }
            else
            {
                Database.NobilityTable.UpdateNobilityInformation(client.NobilityInformation);
            }
            Sort(client.Entity.UID);
        }
        public static void Sort(uint updateUID)
        {
            SafeDictionary<uint, NobilityInformation> sortedBoard = new SafeDictionary<uint, NobilityInformation>();
            {
                int Place = 0;
                foreach (NobilityInformation entry in Board.Values.OrderByDescending((p) => p.Donation))
                {
                    Client.GameState client = null;
                    try
                    {
                        int previousPlace = entry.Position;
                        entry.Position = Place;
                        NobilityRank Rank = NobilityRank.Serf;

                        if (Place >= 5000)
                        {
                            if (entry.Donation >= 20000)
                            {
                                Rank = NobilityRank.Earl;
                            }
                            else if (entry.Donation >= 1000)
                            {
                                Rank = NobilityRank.Baron;
                            }
                            else if (entry.Donation >= 300)
                            {
                                Rank = NobilityRank.Knight;
                            }
                        }
                        else
                        {
                            if (Place < 3)//serverrank
                            {
                                Rank = NobilityRank.King;
                                if (Place < (rates.king - (rates.king - 1)))
                                {
                                    MaxDonation = entry.Donation;
                                }
                            }
                            else if (Place < 15)
                            {
                                Rank = NobilityRank.Prince;
                                if (Place < (rates.prince + 2))
                                {
                                    MaxDonation1 = entry.Donation;
                                }
                            }
                            else if (Place < 50)
                            {
                                Rank = NobilityRank.Duke;
                                if (Place < (rates.duke + 2))
                                {
                                    MaxDonation2 = entry.Donation;
                                }
                            }
                        }
                        var oldRank = entry.Rank;
                        entry.Rank = Rank;
                        if (Kernel.GamePool.TryGetValue(entry.EntityUID, out client))
                        {
                            bool updateTheClient = false;
                            if (oldRank != Rank)
                            {
                                updateTheClient = true;
                                if (Rank == NobilityRank.Knight)
                                {
                                    Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + " Donation To Knight in Nobility Rank.", Color.Red, Network.GamePackets.Message.Center));
                                }
                                if (Rank == NobilityRank.Baron)
                                {
                                    Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + " Donation To Baron in Nobility Rank.", Color.Red, Network.GamePackets.Message.Center));
                                }
                                if (Rank == NobilityRank.Earl)
                                {
                                    Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + " Donation To Earl in Nobility Rank.", Color.Red, Network.GamePackets.Message.Center));
                                }
                                if (Rank == NobilityRank.Duke)
                                {
                                    Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + " Donation To Duke in Nobility Rank.", Color.Red, Network.GamePackets.Message.Center));
                                }
                                if (Rank == NobilityRank.Prince)
                                {
                                    Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + " Donation To Knight in Nobility Rank.", Color.Red, Network.GamePackets.Message.Center));
                                }
                                if (Rank == NobilityRank.King)
                                {
                                    Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + " has become the new King/Queen in Nobility Rank.", Color.Red, Network.GamePackets.Message.Center));
                                }
                            }
                            else
                            {
                                if (previousPlace != Place)
                                {
                                    updateTheClient = true;
                                }
                            }

                            if (updateTheClient || client.Entity.UID == updateUID)
                            {
                                NobilityInfo update = new NobilityInfo(true);
                                update.Type = NobilityInfo.Icon;
                                update.dwParam = entry.EntityUID;
                                update.UpdateString(entry);
                                client.SendScreen(update, true);
                                client.Entity.NobilityRank = entry.Rank;
                            }
                        }
                        sortedBoard.Add(entry.EntityUID, entry);
                        Place++;
                    }
                    catch { }
                }

            }
            Board = sortedBoard;
            lock (BoardList)
                BoardList = Board.Values.ToList();
        }
    }
    public unsafe class NobilityInformation
    {
        public string Name;
        public uint EntityUID;
        public uint Mesh;
        public ulong Donation;
        public byte Gender;
        public int Position;
        public NobilityRank Rank;
    }
    public enum NobilityRank : byte
    {
        Serf = 0,
        Knight = 1,
        Baron = 3,
        Earl = 5,
        Duke = 7,
        Prince = 9,
        King = 12,
    }
}