using System;
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
    private static bool Exists(uint id) {
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
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select(FlowerSchema.Tables.FlowersTable)
                .Where(FlowerSchema.Flowers.EntityId, id);
            using var reader = new MySqlReader(cmd);
            if (reader.Read()) return FlowerMappers.MapFlower(reader);
        }
        catch {
            // Return null if not found or error
        }

        return null;
    }

    /// <summary>
    ///     Inserts a new flower record
    /// </summary>
    public static void Insert(GameState client) {
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
    /// </summary>
    public static void Save(GameState client) {
        if (!Exists(client.Entity.UID))
            Insert(client);
        else
            Update(client);
    }
}