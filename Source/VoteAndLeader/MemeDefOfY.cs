using RimWorld;

namespace election;

[DefOf]
public static class MemeDefOfY
{
    [MayRequireIdeology] public static MemeDef Supremacist;

    [MayRequireIdeology] public static MemeDef Collectivist;

    [MayRequireIdeology] public static MemeDef Proselytizer;


    static MemeDefOfY()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MemeDefOf));
    }
}