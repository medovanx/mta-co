using MTA.Game;

namespace MTA.Database
{
    public class SubClassTable
    {
        public static void Load(Entity Entity)
        {
            using MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.SELECT).Select("subclasses").Where("id", (long)(ulong)Entity.UID);
            using MySqlReader mySqlReader = mySqlCommand.CreateReader();
            while (mySqlReader.Read())
            {
                SubClass subClass = new SubClass
                {
                    ID = mySqlReader.ReadByte("Uid"),
                    Level = mySqlReader.ReadByte("Level"),
                    Phase = mySqlReader.ReadByte("Phase")
                };
                Entity.SubClasses.Classes.Add(subClass.ID, subClass);
                Entity.SubClasses.SendLearn((ClassID)subClass.ID, subClass.Level, Entity.Owner);
            }
        }

        public static bool Contains(Entity Entity, byte id)
        {
            bool result = false;
            using (MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.SELECT))
            {
                mySqlCommand.Select("subclasses").Where("id", (long)(ulong)Entity.UID).And("uid", (long)(ulong)id);
                using MySqlReader mySqlReader = mySqlCommand.CreateReader();
                if (mySqlReader.Read() && mySqlReader.ReadByte("uid") == id)
                {
                    result = true;
                }
            }
            return result;
        }

        public static void Insert(Entity Entity, byte id)
        {
            using (MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.INSERT))
            {
                mySqlCommand.Insert("subclasses")
                    .Insert("uid", (long)(ulong)id)
                    .Insert("id", (long)(ulong)Entity.UID)
                    .Insert("Level", 1)
                    .Insert("Phase", 1)
                    .Execute();
            }
        }

        public static void Update(Entity Entity, Game.SubClass SubClass)
        {
            using MySqlCommand mySqlCommand = new MySqlCommand(MySqlCommandType.UPDATE);
            mySqlCommand.Update("subclasses")
            .Set("phase", (long)(ulong)SubClass.Phase)
            .Set("level", (long)(ulong)SubClass.Level)
            .Where("id", (long)(ulong)Entity.UID)
            .And("uid", (long)(ulong)SubClass.ID)
            .Execute();
        }

        public static void Update(Client.GameState client)
        {
            EntityTable.UpdateData(client, "StudyPoints", client.Entity.SubClasses.StudyPoints);
        }
    }
}