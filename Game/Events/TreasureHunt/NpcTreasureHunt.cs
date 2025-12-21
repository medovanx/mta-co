using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.TreasureHunt;

/// <summary>
///     Treasure Hunt Event Entry NPC - Handles player entry to Treasure Hunt event
///     NPC should be placed on the Trade Map (7010) at coordinates (59, 59)
/// </summary>
[NpcHandler(115522009)]
public static class NpcTreasureHunt {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                var treasureEvent = (BaseEvent)EventScheduler.GetEvent("TREASURE_HUNT")!;
                var scheduleDesc = treasureEvent.GetScheduleDescription();
                var duration = treasureEvent.EventDurationMinutes ?? 60;
                var durationText = duration == 60 ? "one hour" : $"{duration} minutes";

                dialog.Text(
                    $"Greetings, brave adventurer! The Treasure Hunt begins {scheduleDesc} and lasts for {durationText}.\n\n" +
                    "Venture forth and collect precious coins from the monsters that roam these lands. Trade your bounty for magnificent rewards!");

                if (treasureEvent.IsActive)
                    dialog.Option("Enter the Treasure Hunt", 1);
                else
                    dialog.Option("I understand.", 255);

                dialog.Avatar(31);
                dialog.Send();
                break;
            }

            case 1: {
                var treasureEvent = (BaseEvent)EventScheduler.GetEvent("TREASURE_HUNT")!;
                if (treasureEvent.IsActive) {
                    client.Entity.Teleport(TreasureHuntEvent.CoinsMap, TreasureHuntEvent.CoinsX,
                        TreasureHuntEvent.CoinsY);
                    client.Entity.Update(_String.Effect, "accession4", true);
                    client.Send(
                        "Welcome to the Treasure Hunt! May fortune favor you as you collect coins and claim your rewards!");
                }
                else {
                    var scheduleDesc = treasureEvent.GetScheduleDescription();
                    var duration = treasureEvent.EventDurationMinutes ?? 60;
                    var durationText = duration == 60 ? "one hour" : $"{duration} minutes";
                    dialog.Text(
                        $"The event begins {scheduleDesc} and lasts for {durationText}. Please return when the event is active.");
                    dialog.Option("I understand.", 255);
                    dialog.Avatar(31);
                    dialog.Send();
                }

                break;
            }
        }
    }
}