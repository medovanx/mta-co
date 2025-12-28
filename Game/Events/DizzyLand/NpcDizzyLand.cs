using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.DizzyLand;

/// <summary>
///     DizzyLand Event Entry NPC - Handles player entry to DizzyLand event
/// </summary>
[NpcHandler(13)]
public static class NpcDizzyLand {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                dialog.Text("Greetings, " + client.Entity.Name +
                            "! The DizzyLand War awaits brave warriors like yourself. Will you join the battle?");
                dialog.Option("Yes, I will join!", 1);
                dialog.Option("Not right now.", 255);
                dialog.Send();
                break;
            }

            case 1: {
                var dizzyEvent = (BaseEvent)EventScheduler.GetEvent("DIZZY_LAND")!;
                if (dizzyEvent.IsActive) {
                    client.Entity.RemoveFlag(Update.Flags.Ride);
                    client.Entity.Teleport(Maps.DizzyLand, 105, 159);
                    if (!client.Entity.ContainsFlag(Update.Flags.Confused))
                        client.Entity.AddFlag(Update.Flags.Confused);

                    client.Send("Welcome to the DizzyLand War! May the strongest warrior prevail!");
                }
                else {
                    var scheduleDesc = dizzyEvent.GetScheduleDescription();
                    dialog.Text(
                        $"The DizzyLand War begins {scheduleDesc}. Return when the battle is about to commence!");
                    dialog.Option("I understand.", 255);
                    dialog.Send();
                }

                break;
            }
        }
    }
}