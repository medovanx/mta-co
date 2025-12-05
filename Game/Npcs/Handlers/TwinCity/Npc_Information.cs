using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Information NPC - Provides teleportation services to various event areas
    /// </summary>
    [NpcHandler(115522004)]
    public static class Npc_Information
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Greetings, adventurer! Welcome to " + MTA.rates.servername + ". Where would you like to go?");
                        dialog.Option("Elite Guild War Arena", 2);
                        dialog.Option("Guild Wars & Super Guild War", 3);
                        dialog.Option("Top Spouse / Beauty Contest Events", 6);
                        dialog.Option("Elite PK / Skill Team / Team Tournaments", 5);
                        dialog.Option("What~about~Online~Point's ?", 4);
                        dialog.Option("Pole Competitions & Rewards", 7);
                        dialog.Option("Visit our website", 50);
                        dialog.Send();
                        break;
                    }
                case 2:
                    {
                        dialog.Text("Hello " + client.Entity.Name + "! Do you wish to be transported to the Elite Guild War arena?");
                        dialog.Option("Teleport me there", 11);
                        dialog.Send();
                        break;
                    }
                case 3:
                    {
                        dialog.Text("Hello " + client.Entity.Name + "! Do you wish to be transported to the Guild War battlegrounds?");
                        dialog.Option("Teleport me there", 12);
                        dialog.Send();
                        break;
                    }
                case 4:
                    {
                        dialog.Text("Hello " + client.Entity.Name + "! You can earn automatic CPs every 15 minutes by staying in Twin City. You'll receive 1,000,000 ConquerPoints and 1 OnlinePoint.");
                        dialog.Text("After 12 AM, the reward increases to 2,000,000 CPs every 15 minutes while in Twin City.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                        break;
                    }
                case 5:
                    {
                        dialog.Text("Hello " + client.Entity.Name + "! Do you wish to be transported to the tournament grounds?");
                        dialog.Option("Teleport me there", 13);
                        dialog.Send();
                        break;
                    }
                case 6:
                    {
                        dialog.Text("Hello " + client.Entity.Name + "! Do you wish to be transported to the contest area?");
                        dialog.Option("Teleport me there", 15);
                        dialog.Send();
                        break;
                    }
                case 7:
                    {
                        dialog.Text("Hello " + client.Entity.Name + "! Do you wish to be transported to the pole competition area?");
                        dialog.Option("Teleport me there", 17);
                        dialog.Send();
                        break;
                    }
                case 11:
                    {
                        client.Entity.Teleport(1002, 286, 160);
                        break;
                    }
                case 12:
                    {
                        client.Entity.Teleport(1002, 225, 237);
                        break;
                    }
                case 13:
                    {
                        client.Entity.Teleport(1002, 302, 144);
                        break;
                    }
                case 15:
                    {
                        client.Entity.Teleport(1002, 282, 189);
                        break;
                    }
                case 16:
                    {
                        client.Entity.Teleport(1002, 408, 407);
                        break;
                    }
                case 17:
                    {
                        client.Entity.Teleport(1002, 241, 221);
                        break;
                    }
                case 50:
                    {
                        client.Send(new Message("https://www.linkedin.com/in/medovanx", System.Drawing.Color.Red, Network.GamePackets.Message.Website));
                        break;
                    }
            }
        }
    }
}

