using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class WareHousePassword : Writer, Interfaces.IPacket
    {
        public const byte VerifiedPassword = 1, SetNewPass = 3, SendInformation = 4, PasswordCorrect = 5, PasswordWrong = 6;
        private byte[] Buffer;
        public WareHousePassword(bool create)
        {
            if (create)
            {
                Buffer = new byte[15 + 8];
                WriteUInt16(15, 0, Buffer);
                WriteUInt16(2261, 2, Buffer);
            }
        }
        public byte type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }
        public uint OldPassword
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public uint NewPassword
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }

    }
}
