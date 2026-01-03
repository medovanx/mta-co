using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Packets.Writers;

namespace MTA.Game.Features.Guilds.Packets.Handlers.Command;

/// <summary>
///     Handles guild donation commands (silver and CP), processing donations to guild funds and tracking member contributions.
/// </summary>
public static class GuildDonationHandler {
    /// <summary>
    ///     Processes silver donation to guild fund, deducting from player's money and updating both guild fund and member donation tracking.
    /// </summary>
    public static void HandleDonateSilvers(GuildCommand command, GameState client) {
        if (client.Trade.InTrade)
            return;
        if (client.Entity.Money < command.DwParam) return;
        client.Guild!.SilverFund += command.DwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.SilverDonation += command.DwParam;
        client.Entity.Money -= command.DwParam;
        GuildMemberTable.Save(client.AsMember);
        client.Guild.SendGuild(client);
    }

    /// <summary>
    ///     Processes CP donation to guild fund, deducting from player's CP and updating both guild fund and member donation tracking.
    /// </summary>
    public static void HandleDonateConquerPoints(GuildCommand command, GameState client) {
        if (client.Trade.InTrade)
            return;
        if (client.Entity.ConquerPoints < command.DwParam) return;
        client.Guild!.ConquerPointFund += command.DwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.ConquerPointDonation += command.DwParam;
        client.Entity.ConquerPoints -= command.DwParam;
        GuildMemberTable.Save(client.AsMember);
        client.Guild.SendGuild(client);
    }
}