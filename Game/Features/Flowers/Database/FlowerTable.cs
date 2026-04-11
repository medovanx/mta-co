using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Flowers.Database.Mappers;
using MTA.Game.Features.Flowers.Database.Models;
using MTA.Game.Features.Flowers.Database.Schema;

namespace MTA.Game.Features.Flowers.Database;

/// <summary>
///     Database operations for the flowers table
/// </summary>
public static class FlowerTable {
    /// <summary>
    ///     Checks if a flower record exists for the given entity ID
    /// </summary>
    public static bool Exists(uint id) {
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select(FlowerSchema.Tables.FlowersTable)
                .Where(FlowerSchema.Flowers.EntityId, id);
            using var reader = new MySqlReader(cmd);
            return reader.Read();
        }
        catch {
            return false;
        }
    }

    /// <summary>
    ///     Loads a flower record by entity ID
    /// </summary>
    public static FlowerRecord? LoadByEntityId(uint id) {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(FlowerSchema.Tables.FlowersTable)
            .Where(FlowerSchema.Flowers.EntityId, id);
        using var reader = new MySqlReader(cmd);
        if (reader.Read()) return FlowerMappers.MapFlower(reader);

        return null;
    }

    /// <summary>
    ///     Inserts a new flower record
    /// </summary>
    public static void Insert(GameState client) {
        // Only girls should be in the flowers table (Body IDs: 2001, 2002, 2003, 2004)
        if (client.Entity.Body is not (2001 or 2002 or 2003 or 2004)) {
            Console.WriteLine(
                $"Warning: Attempted to insert boy (Body: {client.Entity.Body}, UID: {client.Entity.UID}) into flowers table");
            return;
        }

        using var cmd = new MySqlCommand(MySqlCommandType.INSERT)
            .Insert(FlowerSchema.Tables.FlowersTable)
            .Insert(FlowerSchema.Flowers.EntityId, client.Entity.UID)
            .Insert(FlowerSchema.Flowers.RedRoses, client.Entity.Flowers.RedRoses)
            .Insert(FlowerSchema.Flowers.RedRosesToday, client.Entity.Flowers.RedRosesToday)
            .Insert(FlowerSchema.Flowers.Lilies, client.Entity.Flowers.Lilies)
            .Insert(FlowerSchema.Flowers.LiliesToday, client.Entity.Flowers.Lilies2day)
            .Insert(FlowerSchema.Flowers.Orchids, client.Entity.Flowers.Orchids)
            .Insert(FlowerSchema.Flowers.OrchidsToday, client.Entity.Flowers.OrchidsToday)
            .Insert(FlowerSchema.Flowers.Tulips, client.Entity.Flowers.Tulips)
            .Insert(FlowerSchema.Flowers.TulipsToday, client.Entity.Flowers.TulipsToday)
            .Insert(FlowerSchema.Flowers.LastFlowerSent,
                DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToBinary())
            .Insert(FlowerSchema.Flowers.SendDay, client.Entity.Flowers.SendDay)
            .Insert(FlowerSchema.Flowers.AFlower, client.Entity.Flowers.AFlower);
        cmd.Execute();
    }

    /// <summary>
    ///     Updates an existing flower record
    /// </summary>
    private static void Update(GameState client) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(FlowerSchema.Tables.FlowersTable)
            .Set(FlowerSchema.Flowers.RedRoses, client.Entity.Flowers.RedRoses)
            .Set(FlowerSchema.Flowers.RedRosesToday, client.Entity.Flowers.RedRosesToday)
            .Set(FlowerSchema.Flowers.Lilies, client.Entity.Flowers.Lilies)
            .Set(FlowerSchema.Flowers.LiliesToday, client.Entity.Flowers.Lilies2day)
            .Set(FlowerSchema.Flowers.Orchids, client.Entity.Flowers.Orchids)
            .Set(FlowerSchema.Flowers.OrchidsToday, client.Entity.Flowers.OrchidsToday)
            .Set(FlowerSchema.Flowers.Tulips, client.Entity.Flowers.Tulips)
            .Set(FlowerSchema.Flowers.TulipsToday, client.Entity.Flowers.TulipsToday)
            .Set(FlowerSchema.Flowers.LastFlowerSent, client.Entity.Flowers.LastFlowerSent.ToBinary())
            .Set(FlowerSchema.Flowers.SendDay, client.Entity.Flowers.SendDay)
            .Set(FlowerSchema.Flowers.AFlower, client.Entity.Flowers.AFlower)
            .Where(FlowerSchema.Flowers.EntityId, client.Entity.UID);
        cmd.Execute();
    }

    /// <summary>
    ///     Saves a flower record (inserts if not exists, updates if exists)
    ///     Only saves for girls - boys should not be in the flowers table
    /// </summary>
    public static void Save(GameState client) {
        // Only save girls to the flowers table
        if (client.Entity.Body is not (2001 or 2002 or 2003 or 2004)) return;

        if (!Exists(client.Entity.UID))
            Insert(client);
        else
            Update(client);
    }

    /// <summary>
    ///     Deletes a flower record by entity ID
    /// </summary>
    public static void DeleteByEntityId(uint entityId) {
        using var cmd = new MySqlCommand(MySqlCommandType.DELETE)
            .Delete(FlowerSchema.Tables.FlowersTable, FlowerSchema.Flowers.EntityId, entityId);
        cmd.Execute();
    }

    /// <summary>
    ///     Top 100 flower rows for one leaderboard column, with player name from <c>entities</c>.
    /// </summary>
    public static List<(FlowerRecord Record, string Name)> LoadTop100WithNames(string whereNonZeroColumn,
        string orderByColumnDesc) {
        var list = new List<(FlowerRecord, string)>();
        var t = FlowerSchema.Tables.FlowersTable;
        var sql =
            $"SELECT f.{FlowerSchema.Flowers.EntityId} AS entity_id, f.{FlowerSchema.Flowers.RedRoses} AS redroses, f.{FlowerSchema.Flowers.RedRosesToday} AS redroses_today, " +
            $"f.{FlowerSchema.Flowers.Lilies} AS lilies, f.{FlowerSchema.Flowers.LiliesToday} AS lilies_today, f.{FlowerSchema.Flowers.Orchids} AS orchids, " +
            $"f.{FlowerSchema.Flowers.OrchidsToday} AS orchids_today, f.{FlowerSchema.Flowers.Tulips} AS tulips, f.{FlowerSchema.Flowers.TulipsToday} AS tulips_today, " +
            $"f.{FlowerSchema.Flowers.LastFlowerSent} AS last_flower_sent, f.{FlowerSchema.Flowers.SendDay} AS send_day, f.{FlowerSchema.Flowers.AFlower} AS a_flower, e.Name AS Name " +
            $"FROM `{t}` f INNER JOIN `entities` e ON f.{FlowerSchema.Flowers.EntityId} = e.UID WHERE {whereNonZeroColumn} " +
            $"ORDER BY {orderByColumnDesc} DESC LIMIT 100";
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(t);
            cmd.Command = cmd.Command.Replace($"SELECT * FROM `{t}`", sql);
            using var reader = new MySqlReader(cmd);
            while (reader.Read()) {
                var record = FlowerMappers.MapFlower(reader);
                var name = reader.ReadString("Name");
                list.Add((record, name));
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"FlowerTable.LoadTop100WithNames: {ex.Message}");
        }

        return list;
    }
}