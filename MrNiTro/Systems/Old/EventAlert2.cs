using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets.EventAlert
{
    public class EventAlert2
    {
        public const uint

            CaptureFlag = 10539,
            SkillTeam = 10541,
            TeamPk = 10543,
            PowerArena = 10531,
            ElitePk = 10533,
            HorseRace = 10525,
            MonthlyPk = 10523,
            ClassPKWar = 10519,
            GuildWar = 10515,
            GuildWar2 = 10515,
            DizzyLand = 10545,
            WeeklyPk = 10529;



        public static void Handle(byte[] Data, MTA.Client.GameState client)
        {

            EventAlert alert = new EventAlert(Data);

            switch (client.Entity.StrResID)
            {
                case DizzyLand:
                    {
                        client.Entity.Teleport(5528, 50, 50);
                        Data data = new Data(true);
                        data.ID = GamePackets.Data.OpenCustom;
                        data.UID = client.Entity.UID;
                        data.TimeStamp = Time32.Now;
                        data.dwParam = 3378;
                        data.wParam1 = client.Entity.X;
                        data.wParam2 = client.Entity.Y;
                        client.Send(data);
                        EventAlert alert2 = new EventAlert
                        {
                            StrResID = 10546,
                            Countdown = 4,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 0;
                        client.Send((byte[])alert2);
                        break;
                    }
                case ClassPKWar:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            client.Entity.Teleport(7001, 25, 40);
                        }
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            client.Entity.Teleport(4500, 25, 40);
                        }
                        if (client.Entity.Class >= 40 && client.Entity.Class <= 45)
                        {
                            client.Entity.Teleport(4501, 25, 40);
                        }
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            client.Entity.Teleport(4502, 25, 40);
                        }
                        if (client.Entity.Class >= 60 && client.Entity.Class <= 65)
                        {
                            client.Entity.Teleport(4503, 25, 40);
                        }
                        if (client.Entity.Class >= 70 && client.Entity.Class <= 75)
                        {
                            client.Entity.Teleport(4504, 25, 40);
                        }
                        if (client.Entity.Class >= 132 && client.Entity.Class <= 135)
                        {
                            client.Entity.Teleport(4505, 25, 40);
                        }
                        if (client.Entity.Class >= 142 && client.Entity.Class <= 145)
                        {
                            client.Entity.Teleport(4506, 25, 40);
                        }
                        Data data = new Data(true);
                        data.ID = GamePackets.Data.OpenCustom;
                        data.UID = client.Entity.UID;
                        data.TimeStamp = Time32.Now;
                        data.dwParam = 3378;
                        data.wParam1 = client.Entity.X;
                        data.wParam2 = client.Entity.Y;
                        client.Send(data);
                        EventAlert alert2 = new EventAlert
                        {
                            StrResID = 10520,
                            Countdown = 4,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 0;
                        client.Send((byte[])alert2);
                        break;
                    }
                case CaptureFlag:
                    client.Entity.Teleport(2057, 496, 366);
                    EventAlert alert3 = new EventAlert
                    {
                        StrResID = 10539,
                        Countdown = 4,
                        UK12 = 1
                    };
                    client.Entity.StrResID = 0;
                    client.Send((byte[])alert3);
                    break;
                case SkillTeam://da al sa7
                    {
                        client.Entity.Teleport(1002, 309, 145);
                        client.Entity.StrResID = 0;
                        break;
                    }
                case TeamPk://da al sa7
                    {
                        client.Entity.Teleport(1002, 305, 145);
                        client.Entity.StrResID = 0;
                        break;
                    }
                case PowerArena:
                    {
                        client.Entity.Teleport(8877, 52, 44);
                        Data datas = new Data(true);
                        datas.ID = GamePackets.Data.OpenCustom;
                        datas.UID = client.Entity.UID;
                        datas.TimeStamp = Time32.Now;
                        datas.dwParam = 3378;
                        datas.wParam1 = client.Entity.X;
                        datas.wParam2 = client.Entity.Y;
                        client.Send(datas);
                        EventAlert alert5 = new EventAlert
                        {
                            StrResID = 10532,
                            Countdown = 4,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 0;
                        client.Send((byte[])alert5);
                        break;
                    }
                case ElitePk:
                    {
                        client.Entity.Teleport(1002, 295, 145);
                        Data datass = new Data(true);
                        datass.ID = GamePackets.Data.OpenCustom;
                        datass.UID = client.Entity.UID;
                        datass.TimeStamp = Time32.Now;
                        datass.dwParam = 3378;
                        datass.wParam1 = client.Entity.X;
                        datass.wParam2 = client.Entity.Y;
                        client.Send(datass);
                        EventAlert alert2 = new EventAlert
                        {
                            StrResID = 10534,
                            Countdown = 4,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 0;
                        client.Send((byte[])alert2);
                        break;
                    }
                case HorseRace:
                    client.Entity.Teleport(0x3ea, 0x1a7, 0xf5);
                    EventAlert alert6 = new EventAlert
                    {
                        StrResID = 10526,
                        Countdown = 4,
                        UK12 = 1
                    };
                    client.Entity.StrResID = 0;
                    client.Send((byte[])alert6);
                    break;
                case MonthlyPk:
                    client.Entity.Teleport(1002, 299, 145);
                    EventAlert alert7 = new EventAlert
                    {
                        StrResID = 10524,
                        Countdown = 4,
                        UK12 = 1
                    };
                    client.Entity.StrResID = 0;
                    client.Send((byte[])alert7);
                    break;
                case GuildWar:
                    client.Entity.Teleport(1038, 340, 331);
                    EventAlert alert8 = new EventAlert
                    {
                        StrResID = 10516,
                        Countdown = 4,
                        UK12 = 1
                    };
                    client.Entity.StrResID = 0;
                    client.Send((byte[])alert8);
                    break;
                case WeeklyPk:
                    client.Entity.Teleport(1002, 325, 194);
                    EventAlert alert9 = new EventAlert
                    {
                        StrResID = 10530,
                        Countdown = 4,
                        UK12 = 1
                    };
                    client.Entity.StrResID = 0;
                    client.Send((byte[])alert9);
                    break;
            }
        }
    }
    public class EventAlert
    {
        private byte[] mData;

        public EventAlert()
        {
            this.mData = new byte[20 + 8];
            Writer.Ushort(20, 0, this.mData);
            Writer.Ushort((ushort)0x466, 2, this.mData);
        }

        public EventAlert(byte[] d)
        {
            this.mData = new byte[d.Length];
            d.CopyTo(this.mData, 0);
        }

        public static implicit operator byte[](EventAlert d)
        {
            return d.mData;
        }

        public uint Countdown
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x10);
            }
            set
            {
                Writer.Uint(value, 0x10, this.mData);
            }
        }

        public uint StrResID
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 8);
            }
            set
            {
                Writer.Uint(value, 8, this.mData);
            }
        }

        public uint UK12
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 12);
            }
            set
            {
                Writer.Uint(value, 12, this.mData);
            }
        }
    }
}


