using System;
using System.Collections.Generic;

namespace MTA.Database {
    public class NameChange {
        public static void UpdateNames() {
            Dictionary<String, NameChangeC> UPDATE = new Dictionary<string, NameChangeC>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities"))
            using (var r = new MySqlReader(cmd)) {
                String newname = "", name = "";
                UInt32 uid = 0;
                while (r.Read()) {
                    newname = r.ReadString("namechange"); //debug make
                    name = r.ReadString("name");
                    if (newname != "" && newname != " ") {
                        uid = r.ReadUInt32("uid");

                        MySqlCommand cmdupdate = null; //lol i see the problem hold on ,,, hold on what? :$ try now
                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("guilds").Set("Name", newname).Where("Name", name).Execute();

                        // LeaderName is now computed from entities table via LeaderID, so no update needed
                        // LeaderID points to UID which doesn't change, and LeaderName getter will get new name automatically

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("partners").Set("PartnerName", newname).Where("PartnerID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("teamarena").Set("EntityName", newname).Where("EntityID", uid).Execute();

                        if (!UPDATE.ContainsKey(name))
                            UPDATE.Add(name, new NameChangeC() { NewName = newname, OldName = name });
                    }
                }
            }

            if (UPDATE.Count > 0) {
                Console.WriteLine(" [NAME CHANGES]");
            }

            foreach (NameChangeC names in UPDATE.Values) {
                using (var cmdupdate2 = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmdupdate2.Update("entities").Set("name", names.NewName).Set("namechange", "")
                        .Where("name", names.OldName).Execute();
                Console.WriteLine(" -[" + names.OldName + "] : -[" + names.NewName + "]");
            }

            UPDATE.Clear();
        }
    }

    public class NameChangeC {
        public String NewName;
        public String OldName;
    }
}