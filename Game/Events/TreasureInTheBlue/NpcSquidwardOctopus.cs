using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
/// Entry point for the Treasure in the Blue event
/// </summary>
/// <event>Treasure in the Blue</event>
/// <npc>Squidward Octopus</npc>
/// <description>Provides information and teleportation to the Proud Sea for the Treasure in the Blue event.</description>
[NpcHandler(12)]
public static class NpcSquidwardOctopus {
    private const int RequiredLevel = 80;

    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                var treasureEvent = (BaseEvent)EventScheduler.GetEvent("TREASURE_IN_THE_BLUE")!;
                var scheduleDesc = treasureEvent.GetScheduleDescription();
                var duration = treasureEvent.EventDurationMinutes ?? 60;
                var durationText = duration == 60 ? "one hour" : $"{duration} minutes";

                dialog.Text(
                    $"Greetings, brave adventurer! The Treasure in the Blue begins {scheduleDesc} and lasts for {durationText}.\n\n" +
                    "Have you heard of the Proud Sea? It's a place where lots of pirate ships sank and lost their stolen treasure. " +
                    "Venture into the depths and collect ancient coins from the monsters that roam these waters. " +
                    "Remember: coins expire after 60 minutes, so exchange them quickly with the Mammon Envoy!\n\n" +
                    "Beware! The fearsome Blackbeard appears 5 minutes after the event starts and respawns every 15 minutes. " +
                    "Defeat him to claim Gold Coins!\n\n" +
                    $"Requirement: Level {RequiredLevel} or above");

                if (treasureEvent.IsActive)
                    dialog.Option("Enter the Proud Sea", 1);
                else
                    dialog.Option("I understand.", 255);

                dialog.Avatar(31);
                dialog.Send();
                break;
            }

            case 1: {
                var treasureEvent = (BaseEvent)EventScheduler.GetEvent("TREASURE_IN_THE_BLUE")!;
                if (!treasureEvent.IsActive) {
                    var scheduleDesc = treasureEvent.GetScheduleDescription();
                    var duration = treasureEvent.EventDurationMinutes ?? 60;
                    var durationText = duration == 60 ? "one hour" : $"{duration} minutes";
                    dialog.Text(
                        $"The event begins {scheduleDesc} and lasts for {durationText}. Please return when the event is active.");
                    dialog.Option("I understand.", 255);
                    dialog.Avatar(31);
                    dialog.Send();
                    break;
                }

                // Check level requirement
                if (client.Entity.Level < RequiredLevel) {
                    dialog.Text(
                        $"You must be at least level {RequiredLevel} to participate in Treasure in the Blue! Come back when you're stronger, adventurer!");
                    dialog.Option("I understand.", 255);
                    dialog.Avatar(31);
                    dialog.Send();
                    return;
                }

                // Teleport to Proud Sea
                client.Entity.Teleport(MapConstants.ProudSea, 200, 067);
                client.Entity.Update(_String.Effect, "accession4", true);
                client.Send(
                    "Welcome to the Proud Sea! The Treasure in the Blue awaits! Collect ancient coins from monsters, but remember: they expire after 60 minutes. Exchange them quickly with the Mammon Envoy at the Prize Center!");

                break;
            }
        }
    }
}