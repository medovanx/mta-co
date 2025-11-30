using System;
using MTA.Network.GamePackets;

namespace MTA.Client.Commands
{
    public static class GameCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            switch (data[0])
            {
                case "tone":
                    return HandleToneCommand(client, data, mess);

                case "guiedit":
                    return HandleGuiEditCommand(client, data, mess);

                default:
                    return false;
            }
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
                    return true;
                }
            }
            else
            {
                client.Send(new Message("Usage: @tone 0 or @tone <R> <G> <B> (values 0-255)", System.Drawing.Color.Red, Message.TopLeft));
                return true;
            }

            // Apply the screen color
            Program.ScreenColor = colorValue;
            GameState.ScreenColor = colorValue;
            foreach (GameState c in Kernel.GamePool.Values)
            {
                Data colorPacket = new Data(true)
                {
                    UID = c.Entity.UID,
                    ID = 104,
                    dwParam = colorValue
                };
                c.Send(colorPacket);
            }

            if (colorValue == 0)
            {
                client.Send(new Message("Screen color effect disabled.", System.Drawing.Color.Green, Message.TopLeft));
            }
            else
            {
                client.Send(new Message($"Screen color set to RGB({data[1]}, {data[2]}, {data[3]})", System.Drawing.Color.Green, Message.TopLeft));
            }
            return true;
        }

        private static bool HandleGuiEditCommand(GameState client, string[] data, string mess)
        {
            // Default parameter value (3276 for most patches, can be changed for different patches)
            uint guiParam = 3276;

            // Allow custom parameter if provided
            if (data.Length >= 2)
            {
                if (!uint.TryParse(data[1], out guiParam))
                {
                    client.Send(new Message("Usage: @guiedit [parameter]", System.Drawing.Color.Yellow, Message.Tip));
                    client.Send(new Message("Example: @guiedit 3276 (default)", System.Drawing.Color.Yellow, Message.Tip));
                    client.Send(new Message("After sending, press EFocusEx button in client PM window to start editing GUI", System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }
            }

            // Send the GUI editing packet (matching forum implementation)
            Data packet = new Data(true)
            {
                UID = client.Entity.UID,        // Id = client.EntityUID
                ID = Data.OpenCustom,           // Action = 116 (PostCmd)
                dwParam = guiParam,             // Data1 = 3276
                wParam1 = client.Entity.X,      // Data3Low = client.X
                wParam2 = client.Entity.Y       // Data3High = client.Y
            };

            client.Send(packet);

            client.Send(new Message($"GUI editing enabled with parameter {guiParam}. Press EFocusEx in client PM window to start.", 
                System.Drawing.Color.Green, Message.Tip));
            
            return true;
        }
    }
}

