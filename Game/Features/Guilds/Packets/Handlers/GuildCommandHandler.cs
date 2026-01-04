using System;
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
using MTA.Network.PacketHandlers;
using Message = MTA.Network.GamePackets.Message;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Central router for guild command packets (1107), handling all guild-related actions such as promotions, donations, relationships, and settings.
/// </summary>
[PacketHandler(Game.Constants.Packets.MsgSyndicate)]
public static class GuildCommandHandler {
    /// <summary>
    ///     Routes commands to appropriate handlers based on command type, delegating to specialized handlers for each operation.
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        var command = new GuildCommand(false);
        command.Deserialize(packet);
        switch (command.Type) {
            case GuildCommand.PromoteInfo:
                HandlePromoteInfo(command, packet, client);
                break;
            case GuildCommand.RequestPromote:
                HandleRequestPromote(command, client);
                break;
            case GuildCommand.PromoteWithCP:
                HandlePromote(packet, client);
                break;
            case GuildCommand.Info:
                HandleInfo(command, client);
                break;
            case GuildCommand.ChangeGuildRequirements:
                HandleChangeRequirements(command, client);
                break;
            case GuildCommand.Unally:
            case GuildCommand.Peace:
                HandleUnallyAndPeace(packet, client);
                break;
            case GuildCommand.Ally:
                HandleAlly(packet, client);
                break;
            case GuildCommand.Enemy:
                HandleEnemy(packet, client);
                break;
            case GuildCommand.AddToBlacklist:
                HandleBlacklistAdd(command, client);
                break;
            case GuildCommand.RemoveFromBlacklist:
                HandleBlacklistRemove(command, client);
                break;
            case GuildCommand.Bulletin:
                HandleBulletin(packet, client);
                break;
            case GuildCommand.DonateSilvers:
                HandleDonateSilvers(command, client);
                break;
            case GuildCommand.DonateConquerPoints:
                HandleDonateConquerPoints(command, client);
                break;
            case GuildCommand.Refresh:
                HandleRefresh(client);
                break;
            case GuildCommand.DischargeDeputyLeader:
                HandleDischargeDeputyLeader(packet, client);
                break;
            case GuildCommand.DischargeRank:
                HandleDischargeRank(packet, client);
                break;
            case GuildCommand.DischargeAide:
                HandleDischargeAide(packet, client);
                break;
            case GuildCommand.Promote:
                HandlePromote(packet, client);
                break;
            case GuildCommand.JoinRequest:
                HandleJoinRequest(command, client);
                break;
            case GuildCommand.InviteRequest:
                HandleInviteRequest(command, client);
                break;
            case GuildCommand.Quit:
                HandleQuit(client);
                break;
            case GuildCommand.LeaderAbsenceDonation:
                HandleLeaderAbsenceDonation(client);
                break;
            default:
                Console.WriteLine($"GuildCommandHandler Unhandled: {command.Type}");
                client.Send(packet);
                break;
        }

        return true;
    }

    /// <summary>
    ///     Sends a list of all members with a specific rank to the Guild Leader. This is used when
    ///     the leader wants to view members of a particular rank (e.g., all Deputy Leaders) before
    ///     making promotion decisions. Only Guild Leaders can request this information.
    /// </summary>
    /// <param name="command">The guild command containing the target rank to filter by</param>
    /// <param name="packet">The original packet to forward</param>
    /// <param name="client">The client requesting the member list (must be Guild Leader)</param>
    private static void HandlePromoteInfo(GuildCommand command, byte[] packet, GameState client) {
        if (client.AsMember!.Rank == MemberRank.GuildLeader) {
            var array2 = client.Guild!.Members.Values.Where(p => p.Rank == (MemberRank)command.DwParam)
                .ToDictionary(p => p.Id);

            var array = array2.Values.ToArray();
            {
                var buffer = new byte[8 + 48 + array.Length * 32];
                Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Writer.WriteUInt16((ushort)Game.Constants.Packets.MsgSynMemberList, 2, buffer);
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
    private static void HandleRequestPromote(GuildCommand command, GameState client) {
        command.SendPromote(client, (ushort)GuildCommand.RequestPromote);
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
    ///     Dismisses a Deputy Leader from their rank back to Member rank.
    ///     Used for Deputy Leader only.
    /// </summary>
    /// <param name="packet">The packet containing the member name to discharge</param>
    /// <param name="client">The client performing the discharge (must be Guild Leader)</param>
    private static void HandleDischargeDeputyLeader(byte[] packet, GameState client) {
        Console.WriteLine($"HandleDischarge: {packet}");
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        var member = client.Guild!.Members.Values.FirstOrDefault(m => m.Name == name);

        // Update rank counts - directly use DeputyLeader rank index
        const ushort deputyLeaderIndex = (ushort)MemberRank.DeputyLeader;
        client.Guild.RanksCounts[deputyLeaderIndex]--;

        // Demote to Member
        member!.Rank = MemberRank.Member;
        const ushort memberRankIndex = (ushort)MemberRank.Member;
        client.Guild.RanksCounts[memberRankIndex]++;

        // Update online member
        if (Kernel.TryGetPlayer(member.Id, out var memberClient)) {
            client.Guild.SendGuild(memberClient);
            memberClient.Entity.GuildRank = (ushort)member.Rank;
            memberClient.Screen.FullWipe();
            memberClient.Screen.Reload();
            memberClient.Entity.GuildBattlePower = member.Guild.GetSharedBattlePower(member.Rank);
        }

        // Update database
        GuildMemberTable.UpdateGuildAndRank(member.Id, member.GuildId, (ushort)member.Rank);

        // Refresh member list for the client
        client.Guild.SendMembers(client, 0);
    }

    /// <summary>
    ///     Dismisses a member from their rank back to Member rank.
    ///     Used for ranks other than Deputy Leader.
    /// </summary>
    /// <param name="packet">The packet containing the member name to dismiss</param>
    /// <param name="leader">The client performing the dismissal (must be Guild Leader)</param>
    private static void HandleDischargeRank(byte[] packet, GameState leader) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        var member = leader.Guild!.Members.Values.FirstOrDefault(m => m.Name == name);

        // Update rank counts
        var oldRankIndex = (ushort)member!.Rank;
        leader.Guild.RanksCounts[oldRankIndex]--;

        // Demote to Member
        member.Rank = MemberRank.Member;
        const ushort memberRankIndex = (ushort)MemberRank.Member;
        leader.Guild.RanksCounts[memberRankIndex]++;

        // Update online member
        if (Kernel.TryGetPlayer(member.Id, out var memberClient)) {
            leader.Guild.SendGuild(memberClient);
            memberClient.Entity.GuildRank = (ushort)member.Rank;
            memberClient.Screen.FullWipe();
            memberClient.Screen.Reload();
            memberClient.Entity.GuildBattlePower = member.Guild.GetSharedBattlePower(member.Rank);
        }

        // Update database
        GuildMemberTable.UpdateGuildAndRank(member.Id, member.GuildId, (ushort)member.Rank);

        // Refresh member list for the client
        leader.Guild.SendMembers(leader, 0);
    }

    /// <summary>
    ///     Dismisses an Aide from their rank back to Member rank.
    ///     Used for Aide only.
    /// </summary>
    /// <param name="packet">The packet containing the member name to dismiss</param>
    /// <param name="leader">The client performing the dismissal (must be Guild Leader)</param>
    private static void HandleDischargeAide(byte[] packet, GameState leader) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        var member = leader.Guild!.Members.Values.FirstOrDefault(m => m.Name == name);

        // Update rank counts
        var oldRankIndex = (ushort)member!.Rank;
        leader.Guild.RanksCounts[oldRankIndex]--;

        // Demote to Member
        member.Rank = MemberRank.Member;
        const ushort memberRankIndex = (ushort)MemberRank.Member;
        leader.Guild.RanksCounts[memberRankIndex]++;

        // Update online member
        if (Kernel.TryGetPlayer(member.Id, out var memberClient)) {
            leader.Guild.SendGuild(memberClient);
            memberClient.Entity.GuildRank = (ushort)member.Rank;
            memberClient.Screen.FullWipe();
            memberClient.Screen.Reload();
            memberClient.Entity.GuildBattlePower = member.Guild.GetSharedBattlePower(member.Rank);
        }

        // Update database
        GuildMemberTable.UpdateGuildAndRank(member.Id, member.GuildId, (ushort)member.Rank);

        // Refresh member list for the client
        leader.Guild.SendMembers(leader, 0);
    }

    /// <summary>
    ///     Main promotion handler that validates and applies member promotions. Checks if the promoting
    ///     player has permission to promote to the target rank, verifies Conquer Points costs for
    ///     honorary ranks, and ensures rank limits aren't exceeded. Handles special case of guild
    ///     leadership transfer separately. All validation is based on the promotion options defined
    ///     in GuildPromotionOptions.
    /// </summary>
    /// <param name="packet">The packet containing member name and target rank</param>
    /// <param name="client">The client attempting to promote a member</param>
    private static void HandlePromote(byte[] packet, GameState client) {
        var memberName = Program.Encoding.GetString(packet, 26, packet[25]);
        var memberTargetRank = (MemberRank)BitConverter.ToUInt16(packet, 8);
        var member = client.Guild!.Members.Values.FirstOrDefault(m => m.Name == memberName);

        if (member == null || client.AsMember == null) return;

        // Prevent changing the guild leader's rank except for leadership transfer
        if (member.Rank == MemberRank.GuildLeader && memberTargetRank != MemberRank.GuildLeader) {
            client.MessageBox("You cannot change the Guild Leader's rank!");
            return;
        }

        // Prevent demoting players of the same or higher rank
        if (client.AsMember.Rank <= member.Rank) {
            client.MessageBox("You cannot demote someone of the same or higher rank!");
            return;
        }

        // Special case: Guild Leader transfer
        if (client.AsMember!.Rank == MemberRank.GuildLeader && memberTargetRank == MemberRank.GuildLeader) {
            HandleGuildLeaderTransfer(client, member);
            return;
        }

        // Get promotion option for the target rank
        var option = GuildPromotionOptions.GetPromotionOption(client.AsMember.Rank, memberTargetRank);

        // Check CP cost
        if (option!.ConquerPointsCost > 0 && client.Entity.ConquerPoints < (uint)option.ConquerPointsCost) {
            var cpCostMessages = new Dictionary<int, string> {
                { 650, "You need 650 Conquer Points to appoint Honorary Deputy Leader!" },
                { 320, "You need 320 Conquer Points to appoint Honorary Manager!" },
                { 270, "You need 270 Conquer Points to appoint Honorary Supervisor!" },
                { 100, "You need 100 Conquer Points to appoint Honorary Steward!" }
            };
            var message = cpCostMessages.GetValueOrDefault(option.ConquerPointsCost,
                $"You need {option.ConquerPointsCost} Conquer Points!");
            client.MessageBox(message);
            return;
        }

        // Deduct CP cost if applicable
        if (option.ConquerPointsCost > 0) {
            client.Entity.ConquerPoints -= (uint)option.ConquerPointsCost;
            EntityTable.UpdateData(client.Entity.UID, "ConquerPoints", (int)client.Entity.ConquerPoints);
        }

        // Apply the promotion
        ApplyPromotion(client.Guild, member, memberTargetRank, client);
        client.Entity.GuildBattlePower = client.Guild.GetSharedBattlePower((MemberRank)client.Entity.GuildRank);
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

    /// <summary>
    ///     Handles the leader absence donation request. Sends a DonateSilvers command to the client
    ///     to open the donation UI, allowing them to donate 1 million to take over leadership.
    /// </summary>
    /// <param name="client">The client requesting to take over leadership</param>
    private static void HandleLeaderAbsenceDonation(GameState client) {
        if (client.Guild == null || client.AsMember == null) return;

        // Send DonateSilvers command to open donation UI
        var donateCommand = new GuildCommand(true) {
            Type = GuildCommand.DonateSilvers
        };
        client.Send(donateCommand);
    }

    /// <summary>
    ///     Transfers guild leadership when the current leader has been absent and a member donates 1 million.
    ///     The old leader becomes a Deputy Leader (if online) or remains in their current state (if offline).
    /// </summary>
    /// <param name="newLeaderClient">The client who donated 1 million to take over leadership</param>
    private static void HandleLeaderAbsenceTakeover(GameState newLeaderClient) {
        var guild = newLeaderClient.Guild!;
        var newLeader = newLeaderClient.AsMember!;
        var oldLeader = guild.Leader;

        // Transfer leadership
        newLeader.Rank = MemberRank.GuildLeader;
        guild.LeaderId = newLeader.Id;
        guild.Leader = newLeader;
        guild.LeaderName = newLeader.Name;

        // Update new leader's client
        guild.SendGuild(newLeaderClient);
        newLeaderClient.Entity.GuildBattlePower = guild.GetSharedBattlePower(newLeader.Rank);
        newLeaderClient.Entity.GuildRank = (ushort)newLeader.Rank;
        newLeaderClient.Screen.FullWipe();
        newLeaderClient.Screen.Reload();

        // Update old leader if they're online
        if (oldLeader != null && Kernel.TryGetPlayer(oldLeader.Id, out var oldLeaderClient)) {
            oldLeader.Rank = MemberRank.DeputyLeader;
            oldLeaderClient.Entity.GuildRank = (ushort)oldLeader.Rank;
            guild.SendGuild(oldLeaderClient);
            oldLeaderClient.Screen.FullWipe();
            oldLeaderClient.Screen.Reload();
            GuildMemberTable.UpdateGuildAndRank(oldLeader.Id, oldLeader.GuildId, (ushort)oldLeader.Rank);
        }
        else if (oldLeader != null) {
            // Old leader is offline - update their rank in the database
            oldLeader.Rank = MemberRank.DeputyLeader;
            GuildMemberTable.UpdateGuildAndRank(oldLeader.Id, oldLeader.GuildId, (ushort)oldLeader.Rank);
        }

        // Update new leader in database
        GuildMemberTable.UpdateGuildAndRank(newLeader.Id, newLeader.GuildId, (ushort)newLeader.Rank);
        GuildTable.SaveLeader(guild);
        guild.SendMembers(newLeaderClient, 0);
    }

    /// <summary>
    ///     Adds player to blacklist, preventing them from sending join requests to the guild.
    /// </summary>
    private static void HandleBlacklistAdd(GuildCommand command, GameState client) {
        var uid = command.DwParam;
        if (!Kernel.GamePool.TryGetValue(uid, out var c)) return;
        if (!client.Guild!.BlackList.Contains(uid))
            client.Guild.BlackList.Add(uid);
        c.Send(command);
    }

    /// <summary>
    ///     Removes player from blacklist, allowing them to send join requests again.
    /// </summary>
    private static void HandleBlacklistRemove(GuildCommand command, GameState client) {
        var uid = command.DwParam;
        client.Guild!.BlackList.Remove(uid);
        client.Send(command);
    }

    /// <summary>
    ///     Processes silver donation to guild fund, deducting from player's money and updating both guild fund and member donation tracking.
    ///     If the donation is exactly 1 million and the leader has been absent, transfers leadership to the donating member.
    /// </summary>
    private static void HandleDonateSilvers(GuildCommand command, GameState client) {
        if (client.Trade.InTrade)
            return;
        if (client.Entity.Money < command.DwParam) return;

        const ulong leadershipTakeoverAmount = 1_000_000; // 1kk
        var isLeadershipTakeover = command.DwParam == leadershipTakeoverAmount &&
                                   client.Guild!.Leader != null &&
                                   client.Guild.Leader.Id != client.Entity.UID &&
                                   !Kernel.TryGetPlayer(client.Guild.Leader.Id, out _);

        client.Guild.SilverFund += command.DwParam;
        GuildTable.SaveFunds(client.Guild);
        client.AsMember!.SilverDonation += command.DwParam;
        client.Entity.Money -= command.DwParam;
        GuildMemberTable.Save(client.AsMember);
        client.Guild.SendGuild(client);

        // If this is a leadership takeover donation (1kk and leader is absent), transfer leadership
        if (isLeadershipTakeover) {
            HandleLeaderAbsenceTakeover(client);
        }
    }

    /// <summary>
    ///     Processes CP donation to guild fund, deducting from player's CP and updating both guild fund and member donation tracking.
    /// </summary>
    private static void HandleDonateConquerPoints(GuildCommand command, GameState client) {
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

    /// <summary>
    ///     Processes player request to join a guild, checking requirements and blacklist, then adding member if approved.
    /// </summary>
    private static void HandleJoinRequest(GuildCommand command, GameState client) {
        if (!Kernel.GamePool.TryGetValue(command.DwParam, out var target)) return;
        client.GuildJoinTarget = target.Entity.UID;
        if (client.GuildJoinTarget == target.Entity.UID &&
            target.GuildJoinTarget == client.Entity.UID) {
            client.GuildJoinTarget = 0;
            target.GuildJoinTarget = 0;

            if (target.Guild!.BlackList.Contains(client.Entity.UID)) {
                command.Type = 47;
                client.Send(command);
                return;
            }

            if (!Kernel.Guilds.TryGetValue(target.Entity.GuildID, out var g)) return;
            if (target.AsMember!.Rank == MemberRank.Member) return;
            if (client.Entity.GuildID == 0)
                g.AddMember(client);
        }
        else {
            if (!Kernel.Guilds.TryGetValue(target.Entity.GuildID, out var tG)) return;
            if (target.AsMember!.Rank == MemberRank.Member) return;
            if (target.Guild!.BlackList.Contains(client.Entity.UID)) {
                command.Type = 47;
                client.Send(command);
                return;
            }

            if (!PassJoinRequirements(client, tG)) return;
            client.Entity.GuildRequest = Time32.Now;
            command.DwParam = client.Entity.UID;

            var inf = new PopupLevelandBP {
                Level = client.Entity.Level,
                BattlePower = (uint)client.Entity.BattlePower,
                Receiver = target.Entity.UID,
                Requester = client.Entity.UID
            };

            target.Send(inf.ToArray());
            target.Send(command);
        }
    }

    /// <summary>
    ///     Processes guild leader inviting a player, sending invitation request and adding member if both parties agree.
    /// </summary>
    private static void HandleInviteRequest(GuildCommand command, GameState client) {
        if (!Kernel.GamePool.TryGetValue(command.DwParam, out var target)) return;
        client.GuildJoinTarget = target.Entity.UID;
        if (client.GuildJoinTarget == target.Entity.UID &&
            target.GuildJoinTarget == client.Entity.UID) {
            client.GuildJoinTarget = 0;
            target.GuildJoinTarget = 0;

            if (client.Guild!.BlackList.Contains(target.Entity.UID)) {
                command.Type = 49;
                client.Send(command);
                return;
            }

            if (!Kernel.Guilds.TryGetValue(client.Entity.GuildID, out var g)) return;
            if (client.AsMember!.Rank != MemberRank.Member)
                g.AddMember(target);
        }
        else {
            if (client.AsMember!.Rank == MemberRank.Member) return;
            if (client.Guild!.BlackList.Contains(target.Entity.UID)) {
                command.Type = 49;
                client.Send(command);
                return;
            }

            client.Entity.GuildRequest = Time32.Now;
            command.DwParam = client.Entity.UID;
            var inf = new PopupLevelandBP {
                Level = client.Entity.Level,
                BattlePower = (uint)client.Entity.BattlePower,
                Receiver = target.Entity.UID,
                Requester = client.Entity.UID
            };

            target.Send(inf.ToArray());
            target.Send(command);
        }
    }

    /// <summary>
    ///     Handles member leaving the guild voluntarily (cannot be used by Guild Leader).
    /// </summary>
    private static void HandleQuit(GameState client) {
        if (client is { Guild: not null, AsMember.Rank: not MemberRank.GuildLeader })
            client.Guild.ExpelMember(client.Entity.Name, true);
    }

    /// <summary>
    ///     Checks if a player meets the guild's join requirements (level, reborn, class restrictions).
    /// </summary>
    private static bool PassJoinRequirements(GameState client, Guild guild) {
        var cmd = new GuildCommand(true) {
            Type = GuildCommand.GuildRequirements,
            DwParam2 = guild.LevelRequirement,
            DwParam3 = guild.RebornRequirement,
            DwParam4 = guild.ClassRequirement
        };
        if (guild.IsClassAllowed(client.Entity.Class) &&
            client.Entity.Reborn >= guild.RebornRequirement &&
            client.Entity.Level >= guild.LevelRequirement) return true;
        client.Send(cmd);
        return false;
    }

    private static void HandleAlly(byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client.Guild!.Ally.Count < GuildRelations.GetMaxRelations(client.Guild.Level))
            GuildRelations.AllianceConfirmationPopup(name, client);
    }

    /// <summary>
    ///     Adds enemy relationship, marking another guild as an enemy (one-way relationship, no approval needed).
    /// </summary>
    private static void HandleEnemy(byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is { Guild: not null, AsMember.Rank: MemberRank.GuildLeader } &&
            client.Guild.Enemy.Count < GuildRelations.GetMaxRelations(client.Guild.Level))
            GuildRelations.AddEnemy(client.Guild, name);
    }

    /// <summary>
    ///     Removes alliance or enemy relationship, returning to neutral status (mutually removes alliance if exists).
    /// </summary>
    private static void HandleUnallyAndPeace(byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: MemberRank.GuildLeader }) return;
        GuildRelations.RemoveAlly(client.Guild, name);
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name && client.Guild.Name != name)) {
            GuildRelations.RemoveAlly(guild, client.Guild.Name);
        }

        // Remove enemy relationship if it exists (any guild can remove their own enemies)
        var targetGuild = Kernel.Guilds.Values.FirstOrDefault(g => g.Name == name);
        if (targetGuild != null && client.Guild.Enemy.ContainsKey(targetGuild.Id))
            GuildRelations.RemoveEnemy(client.Guild, name);
    }


    /// <summary>
    ///     Sends the guild name to the client when they request guild information.
    ///     Used when a player wants to view details about a specific guild.
    /// </summary>
    /// <param name="command">The guild command containing the guild ID to look up</param>
    /// <param name="client">The client requesting the guild information</param>
    private static void HandleInfo(GuildCommand command, GameState client) {
        if (Kernel.Guilds.TryGetValue(command.DwParam, out var guild)) guild.SendName(client);
    }

    /// <summary>
    ///     Updates the guild's join requirements (level, reborn, class restrictions) when the
    ///     Guild Leader changes them. Only Guild Leaders can modify these requirements, and the
    ///     values are capped to prevent invalid settings. All guild members are notified of the change.
    /// </summary>
    /// <param name="command">The guild command containing the new requirement values</param>
    /// <param name="client">The client (must be Guild Leader) updating the requirements</param>
    private static void HandleChangeRequirements(GuildCommand command, GameState client) {
        if (client.AsMember!.Rank != MemberRank.GuildLeader) return;
        client.Guild!.LevelRequirement = Math.Min(command.DwParam2, 140);
        client.Guild.RebornRequirement = Math.Min(command.DwParam3, 2);
        client.Guild.ClassRequirement = Math.Min(command.DwParam4, 127);
        foreach (var member in client.Guild.Members.Values) {
            if (Kernel.TryGetPlayer(member.Id, out var memberClient))
                client.Guild.SendGuild(memberClient);
        }

        GuildTable.SaveRequirements(client.Guild);
    }

    /// <summary>
    ///     Updates the guild bulletin message when the Guild Leader sets a new announcement.
    ///     The bulletin is displayed to all guild members and stored in the database. Only
    ///     Guild Leaders can modify the bulletin.
    /// </summary>
    /// <param name="packet">The packet containing the bulletin message text</param>
    /// <param name="client">The client (must be Guild Leader) setting the bulletin</param>
    private static void HandleBulletin(byte[] packet, GameState client) {
        var message = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: MemberRank.GuildLeader }) return;
        client.Guild.Bulletin = message;
        client.Guild.CreateBulletinTime();
        client.Guild.SendGuild(client);
        GuildTable.UpdateBulletin(client.Guild, client.Guild.Bulletin);
    }

    /// <summary>
    ///     Refreshes the guild data display for the client. This sends the current guild
    ///     information to the client, updating their view with the latest guild state.
    /// </summary>
    /// <param name="client">The client requesting the refresh</param>
    private static void HandleRefresh(GameState client) {
        if (client is { AsMember: not null, Guild: not null }) client.Guild.SendGuild(client);
    }
}