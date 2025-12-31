using System.Drawing;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using Update = MTA.Network.GamePackets.Update;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.Constants.Items.GuildItems;
using static MTA.Game.Events.GuildWar.GuildWarConstants;

namespace MTA.Game.Events.GuildWar.Items {
    /// <summary>
    /// Handles Guild War Bomb item.
    /// </summary>
    [ItemHandler(Bomb)]
    public static class BombHandler {
        /// <summary>
        /// Activates the bomb on a gate - damages the gate and kills the player
        /// </summary>
        private static void ActivateBomb(GameState client, ConquerItem item, SobNpcSpawn gate,
            ushort brokenMesh) {
            // Calculate damage per bomb (4 bombs should destroy the gate)
            var damagePerBomb = gate.MaxHitpoints / BombsRequiredToDestroyGate;

            // Apply damage to gate
            if (gate.Hitpoints <= damagePerBomb) {
                gate.Hitpoints = 0;
                gate.Mesh = brokenMesh;
            }
            else {
                gate.Hitpoints -= damagePerBomb;
                // Gate is damaged but not destroyed yet - keep current mesh state
            }

            var upd = new Update(true) {
                UID = gate.UID
            };
            upd.Append(Update.Mesh, gate.Mesh);
            upd.Append(Update.Hitpoints, gate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, Maps.GuildWarMap);

            var str = new _String(true) {
                UID = client.Entity.UID,
                TextsCount = 1,
                Type = _String.Effect
            };
            str.Texts.Add("bombFranko");
            client.Entity.SendScreen(str);
            // Kill the player who used the bomb
            client.Entity.Update(_String.Effect, "firemagic", true);
            client.Entity.Update(_String.Effect, "bombarrow7", true);
            client.Entity.Die(0);
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }

        /// <summary>
        /// Shows bomb confirmation dialog and handles activation
        /// </summary>
        private static void HandleBombConfirmation(GameState client, SobNpcSpawn gate,
            ushort brokenMesh, ConquerItem item) {
            client.MessageBox(
                "Are you sure you want to use the bomb? This will damage the gate and kill you!",
                p => { ActivateBomb(p, item, gate, brokenMesh); }, // OK callback - activate bomb
                null, // Cancel callback - do nothing
                30, // 30 second timeout
                force: true
            );
        }

        public static void Handle(GameState client, ConquerItem item) {
            var gwEvent = GuildWarEvent.GetActiveEvent();
            if (gwEvent?.IsActive != true) {
                return;
            }

            // Check if player is on Guild War map
            if (client.Entity.MapID != Maps.GuildWarMap) {
                return;
            }

            var playerX = client.Entity.X;
            var playerY = client.Entity.Y;

            // Check West Gate location with tolerance
            var westGateDistanceX = playerX > WestGateBombX
                ? playerX - WestGateBombX
                : WestGateBombX - playerX;
            var westGateDistanceY = playerY > WestGateBombY
                ? playerY - WestGateBombY
                : WestGateBombY - playerY;

            if (westGateDistanceX <= BombLocationTolerance &&
                westGateDistanceY <= BombLocationTolerance) {
                HandleBombConfirmation(client, gwEvent.WestGate!, WestGateBrokenMesh, item);
                return;
            }

            // Check East Gate location with tolerance
            var eastGateDistanceX = playerX > EastGateBombX
                ? playerX - EastGateBombX
                : EastGateBombX - playerX;
            var eastGateDistanceY = playerY > EastGateBombY
                ? playerY - EastGateBombY
                : EastGateBombY - playerY;

            if (eastGateDistanceX <= BombLocationTolerance &&
                eastGateDistanceY <= BombLocationTolerance) {
                HandleBombConfirmation(client, gwEvent.EastGate!, EastGateBrokenMesh, item);
                return;
            }

            // Player is not at either bomb location
            client.Send(new Message(
                $"You need to be closer to the gate to use the bomb.",
                Color.Red, Message.TopLeft));
        }
    }
}