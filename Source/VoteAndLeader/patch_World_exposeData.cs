using HarmonyLib;
using RimWorld.Planet;

namespace election;

[HarmonyPatch(typeof(World))]
[HarmonyPatch("ExposeData")]
public class patch_World_exposeData
{
    private static void Postfix()
    {
        dataUtility.GetData().ExposeData();
    }
}