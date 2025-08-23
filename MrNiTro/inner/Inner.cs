// ☺ Created by MTA TQ
// ☺ Copyright © 2013 - 2016 TQ Digital
// ☺ MTA TQ - Project


using System;
using System.IO;
using System.Linq;
using System.Text;
using MTA.Network;

namespace MTA.Network.GamePackets
{
    public static class InnerPowerStage
    {
        public enum ActionID : ushort
        {
            UpdateStage = 1,
            UpdateScore = 2
        }

    }
    public class MsgInnerPower
    {
        public enum ActionID : byte
        {
            InfoStage = 0,
            UpdateGong = 3,
            UnlockStage = 4,
            OpenInner = 5,
            TransferInnerPowers = 6,
        }
        public unsafe static void InnerPowerHandler(Client.GameState client, byte[] stream)
        {
            ActionID Action;
            uint dwparam;
            uint dwparam2;

            stream.GetInnerPower(out Action, out dwparam, out dwparam2);

            switch (Action)
            {
                case ActionID.TransferInnerPowers:
                    {
                        if (stream != null)
                        {
                            InnerPower InnerPower;
                            Client.GameState TransferdFrom;
                            uint TheEntityWhoTransferFromUID = BitConverter.ToUInt32(stream, 5);
                            foreach (var inner in InnerPower.InnerPowerPolle.Values)
                            {
                                if (inner.UID == TheEntityWhoTransferFromUID)
                                {
                                    if (Kernel.GamePool.TryGetValue(TheEntityWhoTransferFromUID, out TransferdFrom))
                                    {
                                        if (client.Entity.ConquerPoints >= 270 && client.Entity.Level >= 15 && client.Entity.Reborn >= 2 && TransferdFrom.Entity.Level >= 15 && TransferdFrom.Entity.Reborn >= 2)
                                        {
                                            InnerPower = inner;
                                            InnerPower.Name = client.Entity.Name;
                                            InnerPower.UID = client.Entity.UID;
                                            if (InnerPower.InnerPowerPolle.TryRemove(client.Entity.UID, out client.Entity.InnerPower))
                                            {
                                                if (InnerPower.InnerPowerPolle.TryAdd(client.Entity.UID, InnerPower))
                                                {
                                                    client.Entity.InnerPower = InnerPower;
                                                    client.Entity.ConquerPoints -= 270;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ActionID.OpenInner:
                    {
                        Client.GameState user;
                        if (Kernel.GamePool.TryGetValue(dwparam, out user))
                        {
                            if (user == null)
                                return;
                            if (user.Entity == null)
                                return;
                            if (user.Entity.InnerPower == null)
                                return;
                            client.Send(stream.InnerPowerGui(user.Entity.InnerPower.GetNeiGongs()));
                            var stages = client.Entity.InnerPower.Stages.Where(p => p.Complete);
                            if (stages.Count() > 0)
                                client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, user.Entity.UID, stages.Last()));
                        }
                        break;
                    }
                case ActionID.InfoStage:
                    {

                        Client.GameState user;
                        if (Kernel.GamePool.TryGetValue(dwparam, out user))
                        {
                            client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, user.Entity.UID, user.Entity.InnerPower.Stages[dwparam2 - 1]));
                            client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateScore, user.Entity.UID, user.Entity.InnerPower.Stages[dwparam2 - 1]));
                        }
                        break;
                    }
                case ActionID.UnlockStage:
                    {
                        Database.InnerPowerTable.Stage DBStage = null;
                        Database.InnerPowerTable.Stage.NeiGong DBGong = null;
                        if (Database.InnerPowerTable.GetDBInfo(dwparam, out DBStage, out DBGong))
                        {
                            if (DBGong.CheckAccount(client.Entity.Reborn, client.Entity.Level) && client.Inventory.Contains(DBGong.ItemID, 1)
                                && client.Entity.InnerPower.isUnlockedNeiGong((byte)dwparam))
                            {
                                InnerPower.Stage stage = null;
                                InnerPower.Stage.NeiGong gong = null;
                                if (client.Entity.InnerPower.TryGetStageAndGong((byte)dwparam, out stage, out gong))
                                {
                                    stage.UnLocked = gong.Unlocked = true;

                                    stream.InnerPowerGui(client.Entity.InnerPower.GetNeiGongs());
                                    client.Send(stream);
                                    client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, client.Entity.UID, stage));
                                    client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateScore, client.Entity.UID, stage));

                                    client.Inventory.Remove(DBGong.ItemID, 1);
                                }
                            }
                        }
                        break;
                    }
                case ActionID.UpdateGong:
                    {
                        Database.InnerPowerTable.Stage DBStage = null;
                        Database.InnerPowerTable.Stage.NeiGong DBGong = null;
                        if (Database.InnerPowerTable.GetDBInfo(dwparam, out DBStage, out DBGong))
                        {
                            InnerPower.Stage stage = null;
                            InnerPower.Stage.NeiGong gong = null;
                            if (client.Entity.InnerPower.TryGetStageAndGong((byte)dwparam, out stage, out gong))
                            {
                                if (stage.UnLocked && gong.Unlocked && gong.level < DBGong.MaxLevel)
                                {
                                    int potency_cost = (int)DBGong.ProgressNeiGongValue[Math.Min(gong.level, (int)(DBGong.ProgressNeiGongValue.Length - 1))];
                                    if (client.Entity.InnerPower.Potency >= potency_cost)
                                    {
                                        client.Entity.InnerPower.AddPotency(stream, client, -potency_cost);

                                        gong.level += 1;
                                        gong.Score = (byte)Math.Ceiling(((float)((float)100 / (float)DBGong.MaxLevel) * (float)gong.level));
                                        gong.Complete = gong.level == DBGong.MaxLevel;

                                        client.Send(stream.InnerPowerGui(client.Entity.InnerPower.GetNeiGongs()));
                                        client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, client.Entity.UID, stage));
                                        client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateScore, client.Entity.UID, stage));
                                        client.Entity.InnerPower.UpdateStatus();
                                        client.LoadItemStats();
                                        InnerPower.InnerPowerRank.UpdateRank(client.Entity.InnerPower);
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
    public static class InnerMsg
    {
        public static unsafe void GetInnerPower(this byte[] packet, out MsgInnerPower.ActionID mode, out uint dwparam, out uint dwparam2)
        {
            mode = (MsgInnerPower.ActionID)packet[4];
            dwparam = BitConverter.ToUInt32(packet, 5);
            dwparam2 = BitConverter.ToUInt32(packet, 9);
        }

        public static unsafe byte[] InnerPowerGui(this byte[] packet, InnerPower.Stage.NeiGong[] gongs)
        {
            packet = new byte[8 + 6 + (ushort)gongs.Length * 5];
            Writer.WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            Writer.WriteUInt16(2612, 2, packet);
            if (gongs != null)
            {
                Writer.WriteUInt16((ushort)gongs.Length, 4, packet);
                int offset = 6;
                for (int x = 0; x < gongs.Length; x++)
                {
                    var element = gongs[x];
                    Writer.WriteByte((byte)element.ID, offset, packet);
                    offset += 1;
                    Writer.WriteUInt32((uint)element.Score, offset, packet);
                    offset += 4;
                }
            }
            return packet;
        }

        public static unsafe byte[] InnerPowerStageInfo(this byte[] packet, InnerPowerStage.ActionID action, uint UID, InnerPower.Stage stage)
        {
            var array_gongs = stage.NeiGongs.Where(p => p.Unlocked).ToArray();
            var DBStage = Database.InnerPowerTable.Stages[stage.ID - 1];

            MemoryStream strm = new MemoryStream();
            BinaryWriter stream = new BinaryWriter(strm);
            stream.Write((ushort)0);
            stream.Write((ushort)2611);
            stream.Write(UID);
            stream.Write((uint)stage.Score);
            stream.Write(0);
            stream.Write((ushort)action);
            stream.Write((ushort)array_gongs.Length);
            stream.Write(stage.GetNoumberAtributes(array_gongs));
            for (int x = 0; x < array_gongs.Length; x++)
            {
                var element = array_gongs[x];
                stream.Write((ushort)element.ID);
                stream.Write(element.level);
                stream.Write(element.Score);
                stream.Write((byte)(element.Complete ? 1 : 0));
            }

            for (int x = 0; x < array_gongs.Length; x++)
            {
                var element = array_gongs[x];
                for (int y = 0; y < DBStage.NeiGongAtributes[x].AtributesType.Length; y++)
                {
                    stream.Write((ushort)element.ID);
                    var atribut = DBStage.NeiGongAtributes[x];
                    stream.Write((byte)atribut.AtributesType[y]);
                    stream.Write((uint)((atribut.AtributesValues[y] / atribut.MaxLevel) * element.level));
                }
            }
            int packetlength = (int)strm.Length;
            strm.Position = 0;
            stream.Write((ushort)packetlength);
            strm.Position = strm.Length;
            stream.Write(Encoding.Default.GetBytes("TQServer"));
            strm.Position = 0;
            byte[] buf = new byte[strm.Length];
            strm.Read(buf, 0, buf.Length);
            stream.Close();
            strm.Close();
            return buf;
        }
    }
}
