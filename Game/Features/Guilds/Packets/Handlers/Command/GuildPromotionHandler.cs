using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Models;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Game.Features.Guilds.Services;
using MTA.Network.GamePackets;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Packets.Handlers.Command;

/// <summary>
///     Handles guild member promotion and demotion operations. Manages the promotion hierarchy where
///     higher-ranked members can promote lower-ranked members to specific ranks based on their own rank.
///     Validates permissions, Conquer Points costs, and rank limits before applying promotions.
///     Also handles special cases like guild leadership transfer and Deputy Leader demotion.
/// </summary>
public static class GuildPromotionHandler {
    /// <summary>
    ///     Sends a list of all members with a specific rank to the Guild Leader. This is used when
    ///     the leader wants to view members of a particular rank (e.g., all Deputy Leaders) before
    ///     making promotion decisions. Only Guild Leaders can request this information.
    /// </summary>
    /// <param name="command">The guild command containing the target rank to filter by</param>
    /// <param name="packet">The original packet to forward</param>
    /// <param name="client">The client requesting the member list (must be Guild Leader)</param>
    public static void HandlePromoteInfo(GuildCommand command, byte[] packet, GameState client) {
        if (client.AsMember!.Rank == MemberRank.GuildLeader) {
            var array2 = client.Guild!.Members.Values.Where(p => p.Rank == (MemberRank)command.DwParam)
                .ToDictionary(p => p.Id);

            var array = array2.Values.ToArray();
            {
                var buffer = new byte[8 + 48 + array.Length * 32];
                Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Writer.WriteUInt16(2102, 2, buffer);
                Writer.Uint(1, 4, buffer);
                Writer.Uint((uint)array.Length, 12, buffer);
                var offset = 16;
                foreach (var t in array) {
                    Writer.Uint(t.Level, offset, buffer); //level
                    offset += 4;
                    Writer.Uint((uint)(Kernel.TryGetPlayer(t.Id, out _) ? 1 : 0), offset, buffer); //online
                    offset += 4;
                    if (Kernel.TryGetPlayer(t.Id, out var tClient))
                        Writer.Uint((uint)tClient.Entity.BattlePower, offset, buffer); //bp
                    offset += 4;
                    offset += 4;
                    Writer.String(t.Name, offset, buffer);
                    offset += 16;
                }

                client.Send(buffer);
            }
        }

        client.Send(packet);
    }

    /// <summary>
    ///     Sends the available promotion options to the client based on their current rank.
    ///     This populates the promotion UI with ranks the player can promote members to, along with
    ///     CP costs and current counts. The options are determined by the player's rank in the guild.
    /// </summary>
    /// <param name="command">The guild command</param>
    /// <param name="client">The client requesting promotion options</param>
    public static void HandleRequestPromote(GuildCommand command, GameState client) {
        command.SendPromote(client, (ushort)GuildCommand.RequestPromote);
    }

    /// <summary>
    ///     Demotes a Deputy Leader back to Member rank. Only the Guild Leader can discharge Deputy Leaders.
    ///     This is a special demotion case that doesn't go through the normal promotion system.
    ///     The discharged member loses their Deputy Leader privileges and returns to basic Member status.
    /// </summary>
    /// <param name="command">The guild command</param>
    /// <param name="packet">The packet containing the member name to discharge</param>
    /// <param name="client">The client performing the discharge (must be Guild Leader)</param>
    public static void HandleDischarge(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: MemberRank.GuildLeader }) return;
        var member = client.Guild.Members.Values.First(member => member.Name == name);
        if (member?.Id == client.Entity.UID) return;
        if (member?.Rank != MemberRank.DeputyLeader) return;
        client.Guild.RanksCounts[(ushort)MemberRank.DeputyLeader]--;
        member.Rank = MemberRank.Member;
        if (Kernel.TryGetPlayer(member.Id, out var memberClient)) {
            client.Guild.SendGuild(memberClient);
            memberClient.Entity.GuildRank = (ushort)member.Rank;
            memberClient.Screen.FullWipe();
            memberClient.Screen.Reload();
            memberClient.Entity.GuildBattlePower =
                member.Guild.GetSharedBattlePower(member.Rank);
        }

        GuildMemberTable.UpdateGuildAndRank(member.Id, member.GuildId, (ushort)member.Rank);
    }

    /// <summary>
    ///     Applies a promotion to a guild member. Updates the member's rank, adjusts guild rank counts,
    ///     updates the database, and refreshes the client's UI if they are online. Also updates the
    ///     promoting client's member list to reflect the change.
    /// </summary>
    /// <param name="guild">The guild the member belongs to</param>
    /// <param name="member">The member being promoted</param>
    /// <param name="newRank">The new rank to assign</param>
    /// <param name="promotingClient">The client who initiated the promotion</param>
    private static void ApplyPromotion(Guild guild, GuildMember member, MemberRank newRank,
        GameState promotingClient) {
        // Update rank counts
        if (member.Rank != newRank) {
            var oldRankIndex = (ushort)member.Rank;
            var newRankIndex = (ushort)newRank;

            // Bounds check
            if (oldRankIndex < guild.RanksCounts.Length) guild.RanksCounts[oldRankIndex]--;

            member.Rank = newRank;

            // Bounds check
            if (newRankIndex < guild.RanksCounts.Length) guild.RanksCounts[newRankIndex]++;
        }

        // Update online member
        if (Kernel.TryGetPlayer(member.Id, out var promoteClient)) {
            guild.SendGuild(promoteClient);
            promoteClient.Entity.GuildBattlePower = guild.GetSharedBattlePower(member.Rank);
            promoteClient.Entity.GuildRank = (ushort)member.Rank;
            promoteClient.Screen.FullWipe();
            promoteClient.Screen.Reload();
        }

        // Update database
        GuildMemberTable.UpdateGuildAndRank(member.Id, member.GuildId, (ushort)member.Rank);

        // Refresh member list for the promoting client
        guild.SendMembers(promotingClient, 0);
    }

    /// <summary>
    ///     Main promotion handler that validates and applies member promotions. Checks if the promoting
    ///     player has permission to promote to the target rank, verifies Conquer Points costs for
    ///     honorary ranks, and ensures rank limits aren't exceeded. Handles special case of guild
    ///     leadership transfer separately. All validation is based on the promotion options defined
    ///     in GuildPromotionOptions.
    /// </summary>
    /// <param name="command">The guild command</param>
    /// <param name="packet">The packet containing member name and target rank</param>
    /// <param name="client">The client attempting to promote a member</param>
    public static void HandlePromote(GuildCommand command, byte[] packet, GameState client) {
        var getMemberName = Program.Encoding.GetString(packet, 26, packet[25]);
        var getMemberRank = BitConverter.ToUInt16(packet, 8);
        var targetRank = (MemberRank)getMemberRank;

        if (!client.Guild!.GetMember(getMemberName, out var memberPromote)) {
            client.Send(new Message("Sorry Can't Find " + getMemberName,
                Color.White, Message.System));
            return;
        }

        // GetMember returns true only when member is found, so memberPromote is guaranteed non-null here
        var member = memberPromote!;

        // Special case: Guild Leader transfer
        if (client.AsMember!.Rank == MemberRank.GuildLeader && targetRank == MemberRank.GuildLeader) {
            HandleGuildLeaderTransfer(client, member);
            return;
        }

        // Get promotion option for the target rank
        // If option is null, the UI shouldn't have allowed this - silently return (potential hack attempt or UI bug)
        var option = GuildPromotionOptions.GetPromotionOption(client.AsMember.Rank, targetRank);
        if (option == null) return;

        // Check CP cost
        if (option.ConquerPointsCost > 0 && client.Entity.ConquerPoints < (uint)option.ConquerPointsCost) {
            var cpCostMessages = new Dictionary<int, string> {
                { 650, "You need 650 Conquer Points to appoint Honorary Deputy Leader!" },
                { 320, "You need 320 Conquer Points to appoint Honorary Manager!" },
                { 270, "You need 270 Conquer Points to appoint Honorary Supervisor!" },
                { 100, "You need 100 Conquer Points to appoint Honorary Steward!" }
            };
            var message = cpCostMessages.GetValueOrDefault(option.ConquerPointsCost,
                $"You need {option.ConquerPointsCost} Conquer Points!");
            client.Send(new Message(message, Color.White, Message.System));
            return;
        }

        // Check rank limit
        if (option.LimitCheck != null && option.LimitCheck(client.Guild.RanksCounts, targetRank, client.Guild.Level)) {
            var errorMessage = option.LimitErrorMessage ?? "Sorry, this rank is at its limit!";
            client.Send(new Message(errorMessage, Color.White, Message.System));
            return;
        }

        // Deduct CP cost if applicable
        if (option.ConquerPointsCost > 0) {
            client.Entity.ConquerPoints -= (uint)option.ConquerPointsCost;
            EntityTable.UpdateData(client.Entity.UID, "ConquerPoints", (int)client.Entity.ConquerPoints);
        }

        // Apply the promotion
        ApplyPromotion(client.Guild, member, targetRank, client);
        client.Entity.GuildBattlePower = client.Guild.GetSharedBattlePower(client.Entity.GuildRank);
    }

    /// <summary>
    ///     Transfers guild leadership from the current Guild Leader to another member. The current
    ///     leader becomes a Deputy Leader, and the new leader receives full guild control. This is
    ///     the only way to change guild leadership and requires the current leader to initiate it.
    /// </summary>
    /// <param name="client">The current Guild Leader transferring leadership</param>
    /// <param name="newLeader">The member who will become the new Guild Leader</param>
    private static void HandleGuildLeaderTransfer(GameState client, GuildMember newLeader) {
        // Transfer leadership
        newLeader.Rank = MemberRank.GuildLeader;
        client.Guild!.LeaderId = newLeader.Id;
        client.Guild.Leader = newLeader;
        client.Guild.LeaderName = newLeader.Name;

        if (Kernel.TryGetPlayer(newLeader.Id, out var promoteClient)) {
            client.Guild.SendGuild(promoteClient);
            promoteClient.Entity.GuildBattlePower = client.Guild.GetSharedBattlePower(newLeader.Rank);
            promoteClient.Entity.GuildRank = (ushort)newLeader.Rank;
            promoteClient.Screen.FullWipe();
            promoteClient.Screen.Reload();
        }

        client.AsMember!.Rank = MemberRank.DeputyLeader;
        client.Entity.GuildRank = (ushort)client.AsMember.Rank;
        client.Guild.SendGuild(client);
        client.Screen.FullWipe();
        client.Screen.Reload();
        GuildTable.SaveLeader(client.Guild);
        client.Guild.SendMembers(client, 0);
    }
}