using System;
using MTA.Network.GamePackets;

namespace MTA.Client.Commands
{
    public static class GameCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                return false;
            }

            switch (data[0])
            {
                case "tone":
                    return HandleToneCommand(client, data, mess);

                default:
                    return false;
            }
            return true;
        }

        private static bool HandleToneCommand(GameState client, string[] data, string mess)
        {
            uint colorValue = 0;

            // Check if command should be disabled/reset
            if (data.Length == 1 ||
                (data.Length > 1 && data[1] == "0"))
            {
                // Reset to normal (disable effect)
                colorValue = 0;
            }
            else if (data.Length >= 4)
            {
                // Parse RGB values: @g R G B
                if (byte.TryParse(data[1], out byte r) &&
                    byte.TryParse(data[2], out byte g) &&
                    byte.TryParse(data[3], out byte b))
                {
                    // Convert RGB to uint: (R << 16) | (G << 8) | B
                    colorValue = (uint)((r << 16) | (g << 8) | b);
                }
                else
                {
                    client.Send(new Message("Usage: @tone 0 or @tone <R> <G> <B> (values 0-255)", System.Drawing.Color.Red, Message.TopLeft));
                    break;
                }
            }
            else
            {
                client.Send(new Message("Usage: @tone 0 or @tone <R> <G> <B> (values 0-255)", System.Drawing.Color.Red, Message.TopLeft));
                break;
            }

            // Apply the screen color
            Program.ScreenColor = colorValue;
            client.ScreenColor = colorValue;
            foreach (GameState c in Kernel.GamePool.Values)
            {
                Data data = new Data(true);
                data.UID = c.Entity.UID;
                data.ID = 104;
                data.dwParam = colorValue;
                c.Send(data);
            }

            if (colorValue == 0)
            {
                client.Send(new Message("Screen color effect disabled.", System.Drawing.Color.Green, Message.TopLeft));
            }
            else
            {
                client.Send(new Message($"Screen color set to RGB({data[1]}, {data[2]}, {data[3]})", System.Drawing.Color.Green, Message.TopLeft));
            }
            break;
        }
    }
}

