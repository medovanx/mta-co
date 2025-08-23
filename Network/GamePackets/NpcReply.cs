using System;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class NpcReply : Writer, Interfaces.IPacket
    {
        public const byte
            Dialog = 1,
            Option = 2,
            Input = 3,
            Avatar = 4,
            MessageBox = 6,
            Finish = 100;

        private byte[] Buffer;

        public NpcReply()
        {
            Buffer = new byte[28];
            WriteUInt16((ushort)20, 0, Buffer);
            WriteUInt16(2032, 2, Buffer);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
        }
        public NpcReply(byte interactType, string text)
        {
            Buffer = new byte[29];
            WriteUInt16((ushort)(21 + text.Length), 0, Buffer);
            WriteUInt16(2032, 2, Buffer);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
            InteractType = interactType;
            OptionID = 255;
            DontDisplay = true;
            Text = text;
        }
        public void Reset()
        {
            OptionID = 255;
            DontDisplay = true;
            Text = "";
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        /// <summary>
        /// This should be the max length of the input string if the interact type is
        /// `Input`. Otherwise, if it is neither of these two, it should be 0.
        /// </summary>
        public ushort InputMaxLength
        {
            get { return BitConverter.ToUInt16(Buffer, 12); }
            set { WriteUInt16(value, 12, Buffer); }
        }
        public byte OptionID
        {
            get { return Buffer[14]; }
            set { Buffer[14] = value; }
        }
        public byte InteractType
        {
            get { return Buffer[15]; }
            set { Buffer[15] = value; }
        }
        /// <summary>
        /// This should be set to false when your sending the packet with the
        /// interaction type `Finish`, otherwise true
        /// </summary>
        public bool DontDisplay
        {
            get { return (Buffer[16] == 1); }
            set { Buffer[16] = (byte)(value ? 1 : 0); }
        }
        public string Text
        {
            get { return Program.Encoding.GetString(Buffer, 18, Buffer[17]); }
            set
            {
                int realloc = value.Length + 12 + 17;
                if (realloc != Buffer.Length)
                {
                    byte[] new_Packet = new byte[realloc];
                    System.Buffer.BlockCopy(Buffer, 0, new_Packet, 0, 28);
                    Buffer = new_Packet;
                }
                WriteUInt16((ushort)(value.Length + 21), 0, Buffer);
                WriteStringWithLength(value, 17, Buffer);
            }
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}
