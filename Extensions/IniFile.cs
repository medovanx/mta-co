using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MTA.Extensions {
    public class IniFile {
        private readonly string _fileName;
        private readonly string _fileSection;

        public IniFile() { }

        public IniFile(string fileName, string section = "data") {
            // Normalize path separators to backslashes for Windows API
            var normalizedPath = fileName.Replace('/', '\\');
            _fileName = Path.Combine(Environment.CurrentDirectory, normalizedPath);
            _fileSection = section;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int GetPrivateProfileStringA(string section, string key, string @default,
            StringBuilder buffer, int bufferSize, string fileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int WritePrivateProfileStringA(string section, string key, string arg, string fileName);

        public object this[object key, object @default = null] {
            get => ReadString(_fileSection, key.ToString(), @default.ToString(), 1024);
            set => Write(_fileSection, key.ToString(), value);
        }

        public byte ReadByte(string section, string key, byte @default) {
            byte.TryParse(ReadString(section, key, @default.ToString(), 6), out var buf);
            return buf;
        }

        public short ReadInt16(string section, string key, short @default) {
            short.TryParse(ReadString(section, key, @default.ToString(), 9), out var buf);
            return buf;
        }

        public int ReadInt32(string section, string key, int @default) {
            int.TryParse(ReadString(section, key, @default.ToString(), 15), out var buf);
            return buf;
        }

        public sbyte ReadSByte(string section, string key, byte @default) {
            sbyte.TryParse(ReadString(section, key, @default.ToString(), 6), out var buf);
            return buf;
        }

        public string ReadString(string section, string? key, string? @default = "", int bufSize = 400) {
            var buffer = new StringBuilder(bufSize);
            GetPrivateProfileStringA(section, key, @default, buffer, bufSize, _fileName);
            return buffer.ToString();
        }

        public ushort ReadUInt16(string section, string key) {
            ushort.TryParse(ReadString(section, key, 0.ToString(), 9), out var buf);
            return buf;
        }

        public uint ReadUInt32(string section, string key) {
            uint.TryParse(ReadString(section, key, 0.ToString(), 15), out var buf);
            return buf;
        }

        private void Write(string section, string? key, object value) {
            WritePrivateProfileStringA(section, key, value.ToString(), _fileName);
        }

        public void Write(string section, string key, string value) {
            WritePrivateProfileStringA(section, key, value, _fileName);
        }
    }
}