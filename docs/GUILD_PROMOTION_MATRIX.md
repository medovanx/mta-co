# Guild Promotion Matrix

This document outlines which ranks can promote to which ranks in the guild system.

## Legend

- ✅ = Can promote to this rank
- ❌ = Cannot promote to this rank
- 💰 = Requires Conquer Points (CPs) to promote
- 🤖 = Auto-assigned based on donations

## Promotion Rules

### Guild Leader

| Target Rank | Can Promote? | Notes |
|------------|--------------|-------|
| Guild Leader | ✅ | Leadership transfer (1 max) |
| Deputy Leader | ✅ | Up to limit based on guild level |
| HDeputyLeader | ✅ 💰 | 650 CPs, up to limit based on guild level |
| HonoraryManager | ✅ 💰 | 320 CPs, up to limit based on guild level |
| HonorarySupervisor | ✅ 💰 | 270 CPs, up to limit based on guild level |
| HonorarySteward | ✅ 💰 | 100 CPs, up to limit based on guild level |
| LSpouseAide | ✅ | Up to limit based on guild level |
| Steward | ✅ | Up to limit based on guild level |
| DeputySteward | ✅ | No limit |
| DLeaderSpouse | ✅ | 1 max |
| DLeaderAide | ✅ | Up to limit based on guild level |
| Aide | ✅ | Up to limit based on guild level |
| ManagerAide | ✅ | Up to limit based on guild level |
| SupervisorAide | ✅ | Up to limit based on guild level |
| Agent | ✅ | No limit |
| TulipAgent | ✅ | 1 per flower type |
| OrchidAgent | ✅ | 1 per flower type |
| RoseAgent | ✅ | 1 per flower type |
| LilyAgent | ✅ | 1 per flower type |
| CPAgent | ✅ | 1 per donation type |
| ArsenalAgent | ✅ | 1 per donation type |
| SilverAgent | ✅ | 1 per donation type |
| GuideAgent | ✅ | 1 per donation type |
| PKAgent | ✅ | 1 per donation type |
| SupervSpouse | ✅ | 1 max |
| ManagerSpouse | ✅ | 1 max |
| StewardSpouse | ✅ | 1 max |
| Follower | ✅ | Up to limit based on guild level |
| TulipFollower | ✅ | 1 per flower type |
| OrchidFollower | ✅ | 1 per flower type |
| RoseFollower | ✅ | 1 per flower type |
| LilyFollower | ✅ | 1 per flower type |
| CPFollower | ✅ | 1 per donation type |
| ArsFollower | ✅ | 1 per donation type |
| SilverFollower | ✅ | 1 per donation type |
| GuideFollower | ✅ | 1 per donation type |
| PKFollower | ✅ | 1 per donation type |
| SeniorMember | ✅ | No limit |
| Member | ✅ | Up to limit based on guild level |

### Deputy Leader / HDeputyLeader / LeaderSpouse

| Target Rank | Can Promote? | Notes |
|------------|--------------|-------|
| Steward | ✅ | Up to limit based on guild level |
| HonorarySteward | ✅ | Up to limit based on guild level |
| DLeaderAide | ✅ | Up to limit based on guild level |
| Follower | ✅ | Up to limit based on guild level |
| Member | ✅ | Up to limit based on guild level |

### Manager / HonoraryManager

| Target Rank | Can Promote? | Notes |
|------------|--------------|-------|
| ManagerAide | ✅ | Up to limit based on guild level |

### Supervisor / HonorarySupervisor / TSupervisor / OSupervisor / CPSupervisor / ASupervisor / SSupervisor / GSupervisor / PKSupervisor / RoseSupervisor / LilySupervisor

| Target Rank | Can Promote? | Notes |
|------------|--------------|-------|
| SupervisorAide | ✅ | Up to limit based on guild level |

### Agent

| Target Rank | Can Promote? | Notes |
|------------|--------------|-------|
| Aide | ✅ | Up to limit based on guild level |

## Auto-Assignment Ranks

The following ranks are automatically assigned based on donations (cannot be manually promoted):

- **Manager** 🤖 - Top 1 player by Arsenal donation
- **Supervisor** 🤖 - Top players by donation type (CP, PK, Rose, Lily, Tulip, Orchid, Silver, Guide)
- **Agent** 🤖 - Top 2 players by donation type
- **Follower** 🤖 - Top 2 players by donation type

## Notes

1. Honorary ranks (HDeputyLeader, HonoraryManager, HonorarySupervisor, HonorarySteward) require CPs and must be manually appointed by the Guild Leader.
2. Rank limits vary based on guild level (see `GuildRankLimits.cs` for details).
3. Some ranks have special restrictions (e.g., only 1 per flower/donation type).
4. Auto-assigned ranks are recalculated periodically based on donation performance.

