using MTA.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.TransferServer
{
    public class CheckUser : Writer
    {
        public byte[] Buffer;
        public CheckUser(uint uid, string user)
        {
            Buffer = new byte[40];
            WriteUInt16(40, 0, Buffer);
            WriteUInt16(5050, 2, Buffer);
            WriteUInt32(uid, 4, Buffer);
            WriteString(user, 8, Buffer);
            WriteString(Constants.ServerName, 24, Buffer);
        }

        public byte[] GetArray() { return Buffer; }
    }
    public enum Reply
    {
        Verified,
        Refused
    }
    public class CheckUserReply : Writer
    {
        public byte[] Buffer;
        public CheckUserReply(uint uid, string user, Reply Reply = Reply.Refused)
        {
            Buffer = new byte[44];
            WriteUInt16(44, 0, Buffer);
            WriteUInt16(5051, 2, Buffer);
            WriteUInt32(uid, 4, Buffer);
            WriteUInt32((uint)Reply, 8, Buffer);
            WriteString(user, 12, Buffer);
            WriteString(Constants.ServerName, 28, Buffer);
        }
        public byte[] GetArray() { return Buffer; }
    }
    public class DoneUser : Writer
    {
        public byte[] Buffer;
        public DoneUser(uint uid, string user)
        {
            Buffer = new byte[40];
            WriteUInt16(40, 0, Buffer);
            WriteUInt16(5053, 2, Buffer);
            WriteUInt32(uid, 4, Buffer);
            WriteString(user, 8, Buffer);
            WriteString(Constants.ServerName, 24, Buffer);
        }

        public byte[] GetArray() { return Buffer; }
    }
}
