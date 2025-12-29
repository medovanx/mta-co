using MTA.Game.ConquerStructures;

namespace MTA.Database {
    public abstract class NobilityTable {
        public static void Load() {
            Console.WriteLine("Loading Nobility information...");
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("nobility");
            cmd.Command = cmd.Command.Replace("SELECT * FROM `nobility`",
                "SELECT n.EntityUID, n.Donation, n.Gender, n.Mesh, e.Name FROM `nobility` n INNER JOIN `entities` e ON n.EntityUID = e.UID");
            using var reader = cmd.CreateReader();
            while (reader.Read()) {
                var nobilityInformation = new NobilityInformation {
                    EntityUID = reader.ReadUInt32("EntityUID"),
                    Name = reader.ReadString("Name"),
                    Donation = reader.ReadUInt64("Donation"),
                    Gender = reader.ReadByte("Gender"),
                    Mesh = reader.ReadUInt32("Mesh")
                };
                Nobility.Board.Add(nobilityInformation.EntityUID, nobilityInformation);
            }

            Nobility.Sort(0);
        }

        public static void InsertNobilityInformation(NobilityInformation information) {
            using var cmd = new MySqlCommand(MySqlCommandType.INSERT);
            cmd.Insert("nobility")
                .Insert("EntityUID", information.EntityUID).Insert("donation", information.Donation)
                .Insert("gender", information.Gender).Insert("mesh", information.Mesh)
                .Execute();
        }

        public static void UpdateNobilityInformation(NobilityInformation information) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("nobility");
            cmd.Set("donation", information.Donation).Where("EntityUID", information.EntityUID)
                .Execute();
        }
    }
}