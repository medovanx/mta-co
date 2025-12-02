using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Database;
using MTA.Client;

namespace MTA.MaTrix
{
    public class AllowedIPs
    {
        public struct AllowedIP
        {
            public uint UID;
            public string Name;
            public string IP;
        }

        public AllowedIPs(Client.GameState client)
        {
            Load(client);
        }
        public AllowedIPs(uint UID)
        {
            Load(UID);
        }

        public Dictionary<string, AllowedIP> Allowed_IPs;
        public void Load(Client.GameState client)
        {
            Allowed_IPs = new Dictionary<string, AllowedIP>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("allowedips").Where("UID", client.Entity.UID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    AllowedIP ip = new AllowedIP();
                    ip.UID = reader.ReadUInt32("UID");
                    ip.Name = reader.ReadString("Name");
                    ip.IP = reader.ReadString("IP");
                    if (!Allowed_IPs.ContainsKey(ip.Name))
                        Allowed_IPs.Add(ip.IP, ip);
                }
            }
        }
        public void Load(uint UID)
        {
            Allowed_IPs = new Dictionary<string, AllowedIP>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("allowedips").Where("UID", UID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    AllowedIP ip = new AllowedIP();
                    ip.UID = reader.ReadUInt32("UID");
                    ip.Name = reader.ReadString("Name");
                    ip.IP = reader.ReadString("IP");
                    if (!Allowed_IPs.ContainsKey(ip.Name))
                        Allowed_IPs.Add(ip.IP, ip);
                }
            }
        }

        public void Add(AllowedIP ip)
        {
            if (!Allowed_IPs.ContainsKey(ip.IP))
            {
                Allowed_IPs.Add(ip.IP, ip);
                using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("allowedips"))
                    cmd.Insert("UID", ip.UID).Insert("Name", ip.Name).
                        Insert("IP", ip.IP)
                         .Execute();
            }
        }
        public void Delete(uint UID, string ip)
        {
            if (Allowed_IPs.ContainsKey(ip))
                Allowed_IPs.Remove(ip);
            new MySqlCommand(MySqlCommandType.DELETE).Delete("allowedips", "UID", UID).Execute();
            foreach (var item in Allowed_IPs.Values)
            {
                try
                {
                    using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("allowedips"))
                        cmd.Insert("UID", item.UID).Insert("Name", item.Name).
                            Insert("IP", item.IP)
                             .Execute();
                }
                catch
                {
                }
            }

        }
    }
}
