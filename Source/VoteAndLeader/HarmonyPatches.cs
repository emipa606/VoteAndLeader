using System.Reflection;
using HarmonyLib;
using Verse;

namespace election;

public class HarmonyPatches : Mod
{
    public HarmonyPatches(ModContentPack content) : base(content)
    {
        new Harmony("com.yayo.election").PatchAll(Assembly.GetExecutingAssembly());
    }
}