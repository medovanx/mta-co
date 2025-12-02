namespace MTA.Database
{
    using MTA.Client;
    using MTA.Interfaces;
    using MTA.Network.GamePackets;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;

    public class SkillTable
    {
        #region Blob

        #region Main
        public void GetProf(System.IO.BinaryReader reader, out List<Network.GamePackets.Proficiency> profscolletion)
        {
            profscolletion = new List<Network.GamePackets.Proficiency>();
            uint count = reader.ReadUInt32();
            for (uint x = 0; x < count; x++)
            {
                Network.GamePackets.Proficiency proficiency = new Network.GamePackets.Proficiency(true);
                proficiency.ID = reader.ReadUInt16();
                proficiency.Level = reader.ReadByte();
                proficiency.PreviousLevel = reader.ReadByte();
                proficiency.Experience = reader.ReadUInt32();
                proficiency.NeededExperience = Database.DataHolder.ProficiencyLevelExperience(proficiency.Level);
                proficiency.Available = true;
                profscolletion.Add(proficiency);
            }
        }
        public void GetSkill(System.IO.BinaryReader reader, out List<Interfaces.ISkill> spellscollection)
        {
            spellscollection = new List<Interfaces.ISkill>();
            uint count = reader.ReadUInt32();
            for (uint x = 0; x < count; x++)
            {
                Interfaces.ISkill spell = new Network.GamePackets.Spell(true);
                spell.ID = reader.ReadUInt16();
                spell.Level = reader.ReadByte();
                spell.PreviousLevel = reader.ReadByte();
                spell.Experience = reader.ReadUInt32();
                spell.Souls = (Spell.Soul_Level)reader.ReadByte();
                spell.Available = true;
                spellscollection.Add(spell);
            }
        }

        #endregion

        #region other
        public static void WriteProf(System.IO.BinaryWriter writer, Interfaces.IProf proficiency)
        {
            writer.Write(proficiency.ID);
            writer.Write(proficiency.Level);
            writer.Write(proficiency.PreviousLevel);
            writer.Write(proficiency.Experience);

        }
        public static byte[] GetArrayProfs(Client.GameState client)
        {
            uint count = (uint)client.Proficiencies.Count;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
            writer.Write(count);
            if (client.Proficiencies != null)
            {
                foreach (var prof in client.Proficiencies.Values)
                    WriteProf(writer, prof);
            }
            return stream.ToArray();
        }
        public static byte[] GetSpellsArray(Client.GameState client)
        {
            uint count = (uint)client.Spells.Count;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
            writer.Write(count);
            foreach (var skill in client.Spells.Values)
                WriteSkill(writer, skill);
            return stream.ToArray();
        }
        public static void WriteSkill(System.IO.BinaryWriter writer, Interfaces.ISkill skill)
        {
            writer.Write(skill.ID);
            writer.Write(skill.Level);
            writer.Write(skill.PreviousLevel);
            writer.Write(skill.Experience);
            writer.Write((byte)skill.Souls);
        }

        #endregion

        #endregion
        public static void removeAllSkills(Client.GameState client)
        {
            using (var conn = DataHolder.MySqlConnection)
            {
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("Delete from skills where EntityID =" + client.Entity.UID.ToString(), conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void removeAllProfs(Client.GameState client)
        {
            using (var conn = DataHolder.MySqlConnection)
            {
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("Delete from profs where EntityID =" + client.Entity.UID.ToString(), conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteSpell(GameState client, ushort ID)
        {
            MTA.Database.MySqlCommand command = new MTA.Database.MySqlCommand(MySqlCommandType.DELETE);
            command.Delete("skills", "ID", (long)ID).And("EntityID", (long)client.Entity.UID).Execute();
        }

        public static void LoadProficiencies(GameState client)
        {
            if (client.Fake) return;
            if (client.TransferedPlayer) return;
            if (client.Entity != null)
            {
                client.Proficiencies = new SafeDictionary<ushort, IProf>(100);
                MySqlReader reader = new MySqlReader(new MTA.Database.MySqlCommand(MySqlCommandType.SELECT).Select("profs").Where("EntityID", (long)client.Entity.UID));
                while (reader.Read())
                {
                    IProf prof = new Proficiency(true)
                    {
                        ID = reader.ReadUInt16("ID"),
                        Level = reader.ReadByte("Level"),
                        PreviousLevel = reader.ReadByte("PreviousLevel"),
                        Experience = reader.ReadUInt32("Experience"),
                        Available = true
                    };
                    if (!client.Proficiencies.ContainsKey(prof.ID))
                    {
                        client.Proficiencies.Add(prof.ID, prof);
                    }
                }
            }
        }

        public static void LoadSpells(GameState client)
        {
            if (client.Fake) return;
            if (client.TransferedPlayer) return;
            if (client.Entity != null)
            {
                client.Spells = new SafeDictionary<ushort, ISkill>(100);
                MySqlReader reader = new MySqlReader(new MTA.Database.MySqlCommand(MySqlCommandType.SELECT).Select("skills").Where("EntityID", (long)client.Entity.UID));
                while (reader.Read())
                {
                    ISkill skill = new Spell(true)
                    {
                        ID = reader.ReadUInt16("ID"),
                        Level = reader.ReadByte("Level"),
                        PreviousLevel = reader.ReadByte("PreviousLevel"),
                        Experience = reader.ReadUInt32("Experience"),
                        Souls = (Spell.Soul_Level)reader.ReadByte("LevelHu"),
                        LevelHu2 = reader.ReadByte("LevelHu2"),
                        Available = true
                    };
                    if (!client.Spells.ContainsKey(skill.ID))
                    {
                        client.Spells.Add(skill.ID, skill);
                    }
                }
            }
        }

        public static void SaveProficiencies(GameState client)
        {
            if (client.Fake) return;
            if (client.TransferedPlayer) return;
            if ((client.Entity != null) && (client.Proficiencies != null) && (client.Proficiencies.Count != 0))
            {
                foreach (IProf prof in client.Proficiencies.Values)
                {
                    try
                    {
                        // Check if proficiency exists in database first
                        bool exists = false;
                        using (var reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT)
                            .Select("profs")
                            .Where("EntityID", (long)client.Entity.UID)
                            .And("ID", (long)prof.ID)))
                        {
                            exists = reader.Read();
                        }

                        MTA.Database.MySqlCommand command;
                        if (exists || prof.Available)
                        {
                            // UPDATE existing proficiency
                            command = new MTA.Database.MySqlCommand(MySqlCommandType.UPDATE);
                            command.Update("profs")
                                .Set("Level", (long)prof.Level)
                                .Set("PreviousLevel", (long)prof.PreviousLevel)
                                .Set("Experience", (long)prof.Experience)
                                .Where("EntityID", (long)client.Entity.UID)
                                .And("ID", (long)prof.ID)
                                .Execute();
                            prof.Available = true;
                        }
                        else
                        {
                            // INSERT new proficiency
                            command = new MTA.Database.MySqlCommand(MySqlCommandType.INSERT);
                            command.Insert("profs")
                                .Insert("ID", (long)prof.ID)
                                .Insert("EntityID", (long)client.Entity.UID)
                                .Insert("Level", (long)prof.Level)
                                .Insert("PreviousLevel", (long)prof.Level)
                                .Insert("Experience", (long)prof.Experience)
                                .Execute();
                            prof.Available = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving proficiency {prof.ID} for entity {client.Entity.UID}: {ex.Message}");
                    }
                }
            }
        }

        public static void SaveSpells(GameState client)
        {
            if (client.Fake) return;
            if (client.TransferedPlayer) return;
            if ((client.Entity != null) && (client.Spells != null) && (client.Spells.Count != 0))
            {
                foreach (ISkill skill in client.Spells.Values)
                {
                    try
                    {
                        // Check if skill exists in database first
                        bool exists = false;
                        using (var reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT)
                            .Select("skills")
                            .Where("EntityID", (long)client.Entity.UID)
                            .And("ID", (long)skill.ID)))
                        {
                            exists = reader.Read();
                        }

                        Database.MySqlCommand command;
                        if (exists || skill.Available)
                        {
                            // UPDATE existing skill
                            command = new Database.MySqlCommand(MySqlCommandType.UPDATE);
                            command.Update("skills")
                                .Set("Level", (long)skill.Level)
                                .Set("PreviousLevel", (long)skill.PreviousLevel)
                                .Set("Experience", (long)skill.Experience)
                                .Set("LevelHu", (long)skill.Souls)
                                .Set("LevelHu2", skill.LevelHu2)
                                .Where("EntityID", (long)client.Entity.UID)
                                .And("ID", (long)skill.ID)
                                .Execute();
                            skill.Available = true;
                        }
                        else
                        {
                            // INSERT new skill
                            command = new Database.MySqlCommand(MySqlCommandType.INSERT);
                            command.Insert("skills")
                                .Insert("EntityID", (long)client.Entity.UID)
                                .Insert("ID", (long)skill.ID)
                                .Insert("Level", (long)skill.Level)
                                .Insert("Experience", (long)skill.Experience)
                                .Insert("PreviousLevel", (long)skill.Level)
                                .Insert("TempLevel", 0)
                                .Insert("LevelHu", (long)skill.Souls)
                                .Insert("LevelHu2", skill.LevelHu2)
                                .Execute();
                            skill.Available = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving skill {skill.ID} for entity {client.Entity.UID}: {ex.Message}");
                    }
                }
            }
        }

    }
}



