using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace election;

[HarmonyPatch(typeof(GenDefDatabase))]
[HarmonyPatch("GetDef")]
internal class patch_GenDefDatabase_GetDef
{
    [HarmonyPostfix]
    private static bool Prefix(ref Def __result, Type defType, string defName)
    {
        if (defType != typeof(PreceptDef) || defName != "RoleChange")
        {
            return true;
        }

        __result = null;
        return false;
    }
}