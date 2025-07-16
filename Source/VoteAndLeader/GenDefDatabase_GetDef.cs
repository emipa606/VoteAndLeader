using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace election;

[HarmonyPatch(typeof(GenDefDatabase), nameof(GenDefDatabase.GetDef))]
internal class GenDefDatabase_GetDef
{
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