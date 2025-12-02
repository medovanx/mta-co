using System;

namespace MTA.Client.Commands
{
    public static class WarCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return data[0] switch
            {
                "guildwar" => HandleGuildWarCommand(client, data, mess),
                "eliteguildwar" => HandleEliteGuildWarCommand(client, data, mess),
                "superguildwar" => HandleSuperGuildWarCommand(client, data, mess),
                "clanwar" => HandleClanWarCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleGuildWarCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Network.GamePackets.Message("Usage: @guildwar <on|off>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            switch (data[1])
            {
                case "on":
                    if (!Game.GuildWar.IsWar)
                    {
                        Game.GuildWar.Start();
                        client.Send(new Network.GamePackets.Message("Guild War has been started.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Guild War is already active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                case "off":
                    if (Game.GuildWar.IsWar)
                    {
                        Game.GuildWar.End();
                        client.Send(new Network.GamePackets.Message("Guild War has been ended.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Guild War is not active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                default:
                    client.Send(new Network.GamePackets.Message("Usage: @guildwar <on|off>", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    break;
            }

            return true;
        }

        private static bool HandleSuperGuildWarCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Network.GamePackets.Message("Usage: @superguildwar <on|off>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            switch (data[1])
            {
                case "on":
                    if (!Game.SuperGuildWar.IsWar)
                    {
                        Game.SuperGuildWar.Start();
                        client.Send(new Network.GamePackets.Message("Super Guild War has been started.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Super Guild War is already active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                case "off":
                    if (Game.SuperGuildWar.IsWar)
                    {
                        Game.SuperGuildWar.End();
                        client.Send(new Network.GamePackets.Message("Super Guild War has been ended.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Super Guild War is not active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                default:
                    client.Send(new Network.GamePackets.Message("Usage: @superguildwar <on|off>", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    break;
            }

            return true;
        }

        private static bool HandleClanWarCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Network.GamePackets.Message("Usage: @clanwar <on|off>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            switch (data[1])
            {
                case "on":
                    if (!Game.ClanWar.IsWar)
                    {
                        Game.ClanWar.Start();
                        client.Send(new Network.GamePackets.Message("Clan War has been started.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Clan War is already active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                case "off":
                    if (Game.ClanWar.IsWar)
                    {
                        Game.ClanWar.End();
                        client.Send(new Network.GamePackets.Message("Clan War has been ended.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Clan War is not active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                default:
                    client.Send(new Network.GamePackets.Message("Usage: @clanwar <on|off>", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    break;
            }

            return true;
        }

        private static bool HandleEliteGuildWarCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Network.GamePackets.Message("Usage: @eliteguildwar <on|off>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            switch (data[1])
            {
                case "on":
                    if (!Game.EliteGuildWar.IsWar)
                    {
                        Game.EliteGuildWar.Start();
                        client.Send(new Network.GamePackets.Message("Elite Guild War has been started.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Elite Guild War is already active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                case "off":
                    if (Game.EliteGuildWar.IsWar)
                    {
                        Game.EliteGuildWar.End();
                        client.Send(new Network.GamePackets.Message("Elite Guild War has been ended.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message("Elite Guild War is not active.", System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
                    }
                    break;

                default:
                    client.Send(new Network.GamePackets.Message("Usage: @eliteguildwar <on|off>", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    break;
            }

            return true;
        }
    }
}
