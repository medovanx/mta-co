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
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("The CP Castle Event starts every day at 14:00 and 20:00 Server Time and lasts for 30 minutes.\n\nRewards:\n- Beginner Map (Safe, No PvP): 500 CPs per Captain kill\n- Advanced Map (PvP Enabled): 2,000 CPs per Captain kill");

                        if (CpCastleEvent.IsEventActive())
                        {
                            dialog.Option("Enter CPs Castle event", 5);
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
                        dialog.Text("Choose your destination:");
                        dialog.Option("Beginner Level (Safe) - 500 CPs/monster", 10);
                        dialog.Option("Advanced Level (PvP) - 2,000 CPs/monster", 11);
                        dialog.Option("No, thank you.", 255);
                        dialog.Avatar(31);
                        dialog.Send();
                        break;
                    }

                case 10: // Beginner Level (Safe)
                    {
                        if (CpCastleEvent.IsEventActive())
                        {
                            client.Entity.Teleport(MapConstants.CP_CASTLE_BEGINNER, 53, 78);
                            client.Entity.Update(_String.Effect, "accession4", true);
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

                case 11: // Advanced Level (PvP)
                    {
                        if (CpCastleEvent.IsEventActive())
                        {
                            client.Entity.Teleport(MapConstants.CP_CASTLE_ADVANCED, 325, 335);
                            client.Entity.Update(_String.Effect, "accession4", true);
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
    /// CP Castle Exit NPC - Teleports players back to Twin City
    /// Exit NPC for map 3030
    /// </summary>
    [NpcHandler(5501)]
    public static class Npc_CpCastleExit_5501
    {
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
                        client.Entity.Teleport(MapConstants.TWIN_CITY, 350, 350);
                        break;
                    }
            }
        }
    }
}

