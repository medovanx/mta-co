using System;
using System.Drawing;
using MTA.Game;
namespace MTA.Network.GamePackets
{
    public class Team : Writer, Interfaces.IPacket
    {
        public enum Mode : uint
        {
            Create = 0,
            JoinRequest = 1,
            ExitTeam = 2,
            AcceptInvitation = 3,
            InviteRequest = 4,
            AcceptJoinRequest = 5,
            Dismiss = 6,
            Kick = 7,
            ForbidJoining = 8,
            UnforbidJoining = 9,
            LootMoneyOff = 10,
            LootMoneyOn = 11,
            LootItemsOff = 12,
            LootItemsOn = 13,
            Unknown = 14,
            TeamLader = 15
        }
        byte[] Buffer;
        public Team()
        {
            Buffer = new byte[12 + 8];
            WriteUInt16(12, 0, Buffer);
            WriteUInt16(1023, 2, Buffer);
        }
        public Mode Action
        {
            get { return (Mode)Buffer[4]; }
            set { Write((ushort)value, 4, Buffer); }
        }
        public uint Type
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
        public static void Process(byte[] Packet, Client.GameState client)
        {
            if (client.Action != 2) return;
            Team Team = new Team();
            Team.Deserialize(Packet);
            switch (Team.Action)
            {
                case Mode.Create:
                    {
                        if (client.Entity.MaxHitpoints == 0) break;
                        client.Team = new Game.ConquerStructures.Team(client);
                        Team.Action = Mode.TeamLader;
                        client.Send(Team.ToArray());
                        Team.Action = Mode.Create;
                        client.Send(Team.ToArray());
                        break;
                    }
                case Mode.AcceptJoinRequest:
                    {
                        if (client.Team != null && client.Entity.Hitpoints > 0)
                        {
                            if (client.Team.AllowADD() && client.Team.TeamLider(client) && !client.Team.ForbidJoin)
                            {
                                Client.GameState NewTeammate;
                                if (Kernel.GamePool.TryGetValue(Team.UID, out NewTeammate))
                                {
                                    if (NewTeammate.Team != null) return;
                                    NewTeammate.Team = client.Team;
                                    client.Team.Add(NewTeammate);
                                }
                                if (client.Team.PickupMoney)
                                {
                                    NewTeammate.Send(new Message("Share Silver: On", Color.Yellow, Message.Team));
                                }
                                if (!client.Team.PickupMoney)
                                {
                                    NewTeammate.Send(new Message("Share Silver: Off", Color.Yellow, Message.Team));
                                }
                                if (client.Team.PickupItems)
                                {
                                    NewTeammate.Send(new Message("Share Loots: On", Color.Yellow, Message.Team));
                                }
                                if (!client.Team.PickupItems)
                                {
                                    NewTeammate.Send(new Message("Share Loots: Off", Color.Yellow, Message.Team));
                                }
                            }
                        }
                        break;
                    }
                case Mode.AcceptInvitation:
                    {
                        if (client.Team == null && client.Entity.Hitpoints > 0)
                        {
                            Client.GameState Leader;
                            if (Kernel.GamePool.TryGetValue(Team.UID, out Leader))
                            {
                                if (Leader.Team != null)
                                {
                                    if (!Leader.Team.AllowADD() || Leader.Team.ForbidJoin) return;
                                    client.Team = Leader.Team;
                                    Leader.Team.Add(client);
                                    if (client.Team.PickupMoney)
                                    {
                                        client.Send(new Message("Share Silver: On", Color.Yellow, Message.Team));
                                    }
                                    if (!client.Team.PickupMoney)
                                    {
                                        client.Send(new Message("Share Silver: Off", Color.Yellow, Message.Team));
                                    }
                                    if (client.Team.PickupItems)
                                    {
                                        client.Send(new Message("Share Loots: On", Color.Yellow, Message.Team));
                                    }
                                    if (!client.Team.PickupItems)
                                    {
                                        client.Send(new Message("Share Loots: Off", Color.Yellow, Message.Team));
                                    }
                                    return;
                                }
                            }
                        }
                        break;
                    }
                case Mode.InviteRequest:
                    {
                        if (client.Team != null)
                        {
                            if (client.Team.AllowADD() && client.Team.TeamLider(client))
                            {
                                Client.GameState Inv;
                                if (Kernel.GamePool.TryGetValue(Team.UID, out Inv))
                                {
                                    if (Inv.Team == null)
                                    {
                                        PopupLevelBP Relation = new PopupLevelBP();
                                        Relation.Level = client.Entity.Level;
                                        Relation.BattlePower = (uint)client.Entity.BattlePower;
                                        Relation.Receiver = Inv.Entity.UID;
                                        Relation.Requester = client.Entity.UID;
                                        Inv.Send(Relation.ToArray());
                                        Team.UID = client.Entity.UID;
                                        client.Send("[Team]The invitation has been send.");
                                        Inv.Send(Team.ToArray());
                                    }
                                    else client.Send("[Team]Failed to invite. This Entity has already joined a team.");
                                }
                            }
                        }
                        break;
                    }
                case Mode.JoinRequest:
                    {
                        if (client.Team == null && client.Entity.Hitpoints > 0)
                        {
                            Client.GameState Leader;
                            if (Kernel.GamePool.TryGetValue(Team.UID, out Leader))
                            {
                                if (Leader.Team != null)
                                {
                                    if (Leader.Team.TeamLider(Leader) && Leader.Team.AllowADD())
                                    {
                                        PopupLevelBP Relation = new PopupLevelBP();
                                        Relation.Level = client.Entity.Level;
                                        Relation.BattlePower = (uint)client.Entity.BattlePower;
                                        Relation.Receiver = Leader.Entity.UID;
                                        Relation.Requester = client.Entity.UID;
                                        Leader.Send(Relation.ToArray());
                                        Team.UID = client.Entity.UID;
                                        client.Send("[Team]The request has been send.");
                                        Leader.Send(Team.ToArray());
                                    }
                                    else client.Send("[Team]This team is full.");
                                }
                                else client.Send("[Team]This Entity has not created a team.");
                            }
                        }
                        break;
                    }
                case Mode.ExitTeam:
                    {
                        if (client.Entity.MapID == 16414) return;
                        if (client.Team != null)
                        {
                            client.Team.Remove(client, true);
                        }
                        break;
                    }
                case Mode.Dismiss:
                    {
                        if (client.Entity.MapID == 16414) return;
                        if (client != null && client.Team != null)
                        {
                            client.Team.Remove(client, false);
                        }
                        break;
                    }
                case Mode.Kick:
                    {
                        if (client.Entity.MapID == 16414) return;
                        if (client.Team == null) break;
                        Client.GameState remov;
                        if (client.Team.TryGetMember(Team.UID, out remov))
                        {
                            client.Team.Remove(remov, false);
                        }
                        break;
                    }
                case Mode.ForbidJoining:
                    {
                        if (client.Team != null)
                        {
                            client.Team.SendTeam(Team.ToArray(), true);
                            client.Team.ForbidJoin = true;
                        }
                        break;
                    }
                case Mode.UnforbidJoining:
                    {
                        if (client.Team != null)
                        {
                            client.Team.SendTeam(Team.ToArray(), true);
                            client.Team.ForbidJoin = false;
                        }
                        break;
                    }
                case Mode.LootMoneyOff:
                    {
                        if (client.Team != null)
                        {
                            client.Team.SendTeam(Team.ToArray(), true);
                            client.Team.PickupMoney = false;
                        }
                        break;
                    }
                case Mode.LootMoneyOn:
                    {
                        if (client.Team != null)
                        {
                            client.Team.SendTeam(Team.ToArray(), true);
                            client.Team.PickupMoney = true;
                        }
                        break;
                    }
                case Mode.LootItemsOff:
                    {
                        if (client.Team != null)
                        {
                            client.Team.SendTeam(Team.ToArray(), true);
                            client.Team.PickupItems = false;
                        }
                        break;
                    }
                case Mode.LootItemsOn:
                    {
                        if (client.Team != null)
                        {
                            client.Team.SendTeam(Team.ToArray(), true);
                            client.Team.PickupItems = true;
                        }
                        break;
                    }
            }
        }
    }
}
