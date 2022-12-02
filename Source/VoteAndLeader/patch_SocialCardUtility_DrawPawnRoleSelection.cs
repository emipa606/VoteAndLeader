using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace election;

[HarmonyPatch(typeof(SocialCardUtility))]
[HarmonyPatch("DrawPawnRoleSelection")]
internal class patch_SocialCardUtility_DrawPawnRoleSelection
{
    private static readonly List<Precept_Role> cachedRoles = AccessTools
        .StaticFieldRefAccess<List<Precept_Role>>(AccessTools.Field(typeof(SocialCardUtility), "cachedRoles")).Invoke();

    [HarmonyPostfix]
    private static bool Prefix(Pawn pawn, Rect rect, Vector2 ___RoleChangeButtonSize)
    {
        if (core.val_debugMode)
        {
            return true;
        }

        if (pawn == core.Leader)
        {
            return false;
        }

        if (pawn.Ideo.GetRole(pawn) == null)
        {
            return false;
        }

        if (!pawn.IsFreeNonSlaveColonist)
        {
            return false;
        }

        var precept_Role = pawn.Ideo?.GetRole(pawn);
        var primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
        var roleChangeRitual = (Precept_Ritual)pawn.Ideo?.GetPrecept(PreceptDefOf.RoleChange);
        //TargetInfo ritualTarget = roleChangeRitual.targetFilter.BestTarget(pawn, TargetInfo.Invalid);
        if (!(cachedRoles.Any() && pawn.Ideo != null))
        {
            GUI.color = Color.gray;
        }

        var y = rect.y + (rect.height / 2f) - 14f;
        var rect2 = new Rect(rect.width - 150f, y, ___RoleChangeButtonSize.x, ___RoleChangeButtonSize.y);
        rect2.xMax = rect.width - 26f - 4f;
        if (Widgets.ButtonText(rect2, "ChooseRole".Translate() + "...", true, true,
                cachedRoles.Any() && pawn.Ideo != null))
        {
            //if (ritualTarget.IsValid)
            if (true)
            {
                var list = new List<FloatMenuOption>();
                if (precept_Role != null)
                {
                    list.Add(new FloatMenuOption("RemoveCurrentRole".Translate(), delegate
                    {
                        if (pawn.Ideo.GetRole(pawn) != null)
                        {
                            pawn.Ideo.GetRole(pawn).Unassign(pawn, false);
                        }
                    }, Widgets.PlaceholderIconTex, Color.white));
                }

                //foreach (Precept_Role cachedRole in cachedRoles)
                //{
                //    Precept_Role newRole = cachedRole;
                //    if (newRole != precept_Role && newRole.Active && newRole.RequirementsMet(pawn) && (!newRole.def.leaderRole || pawn.Ideo == primaryIdeo))
                //    {
                //        string label = newRole.LabelForPawn(pawn) + " (" + newRole.def.label + ")";
                //        list.Add(new FloatMenuOption(label, delegate
                //        {
                //            Dialog_BeginRitual dialog_BeginRitual = (Dialog_BeginRitual)roleChangeRitual.GetRitualBeginWindow(ritualTarget, null, null, pawn, new Dictionary<string, Pawn> { { "role_changer", pawn } });
                //            dialog_BeginRitual.SetRoleToChangeTo(newRole);
                //            Find.WindowStack.Add(dialog_BeginRitual);
                //        }, newRole.Icon, newRole.ideo.Color, MenuOptionPriority.Default, DrawTooltip)
                //        {
                //            orderInPriority = newRole.def.displayOrderInImpact
                //        });
                //    }
                //    void DrawTooltip(Rect r)
                //    {
                //        TipSignal tip = new TipSignal(() => newRole.GetTip(), pawn.thingIDNumber * 39);
                //        TooltipHandler.TipRegion(r, tip);
                //    }
                //}
                //foreach (Precept_Role cachedRole2 in cachedRoles)
                //{
                //    if ((cachedRole2 != precept_Role && !cachedRole2.RequirementsMet(pawn)) || !cachedRole2.Active)
                //    {
                //        string text = cachedRole2.LabelForPawn(pawn) + " (" + cachedRole2.def.label + ")";
                //        if (cachedRole2.ChosenPawnSingle() != null)
                //        {
                //            text = text + ": " + cachedRole2.ChosenPawnSingle().LabelShort;
                //        }
                //        else if (!cachedRole2.RequirementsMet(pawn))
                //        {
                //            text = text + ": " + cachedRole2.GetFirstUnmetRequirement(pawn).GetLabel(cachedRole2).CapitalizeFirst();
                //        }
                //        else if (!cachedRole2.Active && cachedRole2.def.activationBelieverCount > cachedRole2.ideo.ColonistBelieverCountCached)
                //        {
                //            text += ": " + "InactiveRoleRequiresMoreBelievers".Translate(cachedRole2.def.activationBelieverCount, cachedRole2.ideo.memberName, cachedRole2.ideo.ColonistBelieverCountCached).CapitalizeFirst();
                //        }
                //        list.Add(new FloatMenuOption(text, null, cachedRole2.Icon, cachedRole2.ideo.Color)
                //        {
                //            orderInPriority = cachedRole2.def.displayOrderInImpact
                //        });
                //    }
                //}
                Find.WindowStack.Add(new FloatMenu(list));
            }
            else
            {
                Messages.Message("AbilityDisabledNoAltarIdeogramOrRitualsSpot".Translate(), pawn,
                    MessageTypeDefOf.RejectInput);
            }
        }

        GUI.color = Color.white;


        return false;
    }
}