// ☺ Created by MTA TQ
// ☺ Copyright © 2013 - 2016 TQ Digital
// ☺ MTA TQ - Project


using System;
using System.IO;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public static class InnerPowerTable
    {

        public enum AtributeType : byte
        {
            MaxHP = 1,
            PAttack = 2,
            MAttack = 3,
            PDefense = 4,
            MDefense = 5,
            FinalPAttack = 6,
            FinalMAttack = 7,
            FinalPDamage = 8,
            FinalMDamage = 9,
            PStrike = 10,
            MStrike = 11,
            Immunity = 12,
            Break = 13,
            Conteraction = 14
        }


        public static uint Count = 0;
        public static Stage[] Stages;
        public class Stage
        {
            public ushort ID;
            public string Name = "";
            public byte NeiGongNum;
            public AtributeType[] SpecialAtributesType;
            public uint[] AtributesValues;
            public NeiGong[] NeiGongAtributes;

            public class NeiGong
            {
                public byte ID;
                public byte MaxLevel;
                public AtributeType[] AtributesType;
                public uint[] AtributesValues;
                public uint[] ProgressNeiGongValue;
                public uint ItemID;
                public uint ReqLev;

                public bool CheckAccount(uint reborn, uint level)
                {
                    byte r_reborn = (byte)(ReqLev / 1000);
                    byte r_level = (byte)(ReqLev % 1000);
                    return reborn >= r_reborn && level >= r_level;
                }
            }
        }

        public static bool GetDBInfo(uint ID, out Stage stage, out Stage.NeiGong gong)
        {
            foreach (var m_stage in Stages)
            {
                foreach (var m_gong in m_stage.NeiGongAtributes)
                {
                    if (m_gong.ID == ID)
                    {
                        stage = m_stage;
                        gong = m_gong;
                        return true;
                    }
                }
            }
            stage = null;
            gong = null;
            return false;
        }


        public static void LoadDBInformation()
        {
            IniFile Reader = new IniFile("database/NeiGongInfo.ini");
            Count = Reader.ReadUInt32("NeiGong", "Num");
            Stages = new Stage[Count];

            for (int x = 1; x <= Count; x++)
            {
                Stage stage = new Stage();
                stage.ID = Reader.ReadUInt16(x.ToString(), "id");
                stage.Name = Reader.ReadString(x.ToString(), "Name", "");
                stage.NeiGongNum = Reader.ReadByte(x.ToString(), "NeiGongNum", 0);
                //----- special atributes type-------------------------
                string aAttriType = Reader.ReadString(x.ToString(), "AttriType", "");
                string[] atr_t = aAttriType.Split('-');
                stage.SpecialAtributesType = new AtributeType[stage.NeiGongNum];
                for (int y = 0; y < atr_t.Length; y++)
                    stage.SpecialAtributesType[y] = (AtributeType)byte.Parse(atr_t[y]);
                //--------------------------------------

                string atributesvalue = Reader.ReadString(x.ToString(), "AttriValue", "");
                stage.AtributesValues = new uint[stage.NeiGongNum];
                string[] a_atrs = atributesvalue.Split('-');
                for (int y = 0; y < a_atrs.Length; y++)
                    stage.AtributesValues[y] = uint.Parse(a_atrs[y]);

                stage.NeiGongAtributes = new Stage.NeiGong[stage.NeiGongNum];

                for (int i = 1; i <= stage.NeiGongNum; i++)
                {
                    string key = x.ToString() + "-" + i.ToString();
                    Stage.NeiGong gong = new Stage.NeiGong();
                    gong.ID = Reader.ReadByte(key, "Type", 0);
                    gong.MaxLevel = Reader.ReadByte(key, "MaxLev", 0);

                    string g_atributetype = Reader.ReadString(key, "AttriType", "");

                    string[] gg_atributes = g_atributetype.Split('-');
                    gong.AtributesType = new AtributeType[gg_atributes.Length];
                    for (int y = 0; y < gg_atributes.Length; y++)
                        gong.AtributesType[y] = (AtributeType)byte.Parse(gg_atributes[y]);

                    string AttriValue = Reader.ReadString(key, "AttriValue", "");

                    string[] g_AttriValue = AttriValue.Split('-');
                    gong.AtributesValues = new uint[g_AttriValue.Length];
                    for (int y = 0; y < g_AttriValue.Length; y++)
                        gong.AtributesValues[y] = uint.Parse(g_AttriValue[y]);

                    string NeiGongValue = Reader.ReadString(key, "NeiGongValue", "");

                    string[] g_NeiGongValue = NeiGongValue.Split('-');
                    gong.ProgressNeiGongValue = new uint[g_NeiGongValue.Length];
                    for (int y = 0; y < g_NeiGongValue.Length; y++)
                        gong.ProgressNeiGongValue[y] = uint.Parse(g_NeiGongValue[y]);

                    gong.ReqLev = Reader.ReadUInt32(key, "ReqLev");
                    gong.ItemID = Reader.ReadUInt32(key, "ReqItemType");


                    stage.NeiGongAtributes[i - 1] = gong;
                }
                Stages[x - 1] = stage;
            }
        }
        public static void Save()
        {

            var dictionary = InnerPower.InnerPowerPolle.Values.ToArray();
            foreach (var item in dictionary)
            {
                var inner = item as InnerPower;
                if (inner == null)
                    continue;
                if (inner.UID == 0)
                    continue;

                try
                {
                    MemoryStream stream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(stream);
                    inner.Serialize(writer);
                    ///////////                    
                    string SQL = "UPDATE `innerpower` SET Powers=@Powers where UID = " + inner.UID + " ;";
                    byte[] rawData = stream.ToArray();
                    using (var conn = DataHolder.MySqlConnection)
                    {
                        conn.Open();
                        using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = SQL;
                            cmd.Parameters.AddWithValue("@Powers", rawData);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        public static void Load()
        {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
                {
                    cmd.Select("innerpower");
                    using (MySqlReader rdr = new MySqlReader(cmd))
                {
                    while (rdr.Read())
                    {
                        uint uid = rdr.ReadUInt32("UID");
                        var name = rdr.ReadString("Name");
                        InnerPower inner = new InnerPower(name, uid);
                        byte[] data = rdr.ReadBlob("Powers");
                        if (data.Length > 0)
                        {
                            using (var stream = new MemoryStream(data))
                            using (var reader = new BinaryReader(stream))
                            {
                                inner.Deserialize(reader);
                            }
                        }
                        InnerPower.InnerPowerPolle.TryAdd(inner.UID, inner);
                        InnerPower.InnerPowerRank.UpdateRank(inner);
                    }
                }
            }
        }
        public static void New(Client.GameState client)
        {
            if (client.Entity.InnerPower == null)
                return;
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("innerpower").Where("UID", client.Entity.UID);
                using (MySqlReader rdr = new MySqlReader(cmd))
                {
                    if (!rdr.Read())
                    {
                        using (var command = new MySqlCommand(MySqlCommandType.INSERT))
                        {
                            command.Insert("innerpower").Insert("UID", client.Entity.UID).Insert("Name", client.Entity.InnerPower.Name);
                            command.Execute();
                        }
                    }
                }
            }
        }

    }
}
