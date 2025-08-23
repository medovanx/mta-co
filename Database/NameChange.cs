using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    public class NameChange
    {
        public static void UpdateNames()
        {
            Dictionary<String, NameChangeC> UPDATE = new Dictionary<string, NameChangeC>();
            using(var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities"))
            using (var r = new MySqlReader(cmd))
            {
                String newname = "", name = "";
                UInt32 uid = 0;
                while (r.Read())
                {
                    newname = r.ReadString("namechange");//debug make
                    name = r.ReadString("name");
                    if (newname != "" && newname != " ")
                    {
                        uid = r.ReadUInt32("uid");

                        MySqlCommand cmdupdate = null;//lol i see the problem hold on ,,, hold on what? :$ try now
                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("apprentice").Set("MentorName", newname).Where("MentorID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("apprentice").Set("ApprenticeName", newname).Where("ApprenticeID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("arena").Set("EntityName", newname).Where("EntityID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("claimitems").Set("OwnerName", newname).Where("OwnerUID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("claimitems").Set("GainerName", newname).Where("GainerUID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("detaineditems").Set("OwnerName", newname).Where("OwnerUID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("detaineditems").Set("GainerName", newname).Where("GainerUID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("enemy").Set("EnemyName", newname).Where("EnemyID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("friends").Set("FriendName", newname).Where("FriendID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("guilds").Set("Name", newname).Where("Name", name).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("guilds").Set("LeaderName", newname).Where("LeaderName", name).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("nobility").Set("EntityName", newname).Where("EntityUID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("partners").Set("PartnerName", newname).Where("PartnerID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("chi").Set("name", newname).Where("uid", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("teamarena").Set("EntityName", newname).Where("EntityID", uid).Execute();

                        if (!UPDATE.ContainsKey(name))
                            UPDATE.Add(name, new NameChangeC() { NewName = newname, OldName = name });
                    }
                }
            }
            if (UPDATE.Count > 0)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(" [NAME CHANGES]");
            }
            foreach (NameChangeC names in UPDATE.Values)
            {
                using(var cmdupdate2 = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmdupdate2.Update("entities").Set("name", names.NewName).Set("namechange", "").Where("name", names.OldName).Execute();
                Console.WriteLine(" -[" + names.OldName + "] : -[" + names.NewName + "]");
                System.Console.ForegroundColor = ConsoleColor.White;//debug
            }
            UPDATE.Clear();
        }
    }
    public class NameChangeC
    {
        public String NewName;
        public String OldName;
    }
}
