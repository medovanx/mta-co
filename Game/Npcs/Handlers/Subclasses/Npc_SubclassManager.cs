using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game.Npcs.Handlers.Subclasses
{
    /// <summary>
    /// Subclass Manager - Teleports you to subclass map
    /// </summary>
    [NpcHandler(121656)]
    public static class Npc_SubclassManager
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:

                    switch (npcRequest.OptionID)
                    {
                        case 0:
                            {
                                dialog.Text("Helper classes grant you extra attributes. There are six auxiliary classes: Master of Antidotes, Anticritical Master, Master of Witchcraft, Master of Magic, and Dancer. A character can learn all of them, and each assistant provides different effects. What would you like to do?");
                                dialog.Option("Enter the hall.", 1);
                                dialog.Option("See you later.", 255);
                                dialog.Send();
                                break;
                            }
                        case 1:
                            {
                                client.Entity.Teleport(1597, 51, 70);
                                break;
                            }
                    }
                    break;
            }
        }
    }
}
