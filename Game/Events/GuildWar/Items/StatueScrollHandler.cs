using System.Collections.Generic;
using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Items;
using MTA.Network;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.GuildItems;

namespace MTA.Game.Events.GuildWar.Items {
    /// <summary>
    /// Handles Guild War Statue Scroll item.
    /// </summary>
    [ItemHandler(StatueScroll)]
    public static class StatueScrollHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Guild?.Members.GetValueOrDefault(client.Entity.UID)?.Rank !=
                Enums.GuildMemberRank.GuildLeader) {
                client.Send(new Message("You can't use this item unless you're a guild leader!", Color.Tan,
                    Message.TopLeft));
                return;
            }

            // Check database history first (works even after server restart)
            var latest = GuildWarHistoryTable.GetLatest();
            var isWinnerGuildFromHistory = latest?.GuildId == client.Guild.Id;

            // Also check active event (for during active war)
            var gwEvent = GuildWarEvent.GetActiveEvent();
            var isWinnerGuildFromEvent = gwEvent?.PoleKeeper == client.Guild;

            var isWinnerGuild = isWinnerGuildFromHistory || isWinnerGuildFromEvent;

            if (isWinnerGuild && client.Guild != null &&
                client.Guild.Members.GetValueOrDefault(client.Entity.UID)?.Rank ==
                Enums.GuildMemberRank.GuildLeader &&
                GuildWarEvent.GetActiveEvent()?.IsActive != true) {
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

                foreach (var unused in client.Equipment.Objects) {
                    if (item == null)
                        continue;
                    switch (item.Position) {
                        case ConquerItem.Head: {
                            if (item.Purification.Available) {
                                Writer.WriteUInt32(0, 194 + 4, test);
                            }

                            Writer.WriteUInt32(item.ID, 44 + 4, test);
                            Writer.WriteUInt16((byte)item.Color, 139 + 4, test);
                            break;
                        }
                        case ConquerItem.Garment: {
                            Writer.WriteUInt32(item.ID, 48 + 4, test);
                            break;
                        }
                        case ConquerItem.Armor: {
                            if (item.Purification.Available) {
                                Writer.WriteUInt32(item.Purification.PurificationItemID, 200 + 4,
                                    test);
                            }

                            Writer.WriteUInt32(item.ID, 52 + 4, test);
                            Writer.WriteUInt16((byte)item.Color, 137 + 4, test);
                            break;
                        }
                        case ConquerItem.RightWeapon: {
                            if (item.Purification.Available) {
                                Writer.WriteUInt32(item.Purification.PurificationItemID, 208 + 4,
                                    test);
                            }

                            Writer.WriteUInt32(item.ID, 60 + 4, test);
                            break;
                        }
                        case ConquerItem.LeftWeapon: {
                            if (item.Purification.Available) {
                                Writer.WriteUInt32(item.Purification.PurificationItemID, 204 + 4,
                                    test);
                            }

                            Writer.WriteUInt32(item.ID, 56 + 4, test);
                            Writer.WriteUInt16((byte)item.Color, 137 + 4,
                                test);
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
            }
            else {
                client.Send(new Message("You can't use this item unless you win the war!", Color.Tan,
                    Message.TopLeft));
            }
        }
    }
}