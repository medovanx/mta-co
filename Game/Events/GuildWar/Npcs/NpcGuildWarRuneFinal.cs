using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War Final Rune Upgrade NPC - Final rune upgrade, only works if Flame10th is true
/// </summary>
[NpcHandler(4462)]
public static class NpcGuildWarRuneFinal {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        dialog.Avatar(0);
        var takeFlame = 725507 + client.ActiveNpc;
        var addFlame = 725507 + client.ActiveNpc + 1;
        var gwEvent = GuildWarEvent.GetActiveEvent();
        if (gwEvent?.IsActive != true) return;
        if (gwEvent.Flame10Th) {
            if (client.Inventory.Contains(takeFlame, 1)) {
                client.Inventory.Remove(takeFlame, 1);
                client.Inventory.Add(addFlame, 0, 1);
                dialog.Text("Well done! Nothing left to light up. Congratulations!");
                dialog.Send();
            }
            else {
                dialog.Text("You cannot flame up this stone without the proper rune.");
                dialog.Send();
            }
        }
        else {
            dialog.Text("It's not the right time to flame up this rune.");
            dialog.Send();
        }
    }
}