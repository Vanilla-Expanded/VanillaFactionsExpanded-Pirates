using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VFEPirates
{
    public class DeathActionWorker_SpidermineExplosion : DeathActionWorker
    {
        public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

        public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            GenExplosion.DoExplosion(corpse.Position, corpse.Map, 4.9f, DamageDefOf.Flame, corpse.InnerPawn);
        }
    }
}
