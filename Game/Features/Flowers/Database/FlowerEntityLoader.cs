using System;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Features.Flowers.Database.Models;

namespace MTA.Game.Features.Flowers.Database;

/// <summary>
///     Loads (or initializes) the per-player <see cref="Flowers"/> object on login,
///     hydrating it from the database for girl bodies and creating a fresh record if
///     none exists. Boys still get an in-memory object for the sending flow but no DB row.
/// </summary>
public static class FlowerEntityLoader {
    public static void Load(GameState client) {
        // Initialize flowers object for all players (boys need it for sending, girls for receiving)
        client.Entity.Flowers = new Flowers(client.Entity.UID, client.Entity.Name);

        // Only load from database for girls (boys should not have database records)
        if (!BodyTypes.IsGirl(client.Entity.Body)) return;

        var record = FlowerTable.LoadByEntityId(client.Entity.UID);
        if (record != null) {
            Apply(client, record);
            return;
        }

        if (FlowerTable.Exists(client.Entity.UID)) {
            // Record exists but LoadByEntityId returned null — retry once.
            var retryRecord = FlowerTable.LoadByEntityId(client.Entity.UID);
            if (retryRecord != null) {
                Apply(client, retryRecord);
            }
        } else {
            // Record doesn't exist, create new one
            FlowerTable.Insert(client);
        }
    }

    private static void Apply(GameState client, FlowerRecord record) {
        client.Entity.Flowers.RedRoses = record.RedRoses;
        client.Entity.Flowers.RedRosesToday = record.RedRosesToday;
        client.Entity.Flowers.Lilies = record.Lilies;
        client.Entity.Flowers.LiliesToday = record.LiliesToday;
        client.Entity.Flowers.Orchids = record.Orchids;
        client.Entity.Flowers.OrchidsToday = record.OrchidsToday;
        client.Entity.Flowers.Tulips = record.Tulips;
        client.Entity.Flowers.TulipsToday = record.TulipsToday;
        client.Entity.Flowers.SendDay = record.SendDay;
        client.Entity.Flowers.AFlower = record.AFlower;
        try {
            client.Entity.Flowers.LastFlowerSent = DateTime.FromBinary(record.LastFlowerSent);
        } catch {
            client.Entity.Flowers.LastFlowerSent = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        }

        client.Entity.Flowers.Reset();
    }
}
