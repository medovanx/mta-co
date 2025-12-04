using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers
{
    /// <summary>
    /// Tree Conductress
    /// </summary>
    public static class Npc_1002
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("What can I do for you?");
                        dialog.Option("Enter Moon Platform", 1);
                        dialog.Option("Enter Dream Land", 2);
                        dialog.Option("Just Passing By", 255);
                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        client.Entity.Teleport(1012, 120, 115);
                        break;
                    }
                case 2:
                    {
                        client.Entity.Teleport(2002, 150, 150);
                        break;
                    }
            }
        }
    }
}
