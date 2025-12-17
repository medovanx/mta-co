using System;
using MTA.Client;
using MTA.Database;
using MTA.Interfaces;

namespace MTA.Network.GamePackets.Roulette {
    public unsafe struct MsgRouletteSignUp : IPacket {
        public enum ActionJoin : byte {
            Join = 0,
            Watch = 4,
            InfoTable = 5, //?????
            Quit = 7
        }

        public ushort Length {
            set {
                packet = new byte[value + 8];
                Writer.WriteUshort((ushort)(packet.Length - 8), 0, packet);
            }
        }

        public ushort PacketID {
            set { Writer.WriteUshort(value, 2, packet); }
        }

        public ActionJoin Typ {
            get { return (ActionJoin)packet[4]; }
            set { packet[4] = (byte)value; }
        }

        public uint UID {
            get { return BitConverter.ReadUint(packet, 5); }
            set { Writer.WriteUint(value, 5, packet); }
        }


        public static MsgRouletteSignUp Create() {
            MsgRouletteSignUp ptr = new MsgRouletteSignUp();
            ptr.Length = 9;
            ptr.PacketID = GamePackets.MsgRouletteSignUp;
            return ptr;
        }

        public MsgRouletteSignUp(byte[] stream) {
            packet = stream;
        }

        public static void Poroces(GameState user, byte[] stream) {
            MsgRouletteSignUp Info = new MsgRouletteSignUp(stream);
            switch (Info.Typ) {
                case ActionJoin.InfoTable: {
                    Roulettes.RouletteTable Table;
                    if (Roulettes.RoulettesPoll.TryGetValue(Info.UID, out Table)) {
                        Table.SpawnPacket.PlayersCount = (byte)Table.RegistredPlayers.Count;
                        user.Send(Table.SpawnPacket);
                    }

                    break;
                }
                case ActionJoin.Join: {
                    Roulettes.RouletteTable Table;
                    if (Roulettes.RoulettesPoll.TryGetValue(Info.UID, out Table)) {
                        switch (Table.SpawnPacket.MoneyType) {
                            case MsgRouletteTable.TableType.ConquerPoints: {
                                if (user.Entity.ConquerPoints >= 5) {
                                    Table.AddPlayer(user);
                                }
                                else
                                    user.Send(new Message(
                                        "Sorry, you should have at least 5 CPs to join in the Roulette.",
                                        Message.Service));

                                break;
                            }
                            case MsgRouletteTable.TableType.Money: {
                                if (user.Entity.Money >= 5000) {
                                    Table.AddPlayer(user);
                                }
                                else
                                    user.Send(new Message(
                                        "Sorry, you should have at least 5,000 Silvers to join in the Roulette.",
                                        Message.Service));

                                break;
                            }
                        }
                    }

                    break;
                }
                case ActionJoin.Watch: {
                    Roulettes.RouletteTable Table;
                    if (Roulettes.RoulettesPoll.TryGetValue(Info.UID, out Table)) {
                        Table.AddWatch(user);
                    }

                    break;
                }
            }
        }

        public byte[] packet;

        public byte[] ToArray() {
            return packet;
        }

        public void Send(GameState client) {
            client.Send(ToArray());
        }


        public void Deserialize(byte[] buffer) {
            throw new NotImplementedException();
        }
    }
}