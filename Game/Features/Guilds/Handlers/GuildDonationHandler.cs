using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildDonationHandler {
    public static void HandleDonateSilvers(GuildCommand command, GameState client) {
        if (client.Guild == null) return;
        if (client.Trade.InTrade)
            return;
        if (client.Entity.Money < command.dwParam) return;
        client.Guild.SilverFund += command.dwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.SilverDonation += command.dwParam;
        client.Entity.Money -= command.dwParam;
        client.Guild.SendGuild(client);
    }

    public static void HandleDonateConquerPoints(GuildCommand command, GameState client) {
        if (client.Guild == null) return;
        if (client.Trade.InTrade)
            return;
        if (client.Entity.ConquerPoints < command.dwParam) return;
        client.Guild.ConquerPointFund += command.dwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.ConquerPointDonation += command.dwParam;
        client.Entity.ConquerPoints -= command.dwParam;
        client.Guild.SendGuild(client);
    }
}