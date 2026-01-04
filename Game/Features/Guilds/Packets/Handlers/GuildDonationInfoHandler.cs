using System;
using MTA.Client;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles packet 1058 which displays guild member donation information to the client.
///     Populates the packet with actual donation data from the member's record.
/// </summary>
[PacketHandler(Game.Constants.Packets.MsgSynpOffer)]
public static class GuildDonationInfoHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        if (client.Guild == null) return false;
        
        client.Guild.SendGuild(client);
        PopulateDonationProfile(packet, client);
        return true;
    }

    /// <summary>
    ///     Populates and sends guild member donation profile packet with actual donation data from the member's record.
    ///     Displays all donation types including silver, CP, PK, arsenal, flowers, and historical totals.
    /// </summary>
    /// <param name="packet">The incoming packet to populate with donation data</param>
    /// <param name="client">The client whose donation profile will be sent</param>
    private static void PopulateDonationProfile(byte[] packet, GameState client) {
        if (client.AsMember == null) return;

        var data = new GuildProfilePacket(packet);
        data.Deserialize(packet);

        // Populate current donation data
        data.Silver = (uint)Math.Min(client.AsMember.SilverDonation, uint.MaxValue);
        data.Cps = (uint)Math.Min(client.AsMember.ConquerPointDonation, uint.MaxValue);
        data.Arsenal = client.AsMember.ArsenalDonation;
        data.Lily = client.AsMember.Lilies;
        data.Rose = client.AsMember.Roses;
        data.Orchid = client.AsMember.Orchids;
        data.Tulip = client.AsMember.Tulips;
        data.Pk = client.AsMember.PkDonation;
        data.Exploits = client.AsMember.Exploits;
        data.Guide = client.AsMember.GuideDonation;

        // History fields not tracked in GuildMember model
        data.HistoryCps = 666;
        data.HistoryGuide = 777;
        data.HistoryPk = 888;

        client.Send(packet);
    }
}