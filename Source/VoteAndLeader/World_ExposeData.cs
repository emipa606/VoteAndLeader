using HarmonyLib;
using RimWorld.Planet;

namespace election;

[HarmonyPatch(typeof(World), nameof(World.ExposeData))]
public class World_ExposeData
{
    private static void Postfix()
    {
        dataUtility.GetData().ExposeData();
    }
}