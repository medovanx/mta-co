using System;

namespace MTA.Network {
    public class PacketBuilder {
        private byte[] _buffer = new byte[1024];
        protected int Len;
        private int _position;
        private readonly byte[] _tqServer = Program.Encoding.GetBytes("TQServer");

        public PacketBuilder(int T, int l) {
            Len = l;
            Length(l);
            Type(T);
        }

        public int GetPos() {
            return _position;
        }

        public void SetPosition(int pos) {
            _position = pos;
        }

        public void Short(int value) {
            _buffer[_position] = ((byte)(value & 0xff));
            _position++;
            _buffer[_position] = ((byte)((value >> 8) & 0xff));
            _position++;
        }

        public void Short(uint value) {
            _buffer[_position] = ((byte)(value & 0xff));
            _position++;
            _buffer[_position] = ((byte)((value >> 8) & 0xff));
            _position++;
        }

        private void Length(int value) {
            _buffer[_position] = ((byte)(value & 0xff));
            _position++;
            _buffer[_position] = ((byte)((value >> 8) & 0xff));
            _position++;
        }

        private void Type(int value) {
            _buffer[_position] = ((byte)(value & 0xff));
            _position++;
            _buffer[_position] = ((byte)((value >> 8) & 0xff));
            _position++;
        }

        public void Long(int value) {
            _buffer[_position] = ((byte)(value & 0xff));
            _position++;
            _buffer[_position] = ((byte)(value >> 8 & 0xff));
            _position++;
            _buffer[_position] = (byte)(value >> 16 & 0xff);
            _position++;
            _buffer[_position] = ((byte)(value >> 24 & 0xff));
            _position++;
        }

        public void Long(ulong value) {
            _buffer[_position] = ((byte)(value & 0xffL));
            _position++;
            _buffer[_position] = ((byte)(value >> 8 & 0xff));
            _position++;
            _buffer[_position] = (byte)(value >> 16 & 0xff);
            _position++;
            _buffer[_position] = ((byte)(value >> 24 & 0xff));
            _position++;
        }

        public void ULong(ulong value) {
            _buffer[_position] = (byte)(value);
            _position++;
            _buffer[_position] = (byte)(value >> 8);
            _position++;
            _buffer[_position] = (byte)(value >> 16);
            _position++;
            _buffer[_position] = (byte)(value >> 24);
            _position++;
            _buffer[_position] = (byte)(value >> 32);
            _position++;
            _buffer[_position] = (byte)(value >> 40);
            _position++;
            _buffer[_position] = (byte)(value >> 48);
            _position++;
            _buffer[_position] = (byte)(value >> 56);
            _position++;
        }

        public void Int(int value) {
            _buffer[_position] = (Convert.ToByte(value & 0xff));
            _position++;
        }

        public void Int(uint value) {
            _buffer[_position] = (Convert.ToByte(value & 0xff));
            _position++;
        }

        public void Long(uint value) {
            _buffer[_position] = ((byte)(value & 0xff));
            _position++;
            _buffer[_position] = ((byte)(value >> 8 & 0xff));
            _position++;
            _buffer[_position] = (byte)(value >> 16 & 0xff);
            _position++;
            _buffer[_position] = ((byte)(value >> 24 & 0xff));
            _position++;
        }

        public void Move(int value) {
            for (var x = 0; x < value; x++) {
                _buffer[_position] = 0;
                _position++;
            }
        }

        public void Text(string value) {
            var nvalue = Program.Encoding.GetBytes(value);
            Array.Copy(nvalue, 0, _buffer, _position, nvalue.Length);
            _position += nvalue.Length;
        }

        private void Seal() {
            Array.Copy(_tqServer, 0, _buffer, _position, _tqServer.Length);
            _position += _tqServer.Length + 1;
            var x = new byte[_position - 1];
            Array.Copy(_buffer, x, _position - 1);
            _buffer = new byte[x.Length];
            Array.Copy(x, _buffer, x.Length);
        }

        public byte[] GetFinal() {
            Seal();
            return _buffer;
        }

        internal void Fill(int end) {
            for (var x = _position; x < end; x++)
                Int(0);
        }

        internal void PrintThis() {
            var dat = "";
            for (var x = 0; x < _position; x++)
                dat += _buffer[x].ToString("X") + " ";
            System.Console.WriteLine(dat);
        }

        #region Add from offset

        public void Short(int value, int offset) {
            _buffer[offset] = ((byte)(value & 0xff));
            _buffer[offset + 1] = ((byte)((value >> 8) & 0xff));
        }

        public void Short(uint value, int offset) {
            _buffer[offset] = ((byte)(value & 0xff));
            offset++;
            _buffer[offset] = ((byte)((value >> 8) & 0xff));
        }

        public void Length(int value, int offset) {
            _buffer[offset] = ((byte)(value & 0xff));
            offset++;
            _buffer[offset] = ((byte)((value >> 8) & 0xff));
        }

        public void Type(int value, int offset) {
            _buffer[offset] = ((byte)(value & 0xff));
            offset++;
            _buffer[offset] = ((byte)((value >> 8) & 0xff));
        }

        public void Long(int value, int offset) {
            _buffer[offset] = ((byte)(value & 0xff));
            offset++;
            _buffer[offset] = ((byte)(value >> 8 & 0xff));
            offset++;
            _buffer[offset] = (byte)(value >> 16 & 0xff);
            offset++;
            _buffer[offset] = ((byte)(value >> 24 & 0xff));
        }

        public void Long(ulong value, int offset) {
            _buffer[offset] = ((byte)(value & 0xffL));
            offset++;
            _buffer[offset] = ((byte)(value >> 8 & 0xff));
            offset++;
            _buffer[offset] = (byte)(value >> 16 & 0xff);
            offset++;
            _buffer[offset] = ((byte)(value >> 24 & 0xff));
        }

        public void ULong(ulong value, int offset) {
            _buffer[offset] = (byte)(value);
            offset++;
            _buffer[offset] = (byte)(value >> 8);
            offset++;
            _buffer[offset] = (byte)(value >> 16);
            offset++;
            _buffer[offset] = (byte)(value >> 24);
            offset++;
            _buffer[offset] = (byte)(value >> 32);
            offset++;
            _buffer[offset] = (byte)(value >> 40);
            offset++;
            _buffer[offset] = (byte)(value >> 48);
            offset++;
            _buffer[offset] = (byte)(value >> 56);
        }

        public void Int(int value, int offset) {
            _buffer[offset] = (Convert.ToByte(value & 0xff));
        }

        public void Int(uint value, int offset) {
            _buffer[offset] = (Convert.ToByte(value & 0xff));
        }

        public void Long(uint value, int offset) {
            _buffer[offset] = ((byte)(value & 0xff));
            offset++;
            _buffer[offset] = ((byte)(value >> 8 & 0xff));
            offset++;
            _buffer[offset] = (byte)(value >> 16 & 0xff);
            offset++;
            _buffer[offset] = ((byte)(value >> 24 & 0xff));
        }

        #endregion
    }
}