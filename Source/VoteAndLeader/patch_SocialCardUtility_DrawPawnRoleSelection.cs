using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace election;

[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRoleSelection))]
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
        _ = Faction.OfPlayer.ideos.PrimaryIdeo;
        _ = (Precept_Ritual)pawn.Ideo?.GetPrecept(PreceptDefOf.RoleChange);
        //TargetInfo ritualTarget = roleChangeRitual.targetFilter.BestTarget(pawn, TargetInfo.Invalid);
        if (!(cachedRoles.Any() && pawn.Ideo != null))
        {
            GUI.color = Color.gray;
        }

        var y = rect.y + (rect.height / 2f) - 14f;
        var rect2 = new Rect(rect.width - 150f, y, ___RoleChangeButtonSize.x, ___RoleChangeButtonSize.y)
        {
            xMax = rect.width - 26f - 4f
        };
        if (Widgets.ButtonText(rect2, "ChooseRole".Translate() + "...", true, true,
                cachedRoles.Any() && pawn.Ideo != null))
        {
            //if (ritualTarget.IsValid)
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

            Find.WindowStack.Add(new FloatMenu(list));
        }

        GUI.color = Color.white;


        return false;
    }
}