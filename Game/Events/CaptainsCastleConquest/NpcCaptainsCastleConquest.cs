using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.CaptainsCastleConquest;

/// <summary>
///     Captain's Castle Conquest - Handles Captain's Castle Conquest event entry and exit
/// </summary>
[NpcHandler(115522005)]
public static class NpcCaptainsCastleConquest {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                var cpEvent = (BaseEvent)EventScheduler.GetEvent("CAPTAINS_CASTLE_CONQUEST")!;
                var scheduleDesc = cpEvent.GetScheduleDescription();
                var duration = cpEvent.EventDurationMinutes ?? 30;
                var durationText = duration == 30 ? "30 minutes" : $"{duration} minutes";

                dialog.Text(
                    $"Greetings, brave warrior! The Captain's Castle Conquest begins {scheduleDesc}, lasting for {durationText}.\n\n" +
                    "Choose your path wisely:\n" +
                    "- Beginner's Path (Safe Zone, No PvP): Earn 500 CPs per Captain slain\n" +
                    "- Champion's Path (PvP Enabled): Earn 2,000 CPs per Captain slain");

                if (cpEvent.IsActive)
                    dialog.Option("Enter the Captain's Castle Conquest", 5);
                else
                    dialog.Option("I understand.", 255);

                dialog.Avatar(31);
                dialog.Send();
                break;
            }

            case 5: {
                dialog.Text("Which path shall you take, adventurer?");
                dialog.Option("Beginner's Path (Safe) - 500 CPs per Captain", 10);
                dialog.Option("Champion's Path (PvP) - 2,000 CPs per Captain", 11);
                dialog.Option("I need to think about it.", 255);
                dialog.Avatar(31);
                dialog.Send();
                break;
            }

            case 10: // Beginner Level (Safe)
            {
                var cpEvent = (BaseEvent)EventScheduler.GetEvent("CAPTAINS_CASTLE_CONQUEST")!;
                if (cpEvent.IsActive) {
                    client.Entity.Teleport(MapConstants.CAPTAIN_CASTLE_BEGINNER, 53, 78);
                    client.Entity.Update(_String.Effect, "accession4", true);
                    client.Send(
                        "Welcome to the Beginner's Path! May your blade strike true and your rewards be plentiful!");
                }
                else {
                    var scheduleDesc = cpEvent.GetScheduleDescription();
                    var duration = cpEvent.EventDurationMinutes ?? 30;
                    var durationText = duration == 30 ? "30 minutes" : $"{duration} minutes";
                    dialog.Text(
                        $"The Captain's Castle Conquest begins {scheduleDesc}, lasting for {durationText}. Please return when the battle commences.");
                    dialog.Option("I understand.", 255);
                    dialog.Avatar(31);
                    dialog.Send();
                }

                break;
            }

            case 11: // Advanced Level (PvP)
            {
                var cpEvent = (BaseEvent)EventScheduler.GetEvent("CAPTAINS_CASTLE_CONQUEST")!;
                if (cpEvent.IsActive) {
                    client.Entity.Teleport(MapConstants.CAPTAIN_CASTLE_ADVANCED, 325, 335);
                    client.Entity.Update(_String.Effect, "accession4", true);
                    client.Send(
                        "Welcome to the Champion's Path! Beware, for here you face both monsters and other warriors. May the strongest prevail!");
                }
                else {
                    var scheduleDesc = cpEvent.GetScheduleDescription();
                    var duration = cpEvent.EventDurationMinutes ?? 30;
                    var durationText = duration == 30 ? "30 minutes" : $"{duration} minutes";
                    dialog.Text(
                        $"The Captain's Castle Conquest begins {scheduleDesc}, lasting for {durationText}. Please return when the battle commences.");
                    dialog.Option("I understand.", 255);
                    dialog.Avatar(31);
                    dialog.Send();
                }

                break;
            }
        }
    }
}

/// <summary>
///     Captain's Castle Conquest Exit NPC - Teleports players back to Twin City
///     Exit NPC for map 3030
/// </summary>
[NpcHandler(5501)]
public static class NpcCaptainsCastleConquestExit {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                dialog.Text("Greetings, traveler! Would you like to return to Twin City?");
                dialog.Option("Yes, take me back.", 1);
                dialog.Option("Not yet.", 255);
                dialog.Send();
                break;
            }

            case 1: {
                client.Entity.Teleport(MapConstants.TwinCity, 350, 350);
                break;
            }
        }
    }
}