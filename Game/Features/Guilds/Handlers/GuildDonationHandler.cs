using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Packets;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildDonationHandler {
    public static void HandleDonateSilvers(GuildCommand command, GameState client) {
        if (client.Guild == null) return;
        if (client.Trade.InTrade)
            return;
        if (client.Entity.Money < command.DwParam) return;
        client.Guild.SilverFund += command.DwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.SilverDonation += command.DwParam;
        client.Entity.Money -= command.DwParam;
        GuildMemberTable.Save(client.AsMember);
        client.Guild.SendGuild(client);
    }

    public static void HandleDonateConquerPoints(GuildCommand command, GameState client) {
        if (client.Guild == null) return;
        if (client.Trade.InTrade)
            return;
        if (client.Entity.ConquerPoints < command.DwParam) return;
        client.Guild.ConquerPointFund += command.DwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.ConquerPointDonation += command.DwParam;
        client.Entity.ConquerPoints -= command.DwParam;
        GuildMemberTable.Save(client.AsMember);
        client.Guild.SendGuild(client);
    }
}