using MTA.Database;
using System;
using MTA.Game;

namespace MTA.Network.GamePackets
{
    public class SobNpcSpawn : Writer, Interfaces.IPacket, Interfaces.INpc, Interfaces.ISobNpc, Interfaces.IMapObject
    {
        private byte[] Buffer;
     //   public byte[] SpawnPacket;
        public _String Effect { get; set; }

        public SobNpcSpawn()
        {
            Buffer = new byte[90];
            WriteUInt16(82, 0, Buffer);
            WriteUInt16(1109, 2, Buffer);
            ShowName = false;
        }

        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }

        public uint MaxHitpoints
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public uint Hitpoints
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set
            {
                WriteUInt32(value, 16, Buffer);
            }
        }

        public ushort X
        {
            get { return BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt16(value, 20, Buffer); }
        }

        public ushort Y
        {
            get { return BitConverter.ToUInt16(Buffer, 22); }
            set { WriteUInt16(value, 22, Buffer); }
        }

        public ushort Mesh
        {
            get { return BitConverter.ToUInt16(Buffer, 24); }
            set { WriteUInt16(value, 24, Buffer); }
        }

        public MTA.Game.Enums.NpcType Type
        {
            get { return (MTA.Game.Enums.NpcType)Buffer[26]; }
            set { Buffer[26] = (byte)value; }
        }

        public ushort Sort
        {
            get { return BitConverter.ToUInt16(Buffer, 28); }
            set { WriteUInt16(value, 28, Buffer); }
        }

        public bool SpawnOnMinutes
        {
            get;
            set;
        }
        public byte BeforeHour
        {
            get;
            set;
        }

        public bool ShowName
        {
            get { return Buffer[40] == 1; }
            set { Buffer[40] = value == true ? (byte)1 : (byte)0; }
        }

        public string LoweredName;
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                LoweredName = value.ToLower();
                byte[] buffer = new byte[90];
                Buffer.CopyTo(buffer, 0);
                WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                WriteStringWithLength(value, 41, buffer);
                Buffer = buffer;
            }
        }

        public ushort MapID { get; set; }

        public MTA.Game.MapObjectType MapObjType
        {
            get
            {
                if (MaxHitpoints == 0)
                    return MTA.Game.MapObjectType.Npc;
                return MTA.Game.MapObjectType.SobNpc;
            }
        }

        public void Die(Game.Entity killer)
        {
if (MapID == 2078)
            {
                if (UID != 819)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
            if (MapID == 2077)
            {
                if (UID != 817)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
            if (MapID == 2076)
            {
                if (UID != 816)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
            if (MapID == 2075)
            {
                if (UID != 815)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
            if (MapID == 2072)
            {
                if (UID != 818)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
           if (MapID == 1038 || MapID == 2071)
            {
                if (UID != 810 || UID != 811)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
           if (MapID == CaptureTheFlag.MapID)
           {
               if (Program.World.CTF.Bases.ContainsKey(UID))
               {
                   var _base = Program.World.CTF.Bases[UID];
                   _base.Capture();
               }
           }
            else if (MapID == 8175)
            {
                if (UID != 810)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }

            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
            if (UID == 123456)
            {
                Hitpoints = 0;
                Program.World.PoleDomination.KillPole();
                return;
            }
            if (MapID == CaptureTheFlag.MapID)
            {
                if (Program.World.CTF.Bases.ContainsKey(UID))
                {
                    var _base = Program.World.CTF.Bases[UID];
                    _base.Capture();
                }
            }
                //////////
            else if (MapID == 5002)
            {
                if (UID != 8185)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else if (MapID == 5002)
            {
                if (UID != 8186)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else if (MapID == 5002)
            {
                if (UID != 8187)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else if (MapID == 5002)
            {
                if (UID != 8188)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else if (MapID == 5002)
            {
                if (UID != 8189)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
                ////////////////
            else if (MapID == 1038)
            {
                if (UID != 810)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else if (MapID == 10380)
            {
                if (UID != 8100)
                {
                    if (Hitpoints != 0 || Mesh != 251 && Mesh != 281)
                    {
                        if (Mesh == 241)
                            Mesh = (ushort)(250 + Mesh % 10);
                        else
                            Mesh = (ushort)(280 + Mesh % 10);

                        Update upd = new Update(true);
                        upd.UID = UID;
                        upd.Append(Update.Mesh, Mesh);
                        killer.Owner.SendScreen(upd, true);
                        Hitpoints = 0;
                    }
                    Attack attack = new Attack(true);
                    attack.Attacker = killer.UID;
                    attack.Attacked = UID;
                    attack.AttackType = Network.GamePackets.Attack.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    killer.Owner.Send(attack);
                    killer.KOCount++;
                }
            }
            else
            {
                Attack attack = new Attack(true);
                attack.Attacker = killer.UID;
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                killer.Owner.Send(attack);
                Hitpoints = MaxHitpoints;
                Update upd = new Update(true);
                upd.UID = UID;
                upd.Append(Update.Hitpoints, MaxHitpoints);
                killer.Owner.SendScreen(upd, true);
            }
            if (Kernel.Maps[1038].Statues.ContainsKey(UID))
            {
                Kernel.Maps[1038].Statues.Remove(UID);
                Kernel.Maps[1038].Npcs.Remove(UID);
                killer.Owner.Screen.FullWipe();
                killer.Owner.Screen.Reload(null);
            }
        }
        private Client.GameState owner_null = null;
        public bool _isprize;
        public Client.GameState Owner
        {
            get
            {
                return owner_null;
            }
            set
            {
                owner_null = value;
            }
        }
        public byte[] SpawnPacket;
        public uint GuildRank;
        public byte Class;
        public void SendSpawn(Client.GameState client, bool checkScreen)
        {
            if (client.Screen.Add(this) || !checkScreen)
            {
                if (Kernel.Maps[MapID].Statues.ContainsKey(UID))
                {
                    Writer.WriteUshort(Mesh, Entity._Facing, SpawnPacket);
                    Writer.WriteUshort((ushort)Type, Entity._Action, SpawnPacket);
                    Writer.WriteUshort(X, Entity._X, SpawnPacket);
                    Writer.WriteUshort(Y, Entity._Y, SpawnPacket);
                    Writer.WriteUInt32(Hitpoints, Entity._Hitpoints, SpawnPacket);
                    Writer.WriteUInt32(UID, Entity._UID, SpawnPacket);
                    client.Send(SpawnPacket);
                    return;
                }
                client.Send(Buffer);
                if (effect != "" && effect != null)
                {
                    client.SendScreen(new _String(true)
                    {
                        UID = UID,
                        TextsCount = 22,
                        Type = 10,
                        Texts = { effect }
                    }, true);
                }
            }
        }
        public string effect
        {
            get;
            set;
        }
        public void SendSpawn(Client.GameState client)
        {
            SendSpawn(client, false);
            if (effect != "" && effect != null)
            {
                client.SendScreen(new _String(true)
                {
                    UID = UID,
                    TextsCount = 22,
                    Type = 10,
                    Texts = { effect }
                }, true);
            }
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            SendSpawn(client, false);
        }
    }
}
