using System.Collections.Generic;
using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;
using static MTA.Game.Enums;
using static MTA.Game.Events.GuildWar.GuildWarConstants;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War Pole NPC - Repair pole using guild fund
/// </summary>
[NpcHandler(PoleNpcId)]
public static class NpcGuildWarPole {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        var gwEvent = GuildWarEvent.GetActiveEvent();
        if (gwEvent?.IsActive != true) return;

        // Only allow guild leader of current pole keeper during active war
        if (client.Guild == null || gwEvent.PoleKeeper != client.Guild) return;
        var member = client.Guild.Members.GetValueOrDefault(client.Entity.UID);
        if (member?.Rank != GuildMemberRank.GuildLeader) return;
        if (gwEvent.Pole == null) return;

        switch (npcRequest.OptionID) {
            case 0: {
                // Initial dialog
                var currentHp = gwEvent.Pole.Hitpoints;
                var maxHp = gwEvent.Pole.MaxHitpoints;
                var hpPercent = maxHp > 0 ? (currentHp * 100 / maxHp) : 0;

                dialog.Text($"Guild Pole Status:\nHP: {currentHp:N0} / {maxHp:N0} ({hpPercent}%)");

                if (currentHp < maxHp) {
                    dialog.Option("Repair pole.", 1);
                }

                if (gwEvent.IsRepairActive) {
                    dialog.Option("Stop repair.", 3);
                    dialog.Text($"\nRepair in progress. Allocated: {gwEvent.RepairAllocatedFunds:N0} Silver");
                }

                dialog.Option("Nothing.", 255);
                dialog.Send();
                break;
            }
            case 1: {
                // Start repair - ask for amount
                if (gwEvent.Pole.Hitpoints >= gwEvent.Pole.MaxHitpoints) {
                    dialog.Text("The pole is already at full HP.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }

                var availableFunds = client.Guild.SilverFund;
                dialog.Text($"Enter the amount of Silver to allocate for repair (1 Silver per 10 HP).\n" +
                            $"Available Guild Fund: {availableFunds:N0} Silver\n" +
                            $"Pole HP: {gwEvent.Pole.Hitpoints:N0} / {gwEvent.Pole.MaxHitpoints:N0}\n" +
                            $"Repair rate: {PoleRepairHpPerInterval:N0} HP every {PoleRepairIntervalSeconds} seconds");
                dialog.Input("Enter amount (Silver):", 2, 20);
                dialog.Option("Cancel.", 255);
                dialog.Send();
                break;
            }
            case 2: {
                // Process repair allocation - get amount from input
                // Check if already repairing
                if (gwEvent.IsRepairActive) {
                    dialog.Text("Repair is already in progress. Stop it first if you want to change the amount.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }

                // Get amount from input
                if (string.IsNullOrEmpty(npcRequest.Input) || !ulong.TryParse(npcRequest.Input, out var amount) ||
                    amount <= 0) {
                    dialog.Text("Invalid amount. Please enter a positive number.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }

                if (client.Guild.SilverFund < amount) {
                    dialog.Text(
                        $"Insufficient funds. You have {client.Guild.SilverFund:N0} Silver, but need {amount:N0} Silver.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }

                // Start repair
                if (gwEvent.StartRepair(client.Guild, amount)) {
                    dialog.Text($"Repair started! {amount:N0} Silver allocated from guild fund.\n\n" +
                                $"The pole will be repaired at a rate of {PoleRepairHpPerInterval:N0} HP every {PoleRepairIntervalSeconds} seconds.");
                    dialog.Option("Thank you!", 255);
                    dialog.Send();
                }
                else {
                    dialog.Text("Failed to start repair. The pole might be at full HP or repair is already active.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                }

                break;
            }
            case 3: {
                // Stop repair
                if (!gwEvent.IsRepairActive) {
                    dialog.Text("No repair is currently active.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }

                var refunded = gwEvent.StopRepair(client.Guild);
                dialog.Text($"Repair stopped. {refunded:N0} Silver refunded to guild fund.");
                dialog.Option("Thank you!", 255);
                dialog.Send();
                break;
            }
        }
    }
}