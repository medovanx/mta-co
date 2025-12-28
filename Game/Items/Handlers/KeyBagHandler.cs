using MTA.Client;
using MTA.Network.GamePackets;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles KeyBag and key items for the Golden Secret quest.
    /// </summary>
    [ItemHandler(KeyBag, IronKey, CopperKey, SilverKey)]
    public static class KeyBagHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == KeyBag) {
                if (client.Entity.ChestDemonkill >= 1) {
                    if (Kernel.ChanceSuccess(10)) {
                        client.Inventory.Add(SilverKey, 0, 1);
                        var npc = new NpcReply(6, "You gained a SilverKey from opening KeyBag.");
                        client.Send(npc.ToArray());
                        var str = new _String(true) {
                            UID = client.Entity.UID,
                            Type = _String.Effect
                        };
                        str.Texts.Add("lounder1");
                        str.TextsCount = 1;
                        client.Entity.SendScreen(str);
                    }
                    else if (Kernel.ChanceSuccess(40)) {
                        client.Inventory.Add(CopperKey, 0, 1);
                        var npc = new NpcReply(6, "You gained a CopperKey from opening KeyBag.");
                        client.Send(npc.ToArray());
                        var str = new _String(true) {
                            UID = client.Entity.UID,
                            Type = _String.Effect
                        };
                        str.Texts.Add("lounder1");
                        str.TextsCount = 1;
                        client.Entity.SendScreen(str);
                    }
                    else if (Kernel.ChanceSuccess(80)) {
                        client.Inventory.Add(IronKey, 0, 1);
                        var npc = new NpcReply(6, "You gained a IronKey from opening KeyBag.");
                        client.Send(npc.ToArray());
                        var str = new _String(true) {
                            UID = client.Entity.UID,
                            Type = _String.Effect
                        };
                        str.Texts.Add("lounder1");
                        str.TextsCount = 1;
                        client.Entity.SendScreen(str);
                    }

                    client.Entity.ChestDemonkill = 0;
                }
                else {
                    var npc = new NpcReply(6, "You need to kill a ChestDemon to open KeyBag.");
                    client.Send(npc.ToArray());
                }
            }
            else if (item.ID == IronKey) {
                if (client.Entity.ChestDemonkill >= 1) {
                    if (Kernel.ChanceSuccess(10)) {
                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        client.Inventory.Remove(KeyBag, 1);
                        client.Inventory.Add(GoldKey, 0, 1);
                        var str = new _String(true) {
                            UID = client.Entity.UID,
                            Type = _String.Effect
                        };
                        str.Texts.Add("break_start");
                        str.TextsCount = 1;
                        client.Entity.SendScreen(str);
                        var npc = new NpcReply(6,
                            "You received a Gold Key! Hurry and use it to open the Treasure Chest!");
                        client.Send(npc.ToArray());
                    }
                    else {
                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        var npc = new NpcReply(6,
                            "The Chest Demon suddenly bit you and fied away. What a pity...");
                        client.Send(npc.ToArray());
                    }
                }
                else {
                    var npc = new NpcReply(6, "You have to kill ChestDemon again.");
                    client.Send(npc.ToArray());
                }
            }
            else if (item.ID == CopperKey) {
                if (client.Entity.ChestDemonkill >= 1) {
                    if (Kernel.ChanceSuccess(20)) {
                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        client.Inventory.Remove(KeyBag, 1);
                        client.Inventory.Add(GoldKey, 0, 1);
                        var str = new _String(true) {
                            UID = client.Entity.UID,
                            Type = _String.Effect
                        };
                        str.Texts.Add("break_start");
                        str.TextsCount = 1;
                        client.Entity.SendScreen(str);
                        var npc = new NpcReply(6,
                            "You received a Gold Key! Hurry and use it to open the Treasure Chest!");
                        client.Send(npc.ToArray());
                    }
                    else {
                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        var npc = new NpcReply(6,
                            "The Chest Demon suddenly bit you and fied away. What a pity...");
                        client.Send(npc.ToArray());
                    }
                }
                else {
                    var npc = new NpcReply(6, "You have to kill ChestDemon again.");
                    client.Send(npc.ToArray());
                }
            }
            else if (item.ID == SilverKey) {
                if (client.Entity.ChestDemonkill >= 1) {
                    if (Kernel.ChanceSuccess(30)) {
                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        client.Inventory.Remove(KeyBag, 1);
                        client.Inventory.Add(GoldKey, 0, 1);
                        var str = new _String(true) {
                            UID = client.Entity.UID,
                            Type = _String.Effect
                        };
                        str.Texts.Add("break_start");
                        str.TextsCount = 1;
                        client.Entity.SendScreen(str);
                        var npc = new NpcReply(6,
                            "You received a Gold Key! Hurry and use it to open the Treasure Chest!");
                        client.Send(npc.ToArray());
                    }
                    else {
                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        var npc = new NpcReply(6,
                            "The Chest Demon suddenly bit you and fied away. What a pity...");
                        client.Send(npc.ToArray());
                    }
                }
                else {
                    var npc = new NpcReply(6, "You have to kill ChestDemon again.");
                    client.Send(npc.ToArray());
                }
            }
        }
    }
}

