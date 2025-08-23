using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Database;

namespace MTA.Game
{
    class guildtop
    {
        public static uint GuildLeader, DeputyLeader;
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guildtop"))
            using (var r = new Database.MySqlReader(cmd))
                if (r.Read())
                {
                    GuildLeader = r.ReadUInt32("GuildLeader");
                    DeputyLeader = r.ReadUInt32("DeputyLeader");
                }
        }
        public static void update()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("guildtop").Set("GuildLeader", GuildLeader).Set("DeputyLeader", DeputyLeader).Execute();
        }
        public static void Rest()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("guildtop").Set("GuildLeader", 1).Set("DeputyLeader", 25).Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("entities").Set("DeputyLeader", 0).Execute();
        }
    }
}
