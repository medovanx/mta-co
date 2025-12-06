using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;
using MTA.Game.Events.CpCastle;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// CP Castle NPC - Handles CP Castle event entry and exit
    /// </summary>
    [NpcHandler(115522005)]
    public static class Npc_CpCastle
    {
        private const int CASTLE_MAP_ID = 3033;
        private const int CASTLE_X = 54;
        private const int CASTLE_Y = 82;
        private const int MIN_LEVEL = 1;
        private const int MAX_LEVEL = 140;

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("The CP Castle Event starts every day at 14:00 and 20:00 Server Time and lasts for 30 minutes.\n\nRewards: Kill Captain monsters to receive 1,000 Conquer Points per kill.");

                        if (CpCastleEvent.IsEventActive())
                        {
                            dialog.Option("Enter CP Castle Event", 5);
                        }
                        else
                        {
                            dialog.Option("I see!", 255);
                        }

                        dialog.Avatar(31);
                        dialog.Send();
                        break;
                    }

                case 5:
                    {
                        dialog.Text("Enter the CP Castle Event?");
                        dialog.Option("Yes, enter.", 7);
                        dialog.Option("No, thank you.", 255);
                        dialog.Avatar(31);
                        dialog.Send();
                        break;
                    }

                case 7:
                    {
                        if (CpCastleEvent.IsEventActive())
                        {
                            if (client.Entity.Level >= MIN_LEVEL && client.Entity.Level <= MAX_LEVEL &&
                                (client.Entity.Reborn == 1 || client.Entity.Reborn == 2))
                            {
                                client.Entity.Teleport(CASTLE_MAP_ID, CASTLE_X, CASTLE_Y);
                                client.Entity.Update(_String.Effect, "accession4", true);
                            }
                            else
                            {
                                dialog.Text("Sorry, the CP Castle Event is for Level 1-140 (1st Reborn and 2nd Reborn) only.");
                                dialog.Option("I see!", 255);
                                dialog.Avatar(31);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("The event runs every day at 14:00 and 20:00 Server Time and lasts for 30 minutes.");
                            dialog.Option("I see!", 255);
                            dialog.Avatar(31);
                            dialog.Send();
                        }
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// CP Castle Exit NPCs - Teleports players back to Twin City
    /// All exit NPCs (5501-5516) share the same behavior
    /// </summary>
    public static class Npc_CpCastleExit_Shared
    {
        private const int TWIN_CITY_MAP_ID = 1002;
        private const int TWIN_CITY_X = 287;
        private const int TWIN_CITY_Y = 279;

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Do you want to go to Twin City?");
                        dialog.Option("Yes, please.", 1);
                        dialog.Option("Just passing by.", 255);
                        dialog.Send();
                        break;
                    }

                case 1:
                    {
                        client.Entity.Teleport(TWIN_CITY_MAP_ID, TWIN_CITY_X, TWIN_CITY_Y);
                        break;
                    }
            }
        }
    }

    [NpcHandler(5501)]
    public static class Npc_CpCastleExit_5501 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5502)]
    public static class Npc_CpCastleExit_5502 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5503)]
    public static class Npc_CpCastleExit_5503 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5504)]
    public static class Npc_CpCastleExit_5504 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5505)]
    public static class Npc_CpCastleExit_5505 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5506)]
    public static class Npc_CpCastleExit_5506 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5507)]
    public static class Npc_CpCastleExit_5507 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5508)]
    public static class Npc_CpCastleExit_5508 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5509)]
    public static class Npc_CpCastleExit_5509 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5510)]
    public static class Npc_CpCastleExit_5510 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5511)]
    public static class Npc_CpCastleExit_5511 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5512)]
    public static class Npc_CpCastleExit_5512 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5513)]
    public static class Npc_CpCastleExit_5513 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5514)]
    public static class Npc_CpCastleExit_5514 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5515)]
    public static class Npc_CpCastleExit_5515 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }

    [NpcHandler(5516)]
    public static class Npc_CpCastleExit_5516 { public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) => Npc_CpCastleExit_Shared.Handle(client, npcRequest, dialog); }
}

