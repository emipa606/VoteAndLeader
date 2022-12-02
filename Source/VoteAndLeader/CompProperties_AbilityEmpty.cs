using RimWorld;
using Verse;

namespace election;

public class CompProperties_AbilityEmpty : CompProperties_AbilityEffectWithDuration
{
    public bool applyToSelf;

    public bool applyToTarget = true;
    public HediffDef hediffDef;

    public bool onlyApplyToSelf;

    public bool onlyBrain;

    public bool replaceExisting;
}