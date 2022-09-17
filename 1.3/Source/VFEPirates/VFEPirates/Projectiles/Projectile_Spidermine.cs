using Verse;

namespace VFEPirates
{
    public class Projectile_Spidermine : Projectile
    {
        protected override void Impact(Thing hitThing)
        {
			var pawn = PawnGenerator.GeneratePawn(VFEP_DefOf.VFEP_Mech_Spidermine, launcher.Faction);
            pawn.relations = new RimWorld.Pawn_RelationsTracker(pawn);
            GenSpawn.Spawn(pawn, this.Position, this.Map);
			base.Impact(hitThing);
		}
	}
}
