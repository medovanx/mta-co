using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// Love Stone - Provides marriage service to players
    /// </summary>
    [NpcHandler(390)]
    public static class NpcLoveStone {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0:
                    dialog.Text(
                        "Hey there! There are not many people that can make a marriage last, but I can sense that you are one of them, if you want to marry someone just let me know. Also, if you are heavenly blessed I can give you one hour of double exp each day.");
                    dialog.Option("Yes, I want to marry someone.", 1);
                    dialog.Option("I need double exp.", 2);
                    dialog.Option("Nothing thank you.", 255);
                    dialog.Send();
                    break;
                case 1: {
                    if (client.Entity.Spouse == "None") {
                        dialog.Text("Here, click on the player you want to be your spouse.");
                        dialog.Option("Thank you.", 255);
                        dialog.Send();
                        var data = new Data(true) {
                            UID = client.Entity.UID,
                            ID = Data.OpenCustom,
                            dwParam = Data.CustomCommands.FlowerPointer
                        };
                        client.Send(data);
                    }
                    else {
                        dialog.Text(
                            "You are already married. If you want to divorce your spouse, you have to go to Divorce Manager. He'll handle your request.");
                        dialog.Option("Alright, thank you!", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 2: {
                    if (client.Entity.HeavenBlessing > 0) {
                        if (!client.DoubleExpToday) {
                            dialog.Text(
                                "I have given you one hour of double exp each day. Come back tomorrow for more.");
                            dialog.Option("Alright, thank you!", 255);
                            dialog.Send();

                            client.Entity.DoubleExperienceTime = 3600;
                            client.DoubleExpToday = true;
                        }
                        else {
                            dialog.Text("You already took your double exp today. Come back tomorrow.");
                            dialog.Option("Alright, thank you!", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("You cannot take double exp because you are not heavenly blessed.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}