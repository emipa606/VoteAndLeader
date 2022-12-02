using System.Reflection;
using HarmonyLib;
using Verse;

namespace election;

public class HarmonyPatches : Mod
{
    public HarmonyPatches(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("com.yayo.election");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}