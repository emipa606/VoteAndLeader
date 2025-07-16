using RimWorld;
using Verse;

namespace election;

public class CompAbilityEffect_CheckAssist : CompAbilityEffect_WithDuration
{
    public new CompProperties_AbilityEmpty Props => (CompProperties_AbilityEmpty)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        core.setAssist();
    }
}