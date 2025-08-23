using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class SubClassTable
    {
        public static void Load(Entity Entity)
        {
            using (MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.SELECT).Select("subclasses").Where("id", (long)((ulong)Entity.UID)))
            {
                using (MySqlReader mySqlReader = mySqlCommand.CreateReader())
                {
                    while (mySqlReader.Read())
                    {
                        Game.SubClass subClass = new Game.SubClass();
                        subClass.ID = mySqlReader.ReadByte("Uid");
                        subClass.Level = mySqlReader.ReadByte("Level");
                        subClass.Phase = mySqlReader.ReadByte("Phase");
                        Entity.SubClasses.Classes.Add(subClass.ID, subClass);
                        Entity.SubClasses.SendLearn((ClassID)subClass.ID, subClass.Level, Entity.Owner);
                    }
                }
            }
        }
        public static bool Contains(Entity Entity, byte id)
        {
            bool result = false;
            using (MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.SELECT))
            {
                mySqlCommand.Select("subclasses").Where("id", (long)((ulong)Entity.UID)).And("uid", (long)((ulong)id));
                using (MySqlReader mySqlReader = mySqlCommand.CreateReader())
                {
                    if (mySqlReader.Read() && mySqlReader.ReadByte("uid") == id)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        public static void Insert(Entity Entity, byte id)
        {
            using (MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.INSERT))
            {
                mySqlCommand.Insert("subclasses").Insert("uid", (long)((ulong)id)).Insert("id", (long)((ulong)Entity.UID)).Execute();
            }
        }
        public static void Update(Entity Entity, Game.SubClass SubClass)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.UPDATE);
            mySqlCommand.Update("subclasses").Set("phase", (long)((ulong)SubClass.Phase)).Set("level", (long)((ulong)SubClass.Level)).Where("id", (long)((ulong)Entity.UID)).And("uid", (long)((ulong)SubClass.ID)).Execute();
        }
        public static void Update(Client.GameState client)
        {
            EntityTable.UpdateData(client, "StudyPoints", client.Entity.SubClasses.StudyPoints);
        }
    }
}