using System.Drawing;
using MTA.Client;
using MTA.Network;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using Update = MTA.Network.GamePackets.Update;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Guild War items (Statue and Bomb).
    /// </summary>
    [ItemHandler(GuildWarStatue, GuildWarBomb)]
    public static class GuildWarHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == GuildWarStatue) {
                if (client.AsMember.Rank == Enums.GuildMemberRank.GuildLeader) {
                    if (client.Guild.PoleKeeper && client is
                            { Guild: not null, AsMember.Rank: Enums.GuildMemberRank.GuildLeader } &&
                        !GuildWar.IsWar) {
                        #region Work

                        var test = new byte[((ushort)(247 + client.Entity.Name.Length + 8))];
                        Writer.WriteUshort((ushort)(test.Length - 8), 0, test);
                        Writer.WriteUshort(10014, 2, test);
                        Writer.WriteUint(
                            (uint)(client.Entity.TransformationID * 10000000 + client.Entity.Face * 10000 +
                                   client.Entity.Body), 8, test); //body
                        Writer.WriteUint(105175, 12, test); //UID   
                        Writer.WriteUshort(client.Entity.GuildID, 16, test); //guild ID
                        Writer.WriteUshort(client.Entity.GuildRank, 20, test); //guild Rank
                        Writer.WriteUint(100, 27, test);

                        #region Equip same as me

                        foreach (var item3 in client.Equipment.Objects) {
                            if (item == null)
                                continue;
                            switch (item.Position) {
                                case ConquerItem.Head: {
                                    if (item.Purification.Available) {
                                        Writer.WriteUInt32(0, 194 + 4, test);
                                    }

                                    Writer.WriteUInt32(item.ID, 44 + 4, test); // Offset Correto.
                                    Writer.WriteUInt16((byte)item.Color, 139 + 4, test); // Offset Correto.
                                    break;
                                }
                                case ConquerItem.Garment: {
                                    Writer.WriteUInt32(item.ID, 48 + 4, test); // Offset Correto.
                                    break;
                                }
                                case ConquerItem.Armor: {
                                    if (item.Purification.Available) {
                                        Writer.WriteUInt32(item.Purification.PurificationItemID, 200 + 4,
                                            test); // Offset Correto.
                                    }

                                    Writer.WriteUInt32(item.ID, 52 + 4, test); // Offset Correto.          
                                    Writer.WriteUInt16((byte)item.Color, 137 + 4, test); // Offset Correto.
                                    break;
                                }
                                case ConquerItem.RightWeapon: {
                                    if (item.Purification.Available) {
                                        Writer.WriteUInt32(item.Purification.PurificationItemID, 208 + 4,
                                            test); // Offset Correto.
                                    }

                                    Writer.WriteUInt32(item.ID, 60 + 4, test); // Offset Correto.  
                                    break;
                                }
                                case ConquerItem.LeftWeapon: {
                                    if (item.Purification.Available) {
                                        Writer.WriteUInt32(item.Purification.PurificationItemID, 204 + 4,
                                            test); // Offset Correto.
                                    }

                                    Writer.WriteUInt32(item.ID, 56 + 4, test); // Offset Correto.
                                    Writer.WriteUInt16((byte)item.Color, 137 + 4,
                                        test); // Offset Correto.\                                                    
                                    break;
                                }
                                case ConquerItem.RightWeaponAccessory: {
                                    Writer.WriteUInt32(item.ID, 68 + 4, test);
                                    break;
                                }
                                case ConquerItem.LeftWeaponAccessory: {
                                    Writer.WriteUInt32(item.ID, 64 + 4, test);
                                    break;
                                }
                                case ConquerItem.Steed: {
                                    Writer.WriteUInt32(item.ID, 72 + 4, test);
                                    Writer.WriteUInt16(item.Plus, 147 + 4, test);
                                    Writer.WriteUInt32(item.SocketProgress, 153 + 4, test);
                                    break;
                                }
                                case ConquerItem.SteedArmor: {
                                    Writer.WriteUInt32(item.ID, 76 + 4, test);
                                    break;
                                }
                            }
                        }

                        #endregion Equip same as me

                        Writer.WriteUshort((ushort)client.Entity.Hitpoints, 89, test); //npc hitpoints
                        Writer.WriteUshort(client.Entity.Level, 96, test); //level 
                        Writer.WriteUshort(client.Entity.X, 98, test);
                        Writer.WriteUshort(client.Entity.Y, 100, test);
                        Writer.WriteUshort(client.Entity.HairStyle, 102, test); //npc hitpoints  
                        Writer.WriteByte(7, 104, test); //fascing
                        Writer.WriteByte((byte)client.Entity.Action, 105, test); //action
                        Writer.WriteByte(1, 244, test);
                        Writer.WriteByte((byte)client.Entity.Name.Length, 245, test);
                        Writer.WriteString(client.Entity.Name, 246, test);
                        client.Send(test);

                        #endregion
                    }
                    else {
                        client.Send(new Message("You can't use this item unless you win the war!", Color.Tan,
                            Message.TopLeft));
                    }
                }
                else {
                    client.Send(new Message("You can't use this item unless you're a guild leader!", Color.Tan,
                        Message.TopLeft));
                }
            }
            else if (item.ID == GuildWarBomb) {
                if (Game.GuildWar.IsWar) {
                    if (client.Entity.MapID == 1038 & client.Entity.X == 165 & client.Entity.Y == 213) {
                        MTA.Game.GuildWar.LeftGate.Mesh = (ushort)(250 + MTA.Game.GuildWar.LeftGate.Mesh % 10);
                        var upd = new Update(true);
                        upd.UID = MTA.Game.GuildWar.LeftGate.UID;
                        upd.Append(Update.Mesh, MTA.Game.GuildWar.LeftGate.Mesh);
                        client.SendScreen(upd, true);
                        client.SendScreen(upd, true);
                        var str = new _String(true);
                        str.UID = client.Entity.UID;
                        str.TextsCount = 1;
                        str.Type = _String.Effect;
                        str.Texts.Add("bombFranko");
                        client.Entity.SendScreen(str);
                        client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                    }
                    else {
                        if (client.Entity.MapID == 1038 & client.Entity.X == 225 & client.Entity.Y == 178) {
                            MTA.Game.GuildWar.RightGate.Mesh = (ushort)(280 + MTA.Game.GuildWar.RightGate.Mesh % 10);
                            var upd = new Update(true);
                            upd.UID = MTA.Game.GuildWar.RightGate.UID;
                            upd.Append(Update.Mesh, MTA.Game.GuildWar.RightGate.Mesh);
                            client.SendScreen(upd, true);
                            var str = new _String(true);
                            str.UID = client.Entity.UID;
                            str.TextsCount = 1;
                            str.Type = _String.Effect;
                            str.Texts.Add("bombFranko");
                            client.Entity.SendScreen(str);
                            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                        }
                        else {
                            client.Send(new Message("You Can't Open Here Sorry Open in GuildWar (165,213) OR (225,178)",
                                Color.Red, Message.TopLeft));
                        }
                    }
                }
            }
        }
    }
}

