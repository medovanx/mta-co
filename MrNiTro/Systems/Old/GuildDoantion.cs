using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conquer_Online_Server.Game.ConquerStructures.Society;

namespace Conquer_Online_Server.Game.ConquerStructures.Society
{
    class GuildDoantion
    {
        #region Roses
        public static byte[] BuildPacketRedRosseDonaionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.RosesList.Count > 0xA ? 0xA : client.AsMember.RosesList.Count));
            wtr.Write((uint)(client.AsMember.RosesList.Count > 20 ? 20 : client.AsMember.RosesList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.RosesList.Count > 10 ? x < 10 : x < client.AsMember.RosesList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].RosesList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].RosesList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketRedRossePage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.RosesList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.RosesList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.RosesList.Count > 0x64 ? 0x64 : client.AsMember.RosesList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.RosesList.Count > 20 ? x < 20 : x < client.AsMember.RosesList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].RosesList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].RosesList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].RosesList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region Lilise
        public static byte[] BuildPacketLiliseDonaionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.LiliesList.Count > 0xA ? 0xA : client.AsMember.LiliesList.Count));
            wtr.Write((uint)(client.AsMember.LiliesList.Count > 20 ? 20 : client.AsMember.LiliesList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.LiliesList.Count > 10 ? x < 10 : x < client.AsMember.LiliesList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].LiliesList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].LiliesList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketLiliesPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.LiliesList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.LiliesList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.LiliesList.Count > 20 ? 20 : client.AsMember.LiliesList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.LiliesList.Count > 20 ? x < 20 : x < client.AsMember.LiliesList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].LiliesList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].LiliesList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].LiliesList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region Tuplise
        public static byte[] BuildPacketTupliseDonaionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TulipsList.Count > 0xA ? 0xA : client.AsMember.TulipsList.Count));
            wtr.Write((uint)(client.AsMember.TulipsList.Count > 20 ? 20 : client.AsMember.TulipsList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.TulipsList.Count > 10 ? x < 10 : x < client.AsMember.TulipsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TulipsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TulipsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTuplisePage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.TulipsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TulipsList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TulipsList.Count > 20 ? 20 : client.AsMember.TulipsList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.TulipsList.Count > 20 ? x < 20 : x < client.AsMember.TulipsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TulipsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TulipsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TulipsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region Orchades
        public static byte[] BuildPacketOrchadesDonaionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.OrchidsList.Count > 0xA ? 0xA : client.AsMember.OrchidsList.Count));
            wtr.Write((uint)(client.AsMember.OrchidsList.Count > 20 ? 20 : client.AsMember.OrchidsList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.OrchidsList.Count > 10 ? x < 10 : x < client.AsMember.OrchidsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].OrchidsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].OrchidsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketOrchadesPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.OrchidsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.OrchidsList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.OrchidsList.Count > 20 ? 20 : client.AsMember.OrchidsList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.OrchidsList.Count > 20 ? x < 20 : x < client.AsMember.OrchidsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].OrchidsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].OrchidsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].OrchidsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region GuideDoantion
        public static byte[] BuildPacketGuideDonaionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.GuidesList.Count > 0xA ? 0xA : client.AsMember.GuidesList.Count));
            wtr.Write((uint)(client.AsMember.GuidesList.Count > 20 ? 20 : client.AsMember.GuidesList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.GuidesList.Count > 10 ? x < 10 : x < client.AsMember.GuidesList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].GuidesList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].GuidesList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketGuidePage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.GuidesList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.GuidesList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.GuidesList.Count > 20 ? 20 : client.AsMember.GuidesList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.GuidesList.Count > 20 ? x < 20 : x < client.AsMember.GuidesList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].GuidesList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].GuidesList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].GuidesList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region CpDoantion
        public static byte[] BuildPacketCpDonaionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.CPSList.Count > 0xA ? 0xA : client.AsMember.CPSList.Count));
            wtr.Write((uint)(client.AsMember.CPSList.Count > 20 ? 20 : client.AsMember.CPSList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.CPSList.Count > 10 ? x < 10 : x < client.AsMember.CPSList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].CPSList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].CPSList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketCpsDonationPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.CPSList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.CPSList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.CPSList.Count > 20 ? 20 : client.AsMember.CPSList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.CPSList.Count > 20 ? x < 20 : x < client.AsMember.CPSList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].CPSList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].CPSList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].CPSList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region ArsenalDoantion
        public static byte[] BuildPacketArsenalDonationPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.ArsenalsList.Count > 0xA ? 0xA : client.AsMember.ArsenalsList.Count));
            wtr.Write((uint)(client.AsMember.ArsenalsList.Count > 20 ? 20 : client.AsMember.ArsenalsList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.ArsenalsList.Count > 10 ? x < 10 : x < client.AsMember.ArsenalsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].ArsenalsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].ArsenalsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketArsenalDonationPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.ArsenalsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.ArsenalsList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.ArsenalsList.Count > 20 ? 20 : client.AsMember.ArsenalsList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.ArsenalsList.Count > 20 ? x < 20 : x < client.AsMember.ArsenalsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].ArsenalsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].ArsenalsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].ArsenalsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region PkDoantion
        public static byte[] BuildPacketPkDonationPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.PKSList.Count > 0xA ? 0xA : client.AsMember.PKSList.Count));
            wtr.Write((uint)(client.AsMember.PKSList.Count > 20 ? 20 : client.AsMember.PKSList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.PKSList.Count > 10 ? x < 10 : x < client.AsMember.PKSList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].PKSList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].PKSList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketPkDonationPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.PKSList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.PKSList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.PKSList.Count > 20 ? 20 : client.AsMember.PKSList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.PKSList.Count > 20 ? x < 20 : x < client.AsMember.PKSList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].PKSList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].PKSList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].PKSList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region SilverDonation
        public static byte[] BuildPacketSilverDonationPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.SilversList.Count > 0xA ? 0xA : client.AsMember.SilversList.Count));
            wtr.Write((uint)(client.AsMember.SilversList.Count > 20 ? 20 : client.AsMember.SilversList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.SilversList.Count > 10 ? x < 10 : x < client.AsMember.SilversList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].SilversList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].SilversList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketSilverDonationPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.SilversList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.SilversList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.SilversList.Count > 20 ? 20 : client.AsMember.SilversList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.SilversList.Count > 20 ? x < 20 : x < client.AsMember.SilversList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].SilversList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].SilversList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].SilversList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
        #region TotalDoantion
        public static byte[] BuildPacketTotalDoantionPage1(Client.GameState client, byte Subtype, byte pagenumber)
        {

            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 0xA ? 0xA : client.AsMember.TotalDonationsList.Count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 1
            for (int x = 0; client.AsMember.TotalDonationsList.Count > 10 ? x < 10 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage2(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(20 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 20 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 2

            for (int x = 10; client.AsMember.TotalDonationsList.Count > 20 ? x < 20 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage3(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(30 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 30 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 3
            for (int x = 20; client.AsMember.TotalDonationsList.Count > 30 ? x < 30 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage4(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(40 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 40 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 4
            for (int x = 30; client.AsMember.TotalDonationsList.Count > 40 ? x < 40 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage5(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(50 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 50 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 5
            for (int x = 40; client.AsMember.TotalDonationsList.Count > 50 ? x < 50 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage6(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(60 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 60 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 6
            for (int x = 50; client.AsMember.TotalDonationsList.Count > 60 ? x < 60 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage7(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(70 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 70 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 7
            for (int x = 60; client.AsMember.TotalDonationsList.Count > 70 ? x < 70 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage8(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(80 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 80 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 8
            for (int x = 70; client.AsMember.TotalDonationsList.Count > 80 ? x < 80 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage9(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(90 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 90 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 9
            for (int x = 80; client.AsMember.TotalDonationsList.Count > 90 ? x < 90 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        public static byte[] BuildPacketTotalDoantionPage10(Client.GameState client, byte Subtype, byte pagenumber)
        {
            ushort count = (ushort)(100 - client.AsMember.TotalDonationsList.Count);
            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2101);
            wtr.Write((byte)Subtype);
            wtr.Write((byte)pagenumber);
            wtr.Write((ushort)(client.AsMember.TotalDonationsList.Count > 100 ? 0xA : 10 - count));
            wtr.Write((uint)(client.AsMember.TotalDonationsList.Count > 0x64 ? 0x64 : client.AsMember.TotalDonationsList.Count));
            #region TotalDoantion Packet Page 10
            for (int x = 90; client.AsMember.TotalDonationsList.Count > 100 ? x < 100 : x < client.AsMember.TotalDonationsList.Count; x++)
            {
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].EntityID); // client UID
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].postion); // postion number
                wtr.Write((uint)x); // rank number
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].SilverDonation); // silver doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].cpdoantion); // cp doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].guidedoantion); // guide doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].pkdoantion); // pk doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].arsenaldoantion); // doantion arsenal
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].rosedoantion); // rose doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].liliesdoantion); // lily doantion
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].orchidesdoantion); // orchids doantion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].tuplisedoantion); // tulip dontion 
                wtr.Write((uint)client.Guild.ListMember[x].TotalDonationsList[x].totaldoantion); // total donation :D
                for (int s = 0; s < 16; s++)
                {
                    if (s < client.Guild.ListMember[x].TotalDonationsList[x].Name.Length)
                    {
                        wtr.Write((byte)client.Guild.ListMember[x].TotalDonationsList[x].Name[s]);
                    }
                    else
                        wtr.Write((byte)0);
                }

            }
            #endregion
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(ASCIIEncoding.ASCII.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            return buf;
        }
        #endregion
    }
}