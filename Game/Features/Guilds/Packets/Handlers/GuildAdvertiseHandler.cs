using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Models;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Game.Features.Guilds.Services;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles guild advertisement packets (2226, 2225, 2227) for browsing, registering, and joining guilds through the advertisement system.
/// </summary>
[PacketHandler(2226, 2225, 2227)]
public static class GuildAdvertiseHandler {
    /// <summary>
    ///     Routes advertisement-related packets to appropriate handlers based on packet ID.
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        switch (packetId) {
            case 2226:
                HandleAdvertiseList(packet, client);
                break;
            case 2225:
                HandleAdvertiseRegister(packet, client);
                break;
            case 2227:
                HandleAdvertiseJoin(packet, client);
                break;
        }

        return true;
    }

    /// <summary>
    ///     Sends paginated guild advertisement list to client, showing top guilds by donation ranking.
    /// </summary>
    private static void HandleAdvertiseList(byte[] packet, GameState client) {
        var receiveCount = BitConverter.ToUInt32(packet, 4);
        if (receiveCount != 0 && receiveCount % 4 != 0)
            return;

        List<Guild> advGuilds = [];
        for (ushort x = 0; x < 4; x++) {
            var getPosition = (ushort)(receiveCount + x);
            if (GuildAdvertise.AdvertiseRanks.Length <= getPosition)
                break;
            advGuilds.Add(GuildAdvertise.AdvertiseRanks[getPosition]);
        }

        switch (advGuilds.Count) {
            case <= 2: {
                var adv = new GuildAdvertisePacket((ushort)advGuilds.Count) {
                    AllRegistered = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount,
                    PacketNo = 1
                };

                for (ushort x = 0; x < advGuilds.Count; x++) {
                    var element = advGuilds[x];
                    adv.Append(element);
                }

                client.Send(adv.ToArray());
                break;
            }
            case 3: {
                var adv = new GuildAdvertisePacket(2) {
                    AllRegistered = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount,
                    PacketNo = 1
                };

                for (ushort x = 0; x < 2; x++) {
                    var element = advGuilds[x];
                    adv.Append(element);
                }

                client.Send(adv.ToArray());

                var ndadv = new GuildAdvertisePacket(1) {
                    AllRegistered = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount
                };

                ndadv.Append(advGuilds.Last());
                client.Send(ndadv.ToArray());
                break;
            }
            case 4: {
                var adv = new GuildAdvertisePacket(2) {
                    AllRegistered = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount,
                    PacketNo = 1
                };
                for (ushort x = 0; x < 2; x++) {
                    var element = advGuilds[x];
                    adv.Append(element);
                }

                client.Send(adv.ToArray());

                var ddddadv = new GuildAdvertisePacket(2) {
                    AllRegistered = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount
                };

                for (ushort x = 2; x < 4; x++) {
                    var element = advGuilds[x];
                    ddddadv.Append(element);
                }

                client.Send(ddddadv.ToArray());
                break;
            }
        }
    }

    /// <summary>
    ///     Registers guild for advertisement with donation cost, updating recruitment settings and bulletin.
    /// </summary>
    private static void HandleAdvertiseRegister(byte[] packet, GameState client) {
        if (client.AsMember is not { Rank: MemberRank.GuildLeader }) return;

        BitConverter.ToUInt32(packet, 4);
        var bulletin = PacketHandler.ReadString(packet, 8, 254);
        ulong donation = BitConverter.ToUInt32(packet, 264);
        var autoJoin = packet[272] == 1;
        var level = packet[274];
        var reborn = packet[276];
        var flag = BitConverter.ToUInt16(packet, 278);
        var grade = packet[280];
        if (bulletin.Contains('^')) {
            client.Entity.SendSysMesage(
                "Your bulletin contains invalid characters (^) !");
            return;
        }

        var guild = client.Guild!;
        if (guild.SilverFund >= donation) {
            guild.SilverFund -= donation;
            guild.AdvertiseRecruit.Bulletin = bulletin;
            guild.AdvertiseRecruit.AutoJoin = autoJoin;
            guild.AdvertiseRecruit.Level = level;
            guild.AdvertiseRecruit.Reborn = reborn;
            guild.AdvertiseRecruit.SetFlag(flag, GuildRecruitment.Mode.Recruit);
            guild.AdvertiseRecruit.Grade = grade;
            guild.AdvertiseRecruit.Donations += donation;

            GuildAdvertise.Add(guild);
            GuildTable.SaveAdvertise(guild);
        }
        else {
            client.Entity.SendSysMesage("you guild use small donation!");
        }
    }

    /// <summary>
    ///     Handles player joining guild through advertisement, either directly (auto-join) or via leader approval.
    /// </summary>
    private static void HandleAdvertiseJoin(byte[] packet, GameState client) {
        switch (packet[4]) {
            case 2: //open gui
            {
                var sendGui = new byte[288];
                Writer.WriteUInt16(280, 0, sendGui);
                Writer.WriteUInt16(2225, 2, sendGui);
                client.Send(sendGui);
                break;
            }
            case 1: //join
            {
                var guildId = BitConverter.ToUInt32(packet, 8);
                if (Kernel.Guilds.TryGetValue(guildId, out var guild))
                    if (guild.AdvertiseRecruit.Compare(client.Entity, GuildRecruitment.Mode.Recruit)) {
                        if (guild.AdvertiseRecruit.AutoJoin) {
                            if (client.Entity.GuildID == 0) guild.AddMember(client);
                        }
                        else {
                            if (client.Entity.GuildID == 0)
                                if (guild.Leader != null &&
                                    Kernel.TryGetPlayer(guild.Leader.Id, out var guildLeaderClient)) {
                                    guildLeaderClient.OnMessageBoxEventParams = [guild, client];
                                    guildLeaderClient.MessageOK = delegate {
                                        if (guildLeaderClient.OnMessageBoxEventParams[0] is Guild guild1 &&
                                            client.Entity.GuildID == 0) guild1.AddMember(client);
                                    };
                                    guildLeaderClient.Send(new NpcReply(
                                        NpcReply.MessageBox,
                                        $"{client.Entity.Name} wants to join your guild through advertisement."
                                    ));
                                }
                        }
                    }

                break;
            }
        }
    }
}