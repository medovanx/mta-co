using MTA.Client;
using MTA.Game.ConquerStructures.Society;
using MTA.Network.GamePackets;
using static MTA.Game.Enums;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Guild Director - Handles guild creation, management, and administration
    /// </summary>
    [NpcHandler(10003)]
    public static class NpcGuildDirector {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint guildCost = 1000000;
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        $"Hello there. Do you want to create a new guild? You need to be level 90, have {guildCost:N0} silver, and not belong to any guild. If you are a guild leader, you can manage your guild here.");
                    if (client.Guild == null) {
                        dialog.Option("Create a Guild", 1);
                    }

                    if (client is { Guild: not null, AsMember.Rank: GuildMemberRank.GuildLeader }) {
                        dialog.Option("Disband my Guild", 3);
                        dialog.Option("Change my Guild Name", 5);
                    }

                    dialog.Option("Nevermind", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    dialog.Text("Name your guild. The name must be between 1 and 15 characters.");
                    dialog.Input("Guild name:", 2, 16);
                    dialog.Option("Cancel", 255);
                    dialog.Send();
                    break;
                }
                case 2: {
                    // Validate requirements
                    if (client.Entity.Level < 90) {
                        dialog.Text("You need to be level 90 to create a guild.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                        break;
                    }

                    if (client.Entity.Money < guildCost) {
                        dialog.Text($"You don't have enough silver. You need {guildCost:N0} silver.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                        break;
                    }

                    if (string.IsNullOrEmpty(npcRequest.Input) || npcRequest.Input.Length is < 1 or > 16) {
                        dialog.Text("Invalid guild name. The name must be 16 characters at maximum.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                        break;
                    }

                    if (Guild.CheckNameExist(npcRequest.Input)) {
                        dialog.Text(
                            "Sorry, there is already a guild with this name. Please choose a different name.");
                        dialog.Option("Choose another name", 1);
                        dialog.Option("Cancel", 255);
                        dialog.Send();
                        break;
                    }

                    // All validations passed, deduct cost and create the guild
                    client.Entity.Money -= guildCost;
                    Guild.CreateGuild(client, npcRequest.Input, guildCost);
                    break;
                }
                case 3: {
                    dialog.Text("Are you sure you want to disband your guild? This action cannot be undone!");
                    dialog.Option("Yes, disband my guild", 4);
                    dialog.Option("No, cancel", 255);
                    dialog.Send();
                    break;
                }
                case 4: {
                    if (client is { Guild: not null, AsMember.Rank: GuildMemberRank.GuildLeader }) {
                        client.Guild.Disband(client.Entity.Name);
                    }

                    break;
                }
                case 5: {
                    dialog.Text(
                        $"Name your guild. The name must be a maximum of 16 characters.\nThis will cost 215 CPs.");
                    dialog.Input("Enter new guild name:", 6, 16);
                    dialog.Option("Cancel", 255);
                    dialog.Send();
                    break;
                }
                case 6: {
                    const uint nameChangeCost = 215;
                    if (client.AsMember?.Rank != GuildMemberRank.GuildLeader) {
                        dialog.Text("You must be the Guild Leader to change the guild name.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                        break;
                    }

                    if (client.Entity.ConquerPoints < nameChangeCost) {
                        dialog.Text($"You don't have enough Conquer Points. You need {nameChangeCost} CPs.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                        break;
                    }

                    if (string.IsNullOrEmpty(npcRequest.Input) || npcRequest.Input.Length is < 1 or > 16) {
                        dialog.Text("Invalid guild name. The name must be between 1 and 15 characters.");
                        dialog.Option("Try Again", 5);
                        dialog.Option("Cancel", 255);
                        dialog.Send();
                        break;
                    }

                    if (Guild.CheckNameExist(npcRequest.Input)) {
                        dialog.Text("Sorry, there is already a guild with this name.");
                        dialog.Option("Choose Another Name", 5);
                        dialog.Option("Cancel", 255);
                        dialog.Send();
                        break;
                    }

                    // All validations passed, deduct cost and change the name
                    client.Entity.ConquerPoints -= nameChangeCost;
                    client.Guild!.ChangeName(client, npcRequest.Input);
                    break;
                }
            }
        }
    }
}