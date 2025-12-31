using System.IO;
using MTA.Database;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Database;

public class GuildArsenalTable {
    public static void Load(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guildarsenal").Where("ID", guild.Id);
        using var rdr = new MySqlReader(cmd);
        if (rdr.Read()) {
            var stream = new MemoryStream(rdr.ReadBlob("Data"));
            var reader = new BinaryReader(stream);
            for (var i = 0; i < 8; i++)
                guild.Arsenals[i].Load(reader);
            guild.ArsenalBpChanged = true;
            guild.GetMaxSharedBattlePower();
        }
        else {
            Insert(guild.Id);
        }
    }

    public static void Save(Guild guild) {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        for (var i = 0; i < 8; i++)
            guild.Arsenals[i].Save(writer);
        var sql = "UPDATE `guildarsenal` SET data=@Data, datalength=@DataLength where ID = " + guild.Id + " ;";
        var rawData = stream.ToArray();
        using var conn = DataHolder.MySqlConnection;
        conn.Open();
        using var cmd = new MySql.Data.MySqlClient.MySqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Data", rawData);
        cmd.Parameters.AddWithValue("@DataLength", rawData.Length);
        cmd.ExecuteNonQuery();
    }

    public static void SaveAll() {
        foreach (var guild in Kernel.Guilds.Values)
            Save(guild);
    }

    public static void Insert(uint id) {
        using var cmd = new MySqlCommand(MySqlCommandType.INSERT);
        cmd.Insert("guildarsenal").Insert("ID", id);
        cmd.Execute();
    }
}