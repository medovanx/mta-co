using MTA.Client;
using MTA.Network.GamePackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game.Features
{
    public class SpiritBeadQuest
    {
        //Add reset when new day
        private GameState Client { get; set; }

        public Boolean CanAccept { get; set; }
        private UInt32 mCollectedSpirits;
        public UInt32 CollectedSpirits
        {
            get { return mCollectedSpirits; }
            set { mCollectedSpirits = value; UpdateDB("collectedspirits", mCollectedSpirits); }
        }
        public UInt32 Bead { get; set; }

        public SpiritBeadQuest(GameState c)
        {
            Client = c;
        }
        public void Check()
        {
            if (CollectedSpirits < Requiredspirits)
                Client.Send(new Message("Collected spirits : " + CollectedSpirits + ", You need : " + Requiredspirits + ", To finish the task.", System.Drawing.Color.Red, Message.TopLeft));
            else
            {
                if (Client.Inventory.Contains(Bead, 1))
                {
                    //Add Rewards HERE.
                    switch (Bead)
                    {
                        case 729611: break;//Normal Bead
                        case 729612: break;//Refined Bead
                        case 729613: break;//Unique Bead
                        case 729614: break;//Elite Bead
                        case 729703: break;//Super Bead
                    }
                    Reset();
                    Client.Send(new Message("Congratualations!, You have successfully finished the quest!", System.Drawing.Color.Red, Message.TopLeft));
                }
            }
        }
        public void UpdateDB(String column, UInt32 value)
        {
            if (Client != null)
                new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("entities").Set(column, value).Where("UID", Client.Entity.UID).Execute();
        }
        public void GainSpirits(Byte MobLevel)
        {
            if (Bead != 0)
            {
                if (MobLevel <= 70)
                    CollectedSpirits++;
                else if (MobLevel >= 70 && MobLevel <= 99)
                    CollectedSpirits += 2;
                else if (MobLevel >= 100 && MobLevel <= 119)
                    CollectedSpirits += 3;
                else if (MobLevel >= 120 && MobLevel <= 140)
                    CollectedSpirits += 4;

                if (CollectedSpirits >= Requiredspirits)
                {
                    Client.Send(new Message("You have collected enough spirit beads, Right click the item for your reward.", System.Drawing.Color.Red, Message.TopLeft));
                }
            }
        }
        public void AcceptQuest(UInt32 _Bead)
        {
            if (CanAccept)
            {
                if (Bead == 0)
                {
                    Client.Quests.Accept(MaTrix.QuestID.Spirit_Beads);
                    Reset();
                    Database.ConquerItemBaseInformation CIBI = null;
                    if (Database.ConquerItemInformation.BaseInformations.TryGetValue(_Bead, out CIBI))
                    {
                        ConquerItem i = new ConquerItem(true);
                        i.ID = CIBI.ID;
                        i.UID = Program.NextItemID; //Program.NextItemID++;
                        i.Durability = CIBI.Durability;
                        i.MaximDurability = CIBI.Durability;
                        i.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        Client.Inventory.Add(i, Game.Enums.ItemUse.CreateAndAdd);
                    }
                    Bead = _Bead;
                    UpdateDB("spiritquestbead", _Bead);
                    CanAccept = false;
                    UpdateDB("canacceptspiritbead", Convert.ToByte(CanAccept));
                }
            }
            else Client.Send(new Message("You cant accept another spirit bead quest today", System.Drawing.Color.Red, Message.TopLeft));
        }
        public void Reset(Boolean quit = false, Boolean dailyreset = false)
        {
            if (Client == null)
                return;
            if (!dailyreset)
            {
                CollectedSpirits = 0;
                if (Client.Inventory.Contains(Bead, 1))
                    Client.Inventory.Remove(Bead, 1);
                Bead = 0;
                UpdateDB("spiritquestbead", 0);
                if (quit)
                {
                    CanAccept = true;
                    UpdateDB("canacceptspiritbead", Convert.ToByte(CanAccept));
                }
            }
            else
            {
                if (!Client.Inventory.Contains(Bead, 1) && !CanAccept)
                {
                    Bead = 0;
                    UpdateDB("spiritquestbead", 0);
                    CanAccept = true;
                    UpdateDB("canacceptspiritbead", Convert.ToByte(CanAccept));
                }
            }
        }
        public UInt32 Requiredspirits
        {
            get
            {
                switch (Bead)
                {
                    case 729611: return 2500;
                    case 729612: return 2000;
                    case 729613: return 1500;
                    case 729614: return 1000;
                    case 729703: return 500;
                }
                return 0;
            }
        }
    }
}
