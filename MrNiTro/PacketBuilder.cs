using System;
using MTA;

namespace MTA
{
    public class PacketBuilder
    {
        protected byte[] _buffer = new byte[1024];
        protected int Position = 0;
        protected int Len = 0;
        protected byte[] TQ_SERVER = Program.Encoding.GetBytes("TQServer");
        public int GetPos()
        {
            return Position;
        }
        public void SetPosition(int Pos)
        {
            Position = Pos;
        }
        public PacketBuilder(int T, int L)
        {
            Len = L;
            Length(L);
            Type(T);
        }

        public void Short(int value)
        {
            _buffer[Position] = ((byte)(value & 0xff));
            Position++;
            _buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }
        public void Short(uint value)
        {
            _buffer[Position] = ((byte)(value & 0xff));
            Position++;
            _buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }
        public void Length(int value)
        {
            _buffer[Position] = ((byte)(value & 0xff));
            Position++;
            _buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }
        public void Type(int value)
        {
            _buffer[Position] = ((byte)(value & 0xff));
            Position++;
            _buffer[Position] = ((byte)((value >> 8) & 0xff));
            Position++;
        }
        public void Long(int value)
        {
            _buffer[Position] = ((byte)(value & 0xff));
            Position++;
            _buffer[Position] = ((byte)(value >> 8 & 0xff));
            Position++;
            _buffer[Position] = (byte)(value >> 16 & 0xff);
            Position++;
            _buffer[Position] = ((byte)(value >> 24 & 0xff));
            Position++;
        }
        public void Long(ulong value)
        {
            _buffer[Position] = ((byte)((ulong)value & 0xffL));
            Position++;
            _buffer[Position] = ((byte)(value >> 8 & 0xff));
            Position++;
            _buffer[Position] = (byte)(value >> 16 & 0xff);
            Position++;
            _buffer[Position] = ((byte)(value >> 24 & 0xff));
            Position++;
        }
        public void ULong(ulong value)
        {
            _buffer[Position] = (byte)(value);
            Position++;
            _buffer[Position] = (byte)(value >> 8);
            Position++;
            _buffer[Position] = (byte)(value >> 16);
            Position++;
            _buffer[Position] = (byte)(value >> 24);
            Position++;
            _buffer[Position] = (byte)(value >> 32);
            Position++;
            _buffer[Position] = (byte)(value >> 40);
            Position++;
            _buffer[Position] = (byte)(value >> 48);
            Position++;
            _buffer[Position] = (byte)(value >> 56);
            Position++;
        }
        public void Int(int value)
        {
            _buffer[Position] = (Convert.ToByte(value & 0xff));
            Position++;
        }
        public void Int(uint value)
        {
            _buffer[Position] = (Convert.ToByte(value & 0xff));
            Position++;
        }
        public void Long(uint value)
        {
            _buffer[Position] = ((byte)(value & 0xff));
            Position++;
            _buffer[Position] = ((byte)(value >> 8 & 0xff));
            Position++;
            _buffer[Position] = (byte)(value >> 16 & 0xff);
            Position++;
            _buffer[Position] = ((byte)(value >> 24 & 0xff));
            Position++;
        }
        public void Move(int value)
        {
            for (int x = 0; x < value; x++)
            {
                _buffer[Position] = 0;
                Position++;
            }
        }

        public void Text(string value)
        {
            byte[] nvalue = Program.Encoding.GetBytes(value);
            Array.Copy(nvalue, 0, _buffer, Position, nvalue.Length);
            Position += nvalue.Length;
        }
        protected void Seal()
        {
            Array.Copy(TQ_SERVER, 0, _buffer, Position, TQ_SERVER.Length);
            Position += TQ_SERVER.Length + 1;
            byte[] x = new byte[Position - 1];
            Array.Copy(_buffer, x, Position - 1);
            _buffer = new byte[x.Length];
            Array.Copy(x, _buffer, x.Length);
            x = null;
        }
        public byte[] getFinal()
        {
            Seal();
            return _buffer;
        }

        internal void Fill(int End)
        {
            for (int x = Position; x < End; x++)
                Int(0);
        }
        internal void PrintThis()
        {
            string Dat = "";
            for (int x = 0; x < Position; x++)
                Dat += _buffer[x].ToString("X") + " ";
            System.Console.WriteLine(Dat);
        }

        #region Add from offset
        public void Short(int value, int Offset)
        {
            _buffer[Offset] = ((byte)(value & 0xff));
            _buffer[Offset + 1] = ((byte)((value >> 8) & 0xff));
        }
        public void Short(uint value, int Offset)
        {
            _buffer[Offset] = ((byte)(value & 0xff));
            Offset++;
            _buffer[Offset] = ((byte)((value >> 8) & 0xff));
        }
        public void Length(int value, int Offset)
        {
            _buffer[Offset] = ((byte)(value & 0xff));
            Offset++;
            _buffer[Offset] = ((byte)((value >> 8) & 0xff));
        }
        public void Type(int value, int Offset)
        {
            _buffer[Offset] = ((byte)(value & 0xff));
            Offset++;
            _buffer[Offset] = ((byte)((value >> 8) & 0xff));
        }
        public void Long(int value, int Offset)
        {
            _buffer[Offset] = ((byte)(value & 0xff));
            Offset++;
            _buffer[Offset] = ((byte)(value >> 8 & 0xff));
            Offset++;
            _buffer[Offset] = (byte)(value >> 16 & 0xff);
            Offset++;
            _buffer[Offset] = ((byte)(value >> 24 & 0xff));
        }
        public void Long(ulong value, int Offset)
        {
            _buffer[Offset] = ((byte)((ulong)value & 0xffL));
            Offset++;
            _buffer[Offset] = ((byte)(value >> 8 & 0xff));
            Offset++;
            _buffer[Offset] = (byte)(value >> 16 & 0xff);
            Offset++;
            _buffer[Offset] = ((byte)(value >> 24 & 0xff));
        }
        public void ULong(ulong value, int Offset)
        {
            _buffer[Offset] = (byte)(value);
            Offset++;
            _buffer[Offset] = (byte)(value >> 8);
            Offset++;
            _buffer[Offset] = (byte)(value >> 16);
            Offset++;
            _buffer[Offset] = (byte)(value >> 24);
            Offset++;
            _buffer[Offset] = (byte)(value >> 32);
            Offset++;
            _buffer[Offset] = (byte)(value >> 40);
            Offset++;
            _buffer[Offset] = (byte)(value >> 48);
            Offset++;
            _buffer[Offset] = (byte)(value >> 56);
        }
        public void Int(int value, int Offset)
        {
            _buffer[Offset] = (Convert.ToByte(value & 0xff));
            Offset++;
        }
        public void Int(uint value, int Offset)
        {
            _buffer[Offset] = (Convert.ToByte(value & 0xff));
            Offset++;
        }
        public void Long(uint value, int Offset)
        {
            _buffer[Offset] = ((byte)(value & 0xff));
            Offset++;
            _buffer[Offset] = ((byte)(value >> 8 & 0xff));
            Offset++;
            _buffer[Offset] = (byte)(value >> 16 & 0xff);
            Offset++;
            _buffer[Offset] = ((byte)(value >> 24 & 0xff));
            Offset++;
        }
        #endregion
    }
}