using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEPirates
{
    public class RecipeWorker_WarcasketRemoval : Recipe_Surgery
	{
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
		{
			if (thing is Pawn pawn && pawn.apparel.WornApparel.Any(x => x is Apparel_Warcasket))
			{
				return true;
			}
			return false;
		}

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
			foreach (var partToRemove in GetPartsToRemove(pawn))
            {
				pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, partToRemove));
			}
			foreach (var apparel in pawn.apparel.WornApparel.Where(x => x is Apparel_Warcasket).ToList())
            {
				pawn.apparel.Remove(apparel);
			}
			GenSpawn.Spawn(ThingDefOf.ChunkSlagSteel, pawn.Position, pawn.Map);
		}

		private IEnumerable<BodyPartRecord> GetPartsToRemove(Pawn pawn)
        {
			foreach (var part in pawn.health.hediffSet.GetNotMissingParts().Where(x => x.depth == BodyPartDepth.Outside &&
				(x.IsInGroup(BodyPartGroupDefOf.Legs) || x.IsInGroup(DefDatabase<BodyPartGroupDef>.GetNamed("Arms")))))
            {

				yield return part;
            }
        }
	}
}
