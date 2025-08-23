using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    public class IPBan
    {
        public static Dictionary<int, string> BannedIPs;
        public static void Load()
        {
            BannedIPs = new Dictionary<int, string>();
            using(var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("bannedips"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    int asInt = reader.ReadInt32("ip_int");
                    BannedIPs.Add(asInt, reader.ReadString("ip"));
                }
            }
        }

        public static bool IsBanned(string ip)
        {
            return BannedIPs.ContainsKey(ip.GetHashCode());
        }

        public static void Unban(string ip)
        {
            BannedIPs.Remove(ip.GetHashCode());
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("bannedips", "ip_int", ip.GetHashCode()).Execute();
        }

        public static void Ban(string ip)
        {
            BannedIPs.Add(ip.GetHashCode(), ip);
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert("bannedips").Insert("ip_int", ip.GetHashCode())
                    .Insert("ip", ip).Execute();
        }
    }
   
}
