using System.Text;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Features.Guilds.Models;

public class GuildRecruitment {
    public enum Mode {
        Requirements,
        Recruit
    }

    public bool AutoJoin = true;
    public string Bulletin = "Nothing";
    public ulong Donations;
    public byte Grade;
    public byte Level;
    public int NotAllowFlag;
    public byte Reborn;

    public bool WasLoad;

    public bool ContainFlag(int val) {
        return (NotAllowFlag & val) == val;
    }

    public void AddFlag(int val) {
        if (!ContainFlag(val))
            NotAllowFlag |= val;
    }

    public void Remove(int val) {
        if (ContainFlag(val))
            NotAllowFlag &= ~val;
    }

    public void SetFlag(int mFlag, Mode mod) {
        switch (mod) {
            case Mode.Requirements: {
                switch (mFlag) {
                    case 0:
                        NotAllowFlag = RecruitmentFlags.NoneBlock;
                        break;
                    case >= 127:
                        AddFlag(RecruitmentFlags.Trojan | RecruitmentFlags.Warrior | RecruitmentFlags.Taoist |
                                RecruitmentFlags.Archer | RecruitmentFlags.Ninja |
                                RecruitmentFlags.Monk | RecruitmentFlags.Pirate);
                        break;
                }

                var nFlag = 127 - mFlag;
                AddFlag(nFlag);
                break;
            }
            case Mode.Recruit: {
                if (mFlag == 0) NotAllowFlag = RecruitmentFlags.NoneBlock;
                AddFlag(mFlag);
                break;
            }
        }
    }

    public bool Compare(Entity player, Mode mod) {
        if (player.Level < Level)
            return false;
        if (player.Reborn < Reborn && Reborn != 0)
            return false;
        if (IsArcher(player.Class) && ContainFlag(RecruitmentFlags.Archer))
            return false;
        if (IsTaoist(player.Class) && ContainFlag(RecruitmentFlags.Taoist))
            return false;
        if (IsWarrior(player.Class) && ContainFlag(RecruitmentFlags.Warrior))
            return false;
        if (IsTrojan(player.Class) && ContainFlag(RecruitmentFlags.Trojan))
            return false;
        if (IsPirate(player.Class) && ContainFlag(RecruitmentFlags.Pirate))
            return false;
        if (IsMonk(player.Class) && ContainFlag(RecruitmentFlags.Monk))
            return false;
        if (IsNinja(player.Class) && ContainFlag(RecruitmentFlags.Ninja))
            return false;
        if (mod != Mode.Recruit) return true;
        return Grade == 0 || true;
    }

    public override string ToString() {
        var build = new StringBuilder();
        build.Append(NotAllowFlag + "^" + Level + "^" + Reborn + "^" + Grade + "^" + Donations + "^"
                     + (byte)(AutoJoin ? 1 : 0) + "^" + Bulletin + "^0" + "^0");
        return build.ToString();
    }

    public void Load(string line) {
        if (line == "") return;
        if (!line.Contains('^')) return;
        var data = line.Split('^');
        NotAllowFlag = int.Parse(data[0]);
        Level = byte.Parse(data[1]);
        Reborn = byte.Parse(data[2]);
        Grade = byte.Parse(data[3]);
        Donations = ulong.Parse(data[4]);
        AutoJoin = byte.Parse(data[5]) == 1;
        Bulletin = data[6];
        WasLoad = true;
    }

    public static void Save() { }

    private static class RecruitmentFlags {
        public const int
            NoneBlock = 0,
            Trojan = 1,
            Warrior = 2,
            Taoist = 4,
            Archer = 8,
            Ninja = 16,
            Monk = 32,
            Pirate = 64;
    }
}