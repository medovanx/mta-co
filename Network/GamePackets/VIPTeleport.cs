using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum VIPTeleportLocations
{
    TwinCity = 1,
    PhoenixCastle = 2,
    ApeCity = 3,
    DesertCity = 4,
    BirdIland = 5,
    TCSquare = 6,
    WPFarm = 7,
    WPBridge = 8,
    WPAltar = 9,
    WPApparation = 10,
    WPPoltergiest = 11,
    WPTurtledove = 12,
    PCSqaure = 13,
    MFWaterCave = 14,
    MFVillage = 15,
    MFLake = 16,
    MFMineCave = 17,
    MFBridge = 18,
    MFToApeCity = 19,
    ACSquare = 20,
    ACSouth = 21,
    ACEast = 22,
    ACNorth = 23,
    ACWest = 24,
    BISquare = 25,
    BICenter = 26,
    BISouthWest = 27,
    BINorthWest = 28,
    BINorthEast = 29,
    DCSquare = 30,
    DCSouth = 31,
    DCVillage = 32,
    DCMoonSpring = 33,
    DCAncientMaze = 34
}
public enum VIPTeleportTypes
{
    SelfTeleport = 0,
    TeamTeleport = 1,
    TeammateConfirmation = 2,
    TeammateTeleport = 3,
}

namespace MTA.Network.GamePackets
{
    public class VIPTeleport : Writer, Interfaces.IPacket
    {
        private Byte[] Buffer;
        public static implicit operator Byte[](VIPTeleport d) { return d.Buffer; }

        public VIPTeleport()
        {
            this.Buffer = new Byte[32 + 8];
            WriteUInt16(32, 0, Buffer);
            WriteUInt16(1128, 2, Buffer);
        }

        public VIPTeleportTypes TeleportType
        {
            get { return (VIPTeleportTypes)BitConverter.ToUInt32(this.Buffer, 4); }
            set { WriteUInt32((UInt32)value, 4, Buffer); }
        }
        public VIPTeleportLocations Location
        {
            get { return (VIPTeleportLocations)BitConverter.ToUInt32(this.Buffer, 8); }
            set { WriteUInt32((UInt32)value, 8, Buffer); }
        }
        public Byte Countdown
        {
            get { return Buffer[12]; }
            set { Buffer[12] = value; }
        }
        public String Name
        {
            get { return Encoding.ASCII.GetString(Buffer, 17, Buffer[16]); }
            set { WriteString(value, 16, Buffer); }
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
    }
}