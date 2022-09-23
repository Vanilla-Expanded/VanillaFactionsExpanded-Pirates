using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VFEPirates
{
    public class ThoughtWorker_RumSuit : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (wornApparel[i].def==VFEP_DefOf.VFEP_Apparel_Rumsuit)
				{
					return ThoughtState.ActiveAtStage(0);
				}
			}
			return ThoughtState.Inactive;
		}
    }
}