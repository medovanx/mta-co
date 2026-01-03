using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Services;

/// <summary>
///     Manages guild advertisement rankings for the top guilds by donation.
/// </summary>
public static class GuildAdvertise {
    private static readonly ConcurrentDictionary<uint, Guild> AGuilds = new();

    private static Guild[] _advertiseRanks = [];

    public static Guild[] AdvertiseRanks {
        get {
            lock (_advertiseRanks) {
                return _advertiseRanks;
            }
        }
    }

    /// <summary>
    ///     Adds guild to advertisement system and recalculates rankings based on donation totals.
    /// </summary>
    public static void Add(Guild obj) {
        if (!AGuilds.ContainsKey(obj.Id))
            AGuilds.TryAdd(obj.Id, obj);
        CalculateRanks();
    }

    private static void CalculateRanks() {
        lock (_advertiseRanks) {
            var array = AGuilds.Values.ToArray();
            array =
                (from guild in array orderby guild.AdvertiseRecruit.Donations descending select guild)
                .ToArray();
            List<Guild> guilds = [];
            for (ushort x = 0; x < array.Length; x++) {
                guilds.Add(array[x]);
                if (x == 40) break;
            }

            _advertiseRanks = guilds.ToArray();
        }
    }

    /// <summary>
    ///     Rebuilds advertisement dictionary from current rankings, used when reloading guild data.
    /// </summary>
    public static void FixedRank() {
        AGuilds.Clear();
        foreach (var guild in _advertiseRanks) {
            AGuilds.TryAdd(guild.Id, guild);
        }
    }
}