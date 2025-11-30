using System;
using MTA.Game;

namespace MTA.Client.Commands
{
    public static class CurrencyCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                return false;
            }

            switch (data[0])
            {
                case "gold":
                    {
                        if (!ulong.TryParse(data[1], out var amount))
                        {
                            client.Send(new Network.GamePackets.Message("Usage: @gold <amount>", System.Drawing.Color.Red,
                                Network.GamePackets.Message.Tip));
                            return true;
                        }

                        const ulong maxGold = 9999999999UL; // 9,999,999,999
                        if (amount > maxGold)
                        {
                            client.Send(new Network.GamePackets.Message($"Maximum gold amount is {maxGold:N0}", System.Drawing.Color.Red,
                                Network.GamePackets.Message.Tip));
                            return true;
                        }

                        client.Entity.Money = amount;
                        client.Send(new Network.GamePackets.Message($"Money set to {amount:N0}", System.Drawing.Color.Green,
                            Network.GamePackets.Message.Tip));
                        return true;
                    }

                case "cps":
                    {
                        if (!uint.TryParse(data[1], out var amount))
                        {
                            client.Send(new Network.GamePackets.Message("Usage: @cps <amount>", System.Drawing.Color.Red,
                                Network.GamePackets.Message.Tip));
                            return true;
                        }

                        const uint maxCps = 999999999U; // 999,999,999
                        if (amount > maxCps)
                        {
                            client.Send(new Network.GamePackets.Message($"Maximum CPs amount is {maxCps:N0}", System.Drawing.Color.Red,
                                Network.GamePackets.Message.Tip));
                            return true;
                        }

                        client.Entity.ConquerPoints = amount;
                        client.Send(new Network.GamePackets.Message($"Conquer Points set to {amount:N0}", System.Drawing.Color.Green,
                            Network.GamePackets.Message.Tip));
                        return true;
                    }

                case "bcps":
                    {
                        if (!uint.TryParse(data[1], out var amount))
                        {
                            client.Send(new Network.GamePackets.Message("Usage: @boundcps <amount>", System.Drawing.Color.Red,
                                Network.GamePackets.Message.Tip));
                            return true;
                        }

                        client.Entity.BoundCps = amount;
                        client.Send(new Network.GamePackets.Message($"Bound CPs set to {amount:N0}", System.Drawing.Color.Green,
                            Network.GamePackets.Message.Tip));
                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}

