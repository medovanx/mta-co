using System;
using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Features {
    public class SpiritBeadQuest {
        private UInt32 mCollectedSpirits;

        public SpiritBeadQuest(GameState c) {
            Client = c;
        }

        //Add reset when new day
        private GameState Client { get; set; }

        public Boolean CanAccept { get; set; }

        public UInt32 CollectedSpirits {
            get { return mCollectedSpirits; }
            set {
                mCollectedSpirits = value;
                UpdateDB("collectedspirits", mCollectedSpirits);
            }
        }

        public UInt32 Bead { get; set; }

        public UInt32 Requiredspirits {
            get {
                switch (Bead) {
                    case 729611: return 2500;
                    case 729612: return 2000;
                    case 729613: return 1500;
                    case 729614: return 1000;
                    case 729703: return 500;
                }

                return 0;
            }
        }

        public void Check() {
            if (CollectedSpirits < Requiredspirits)
                Client.Send(new Message(
                    "Collected spirits : " + CollectedSpirits + ", You need : " + Requiredspirits +
                    ", To finish the task.", Color.Red, Message.TopLeft));
            else {
                if (Client.Inventory.Contains(Bead, 1)) {
                    //Add Rewards HERE.
                    switch (Bead) {
                        case 729611: break; //Normal Bead
                        case 729612: break; //Refined Bead
                        case 729613: break; //Unique Bead
                        case 729614: break; //Elite Bead
                        case 729703: break; //Super Bead
                    }

                    Reset();
                    Client.Send(new Message("Congratualations!, You have successfully finished the quest!", Color.Red,
                        Message.TopLeft));
                }
            }
        }

        public void UpdateDB(String column, UInt32 value) {
            if (Client != null)
                new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set(column, value)
                    .Where("UID", Client.Entity.UID).Execute();
        }

        public void GainSpirits(Byte MobLevel) {
            if (Bead != 0) {
                if (MobLevel <= 70)
                    CollectedSpirits++;
                else if (MobLevel >= 70 && MobLevel <= 99)
                    CollectedSpirits += 2;
                else if (MobLevel >= 100 && MobLevel <= 119)
                    CollectedSpirits += 3;
                else if (MobLevel >= 120 && MobLevel <= 140)
                    CollectedSpirits += 4;

                if (CollectedSpirits >= Requiredspirits) {
                    Client.Send(new Message(
                        "You have collected enough spirit beads, Right click the item for your reward.", Color.Red,
                        Message.TopLeft));
                }
            }
        }

        public void AcceptQuest(UInt32 _Bead) {
            if (CanAccept) {
                if (Bead == 0) {
                    Client.Quests.Accept(QuestID.Spirit_Beads);
                    Reset();
                    ConquerItemBaseInformation CIBI = null;
                    if (ConquerItemInformation.BaseInformations.TryGetValue(_Bead, out CIBI)) {
                        ConquerItem i = new ConquerItem(true);
                        i.ID = CIBI.ID;
                        i.UID = Program.GetNextItemId(); //Program.NextItemID++;
                        i.Durability = CIBI.Durability;
                        i.MaximDurability = CIBI.Durability;
                        i.Color = (Enums.Color)Kernel.Random.Next(4, 8);
                        Client.Inventory.Add(i, Enums.ItemUse.CreateAndAdd);
                    }

                    Bead = _Bead;
                    UpdateDB("spiritquestbead", _Bead);
                    CanAccept = false;
                    UpdateDB("canacceptspiritbead", Convert.ToByte(CanAccept));
                }
            }
            else
                Client.Send(new Message("You cant accept another spirit bead quest today", Color.Red, Message.TopLeft));
        }

        public void Reset(Boolean quit = false, Boolean dailyreset = false) {
            if (Client == null)
                return;
            if (!dailyreset) {
                CollectedSpirits = 0;
                if (Client.Inventory.Contains(Bead, 1))
                    Client.Inventory.Remove(Bead, 1);
                Bead = 0;
                UpdateDB("spiritquestbead", 0);
                if (quit) {
                    CanAccept = true;
                    UpdateDB("canacceptspiritbead", Convert.ToByte(CanAccept));
                }
            }
            else {
                if (!Client.Inventory.Contains(Bead, 1) && !CanAccept) {
                    Bead = 0;
                    UpdateDB("spiritquestbead", 0);
                    CanAccept = true;
                    UpdateDB("canacceptspiritbead", Convert.ToByte(CanAccept));
                }
            }
        }
    }
}