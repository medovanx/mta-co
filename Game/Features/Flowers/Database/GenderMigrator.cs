using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Features.Kisses.Database;

namespace MTA.Game.Features.Flowers.Database;

/// <summary>
///     Handles per-player flower/kiss data when a character changes gender:
///     drops the old table's row and creates a fresh row in the new table.
/// </summary>
public static class GenderMigrator {
    public static void Migrate(GameState client, ushort oldBody) {
        // Delete from old table
        if (BodyTypes.IsGirl(oldBody)) {
            FlowerTable.DeleteByEntityId(client.Entity.UID);
            client.Entity.Flowers = null;
        } else if (BodyTypes.IsBoy(oldBody)) {
            KissTable.DeleteByEntityId(client.Entity.UID);
            client.Entity.Kisses = null;
        }

        // Create new record in correct table based on new body
        if (BodyTypes.IsGirl(client.Entity.Body)) {
            FlowerEntityLoader.Load(client);
        } else if (BodyTypes.IsBoy(client.Entity.Body)) {
            KissEntityLoader.Load(client);
        }
    }
}
