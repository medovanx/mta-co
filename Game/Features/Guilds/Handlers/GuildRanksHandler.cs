using System;
using MTA.Client;
using MTA.Game.Features.Guilds.Packets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Handlers;

[PacketHandler(2101)]
public static class GuildRanksHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        if (client.Guild == null) return false;
        ushort rank = packet[4];
        var displyPage = (ushort)Math.Min(2, (int)packet[5]);

        switch (rank) {
            case Enums.GuildRanksTop20Type.SilverRank: {
                var rankMembers = client.Guild.RankSilversDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.SilverDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.CpRank: {
                var rankMembers = client.Guild.RankCpDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.ConquerPointDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.GuideDonation: {
                var rankMembers = client.Guild.RankGuideDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.VirtuePoints);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.PkRank: {
                var rankMembers = client.Guild.RankPkDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.PkDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.ArsenalRank: {
                var rankMembers = client.Guild.RankArsenalDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.ArsenalDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.RosesRank: {
                var rankMembers = client.Guild.RankRoseDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.Roses);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.OrchidRank: {
                var rankMembers = client.Guild.RankOrchidsDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;

                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.Orchids);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.LilyRank: {
                var rankMembers = client.Guild.RankLiliesDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.Lilies);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.TulipRank: {
                var rankMembers = client.Guild.RankTulipsDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.Tulips);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.TotalDonaion: {
                var rankMembers = client.Guild.RankTotalDonations;
                var offset = displyPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displyPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displyPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Aprend(element, element.TotalDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            default: {
                Console.WriteLine("[packet = " + 2101 + "] unfind typ " + rank + "");
                break;
            }
        }

        return true;
    }
}