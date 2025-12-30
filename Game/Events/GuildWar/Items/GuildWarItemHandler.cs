using System.Collections.Generic;
using System.Drawing;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Items;
using MTA.Network;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using Update = MTA.Network.GamePackets.Update;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.Constants.Items.GuildItems;
using static MTA.Game.Events.GuildWar.GuildWarConstants;

namespace MTA.Game.Events.GuildWar.Items {
    /// <summary>
    /// Handles Guild War items (Statue and Bomb).
    /// </summary>
    [ItemHandler(GuildWarStatue, GuildWarBomb)]
    public static class GuildWarHandler {
        /// <summary>
        /// Activates the bomb on a gate - damages the gate and kills the player
        /// </summary>
        private static void ActivateBomb(GameState client, ConquerItem item, SobNpcSpawn gate,
            ushort brokenMesh) {
            // Calculate damage per bomb (4 bombs should destroy the gate)
            var damagePerBomb = gate.MaxHitpoints / BombsRequiredToDestroyGate;

            // Apply damage to gate
            if (gate.Hitpoints <= damagePerBomb) {
                gate.Hitpoints = 0;
                gate.Mesh = brokenMesh;
            }
            else {
                gate.Hitpoints -= damagePerBomb;
                // Gate is damaged but not destroyed yet - keep current mesh state
            }

            var upd = new Update(true) {
                UID = gate.UID
            };
            upd.Append(Update.Mesh, gate.Mesh);
            upd.Append(Update.Hitpoints, gate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, Maps.GuildWarMap);

            var str = new _String(true) {
                UID = client.Entity.UID,
                TextsCount = 1,
                Type = _String.Effect
            };
            str.Texts.Add("bombFranko");
            client.Entity.SendScreen(str);
            // Kill the player who used the bomb
            client.Entity.Update(_String.Effect, "firemagic", true);
            client.Entity.Update(_String.Effect, "bombarrow7", true);
            client.Entity.Die(0);
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }

        /// <summary>
        /// Shows bomb confirmation dialog and handles activation
        /// </summary>
        private static void HandleBombConfirmation(GameState client, SobNpcSpawn gate,
            ushort brokenMesh, ConquerItem item) {
            client.MessageBox(
                "Are you sure you want to use the bomb? This will damage the gate and kill you!",
                p => { ActivateBomb(p, item, gate, brokenMesh); }, // OK callback - activate bomb
                null, // Cancel callback - do nothing
                30, // 30 second timeout
                force: true
            );
        }

        public static void Handle(GameState client, ConquerItem item) {
            switch (item.ID) {
                case GuildWarStatue when client.Guild?.Members.GetValueOrDefault(client.Entity.UID)?.Rank ==
                                         Enums.GuildMemberRank.GuildLeader: {
                    // Check database history first (works even after server restart)
                    var latest = Database.GuildWarHistoryTable.GetLatest();
                    var isWinnerGuildFromHistory = latest?.GuildId == client.Guild.ID;

                    // Also check active event (for during active war)
                    var gwEvent4 = GuildWarEvent.GetActiveEvent();
                    var isWinnerGuildFromEvent = gwEvent4?.PoleKeeper == client.Guild;

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

                    break;
                }
                case GuildWarStatue:
                    client.Send(new Message("You can't use this item unless you're a guild leader!", Color.Tan,
                        Message.TopLeft));
                    break;
                case GuildWarBomb: {
                    var gwEvent5 = GuildWarEvent.GetActiveEvent();
                    if (gwEvent5?.IsActive != true) {
                        break;
                    }

                    // Check if player is on Guild War map
                    if (client.Entity.MapID != Maps.GuildWarMap) {
                        break;
                    }

                    var playerX = client.Entity.X;
                    var playerY = client.Entity.Y;

                    // Check West Gate location with tolerance
                    var westGateDistanceX = playerX > WestGateBombX
                        ? playerX - WestGateBombX
                        : WestGateBombX - playerX;
                    var westGateDistanceY = playerY > WestGateBombY
                        ? playerY - WestGateBombY
                        : WestGateBombY - playerY;

                    if (westGateDistanceX <= BombLocationTolerance &&
                        westGateDistanceY <= BombLocationTolerance) {
                        HandleBombConfirmation(client, gwEvent5.WestGate!, WestGateBrokenMesh, item);
                        break;
                    }

                    // Check East Gate location with tolerance
                    var eastGateDistanceX = playerX > EastGateBombX
                        ? playerX - EastGateBombX
                        : EastGateBombX - playerX;
                    var eastGateDistanceY = playerY > EastGateBombY
                        ? playerY - EastGateBombY
                        : EastGateBombY - playerY;

                    if (eastGateDistanceX <= BombLocationTolerance &&
                        eastGateDistanceY <= BombLocationTolerance) {
                        HandleBombConfirmation(client, gwEvent5.EastGate!, EastGateBrokenMesh, item);
                        break;
                    }

                    // Player is not at either bomb location
                    client.Send(new Message(
                        $"You need to be closer to the gate to use the bomb.",
                        Color.Red, Message.TopLeft));
                    break;
                }
            }
        }
    }
}