using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MTA.Game.ConquerStructures.Society;
using MTA.Client;
using MTA.Game.Features.Flowers;

namespace MTA.Game.ConquerStructures.Society
{
    class FlowerRank
    {
        public static byte[] BuildPacketRankFlower(Client.GameState client, uint uid, ushort pagenumber)
        {

            MemoryStream strm = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)1151);
            wtr.Write((uint)1);
            wtr.Write((uint)uid);
            wtr.Write((ushort)0);
            wtr.Write((ushort)pagenumber);
            if (uid == 0x1c9c382)
                wtr.Write((uint)(Math.Min(MTA.Game.Features.Flowers.Flowers.Redrosse.Count, 10)));
            if (uid == 0x1c9c3e6)
                wtr.Write((uint)(Math.Min(MTA.Game.Features.Flowers.Flowers.Lilise.Count, 10)));
            if (uid == 0x1c9c44a)
                wtr.Write((uint)(Math.Min(MTA.Game.Features.Flowers.Flowers.Orchides.Count, 10)));
            if (uid == 0x1c9c4ae)
                wtr.Write((uint)(Math.Min(MTA.Game.Features.Flowers.Flowers.Tuplise.Count, 10)));
            wtr.Write((uint)0);

            if (uid == 0x1c9c382)
            {
                for (int b = pagenumber * 10; b <= pagenumber * 10 + Math.Min(10, Game.Features.Flowers.Flowers.Redrosse.Count) - 1; b++)
                {
                    if (pagenumber == 1)
                    {
                        if (MTA.Game.Features.Flowers.Flowers.Redrosse.Count < 11)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 2)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 21)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 3)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 31)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 4)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 41)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 5)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 51)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 6)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 61)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 7)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 71)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 8)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 81)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 9)
                    {
                        if (Game.Features.Flowers.Flowers.Redrosse.Count < 91)
                        {
                            break;
                        }
                    }
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Redrosse[b].rank);
                    wtr.Write((uint)0);
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Redrosse[b].redrosse);
                    wtr.Write((uint)0);
                    wtr.Write((uint)2301694);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Redrosse[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Redrosse[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)0);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Redrosse[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Redrosse[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);

                    }
                }
            }
            if (uid == 0x1c9c3e6)
            {
                for (int b = pagenumber * 10; b <= pagenumber * 10 + Math.Min(9, MTA.Game.Features.Flowers.Flowers.Lilise.Count) - 1; b++)
                {
                    if (pagenumber == 1)
                    {
                        if (MTA.Game.Features.Flowers.Flowers.Lilise.Count < 11)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 2)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 21)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 3)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 31)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 4)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 41)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 5)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 51)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 6)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 61)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 7)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 71)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 8)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 81)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 9)
                    {
                        if (Game.Features.Flowers.Flowers.Lilise.Count < 91)
                        {
                            break;
                        }
                    }
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Lilise[b].rank);
                    wtr.Write((uint)0);
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Lilise[b].lilise);
                    wtr.Write((uint)0);
                    wtr.Write((uint)2301694);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Lilise[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Lilise[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)0);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Lilise[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Lilise[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);

                    }
                }
            }
            if (uid == 0x1c9c44a)
            {
                for (int b = pagenumber * 10; b <= pagenumber * 10 + Math.Min(9, MTA.Game.Features.Flowers.Flowers.Orchides.Count) - 1; b++)
                {
                    if (pagenumber == 1)
                    {
                        if (MTA.Game.Features.Flowers.Flowers.Orchides.Count < 11)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 2)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 21)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 3)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 31)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 4)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 41)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 5)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 51)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 6)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 61)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 7)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 71)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 8)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 81)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 9)
                    {
                        if (Game.Features.Flowers.Flowers.Orchides.Count < 91)
                        {
                            break;
                        }
                    }
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Orchides[b].rank);
                    wtr.Write((uint)0);
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Orchides[b].orchides);
                    wtr.Write((uint)0);
                    wtr.Write((uint)2301694);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Orchides[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Orchides[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)0);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Orchides[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Orchides[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);

                    }
                }
            }
            if (uid == 0x1c9c4ae)
            {
                for (int b = pagenumber * 10; b <= pagenumber * 10 + Math.Min(10, MTA.Game.Features.Flowers.Flowers.Tuplise.Count) - 1; b++)
                {
                    if (pagenumber == 1)
                    {
                        if (MTA.Game.Features.Flowers.Flowers.Tuplise.Count < 11)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 2)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 21)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 3)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 31)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 4)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 41)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 5)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 51)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 6)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 61)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 7)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 71)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 8)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 81)
                        {
                            break;
                        }
                    }
                    if (pagenumber == 9)
                    {
                        if (Game.Features.Flowers.Flowers.Tuplise.Count < 91)
                        {
                            break;
                        }
                    }
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Tuplise[b].rank);
                    wtr.Write((uint)0);
                    wtr.Write((uint)MTA.Game.Features.Flowers.Flowers.Tuplise[b].tuplise);
                    wtr.Write((uint)0);
                    wtr.Write((uint)2301694);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Tuplise[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Tuplise[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);
                    }
                    wtr.Write((uint)0);
                    for (int s = 0; s < 16; s++)
                    {
                        if (s < MTA.Game.Features.Flowers.Flowers.Tuplise[b].name.Length)
                        {
                            wtr.Write((byte)MTA.Game.Features.Flowers.Flowers.Tuplise[b].name[s]);
                        }
                        else
                            wtr.Write((byte)0);

                    }
                }
            }
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
    }
}