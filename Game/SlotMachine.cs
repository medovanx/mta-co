using MTA.Client;
using MTA.Network.GamePackets;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game.ConquerStructures
{
    public class SlotMachine
    {
        public static readonly int[] Rates = new int[8]
        {
            80, //Stancher
            340, //Meteor
            120, //Sword
            180, //TwoSwords
            210, //SwordAndShield
            180, //ExpBall
            600, //DragonBall
            200, //3s line
        };

        public SlotMachineItems[] Wheels = new SlotMachineItems[3];

        public uint NPCID;
        public uint BetAmount;
        public bool Cps;
        public SlotMachine(uint npcid, uint betamount, bool cps = false)
        {
            NPCID = npcid;
            BetAmount = betamount;
            Cps = cps;
        }

        int GetAmount(SlotMachineItems Item)
        {
            int count = 0;
            foreach (SlotMachineItems item in Wheels)
                if (item == Item)
                    count++;
            return count;
        }

        private int GetSLCount()
        {
            return GetAmount(SlotMachineItems.Sword) + GetAmount(SlotMachineItems.SwordAndShield) + GetAmount(SlotMachineItems.TwoSwords) + GetAmount(SlotMachineItems.DragonBall);
        }
        private bool IsSL(SlotMachineItems item)
        {
            return item == SlotMachineItems.DragonBall || item == SlotMachineItems.Sword || item == SlotMachineItems.SwordAndShield || item == SlotMachineItems.TwoSwords;
        }

        public uint GetRewardAmount(GameState client)
        {
            uint win = 0;
            if (GetAmount(SlotMachineItems.DragonBall) == 3)
            {
                Kernel.SendWorldMessage(new Message("Congratulations to " + client.Entity.Name + "! He/She has won the jackpot from the 1-Arm Bandit!", Message.Center));
                client.SendScreen(new _String(true) { UID = client.Entity.UID, Type = _String.Effect, Texts = new List<string>() { "accession5" } });
                if (Cps) return BetAmount * 3000;
                else return BetAmount * 1000;
            }
            if (GetAmount(SlotMachineItems.ExpBall) == 3 - GetAmount(SlotMachineItems.DragonBall))
                win = BetAmount * 60;
            else if (GetAmount(SlotMachineItems.SwordAndShield) == 3 - GetAmount(SlotMachineItems.DragonBall))
                win = BetAmount * 40;
            else if (GetAmount(SlotMachineItems.TwoSwords) == 3 - GetAmount(SlotMachineItems.DragonBall))
                win = BetAmount * 20;
            else if (GetAmount(SlotMachineItems.Sword) == 3 - GetAmount(SlotMachineItems.DragonBall) || GetAmount(SlotMachineItems.Meteor) == 3 - GetAmount(SlotMachineItems.DragonBall))
                win = BetAmount * 10;
            else if (GetAmount(SlotMachineItems.Meteor) == 2 - GetAmount(SlotMachineItems.DragonBall) || GetSLCount() == 3)
                win = BetAmount * 5;
            else if (GetAmount(SlotMachineItems.Meteor) == 1 - GetAmount(SlotMachineItems.DragonBall))
                win = BetAmount * 2;
            if (Cps)
            {
                if (GetAmount(SlotMachineItems.DragonBall) == 1) win *= 3;
                else if (GetAmount(SlotMachineItems.DragonBall) == 2) win *= 9;
            }
            return win;
        }
        public void SpinTheWheels()
        {
            int wheelPick;
            for (int i = 2; i >= 0; i--)
            {
                while (true)
                {
                    wheelPick = Kernel.Random.Next(0, 7);
                    if (Kernel.Rate(1, Rates[wheelPick]))
                    {
                        if (i == 0)
                            if (GetSLCount() == 2 && IsSL((SlotMachineItems)wheelPick) || (GetAmount((SlotMachineItems)wheelPick) == 3 - GetAmount(SlotMachineItems.DragonBall)))
                                if (!Kernel.Rate(1, Rates[7]))
                                    continue;
                        Wheels[i] = (SlotMachineItems)wheelPick;
                        break;
                    }
                }
            }
        }
        public void SendWheelsToClient(GameState client)
        {
            client.Send(new SlotMachineResponse() { Mode = SlotMachineSubType.StartSpin, WheelOne = (byte)Wheels[0], WheelTwo = (byte)Wheels[1], WheelThree = (byte)Wheels[2], NpcID = NPCID });
        }
    }
}
