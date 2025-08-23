using MTA.Client;
using MTA.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game;

namespace MTA.Franko
{
    public class ProgressBar
    {
        private uint actionuid;
        public ProgressBar(Client.GameState Client, string EffectName, Action<GameState> Action = null, string doneeffect = "Done", uint Time = 5)
        {
            // Send_Effect(Client, 220, 164, 1, 5, "Pick");
            //38 + 1 + 8 + EffectName.Length = 47 + EffectName.Length
            //Where 38 is the normal 10010 packet length, 1 for the extra byte at offset 38, 8 for the Server Stamp
            if (Client.ProgressBar != null)
            {
                Client.ProgressBar.End(Client);
                return;
            }
            byte[] Packet = new byte[47 + 4 + EffectName.Length];
            Writer.WriteUInt16((ushort)(Packet.Length - 8), 0, Packet);
            Writer.WriteUInt16(10010, 2, Packet);
            Writer.WriteUInt32(Client.Entity.UID, 8, Packet);
            Writer.WriteUInt32(220, 12, Packet);
            Writer.WriteUInt16(164, 24, Packet);
            Writer.WriteUInt16(1, 26, Packet);
            Writer.WriteUInt32(Time, 36, Packet);
            Writer.WriteByte(1, 41, Packet);
            Writer.WriteByte((byte)(EffectName.Length), 42, Packet);
            Writer.WriteString(EffectName, 43, Packet);
            Client.Send(Packet);
            actionuid = Program.World.DelayedTask.StartDelayedTask(() =>
            {
                if (Action != null)
                    Action.Invoke(Client);
                Action = null;

                Packet = new byte[47 + 4 + doneeffect.Length];
                Writer.WriteUInt16((ushort)(Packet.Length - 8), 0, Packet);
                Writer.WriteUInt16(10010, 2, Packet);
                Writer.WriteUInt32(Client.Entity.UID, 8, Packet);
                Writer.WriteUInt32(220, 12, Packet);
                Writer.WriteUInt16(164, 24, Packet);
                Writer.WriteUInt16((ushort)Enums.ConquerAngle.West, 26, Packet);
                Writer.WriteUInt32(0, 36, Packet);
                Writer.WriteByte(1, 41, Packet);
                Writer.WriteByte((byte)(doneeffect.Length), 42, Packet);
                Writer.WriteString(doneeffect, 43, Packet);
                Client.Send(Packet);
                Client.ProgressBar = null;

            }, (int)Time * 1000);

        }
        public void End(Client.GameState Client)
        {
            var doneeffect = "Failed";
            Program.World.DelayedTask.Remove(actionuid);
            byte[] Packet = new byte[47 + 4 + doneeffect.Length];
            Writer.WriteUInt16((ushort)(Packet.Length - 8), 0, Packet);
            Writer.WriteUInt16(10010, 2, Packet);
            Writer.WriteUInt32(Client.Entity.UID, 8, Packet);
            Writer.WriteUInt32(220, 12, Packet);
            Writer.WriteUInt16(164, 24, Packet);
            Writer.WriteUInt16((ushort)Enums.ConquerAngle.West, 26, Packet);
            Writer.WriteUInt32(0, 36, Packet);
            Writer.WriteByte(1, 41, Packet);
            Writer.WriteByte((byte)(doneeffect.Length), 42, Packet);
            Writer.WriteString(doneeffect, 43, Packet);
            Client.Send(Packet);
            Client.ProgressBar = null;
        }
    }
}
