using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War Rune NPC - Gives runes during war and allows experience claiming after war
/// </summary>
[NpcHandler(4452)]
public static class NpcGuildWarRune {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                dialog.Text(
                    "Hello friend, as you might know, while guild war, you can light up some runes and after guild war is over, you can come to me and I will give you experience in exchange of your rune.");
                dialog.Text(
                    "Once guild war starts, come to me and ask for a rune, and you will receive it. If you lose it, you can come back and reclaim it, but you will start from level 1 once again.");
                dialog.Option("Give me a rune.", 1);
                dialog.Option("Claim experience.", 2);
                dialog.Option("Nothing.", 255);
                dialog.Send();
                break;
            }
            case 1: {
                var gwEvent = GuildWarEvent.GetActiveEvent();
                if (gwEvent?.IsActive == true) {
                    for (var c = 0; c <= 5; c++)
                        if (client.Inventory.Contains((uint)(729960 + c), 1)) {
                            dialog.Text("You already have a rune.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                            return;
                        }

                    if (client.Inventory.Add(729960, 0, 1)) {
                        dialog.Text("Go, and light up this rune.");
                        dialog.Option("Thank you.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("There is not enough room in your inventory.");
                        dialog.Option("Ah, hold on.", 255);
                        dialog.Send();
                    }
                }
                else {
                    dialog.Text("I cannot give you a rune now.");
                    dialog.Option("Ahh.", 255);
                    dialog.Send();
                }

                break;
            }
            case 2: {
                var gwEvent = GuildWarEvent.GetActiveEvent();
                if (gwEvent?.IsActive != true) {
                    for (var c = 0; c <= 10; c++)
                        if (client.Inventory.Contains((uint)(729960 + c), 1)) {
                            if (client.Entity.Level < 140) {
                                var expballs = c;
                                if (729960 + c == 729970)
                                    expballs += 2;

                                for (var ex = 0; ex < expballs; ex++)
                                    client.IncreaseExperience(client.ExpBall, false);

                                client.Inventory.Add(723917, 0, 1);
                            }

                            client.Entity.ConquerPoints += 200;

                            client.Inventory.Remove((uint)(729960 + c), 1);
                            return;
                        }

                    dialog.Text("You cannot claim experience if you don't have a rune.");
                }
                else {
                    dialog.Text("You cannot claim experience while guild war.");
                }

                dialog.Option("Ahh.", 255);
                dialog.Send();

                break;
            }
        }
    }
}