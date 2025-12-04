using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers
{
    /// <summary>
    /// Tree Conductress
    /// </summary>
    [NpcHandler(1002)]
    public static class Npc_Luna_TwinCity
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
                        client.Entity.Teleport(1100, 131, 104);
                        break;
                    }
                case 2:
                    {
                        client.Entity.Teleport(1012, 120, 115);
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Luna - Exit NPC for Moon Platform
    /// </summary>
    [NpcHandler(115522002)]  // Replace with Luna's actual NPC ID in Moon Platform
    public static class Npc_Luna_MoonPlatform
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Would you like to leave Moon Platform?");
                        dialog.Option("Yes, teleport me out", 1);
                        dialog.Option("No, I'll stay", 255);
                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        client.Entity.Teleport(1002, 303, 281); // Twin City
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Luna - Exit NPC for Dream Land
    /// </summary>
    [NpcHandler(115522003)]  // Replace with Luna's actual NPC ID in Dream Land
    public static class Npc_Luna_DreamLand
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Would you like to leave Dream Land?");
                        dialog.Option("Yes, teleport me out", 1);
                        dialog.Option("No, I'll stay", 255);
                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        client.Entity.Teleport(1002, 303, 281); // Twin City
                        break;
                    }
            }
        }
    }
}
