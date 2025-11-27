using System;
using System.Collections.Generic;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Database
{
    public class KnownPersons
    {
        public static void LoadEnemy(Client.GameState client)
        {
            client.Enemy = new SafeDictionary<uint, Enemy>(50);
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("enemy").Where("entityid", client.Entity.UID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    Enemy enemy = new Enemy();
                    enemy.ID = reader.ReadUInt32("EnemyID");
                    enemy.Name = reader.ReadString("EnemyName");
                    client.Enemy.Add(enemy.ID, enemy);
                }
            }
        }
        public static void LoadPartner(Client.GameState client)
        {
            client.Partners = new SafeDictionary<uint, TradePartner>(40);
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("partners").Where("entityid", client.Entity.UID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    TradePartner person = new TradePartner();
                    person.ID = reader.ReadUInt32("PartnerID");
                    person.Name = reader.ReadString("PartnerName");
                    person.ProbationStartedOn = DateTime.FromBinary(reader.ReadInt64("ProbationStartedOn"));
                    client.Partners.Add(person.ID, person);
                }
            }
        }
        public static void LoadMentor(Client.GameState client)
        {
            client.Apprentices = new SafeDictionary<uint, Apprentice>(10);
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("apprentice").Where("MentorID", client.Entity.UID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    Apprentice app = new Apprentice();
                    app.ID = reader.ReadUInt32("ApprenticeID");
                    app.Name = reader.ReadString("ApprenticeName");
                    app.EnroleDate = reader.ReadUInt32("EnroleDate");
                    app.Actual_Experience = reader.ReadUInt64("Actual_Experience");
                    app.Total_Experience = reader.ReadUInt64("Total_Experience");
                    app.Actual_Plus = reader.ReadUInt16("Actual_Plus");
                    app.Total_Plus = reader.ReadUInt16("Total_Plus");
                    app.Actual_HeavenBlessing = reader.ReadUInt16("Actual_HeavenBlessing");
                    app.Total_HeavenBlessing = reader.ReadUInt16("Total_HeavenBlessing");
                    client.PrizeExperience += app.Actual_Experience;
                    client.PrizePlusStone += app.Actual_Plus;
                    client.PrizeHeavenBlessing += app.Actual_HeavenBlessing;
                    client.Apprentices.Add(app.ID, app);
                    client.apprtnum += 1;

                    if (client.PrizeExperience > 50 * 606)
                        client.PrizeExperience = 50 * 606;
                }
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("apprentice").Where("ApprenticeID", client.Entity.UID))
            using (var reader = cmd.CreateReader())
            {
                if (reader.Read())
                {
                    client.Mentor = new Mentor();
                    client.Mentor.ID = reader.ReadUInt32("MentorID");
                    client.Mentor.Name = reader.ReadString("MentorName");
                    client.Mentor.EnroleDate = reader.ReadUInt32("EnroleDate");
                    client.AsApprentice = new MTA.Game.ConquerStructures.Society.Apprentice();
                    client.AsApprentice.ID = client.Entity.UID;
                    client.AsApprentice.Name = client.Entity.Name;
                    client.AsApprentice.EnroleDate = client.Mentor.EnroleDate;
                    client.AsApprentice.Actual_Experience = reader.ReadUInt64("Actual_Experience");
                    client.AsApprentice.Total_Experience = reader.ReadUInt64("Total_Experience");
                    client.AsApprentice.Actual_Plus = reader.ReadUInt16("Actual_Plus");
                    client.AsApprentice.Total_Plus = reader.ReadUInt16("Total_Plus");
                    client.AsApprentice.Actual_HeavenBlessing = reader.ReadUInt16("Actual_HeavenBlessing");
                    client.AsApprentice.Total_HeavenBlessing = reader.ReadUInt16("Total_HeavenBlessing");
                }
            }
        }
        public static void LoaderFriends(Client.GameState client)
        {
            client.Friends = new SafeDictionary<uint, Friend>(50);
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("friends").Where("EntityID", client.Entity.UID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    Friend person = new Friend();
                    person.ID = reader.ReadUInt32("FriendID");
                    person.Name = reader.ReadString("FriendName");
                    person.Message = reader.ReadString("Message");
                    client.Friends.Add(person.ID, person);
                }
            }
        }
        public static void SaveApprenticeInfo(MTA.Game.ConquerStructures.Society.Apprentice app)
        {
            if (app != null)
            {
                using (var mysqlcmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    mysqlcmd.Update("apprentice")
                    .Set("Actual_Experience", app.Actual_Experience.ToString())
                    .Set("Total_Experience", app.Total_Experience.ToString())
                    .Set("Actual_Plus", app.Actual_Plus.ToString())
                    .Set("Total_Plus", app.Total_Plus.ToString())
                    .Set("Actual_HeavenBlessing", app.Actual_HeavenBlessing.ToString())
                    .Set("Total_HeavenBlessing", app.Total_HeavenBlessing.ToString()).Where("ApprenticeID", app.ID).Execute();
            }
        }
        public static void AddMentor(Mentor mentor, MTA.Game.ConquerStructures.Society.Apprentice appr)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert("apprentice").Insert("mentorid", mentor.ID).Insert("apprenticeid", appr.ID)
                    .Insert("mentorname", mentor.Name).Insert("apprenticename", appr.Name)
                    .Insert("enroledate", appr.EnroleDate).Execute();
        }
        public static void RemoveMentor(uint apprenticeuid)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("apprentice", "apprenticeid", apprenticeuid).Execute();
        }
        public static void RemoveApprentice(Client.GameState client, uint apprenticeID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("apprentice", "apprenticeid", apprenticeID).Execute();
        }
        public static void RemoveFriend(Client.GameState client, uint friendID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("friends", "friendid", friendID).And("entityid", client.Entity.UID).Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("friends", "entityid", friendID).And("friendid", client.Entity.UID).Execute();
        }
        public static void RemovePartner(Client.GameState client, uint partnerID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("partners", "entityid", partnerID).And("partnerid", client.Entity.UID).Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("partners", "partnerid", partnerID).And("entityid", client.Entity.UID).Execute();
        }
        public static void RemoveEnemy(Client.GameState client, uint enemyID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("enemy", "enemyid", enemyID).And("entityid", client.Entity.UID).Execute();
        }
        public static void AddFriend(Client.GameState client, Friend friend)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("friends"))
                cmd.Insert("friendid", friend.ID).Insert("entityid", client.Entity.UID).
                    Insert("friendname", friend.Name).Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("friends"))
                cmd.Insert("entityid", friend.ID).Insert("friendid", client.Entity.UID).
                    Insert("friendname", client.Entity.Name).Execute();
        }
        public static void AddPartner(Client.GameState client, TradePartner partner)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert("partners").Insert("entityid", client.Entity.UID).Insert("partnerid", partner.ID)
                    .Insert("partnername", partner.Name).Insert("probationstartedon", partner.ProbationStartedOn.Ticks).Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert("partners").Insert("entityid", partner.ID).Insert("partnerid", client.Entity.UID)
                    .Insert("partnername", client.Entity.Name).Insert("probationstartedon", partner.ProbationStartedOn.Ticks).Execute();
        }
        public static void AddEnemy(Client.GameState client, Enemy enemy)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert("enemy").Insert("entityid", client.Entity.UID).Insert("enemyid", enemy.ID)
                    .Insert("enemyname", enemy.Name).Execute();
        }
        public static void UpdateMessageOnFriend(uint entityID, uint friendID, string message)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("friends").Set("Message", message).Where("EntityID", friendID)
                    .And("FriendID", entityID).Execute();
        }
    }
}