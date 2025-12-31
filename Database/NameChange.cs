using System.Collections.Generic;

namespace MTA.Database {
    public class NameChange {
        public static void UpdateNames() {
            var update = new Dictionary<string, NameChangeC>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities"))
            using (var r = new MySqlReader(cmd)) {
                while (r.Read()) {
                    var newName = r.ReadString("namechange");
                    var name = r.ReadString("name");
                    if (newName is "" or " ") continue;
                    var uid = r.ReadUInt32("uid");

                    var sqlCommand = new MySqlCommand(MySqlCommandType.UPDATE);
                    sqlCommand.Update("guilds").Set("Name", newName).Where("Name", name).Execute();

                    // LeaderName is now computed from entities table via LeaderID, so no update needed
                    // LeaderID points to UID which doesn't change, and LeaderName getter will get new name automatically
                    sqlCommand = new MySqlCommand(MySqlCommandType.UPDATE);
                    sqlCommand.Update("partners").Set("PartnerName", newName).Where("PartnerID", uid).Execute();

                    sqlCommand = new MySqlCommand(MySqlCommandType.UPDATE);
                    sqlCommand.Update("teamarena").Set("EntityName", newName).Where("EntityID", uid).Execute();

                    if (!update.ContainsKey(name))
                        update.Add(name, new NameChangeC() { NewName = newName, OldName = name });
                }
            }

            if (update.Count > 0) {
                Console.WriteLine(" [NAME CHANGES]");
            }

            foreach (var names in update.Values) {
                using (var sqlCommand = new MySqlCommand(MySqlCommandType.UPDATE))
                    sqlCommand.Update("entities").Set("name", names.NewName).Set("namechange", "")
                        .Where("name", names.OldName).Execute();
                Console.WriteLine(" -[" + names.OldName + "] : -[" + names.NewName + "]");
            }

            update.Clear();
        }
    }

    public class NameChangeC {
        public required string NewName;
        public required string OldName;
    }
}