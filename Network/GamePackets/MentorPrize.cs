// * Created by Eslam Abdella
// * Copyright © 2010-2014
// * Emulator - Project

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace MTA.Network.GamePackets
{
    public class MentorPrize : Writer, Interfaces.IPacket
    {
        public const byte
        Show = 0,
        ClaimExperience = 1,
        ClaimSomeExperience = 2,
        ClaimHeavenBlessing = 3,
        ClaimPlus = 4,
        Unknown = 5;

        private byte[] Buffer;
        public MentorPrize(bool create)
        {
            if (!create)
            {
                Buffer = new byte[8 + 40];

                WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                WriteUInt16(2067, 2, Buffer);
            }
        }
        public uint Prize_Type
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }
        public uint Mentor_ID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public ulong Prize_Experience
        {
            get { return BitConverter.ToUInt64(Buffer, 24); }
            set { WriteUInt64(value, 24, Buffer); }
        }
        public ushort Prize_HeavensBlessing
        {
            get { return BitConverter.ToUInt16(Buffer, 32); }
            set { WriteUInt16(value, 32, Buffer); }
        }
        public ushort Prize_PlusStone
        {
            get { return BitConverter.ToUInt16(Buffer, 34); }
            set { WriteUInt16(value, 34, Buffer); }
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
        public static void Process(byte[] packet, Client.GameState client)
        {
            MentorPrize prize = new MentorPrize(false);
            prize.Deserialize(packet);
            switch (prize.Prize_Type)
            {
                case MentorPrize.ClaimExperience:
                case MentorPrize.ClaimSomeExperience:
                    {
                        client.IncreaseExperience((ulong)(((double)client.PrizeExperience / 600) * client.ExpBall), false);
                        client.PrizeExperience = 0;
                        foreach (var appr in client.Apprentices.Values)
                        {
                            appr.Actual_Experience = 0;
                            Database.KnownPersons.SaveApprenticeInfo(appr);
                        }
                        prize.Mentor_ID = client.Entity.UID;
                        prize.Prize_Type = MentorPrize.Show;
                        prize.Prize_Experience = client.PrizeExperience;
                        prize.Prize_HeavensBlessing = client.PrizeHeavenBlessing;
                        prize.Prize_PlusStone = client.PrizePlusStone;
                        client.Send(prize);
                        break;
                    }
                case MentorPrize.ClaimHeavenBlessing:
                    {
                        client.AddBless(client.PrizeHeavenBlessing);
                        client.PrizeHeavenBlessing = 0;
                        foreach (var appr in client.Apprentices.Values)
                        {
                            appr.Actual_HeavenBlessing = 0;
                            Database.KnownPersons.SaveApprenticeInfo(appr);
                        }
                        prize.Mentor_ID = client.Entity.UID;
                        prize.Prize_Type = MentorPrize.Show;
                        prize.Prize_Experience = client.PrizeExperience;
                        prize.Prize_HeavensBlessing = client.PrizeHeavenBlessing;
                        prize.Prize_PlusStone = client.PrizePlusStone;
                        client.Send(prize);
                        break;
                    }
                case MentorPrize.ClaimPlus:
                    {
                        int stones = client.PrizePlusStone / 100;
                        int totake = stones;
                        if (stones > 0)
                        {
                            for (; stones > 0; stones--)
                            {
                                client.PrizePlusStone -= 100;
                                if (!client.Inventory.Add(730001, 1, 1))
                                    break;
                            }
                        }
                        foreach (var appr in client.Apprentices.Values)
                        {
                            if (appr.Actual_Plus >= totake)
                            {
                                appr.Actual_Plus -= (ushort)totake;
                                totake = 0;
                            }
                            else
                            {
                                totake -= appr.Actual_Plus;
                                appr.Actual_Plus = 0;
                            }
                            Database.KnownPersons.SaveApprenticeInfo(appr);
                        }
                        prize.Mentor_ID = client.Entity.UID;
                        prize.Prize_Type = MentorPrize.Show;
                        prize.Prize_Experience = client.PrizeExperience;
                        prize.Prize_HeavensBlessing = client.PrizeHeavenBlessing;
                        prize.Prize_PlusStone = client.PrizePlusStone;
                        client.Send(prize);
                        break;
                    }
                default:
                    {
                        prize.Mentor_ID = client.Entity.UID;
                        prize.Prize_Experience = client.PrizeExperience;
                        prize.Prize_HeavensBlessing = client.PrizeHeavenBlessing;
                        prize.Prize_PlusStone = client.PrizePlusStone;
                        client.Send(prize);
                        break;
                    }
            }
        }
    }
}