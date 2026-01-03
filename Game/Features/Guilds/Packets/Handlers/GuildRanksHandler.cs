using System;
using MTA.Client;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles packet 2101 for guild donation rankings, displaying top members by donation type (Silver, CP, PK, Flowers, etc.).
/// </summary>
[PacketHandler(2101)]
public static class GuildRanksHandler {
    /// <summary>
    ///     Displays donation rankings by type with pagination, showing top 20 members for each donation category.
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        if (client.Guild == null) return false;
        ushort rank = packet[4];
        var displayPage = (ushort)Math.Min(2, (int)packet[5]);

        switch (rank) {
            case Enums.GuildRanksTop20Type.SilverRank: {
                var rankMembers = client.Guild.RankSilversDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.SilverDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.CpRank: {
                var rankMembers = client.Guild.RankCpDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.ConquerPointDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.GuideDonation: {
                var rankMembers = client.Guild.RankGuideDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.VirtuePoints);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.PkRank: {
                var rankMembers = client.Guild.RankPkDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.PkDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.ArsenalRank: {
                var rankMembers = client.Guild.RankArsenalDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.ArsenalDonation);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.RosesRank: {
                var rankMembers = client.Guild.RankRoseDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };

                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.Roses);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.OrchidRank: {
                var rankMembers = client.Guild.RankOrchidsDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;

                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.Orchids);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.LilyRank: {
                var rankMembers = client.Guild.RankLiliesDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.Lilies);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.TulipRank: {
                var rankMembers = client.Guild.RankTulipsDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.Tulips);
                }

                client.Send(ranks.ToArray());
                break;
            }
            case Enums.GuildRanksTop20Type.TotalDonaion: {
                var rankMembers = client.Guild.RankTotalDonations;
                var offset = displayPage * Enums.GuildRanksTop20Type.MaxCounts;
                var count = (ushort)Math.Min(Enums.GuildRanksTop20Type.MaxCounts, rankMembers.Length);
                if (displayPage == 2 && rankMembers.Length < 10)
                    break;
                var ranks = new GuildRanks(count) {
                    Rank = rank,
                    Page = displayPage
                };
                for (byte x = 0; x < count; x++) {
                    if (rankMembers.Length < offset + x)
                        break;
                    var element = rankMembers[offset + x];
                    ranks.Append(element, element.TotalDonation);
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