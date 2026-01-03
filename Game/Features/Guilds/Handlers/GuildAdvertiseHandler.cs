using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Handlers;

[PacketHandler(2226, 2225, 2227)]
public static class GuildAdvertiseHandler {
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

    private static void HandleAdvertiseList(byte[] packet, GameState client) {
        var receiveCount = BitConverter.ToUInt32(packet, 4);
        if (receiveCount != 0 && receiveCount % 4 != 0)
            return;

        List<Guild> advGuilds = [];
        for (ushort x = 0; x < 4; x++) {
            var getposition = (ushort)(receiveCount + x);
            if (GuildAdvertise.AdvertiseRanks.Length <= getposition)
                break;
            advGuilds.Add(GuildAdvertise.AdvertiseRanks[getposition]);
        }

        switch (advGuilds.Count) {
            case <= 2: {
                var adv = new Advertise((ushort)advGuilds.Count) {
                    AllRegistred = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount,
                    PacketNo = 1
                };

                for (ushort x = 0; x < advGuilds.Count; x++) {
                    var element = advGuilds[x];
                    adv.Aprend(element);
                }

                client.Send(adv.ToArray());
                break;
            }
            case 3: {
                var adv = new Advertise(2) {
                    AllRegistred = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount,
                    PacketNo = 1
                };

                for (ushort x = 0; x < 2; x++) {
                    var element = advGuilds[x];
                    adv.Aprend(element);
                }

                client.Send(adv.ToArray());

                var ndadv = new Advertise(1) {
                    AllRegistred = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount
                };

                ndadv.Aprend(advGuilds.Last());
                client.Send(ndadv.ToArray());
                break;
            }
            case 4: {
                var adv = new Advertise(2) {
                    AllRegistred = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount,
                    PacketNo = 1
                };
                for (ushort x = 0; x < 2; x++) {
                    var element = advGuilds[x];
                    adv.Aprend(element);
                }

                client.Send(adv.ToArray());

                var ddddadv = new Advertise(2) {
                    AllRegistred = (ushort)GuildAdvertise.AdvertiseRanks.Length,
                    AtCount = (ushort)receiveCount
                };

                for (ushort x = 2; x < 4; x++) {
                    var element = advGuilds[x];
                    ddddadv.Aprend(element);
                }

                client.Send(ddddadv.ToArray());
                break;
            }
        }
    }

    private static void HandleAdvertiseRegister(byte[] packet, GameState client) {
        if (client.AsMember is not { Rank: MemberRank.GuildLeader }) return;

        BitConverter.ToUInt32(packet, 4);
        var buletin = PacketHandler.ReadString(packet, 8, 254);
        ulong donation = BitConverter.ToUInt32(packet, 264);
        var autoJoin = packet[272] == 1;
        var level = packet[274];
        var reborn = packet[276];
        var flag = BitConverter.ToUInt16(packet, 278);
        var grade = packet[280];
        if (buletin.Contains('^')) {
            client.Entity.SendSysMesage(
                "Your bulletin contains invalid chararacters (^) !");
            return;
        }

        var guild = client.Guild;
        if (guild.SilverFund >= donation) {
            guild.SilverFund -= donation;
            guild.AdvertiseRecruit.Bulletin = buletin;
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

    private static void HandleAdvertiseJoin(byte[] packet, GameState client) {
        switch (packet[4]) {
            case 2: //open gui
            {
                var sendgui = new byte[288];
                Writer.WriteUInt16(280, 0, sendgui);
                Writer.WriteUInt16(2225, 2, sendgui);
                client.Send(sendgui);
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