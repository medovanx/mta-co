using System;
using MTA.Client;
using MTA.Database;
using MTA.Game;
using MTA.Game.ConquerStructures;
using MTA.Interfaces;
using MTA.Network;
using MTA.Network.GamePackets;

namespace MTA.MaTrix {
    public class RetreatChi : Writer, IPacket {
        public enum RetreatType {
            #region client

            Info = 0,
            RequestRetreat = 1,
            RequestRestore = 3,
            RequestExtend = 5,
            RequestPayoff = 7,
            RequestAbondan = 9,
            RequestUpdate = 11,
            RequestExtend2 = 13,

            #endregion

            #region Server

            Retreat = 2,
            Restore = 4,
            Extend = 6,
            Payoff = 8,
            Abondan = 10,
            Update = 12,
            Extend2 = 14,

            #endregion Server
        }

        byte[] Buffer;

        public RetreatChi(bool Create) {
            if (Create) {
                Buffer = new byte[7 + 8];
                WriteUInt16(7, 0, Buffer);
                WriteUInt16(2536, 2, Buffer);
            }
        }

        public RetreatType Type {
            get { return (RetreatType)BitConverter.ToUInt16(Buffer, 4); }
            set { WriteUInt16((ushort)value, 4, Buffer); }
        }

        public Enums.ChiPowerType Mode {
            get { return (Enums.ChiPowerType)Buffer[6]; }
            set { Buffer[6] = (byte)value; }
        }

        public void Send(GameState client) {
            client.Send(Buffer);
        }

        public byte[] ToArray() {
            return Buffer;
        }

        public void Deserialize(byte[] buffer) {
            Buffer = buffer;
        }

        public static void Retreat(byte[] packet, GameState client) {
            RetreatChi RetreatChi = new RetreatChi(false);
            RetreatChi.Deserialize(packet);
            switch (RetreatChi.Type) {
                default: {
                    PacketHandler.PrintPacket(packet);
                    break;
                }
                case RetreatType.Info: {
                    var powers = client.Retretead_ChiPowers;
                    RetreatChi2 RetreatChi2 = new RetreatChi2(powers.Length);
                    RetreatChi2.Append(powers);
                    client.Send(RetreatChi2);
                    break;
                }
                case RetreatType.RequestRetreat: {
                    if (client.ChiPoints >= 4000) {
                        var power = client.ChiPowers[(int)RetreatChi.Mode - 1];
                        if (client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1] == null) {
                            client.ChiPoints -= 4000;
                            var newpower = new ChiPowerStructure();
                            newpower.Attributes = power.Attributes;
                            newpower.Power = power.Power;
                            newpower.CalculatePoints();
                            newpower.Retreat_TimeLeft = DateTime.Now.AddDays(5);
                            client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1] = newpower;
                            RetreatChi.Type = RetreatType.Retreat;
                            RetreatChi.Send(client);
                            ChiTable.Save(client);
                        }
                    }

                    break;
                }
                case RetreatType.RequestRestore: //Restore
                {
                    var power = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1];
                    if (client.Retretead_ChiPowers.Contains(power)) {
                        var newpower = client.ChiPowers[(int)RetreatChi.Mode - 1];
                        newpower = new ChiPowerStructure();
                        newpower.Attributes = power.Attributes;
                        newpower.Power = power.Power;
                        newpower.CalculatePoints();
                        client.ChiPowers[(int)RetreatChi.Mode - 1] = newpower;
                        ChiTable.Sort(RetreatChi.Mode);
                        client.Send(new ChiPowers(true).Query(client));

                        #region update ranking

                        ChiTable.ChiData[] array = null;
                        switch (RetreatChi.Mode) {
                            case Enums.ChiPowerType.Dragon:
                                array = ChiTable.Dragon;
                                break;

                            case Enums.ChiPowerType.Phoenix:
                                array = ChiTable.Phoenix;
                                break;

                            case Enums.ChiPowerType.Tiger:
                                array = ChiTable.Tiger;
                                break;

                            case Enums.ChiPowerType.Turtle:
                                array = ChiTable.Turtle;
                                break;
                        }

                        foreach (var chiData in array) {
                            if (Kernel.GamePool.ContainsKey(chiData.UID)) {
                                var pClient = Kernel.GamePool[chiData.UID];
                                if (pClient == null) continue;
                                if (pClient.ChiData == null) continue;
                                //   PacketHandler.SendRankingQuery(new GenericRanking(true) { Mode = GenericRanking.QueryCount }, pClient, GenericRanking.Chi + (uint)chi.Mode, pClient.ChiData.SelectRank(chi.Mode), pClient.ChiData.SelectPoints(chi.Mode));
                                if (pClient.Entity.UID == client.Entity.UID ||
                                    pClient.ChiData.SelectRank(RetreatChi.Mode) < 50)
                                    pClient.LoadItemStats();
                            }
                        }

                        #endregion

                        ChiTable.Save(client);


                        RetreatChi.Type = RetreatType.Restore; //Restore
                        RetreatChi.Send(client);
                    }

                    break;
                }
                case RetreatType.RequestExtend: //Extend
                {
                    if (client.ChiPoints >= 4000) {
                        var power = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1];
                        if (client.Retretead_ChiPowers.Contains(power)) {
                            client.ChiPoints -= 4000;
                            client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1].Retreat_TimeLeft = client
                                .Retretead_ChiPowers[(int)RetreatChi.Mode - 1].Retreat_TimeLeft.AddDays(5);
                            RetreatChi.Type = RetreatType.Extend; //Extend
                            RetreatChi.Send(client);
                            ChiTable.Save(client);
                        }
                    }
                    else
                        return;

                    break;
                }

                case RetreatType.RequestPayoff: //Restore pay off
                {
                    var power = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1];
                    if (client.Retretead_ChiPowers.Contains(power)) {
                        var newpower = client.ChiPowers[(int)RetreatChi.Mode - 1];
                        newpower = new ChiPowerStructure();
                        newpower.Attributes = power.Attributes;
                        newpower.Power = power.Power;
                        newpower.CalculatePoints();
                        client.ChiPowers[(int)RetreatChi.Mode - 1] = newpower;
                        client.Send(new ChiPowers(true).Query(client));

                        #region update ranking

                        ChiTable.ChiData[] array = null;
                        switch (RetreatChi.Mode) {
                            case Enums.ChiPowerType.Dragon:
                                array = ChiTable.Dragon;
                                break;

                            case Enums.ChiPowerType.Phoenix:
                                array = ChiTable.Phoenix;
                                break;

                            case Enums.ChiPowerType.Tiger:
                                array = ChiTable.Tiger;
                                break;

                            case Enums.ChiPowerType.Turtle:
                                array = ChiTable.Turtle;
                                break;
                        }

                        foreach (var chiData in array) {
                            if (Kernel.GamePool.ContainsKey(chiData.UID)) {
                                var pClient = Kernel.GamePool[chiData.UID];
                                if (pClient == null) continue;
                                if (pClient.ChiData == null) continue;
                                //   PacketHandler.SendRankingQuery(new GenericRanking(true) { Mode = GenericRanking.QueryCount }, pClient, GenericRanking.Chi + (uint)chi.Mode, pClient.ChiData.SelectRank(chi.Mode), pClient.ChiData.SelectPoints(chi.Mode));
                                if (pClient.Entity.UID == client.Entity.UID ||
                                    pClient.ChiData.SelectRank(RetreatChi.Mode) < 50)
                                    pClient.LoadItemStats();
                            }
                        }

                        #endregion

                        ChiTable.Save(client);

                        RetreatChi.Type = RetreatType.Payoff; //Restore pay off
                        RetreatChi.Send(client);
                    }

                    break;
                }
                case RetreatType.RequestAbondan: //Abondan 
                {
                    var power = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1];
                    if (client.Retretead_ChiPowers.Contains(power)) {
                        client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1] = null;

                        RetreatChi.Type = RetreatType.Abondan; //Abondan 
                        RetreatChi.Send(client);
                        ChiTable.Save(client);
                    }

                    break;
                }
                case RetreatType.RequestUpdate: //Update
                {
                    var power = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1];
                    if (client.Retretead_ChiPowers.Contains(power)) {
                        var newpower = client.ChiPowers[(int)RetreatChi.Mode - 1];
                        power = new ChiPowerStructure();
                        power.Attributes = newpower.Attributes;
                        power.Power = newpower.Power;
                        power.Retreat_TimeLeft = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1].Retreat_TimeLeft;
                        power.CalculatePoints();
                        client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1] = power;
                        RetreatChi.Type = RetreatType.Update; //Update
                        RetreatChi.Send(client);
                        ChiTable.Save(client);
                    }

                    break;
                }
                case RetreatType.RequestExtend2: //Extend
                {
                    if (client.ChiPoints >= 4000) {
                        var power = client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1];
                        if (client.Retretead_ChiPowers.Contains(power)) {
                            client.ChiPoints -= 4000;
                            client.Retretead_ChiPowers[(int)RetreatChi.Mode - 1].Retreat_TimeLeft = client
                                .Retretead_ChiPowers[(int)RetreatChi.Mode - 1].Retreat_TimeLeft.AddDays(5);

                            RetreatChi.Type = RetreatType.Extend2; //Extend
                            RetreatChi.Send(client);
                            ChiTable.Save(client);
                        }
                    }
                    else
                        return;

                    break;
                }
            }
        }
    }

    public class RetreatChi2 : Writer, IPacket {
        byte[] Buffer;
        int Offset = 8;

        public RetreatChi2(int count) {
            {
                Buffer = new byte[8 + 8 + 21 * count];
                WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                WriteUInt16(2537, 2, Buffer);
                Count = (uint)count;
            }
        }

        private uint Count {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }

        public void Send(GameState client) {
            client.Send(Buffer);
        }

        public byte[] ToArray() {
            return Buffer;
        }

        public void Deserialize(byte[] buffer) {
            Buffer = buffer;
        }

        public void Append(ChiPowerStructure[] powers) {
            if (powers == null) return;
            for (int i = 0; i < powers.Length; i++) {
                if (powers[i] == null)
                    continue;
                Append(powers[i]);
            }
        }

        private void Append(ChiPowerStructure power) {
            if (power == null) return;
            Buffer[Offset] = (byte)power.Power;
            Offset++;
            var now = power.Retreat_TimeLeft;
            uint secs = (uint)(now.Year % 100 * 100000000 +
                               (now.Month) * 1000000 +
                               now.Day * 10000 +
                               now.Hour * 100 +
                               now.Minute);
            Uint((uint)secs, Offset, Buffer);
            Offset += 4;
            var attributes = power.Attributes;
            foreach (var attribute in attributes) {
                WriteInt32(attribute, Offset, Buffer);
                Offset += 4;
            }
        }
    }
}