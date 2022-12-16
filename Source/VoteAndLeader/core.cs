using System.Collections.Generic;
using System.Linq;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace election;

public class core : ModBase
{
    public static int val_elecCycle;
    public static bool val_testMode;
    public static bool val_debugMode;


    public static int val_readyDay = 3;

    public static int val_problemDay = 1;

    public static int val_assistDay = 2;


    // -----------------------------------------

    private static myData data;
    public static int tickGame;
    private static List<Pawn> ar_voter;
    private static List<pawnAndInt> ar_candi;
    private static List<pawnAndFloat> ar_score;
    private static Pawn p;
    private static List<pawnAndInt> ar_pi;
    private static List<int> ar_int;
    private static int i;
    private static float f;
    private static float f2;
    private static string str;
    private static string str2;
    private static string str3;
    private static string str4;
    private static List<string> ar_string;

    private static bool flag;

    private SettingHandle<bool> val_debugMode_s;

    private SettingHandle<int> val_elecCycle_s;

    private SettingHandle<bool> val_testMode_s;
    public override string ModIdentifier => "election";

    public static Faction PlayerFaction => Find.FactionManager.OfPlayer;

    public static Pawn Leader => PlayerFaction.leader == null ? null :
        !getColonists().Contains(PlayerFaction.leader) ? null : PlayerFaction.leader;

    public List<MemeDef> LeaderMemes => PlayerFaction.leader == null ? null : Leader.Ideo.memes;

    private en_genderRule leaderGenderRule => Leader == null ? en_genderRule.equal :
        !getColonists().Contains(PlayerFaction.leader) ? en_genderRule.equal : getPawnGenderRule(PlayerFaction.leader);

    public override void DefsLoaded()
    {
        val_elecCycle_s = Settings.GetHandle("val_elecCycle", "val_elecCycle_t".Translate(),
            "val_elecCycle_d".Translate(), 60);
        val_testMode_s = Settings.GetHandle<bool>("val_testMode", "test mode", "fast event");
        //val_debugMode_s = Settings.GetHandle<bool>("val_debugMode", "debug mode", "log, cheat");
        SettingsChanged();
    }

    public override void SettingsChanged()
    {
        val_elecCycle_s.Value = Mathf.Clamp(val_elecCycle_s.Value, val_readyDay + 5, 999);
        val_elecCycle = val_elecCycle_s.Value;

        val_testMode = val_testMode_s.Value;

        //val_debugMode = val_debugMode_s.Value;
        val_debugMode = false;
    }

    public override void WorldLoaded()
    {
        base.WorldLoaded();
        data = dataUtility.GetData();
    }


    public override void Tick(int currentTick)
    {
        tickGame = Find.TickManager.TicksGame;

        if (!val_testMode && tickGame < GenDate.TicksPerDay * (5 - val_problemDay))
        {
            return;
        }

        if (val_debugMode && tickGame % 500 == 0)
        {
            if (val_debugMode)
            {
                Log.Message(
                    $"readyTick {(float)(data.readyTick - tickGame) / GenDate.TicksPerDay:0.#}, elecTick {(float)(data.elecTick - tickGame) / GenDate.TicksPerDay:0.#}, assistTick {(float)(data.assistTick - tickGame) / GenDate.TicksPerDay:0.#}");
            }
        }

        // 처음 또는 버그상태 해결
        if (data.readyTick < tickGame && data.elecTick < tickGame)
        {
            data.readyTick = tickGame + (!val_testMode ? GenDate.TicksPerDay * val_problemDay : GenDate.TicksPerHour);
            if (val_debugMode)
            {
                Log.Message($"# election ready tick : {data.readyTick}");
            }
        }

        // 지도자 없는 상태 해결
        p = Leader;
        if (p == null || !getColonists().Contains(p) || p.Dead || p.Destroyed)
        {
            if (data.readyTick > tickGame + (GenDate.TicksPerDay * val_problemDay))
            {
                data.readyTick =
                    tickGame + (!val_testMode ? GenDate.TicksPerDay * val_problemDay : GenDate.TicksPerHour);
                if (val_debugMode)
                {
                    Log.Message($"# election ready tick : {data.readyTick}");
                }
            }
        }

        // 후보설정
        if (tickGame == data.readyTick)
        {
            setCandi();
        }

        // 선거결과
        if (tickGame == data.elecTick)
        {
            election();
        }

        if (tickGame == data.assistTick)
        {
            setAssist();
        }
    }


    private void setCandi()
    {
        if (val_debugMode)
        {
            Log.Message("-----------------------------------------");
        }

        if (val_debugMode)
        {
            Log.Message("# try setCandi");
        }
        //if (getColonists().Count < 3)
        //{
        //    // 정착민이 3명 미만임
        //    data.readyTick = tickGame + (!val_testMode?(GenDate.TicksPerDay * val_problemDay):GenDate.TicksPerHour);
        //    if (val_debugMode) Log.Message($"! can not make leader. colonists number is under 3");
        //    return;
        //}


        // 후보 등록
        str = "";
        str2 = "";
        data.ar_candi = new List<Pawn>();
        foreach (var pawn in getColonists())
        {
            if (!checkLeaderGenderRule(pawn, pawn, true))
            {
                str2 += $"{pawn.NameShortColored} ({"r_ideoSelf".Translate()})\n";
                continue;
            }

            if (pawn.InMentalState)
            {
                str2 += $"{pawn.NameShortColored} ({"c_mental".Translate()})\n";
                continue;
            }

            if (pawn.needs.mood.CurInstantLevelPercentage <= 0.1f)
            {
                str2 += $"{pawn.NameShortColored} ({"c_lowMood".Translate()})\n";
                continue;
            }

            if (pawn.guilt.IsGuilty)
            {
                str2 += $"{pawn.NameShortColored} ({"c_guilt".Translate()})\n";
                continue;
            }

            if (Rand.Chance(0.2f))
            {
                str2 += $"{pawn.NameShortColored} ({"c_noInterest".Translate()})\n";
                continue;
            }

            data.ar_candi.Add(pawn);
            str += $"{pawn.NameShortColored}\n";
        }


        if (data.ar_candi.Count <= 0)
        {
            // 가능한 후보 없음 - 해결 시도
            str = "";
            str2 = "";
            foreach (var pawn in getColonists())
            {
                if (!checkLeaderGenderRule(pawn, pawn, true))
                {
                    str2 += $"{pawn.NameShortColored} ({"r_ideoSelf".Translate()})\n";
                    continue;
                }

                data.ar_candi.Add(pawn);
                str += $"{pawn.NameShortColored}\n";
            }
        }

        if (data.ar_candi.Count <= 0)
        {
            // 가능한 후보 없음
            data.readyTick = tickGame + (!val_testMode ? GenDate.TicksPerDay * val_problemDay : GenDate.TicksPerHour);
            if (val_debugMode)
            {
                Log.Message("! no candidate");
            }

            return;
        }


        // 선거날짜
        data.elecTick = tickGame + (!val_testMode ? GenDate.TicksPerDay * val_readyDay : GenDate.TicksPerHour);
        if (val_debugMode)
        {
            Log.Message($"# election tick : {data.elecTick}");
        }


        foreach (var pawn in data.ar_candi)
        {
            if (val_debugMode)
            {
                Log.Message($"{pawn.NameShortColored}");
            }
        }

        Find.LetterStack.ReceiveLetter(
            string.Format("letter_candiList_t".Translate()),
            string.Format("letter_candiList_d".Translate(), str, str2) + "\n\n" + get_systemEffectVote(Leader),
            LetterDefOf.PositiveEvent
        );
    }


    private void election()
    {
        if (val_debugMode)
        {
            Log.Message("-----------------------------------------");
        }

        if (val_debugMode)
        {
            Log.Message("# try election");
        }
        //if (getColonists().Count < 3)
        //{
        //    // 정착민이 3명 미만임
        //    data.readyTick = tickGame + (!val_testMode ? (GenDate.TicksPerDay * val_problemDay) : GenDate.TicksPerHour);
        //    Find.LetterStack.ReceiveLetter(
        //        string.Format("letter_elecFail_t".Translate()),
        //        string.Format("letter_elecFail_colonistNum_d".Translate(), 3.ToString()),
        //        LetterDefOf.NegativeEvent
        //    );
        //    return;
        //}


        // 후보 리스트 체크
        ar_candi = new List<pawnAndInt>();
        data.ar_candi.RemoveAll(a => a == null);
        data.ar_candi.RemoveAll(a => a.Dead || a.Destroyed);
        foreach (var pawn in data.ar_candi)
        {
            ar_candi.Add(new pawnAndInt(pawn, 0));
        }


        if (ar_candi.Count <= 0)
        {
            // 가능한 후보 없음
            data.readyTick = tickGame + (!val_testMode ? GenDate.TicksPerDay * val_problemDay : GenDate.TicksPerHour);
            Find.LetterStack.ReceiveLetter(
                string.Format("letter_elecFail_t".Translate()),
                string.Format("letter_elecFail_noCandi_d".Translate()),
                LetterDefOf.NegativeEvent
            );
            return;
        }


        // 투표자 리스트 초기화
        ar_voter = getColonists();


        str = "";
        str2 = "";
        if (ar_candi.Count >= 1)
        {
            foreach (var voter in ar_voter)
            {
                // 투표 거부
                if (voter.InMentalState)
                {
                    str += $"{voter.NameShortColored} ({"c_mental".Translate()})\n";
                    continue;
                }

                if (voter.needs.mood.CurInstantLevelPercentage <= 0.1f)
                {
                    str += $"{voter.NameShortColored} ({"c_lowMood".Translate()})\n";
                    continue;
                }

                if (Rand.Chance(0.05f))
                {
                    str += $"{voter.NameShortColored} ({"c_noInterest".Translate()})\n";
                    continue;
                }


                ar_score = new List<pawnAndFloat>(); // 후보 점수 초기화
                foreach (var pi in ar_candi)
                {
                    ar_score.Add(new pawnAndFloat(pi.p, 0f));
                }

                if (val_debugMode)
                {
                    Log.Message($"# {voter.NameShortColored} start vote ...");
                }

                foreach (var pi in ar_score)
                {
                    pi.f = getScore(voter, pi.p, out str2, false, SkillDefOf.Social);
                    if (val_debugMode)
                    {
                        Log.Message($"{pi.p.NameShortColored} : {pi.f}");
                    }
                }

                p = ar_score.MaxBy(a => a.f).p; // 최고점수
                var vote = getVoteNumOfPawn(voter);
                ar_candi.Find(a => a.p == p).i += vote; // 투표

                getScore(voter, p, out str2, true, SkillDefOf.Social);
                str +=
                    $"{voter.NameShortColored} >>>>> {p.NameShortColored} +{vote}{"vote".Translate()}({"reason".Translate()}: {str2})\n";

                if (val_debugMode)
                {
                    Log.Message($"vote to {p.NameShortColored}");
                }
            }
        }


        if (val_debugMode)
        {
            Log.Message("# vote result");
        }

        foreach (var pi in ar_candi)
        {
            if (val_debugMode)
            {
                Log.Message($"{pi.p.NameShortColored} : {pi.i}");
            }
        }


        str4 = get_systemEffectVote(Leader);


        // 당선
        p = ar_candi.Count >= 2 ? ar_candi.MaxBy(a => a.i).p : ar_candi[0].p;

        if (Leader != p)
        {
            if (Leader?.Ideo.GetRole(Leader) != null && Leader.Ideo.GetRole(Leader).def.leaderRole)
            {
                Leader.Ideo.GetRole(Leader).Unassign(Leader, false);
            }

            p.Ideo.RecachePossibleRoles();
            p.Ideo.RolesListForReading.Find(a => a.def == PreceptDefOf.IdeoRole_Leader).Assign(p, true);
        }

        if (val_debugMode)
        {
            Log.Message($"now {p.NameShortColored} is leader");
        }

        if (val_debugMode)
        {
            Log.Message("-----------------------------------------");
        }


        ar_string = new List<string>();
        foreach (var pi in ar_candi.OrderByDescending(a => a.i).ToList())
        {
            ar_string.Add($"{pi.p.NameShortColored}({pi.i}{"vote".Translate()})");
        }

        str3 = string.Join(", ", ar_string);

        // 다음 준비 날짜
        data.readyTick = tickGame +
                         (!val_testMode
                             ? GenDate.TicksPerDay * (val_elecCycle - val_readyDay)
                             : GenDate.TicksPerHour * 2);
        data.assistTick = tickGame + (!val_testMode ? GenDate.TicksPerDay * val_assistDay : GenDate.TicksPerHour);
        if (val_debugMode)
        {
            Log.Message($"# election ready tick : {data.readyTick}");
        }

        if (val_debugMode)
        {
            Log.Message($"# assist tick : {data.assistTick}");
        }

        if (val_debugMode)
        {
            Log.Message("-----------------------------------------");
        }


        Find.LetterStack.ReceiveLetter(
            string.Format("letter_election_t".Translate()),
            str4 + "\n----------------------------------------------\n\n" +
            string.Format("letter_election_d".Translate(), p.NameShortColored, str3, str, val_elecCycle),
            LetterDefOf.PositiveEvent
        );
    }

    public static void setAssist()
    {
        if (val_debugMode)
        {
            Log.Message("-----------------------------------------");
        }

        if (val_debugMode)
        {
            Log.Message("# try setAssist");
        }

        var leader = Leader;
        if (leader == null)
        {
            return;
        }

        // 배정가능한 직위 리스트
        var ar_role = new List<Precept_Role>();
        foreach (var ideo in PlayerFaction.ideos.AllIdeos.ToList())
        {
            ideo.RecachePossibleRoles();
            foreach (var r in ideo.cachedPossibleRoles)
            {
                if (!r.Active)
                {
                    continue;
                }

                if (r.def.leaderRole)
                {
                    continue;
                }

                ar_role.Add(r);
            }
        }


        // 배정시킬 폰 리스트, 순서
        ar_pi = new List<pawnAndInt>();
        foreach (var tmp_pawn in getColonists())
        {
            ar_pi.Add(new pawnAndInt(tmp_pawn, 0));
        }

        ar_pi.RemoveAll(a => a.p == leader);
        foreach (var pi in ar_pi)
        {
            pi.i = (int)getScore(leader, pi.p, out str2, false, SkillDefOf.Social);
        }

        ar_pi = new List<pawnAndInt>(ar_pi.OrderByDescending(a => a.i));

        // 직위 배정
        str = "";
        str2 = "";

        Ideo prevIdeo = null;
        foreach (var r in ar_role)
        {
            if (prevIdeo != r.ideo)
            {
                str += $"\n- {r.ideo.name.Colorize(r.ideo.Color)} -\n";
                prevIdeo = r.ideo;
            }

            flag = false;
            foreach (var pi in ar_pi)
            {
                p = pi.p;
                if (!r.RequirementsMet(p)) // 요구사항 체크
                {
                    continue;
                }

                flag = true;
                r.Assign(p, true);
                getScore(leader, pi.p, out str2, true);
                if (r.ideo != null)
                {
                    str +=
                        $"{r.Label.Colorize(r.ideo.Color)} ({r.def.label.Colorize(r.ideo.Color)}) : {p.NameShortColored} ({"reason".Translate()}: {str2})\n";
                }

                ar_pi.Remove(pi);
                break;
            }

            if (!flag)
            {
                str +=
                    $"{r.Label.Colorize(Color.gray)} ({r.def.label.Colorize(Color.gray)}) : {"c_noCollect".Translate().Colorize(Color.gray)}\n";
            }

            if (ar_pi.Count <= 0)
            {
                break;
            }
        }


        // 레터
        Find.LetterStack.ReceiveLetter(
            string.Format("letter_assist_t".Translate()),
            string.Format("letter_assist_d".Translate(), leader.NameShortColored, str),
            LetterDefOf.PositiveEvent
        );

        data.assistTick = -1;
    }

    public static void checkAssist()
    {
        if (val_debugMode)
        {
            Log.Message("-----------------------------------------");
        }

        if (val_debugMode)
        {
            Log.Message("# try setAssist");
        }

        var leader = Leader;
        if (leader == null)
        {
            return;
        }

        // 배정가능한 직위 리스트
        var ar_role = new List<Precept_Role>();
        foreach (var ideo in PlayerFaction.ideos.AllIdeos.ToList())
        {
            ideo.RecachePossibleRoles();
            foreach (var r in ideo.cachedPossibleRoles)
            {
                if (!r.Active)
                {
                    continue;
                }

                if (r.def.leaderRole)
                {
                    continue;
                }

                if (r.ChosenPawnSingle() != null)
                {
                    continue;
                }

                ar_role.Add(r);
            }
        }


        // 배정시킬 폰 리스트, 순서
        ar_pi = new List<pawnAndInt>();
        foreach (var tmp_pawn in getColonists())
        {
            ar_pi.Add(new pawnAndInt(tmp_pawn, 0));
        }

        ar_pi.RemoveAll(a => a.p == leader);
        ar_pi.RemoveAll(a => a.p.Ideo.GetRole(a.p) != null);
        foreach (var pi in ar_pi)
        {
            pi.i = (int)getScore(leader, pi.p, out str2, false, SkillDefOf.Social);
        }

        ar_pi = new List<pawnAndInt>(ar_pi.OrderByDescending(a => a.i));

        // 직위 배정
        str = "";
        str2 = "";

        Ideo prevIdeo = null;
        foreach (var r in ar_role)
        {
            if (prevIdeo != r.ideo)
            {
                str += $"\n- {r.ideo.name.Colorize(r.ideo.Color)} -\n";
                prevIdeo = r.ideo;
            }

            flag = false;
            foreach (var pi in ar_pi)
            {
                p = pi.p;
                if (!r.RequirementsMet(p)) // 요구사항 체크
                {
                    continue;
                }

                flag = true;
                r.Assign(p, true);
                getScore(leader, pi.p, out str2, true);
                if (r.ideo != null)
                {
                    str +=
                        $"{r.Label.Colorize(r.ideo.Color)} ({r.def.label.Colorize(r.ideo.Color)}) : {p.NameShortColored} ({"reason".Translate()}: {str2})\n";
                }

                ar_pi.Remove(pi);
                break;
            }

            if (!flag)
            {
                str +=
                    $"{r.Label.Colorize(Color.gray)} ({r.def.label.Colorize(Color.gray)}) : {"c_noCollect".Translate().Colorize(Color.gray)}\n";
            }

            if (ar_pi.Count <= 0)
            {
                break;
            }
        }


        // 레터
        Find.LetterStack.ReceiveLetter(
            string.Format("letter_assist_t".Translate()),
            string.Format("letter_assist_d".Translate(), leader.NameShortColored, str),
            LetterDefOf.PositiveEvent
        );

        data.assistTick = -1;
    }


    private static float getScore(Pawn thinker, Pawn about, out string reason, bool makeReason = false,
        SkillDef skill = null)
    {
        reason = "";
        f = 0f;
        ar_string = new List<string>();
        var self = thinker == about;


        f2 = self ? 100f : 0f;
        f += f2;
        if (makeReason && f2 >= 1)
        {
            ar_string.Add("r_self".Translate());
        }


        if (!self)
        {
            f2 = thinker.relations.OpinionOf(about) + (about.relations.OpinionOf(thinker) * 0.5f);
            f += f2;
            if (makeReason && f2 >= 40)
            {
                ar_string.Add("r_opinion".Translate());
            }

            f2 = thinker.relations.FamilyByBlood.ToList().Find(a => a == about) != null ? 50f : 0f;
            f += f2;
            if (makeReason && f2 >= 1)
            {
                ar_string.Add("r_family".Translate());
            }

            f2 = thinker.GetLoveRelations(false).Find(a => a.otherPawn == about) != null ? 50f : 0f;
            f += f2;
            if (makeReason && f2 >= 1)
            {
                ar_string.Add("r_love".Translate());
            }

            f2 = about.guilt.IsGuilty ? -50f : 0f;
            f += f2;

            f2 = thinker.Ideo == about.Ideo ? 50f : 0f;
            f += f2;
            if (makeReason && f2 >= 1)
            {
                ar_string.Add("r_ideo".Translate());
            }
        }


        if (skill != null)
        {
            f2 = about.skills.GetSkill(skill).Level * 5f * about.GetStatValue(StatDefOf.SocialImpact);
            f += f2;
            if (makeReason && f2 >= 40)
            {
                ar_string.Add(skill.skillLabel);
            }
        }

        // 성별
        switch (getPawnGenderRule(thinker))
        {
            case en_genderRule.male:
                if (about.gender == Gender.Male)
                {
                    f += 50f;
                    if (makeReason)
                    {
                        ar_string.Add(MemeDefOf.MaleSupremacy.label);
                    }
                }

                break;
            case en_genderRule.female:
                if (about.gender == Gender.Female)
                {
                    f += 50f;
                    if (makeReason)
                    {
                        ar_string.Add(MemeDefOf.FemaleSupremacy.label);
                    }
                }

                break;
        }


        // 집단주의
        if (thinker.Ideo.memes.Contains(MemeDefOfY.Collectivist))
        {
            f2 = (about == Leader ? 50f : 0f) + (thinker == Leader ? 50f : 0f);
            f += f2;
            if (makeReason && f2 >= 1)
            {
                ar_string.Add(MemeDefOfY.Collectivist.label);
            }
        }


        // 신념 전파
        if (thinker.Ideo.memes.Contains(MemeDefOfY.Proselytizer))
        {
            f2 = thinker.Ideo == about.Ideo ? 50f : 0f;
            f += f2;
            if (makeReason && f2 >= 1)
            {
                ar_string.Add(MemeDefOfY.Proselytizer.label);
            }
        }


        if (makeReason)
        {
            reason = string.Join(", ", ar_string);
        }

        if (makeReason && reason == "")
        {
            reason = "-";
        }

        return f;
    }


    private static List<Pawn> getColonists()
    {
        var ar = new List<Pawn>();
        ar.AddRange(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists);
        //ar.AddRange(Find.ColonistBar.GetColonistsInOrder());
        ar.RemoveAll(a => a == null);
        ar.RemoveAll(a => a.Dead || a.Destroyed);
        ar.RemoveAll(a => a.IsSlave);
        ar.RemoveAll(a => a.IsPrisoner);
        ar.RemoveAll(a => a.IsQuestLodger());
        ar.RemoveAll(a => a.IsQuestHelper());
        ar.Shuffle();
        return ar;
    }


    private string get_systemEffectVote(Pawn p)
    {
        return p == null
            ? ""
            : string.Format("systemEffectVote".Translate(), p.NameShortColored, p.Ideo.name.Colorize(p.Ideo.Color),
                get_str_benefitVotePeople());
    }

    private string get_str_benefitVotePeople()
    {
        if (Leader == null)
        {
            return "";
        }

        var ar_str = new List<string>();
        switch (leaderGenderRule)
        {
            case en_genderRule.male:
                switch (p.gender)
                {
                    case Gender.Male:
                        ar_str.Add($"{"Male".Translate()}({MemeDefOf.MaleSupremacy.label})");
                        break;
                }

                break;
            case en_genderRule.female:
                switch (p.gender)
                {
                    case Gender.Female:
                        ar_str.Add($"{"Female".Translate()}({MemeDefOf.FemaleSupremacy.label})");
                        break;
                }

                break;
        }

        if (LeaderMemes.Contains(MemeDefOfY.Collectivist))
        {
            ar_str.Add($"{"Leader".Translate()}({MemeDefOfY.Collectivist.label})");
        }

        if (LeaderMemes.Contains(MemeDefOfY.Proselytizer))
        {
            ar_str.Add(
                $"{PlayerFaction.ideos.PrimaryIdeo.RolesListForReading.Find(a => a.def.defName == "IdeoRole_Moralist").def.label}({MemeDefOfY.Proselytizer.label})");
        }

        return string.Join(", ", ar_str);
    }


    private int getVoteNumOfPawn(Pawn p)
    {
        var n = 1;

        if (Leader == null)
        {
            return n;
        }

        if (LeaderMemes.Contains(MemeDefOfY.Collectivist) && p == Leader)
        {
            n += Mathf.Max(1, getColonists().Count / 4);
        }

        if (LeaderMemes.Contains(MemeDefOfY.Proselytizer) && p.Ideo.GetRole(p)?.def.defName == "IdeoRole_Moralist")
        {
            n += 1;
        }

        if (checkLeaderGenderRule(Leader, p))
        {
            n += 1;
        }

        return n;
    }

    private static en_genderRule getPawnGenderRule(Pawn p)
    {
        if (p == null)
        {
            return en_genderRule.equal;
        }

        if (p.Ideo.memes.Contains(MemeDefOf.MaleSupremacy))
        {
            return en_genderRule.male;
        }

        return p.Ideo.memes.Contains(MemeDefOf.FemaleSupremacy) ? en_genderRule.female : en_genderRule.equal;
    }

    private bool checkLeaderGenderRule(Pawn thinker, Pawn about, bool defaultReturn = false)
    {
        switch (getPawnGenderRule(thinker))
        {
            default:
                return defaultReturn;
            case en_genderRule.male:
                return about.gender == Gender.Male;

            case en_genderRule.female:
                return about.gender == Gender.Female;
        }
    }

    public class pawnAndInt
    {
        public int i;
        public Pawn p;

        public pawnAndInt(Pawn _p, int _i)
        {
            p = _p;
            i = _i;
        }
    }

    public class pawnAndFloat
    {
        public float f;
        public Pawn p;

        public pawnAndFloat(Pawn _p, float _f)
        {
            p = _p;
            f = _f;
        }
    }


    private enum en_genderRule
    {
        equal,
        male,
        female
    }
}