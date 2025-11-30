using System;

namespace MTA.Database
{
    public class AccountTable
    {
        public enum AccountState : byte
        {
            Player = 10,
            Transfered = 9,
            NotActivated = 8,
            Banned = 7,
            GM = 6,
            DoesntExist = 0
        }

        public string Username;
        public string Password;
        public string Email;
        public string IP;
        public DateTime LastCheck;
        public AccountState State;
        public uint EntityID;
        public int RandomKey;
        public string table = "accounts";
        public bool exists = false;

        public AccountTable(string username, string table = "accounts")
        {
            if (username == null) return;
            this.table = table;
            this.Username = username;
            this.Password = "";
            this.IP = "";
            this.LastCheck = DateTime.Now;
            this.State = AccountState.DoesntExist;
            this.EntityID = 0;

            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(table).Where("Username", username))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    exists = true;
                    this.Password = reader.ReadString("Password");
                    this.IP = reader.ReadString("Ip");
                    this.EntityID = reader.ReadUInt32("EntityID");
                    this.LastCheck = DateTime.FromBinary(reader.ReadInt64("LastCheck"));
                    this.State = (AccountState)reader.ReadInt32("State");
                    this.Email = reader.ReadString("Email");
                }
            }
        }

        public uint GenerateKey(int randomKey = 0)
        {
            if (randomKey == 0)
                RandomKey = Kernel.Random.Next(11, 253) % 100 + 1;
            return (uint)
                (Username.GetHashCode() *
                 Password.GetHashCode() *
                 RandomKey);
        }

        public bool MatchKey(uint key)
        {
            return key == GenerateKey(RandomKey);
        }

        public void SetCurrentIp(string ip)
        {
            IP = ip;
        }

        public void Save()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update(table).Set("Password", Password).Set("Ip", IP).Set("EntityID", EntityID)
                    .Where("Username", Username).Execute();
        }

        public void Insert()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert(table).Insert("Username", Username)
                    .Insert("Password", Password).Insert("State", (int)State)
                    .Execute();
            exists = true;
        }

        public void SaveState()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update(table).Set("State", (int)State)
                    .Where("Username", Username).Execute();
        }

        public void MatrixState(AccountState State)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update(table).Set("State", (byte)State)
                    .Where("Username", Username).Execute();
        }
    }
}
