using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War Rune Upgrade NPCs - Upgrade runes from level N to N+1 during war
/// </summary>
[NpcHandler(4453, 4454, 4455, 4456, 4457, 4458, 4459, 4460, 4461)]
public static class NpcGuildWarRuneUpgrade {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        dialog.Avatar(0);
        var takeFlame = 725507 + client.ActiveNpc;
        var addFlame = 725507 + client.ActiveNpc + 1;
        var gwEvent = GuildWarEvent.GetActiveEvent();
        if (gwEvent?.IsActive == true) {
            if (client.Inventory.Contains(takeFlame, 1)) {
                client.Inventory.Remove(takeFlame, 1);
                client.Inventory.Add(addFlame, 0, 1);
                dialog.Text("Well done! Next rune is number " + (client.ActiveNpc - 4451) + ".");
                dialog.Send();
            }
            else {
                dialog.Text("You cannot flame up this stone without the proper rune.");
                dialog.Send();
            }
        }
        else {
            dialog.Text("You cannot flame up a rune if guild war is not on.");
            dialog.Send();
        }
    }
}