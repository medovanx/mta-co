using System;

namespace MTA.Network {
    public class PacketBuilder {
        protected byte[] Buffer = new byte[1024];
        protected int Len;
        protected int Position;
        protected byte[] TqServer = Program.Encoding.GetBytes("TQServer");

        public PacketBuilder(int T, int l) {
            Len = l;
            Length(l);
            Type(T);
        }

        public int GetPos() {
            return Position;
        }

        public void SetPosition(int pos) {
            Position = pos;
        }

        public void Short(int value) {
            Buffer[Position] = ((byte)(value & 0xff));
            Position++;
            Buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }

        public void Short(uint value) {
            Buffer[Position] = ((byte)(value & 0xff));
            Position++;
            Buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }

        public void Length(int value) {
            Buffer[Position] = ((byte)(value & 0xff));
            Position++;
            Buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }

        public void Type(int value) {
            Buffer[Position] = ((byte)(value & 0xff));
            Position++;
            Buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }

        public void Long(int value) {
            Buffer[Position] = ((byte)(value & 0xff));
            Position++;
            Buffer[Position] = ((byte)(value >> 8 & 0xff));
            Position++;
            Buffer[Position] = (byte)(value >> 16 & 0xff);
            Position++;
            Buffer[Position] = ((byte)(value >> 24 & 0xff));
            Position++;
        }

        public void Long(ulong value) {
            Buffer[Position] = ((byte)(value & 0xffL));
            Position++;
            Buffer[Position] = ((byte)(value >> 8 & 0xff));
            Position++;
            Buffer[Position] = (byte)(value >> 16 & 0xff);
            Position++;
            Buffer[Position] = ((byte)(value >> 24 & 0xff));
            Position++;
        }

        public void ULong(ulong value) {
            Buffer[Position] = (byte)(value);
            Position++;
            Buffer[Position] = (byte)(value >> 8);
            Position++;
            Buffer[Position] = (byte)(value >> 16);
            Position++;
            Buffer[Position] = (byte)(value >> 24);
            Position++;
            Buffer[Position] = (byte)(value >> 32);
            Position++;
            Buffer[Position] = (byte)(value >> 40);
            Position++;
            Buffer[Position] = (byte)(value >> 48);
            Position++;
            Buffer[Position] = (byte)(value >> 56);
            Position++;
        }

        public void Int(int value) {
            Buffer[Position] = (Convert.ToByte(value & 0xff));
            Position++;
        }

        public void Int(uint value) {
            Buffer[Position] = (Convert.ToByte(value & 0xff));
            Position++;
        }

        public void Long(uint value) {
            Buffer[Position] = ((byte)(value & 0xff));
            Position++;
            Buffer[Position] = ((byte)(value >> 8 & 0xff));
            Position++;
            Buffer[Position] = (byte)(value >> 16 & 0xff);
            Position++;
            Buffer[Position] = ((byte)(value >> 24 & 0xff));
            Position++;
        }

        public void Move(int value) {
            for (var x = 0; x < value; x++) {
                Buffer[Position] = 0;
                Position++;
            }
        }

        public void Text(string value) {
            var nvalue = Program.Encoding.GetBytes(value);
            Array.Copy(nvalue, 0, Buffer, Position, nvalue.Length);
            Position += nvalue.Length;
        }

        protected void Seal() {
            Array.Copy(TqServer, 0, Buffer, Position, TqServer.Length);
            Position += TqServer.Length + 1;
            var x = new byte[Position - 1];
            Array.Copy(Buffer, x, Position - 1);
            Buffer = new byte[x.Length];
            Array.Copy(x, Buffer, x.Length);
        }

        public byte[] GetFinal() {
            Seal();
            return Buffer;
        }

        internal void Fill(int end) {
            for (var x = Position; x < end; x++)
                Int(0);
        }

        internal void PrintThis() {
            var dat = "";
            for (var x = 0; x < Position; x++)
                dat += Buffer[x].ToString("X") + " ";
            System.Console.WriteLine(dat);
        }

        #region Add from offset

        public void Short(int value, int offset) {
            Buffer[offset] = ((byte)(value & 0xff));
            Buffer[offset + 1] = ((byte)((value >> 8) & 0xff));
        }

        public void Short(uint value, int offset) {
            Buffer[offset] = ((byte)(value & 0xff));
            offset++;
            Buffer[offset] = ((byte)((value >> 8) & 0xff));
        }

        public void Length(int value, int offset) {
            Buffer[offset] = ((byte)(value & 0xff));
            offset++;
            Buffer[offset] = ((byte)((value >> 8) & 0xff));
        }

        public void Type(int value, int offset) {
            Buffer[offset] = ((byte)(value & 0xff));
            offset++;
            Buffer[offset] = ((byte)((value >> 8) & 0xff));
        }

        public void Long(int value, int offset) {
            Buffer[offset] = ((byte)(value & 0xff));
            offset++;
            Buffer[offset] = ((byte)(value >> 8 & 0xff));
            offset++;
            Buffer[offset] = (byte)(value >> 16 & 0xff);
            offset++;
            Buffer[offset] = ((byte)(value >> 24 & 0xff));
        }

        public void Long(ulong value, int offset) {
            Buffer[offset] = ((byte)(value & 0xffL));
            offset++;
            Buffer[offset] = ((byte)(value >> 8 & 0xff));
            offset++;
            Buffer[offset] = (byte)(value >> 16 & 0xff);
            offset++;
            Buffer[offset] = ((byte)(value >> 24 & 0xff));
        }

        public void ULong(ulong value, int offset) {
            Buffer[offset] = (byte)(value);
            offset++;
            Buffer[offset] = (byte)(value >> 8);
            offset++;
            Buffer[offset] = (byte)(value >> 16);
            offset++;
            Buffer[offset] = (byte)(value >> 24);
            offset++;
            Buffer[offset] = (byte)(value >> 32);
            offset++;
            Buffer[offset] = (byte)(value >> 40);
            offset++;
            Buffer[offset] = (byte)(value >> 48);
            offset++;
            Buffer[offset] = (byte)(value >> 56);
        }

        public void Int(int value, int offset) {
            Buffer[offset] = (Convert.ToByte(value & 0xff));
        }

        public void Int(uint value, int offset) {
            Buffer[offset] = (Convert.ToByte(value & 0xff));
        }

        public void Long(uint value, int offset) {
            Buffer[offset] = ((byte)(value & 0xff));
            offset++;
            Buffer[offset] = ((byte)(value >> 8 & 0xff));
            offset++;
            Buffer[offset] = (byte)(value >> 16 & 0xff);
            offset++;
            Buffer[offset] = ((byte)(value >> 24 & 0xff));
        }

        #endregion
    }
}