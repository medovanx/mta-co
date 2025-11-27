using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.ServerBase;
using MTA.Network.GamePackets;
using MTA.Client;
using System.Drawing;

namespace MTA.Game.Events
{
    public class DizzyLand
    {
        private static string War = "DizzyLand";
        public static bool IsWar = false;
        public static byte Alive = 0;
        public static ushort Map = 5528;

        public static void Start()
        {
            // Kernel.SendWorldMessage(new Message(War + " War has Started Now Get In .", System.Drawing.Color.Red, Message.TopLeft), Program.Values);
            IsWar = true;
            CheackAlive();
        }

        public static void CheackAlive()
        {
            Alive = 0;
            foreach (GameState state in Kernel.GamePool.Values)
            {
                if (state.Entity.MapID == Map && state.Entity.Hitpoints >= 1 && !state.Entity.Dead)
                {
                    Alive++;
                    //      Kernel.SendWorldMessage(new Message("Players Alive in DizzyLand Now: " + Alive + " ", Color.Black, 0x83c), Program.Values);
                }
                if (state.Entity.MapID == Map && state.Entity.Hitpoints >= 1 && !state.Entity.Dead)
                {
                    if (!state.Entity.ContainsFlag(Update.Flags.Confused))
                    {
                        state.Entity.AddFlag(Update.Flags.Confused);
                    }
                    //    _String str = new _String(true);
                    //   str.UID = state.Entity.UID;
                    //   str.TextsCount = 1;
                    //   str.Type = _String.Effect;
                    //   str.Texts.Add("athl_ap");
                    //   state.Entity.Owner.SendScreen(str, true);
                }
            }
        }

        public static void ClaimPrize(GameState client)
        {
            client.Entity.ConquerPoints += 500;
            Kernel.SendWorldMessage(new Message(string.Concat(new object[] { "Congratulations, ", client.Entity.Name, " has won in " + War + " War and claimed 500 ConquerPoints!" }), System.Drawing.Color.Red, Message.TopLeft), Program.Values);
            IsWar = false;
        }

        public static void CloseSignUp()
        {
            Kernel.SendWorldMessage(new Message("You cant signup into " + War + " War Come again next Time!", System.Drawing.Color.Red, Message.TopLeft), Program.Values);
            IsWar = false;
        }

        public static void End()
        {
            Kernel.SendWorldMessage(new Message(War + " War has been ended have fun in the next Time!", System.Drawing.Color.Red, Message.TopLeft), Program.Values);
        }
    }
}
