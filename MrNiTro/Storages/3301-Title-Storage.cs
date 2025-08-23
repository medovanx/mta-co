using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTA.Network.GamePackets
{
    public sealed class TitleStorage 
    {
        public TitleStorage() { }
        public bool Read(byte[] packet)
        {
            using (var memoryStream = new MemoryStream(packet))
            {
                Info = Serializer.DeserializeWithLengthPrefix<TitleStorageProto>(memoryStream, PrefixStyle.Fixed32);
            }
            return true;
        }
        public void Handle(Client.GameState client)
        {
            switch (Info.ActionId)
            {
                case Action.Unequip:
                    if (StorageManager.Wing<bool>(Info.Type, Info.Id))
                    {
                        client.Entity.EquippedWing = 0;
                        client.Entity.WTitles.Update((short)Info.Type, (short)Info.Id, false);
                    }
                    else if (StorageManager.Title<bool>(Info.Type, Info.Id))
                    {
                        client.Entity.EquippedTitle = 0;
                        client.Entity.WTitles.Update((short)Info.Type, (short)Info.Id, false);
                    }
                    client.Send(FinalizeProtoBuf(Info));
                    break;
                case Action.Equip:
                    if (StorageManager.Wing<bool>(Info.Type, Info.Id))
                    {
                        client.Entity.EquippedWing = StorageManager.Wing<int>(Info.Type, Info.Id);
                        client.Entity.WTitles.Update((short)Info.Type, (short)Info.Id, true);
                    }
                    else if (StorageManager.Title<bool>(Info.Type, Info.Id))
                    {
                        client.Entity.EquippedTitle = StorageManager.Title<int>(Info.Type, Info.Id);
                        client.Entity.WTitles.Update((short)Info.Type, (short)Info.Id, true);
                    }

                    Info = new TitleStorageProto()
                    {
                        ActionId = Action.Equip,
                        Points = client.Entity.WTitles.Points,
                        Type = Info.Type,
                        Id = Info.Id,
                        Value = new TitleValue()
                        {
                            Type = Info.Type,
                            Id = Info.Id,
                            Equipped = false
                        }
                    };

                    client.Send(FinalizeProtoBuf(Info));
                    break;
            }
        }
        public void AddTitle(Client.GameState client, short _type, short _id, bool equipped = false)
        {

            if (client.Entity.WTitles == null)
            {
                if (!StorageManager.Data.TryGetValue(client.Entity.UID, out client.Entity.WTitles))
                {
                    client.Entity.WTitles = new WardrobeTitles();
                    client.Entity.WTitles.Id = client.Entity.UID;
                    client.Entity.WTitles.Data = new byte[1];
                    client.Entity.WTitles.Create();
                }
            }
            if (client.Entity.WTitles.Containts(_type, _id)) return;
            var pkt = new TitleStorageProto()
            {
                ActionId = Action.Update,
                Points = client.Entity.WTitles.Points,
                Type = _type,
                Id = _id,
                Value = new TitleValue()
                {
                    Type = _type,
                    Id = _id,
                    Equipped = equipped
                }
            };

            client.Send(FinalizeProtoBuf(pkt));

            client.Entity.WTitles.Points += StorageManager.GetTitlePoints(_type, _id);
            client.Entity.TitlePoints = client.Entity.WTitles.Points;

            client.Entity.WTitles.AddTitle(_type, _id , equipped);

            if (!StorageManager.Data.ContainsKey(client.Entity.WTitles.Id))
                StorageManager.Data.Add(client.Entity.WTitles.Id, client.Entity.WTitles);
            else StorageManager.Data[client.Entity.WTitles.Id] = client.Entity.WTitles;

            if (equipped)
                if (StorageManager.Wing<bool>(_type, _id))
                    client.Entity.EquippedWing = StorageManager.Wing<int>(_type, _id);
                else if (StorageManager.Title<bool>(_type, _id))
                    client.Entity.EquippedTitle = StorageManager.Title<int>(_type, _id);

        }
        public void Login(Client.GameState client)
        {
            if (client.Entity.WTitles == null)
            {
                client.Entity.WTitles = StorageManager.Find(t => t.Id == client.Entity.UID);
            }
            if (client.Entity.WTitles == null || client.Entity.WTitles.Data == null || client.Entity.WTitles.Data.Length == 0) return;
            var myPacketReader = new BinaryReader(new MemoryStream(client.Entity.WTitles.Data));
            var _count = myPacketReader.ReadByte();
            client.Entity.WTitles.Points = 0;
            bool wingEquipped = false;
            bool titleEquipped = false;
            for (var i = 0; i < _count; i++)
            {
                var _type = myPacketReader.ReadInt16();
                var _id = myPacketReader.ReadInt16();
                var _equipped = myPacketReader.ReadBoolean();

                if (_equipped)
                    if (StorageManager.Wing<bool>(_type, _id))
                    {
                        if (wingEquipped)
                            _equipped = false;
                        else
                        {
                            client.Entity.EquippedWing = StorageManager.Wing<int>(_type, _id);
                            wingEquipped = true;
                        }
                    }
                    else if (StorageManager.Title<bool>(_type, _id))
                    {
                        if (titleEquipped)
                            _equipped = false;
                        else
                        {
                            client.Entity.EquippedTitle = StorageManager.Title<int>(_type, _id);
                            titleEquipped = true;
                        }
                    }


                var pkt = new TitleStorageProto()
                {
                    ActionId = Action.Update,
                    Points = client.Entity.WTitles.Points,
                    Type = _type,
                    Id = _id,
                    Value = new TitleValue()
                    {
                        Type = _type,
                        Id = _id,
                        Equipped = _equipped
                    }
                };

                client.Entity.WTitles.Points += StorageManager.GetTitlePoints(_type, _id);


                client.Send(FinalizeProtoBuf(pkt));

            }
            client.Entity.TitlePoints = client.Entity.WTitles.Points;
        }

        public byte[] FinalizeProtoBuf(TitleStorageProto titleStorageProto)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(memoryStream, titleStorageProto, PrefixStyle.Fixed32);
                var pkt = new byte[8 + memoryStream.Length];
                memoryStream.ToArray().CopyTo(pkt, 0);
                Writer.WriteUshort((ushort)memoryStream.Length, 0, pkt);
                Writer.WriteUshort((ushort)3301, 2, pkt);

                return pkt;
            }
        }
        
        
        public TitleStorageProto Info;
        [Flags]
        public enum Action : int
        {
            Update = 0,
            UseTitle = 1,
            RemoveTitle = 3,
            Equip = 4,
            Unequip = 5,
        }
    }
    [ProtoContract]
    public class TitleStorageProto
    {
        [ProtoMember(1, IsRequired = true)]
        public TitleStorage.Action ActionId;
        [ProtoMember(2, IsRequired = true)]
        public int Points;
        [ProtoMember(3, IsRequired = true)]
        public int Type;
        [ProtoMember(4, IsRequired = true)]
        public int Id;
        [ProtoMember(5, IsRequired = true)]
        public TitleValue Value;
    }
    [ProtoContract]
    public class TitleValue
    {
        [ProtoMember(1, IsRequired = true)]
        public int Type;
        [ProtoMember(2, IsRequired = true)]
        public int Id;
        [ProtoMember(3, IsRequired = true)]
        public bool Equipped;
        [ProtoMember(4, IsRequired = true)]
        public int dwParam4;
    }
}
