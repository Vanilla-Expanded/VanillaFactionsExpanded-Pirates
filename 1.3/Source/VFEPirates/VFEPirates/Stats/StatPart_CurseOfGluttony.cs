using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEPirates
{
	public class StatPart_CurseOfGluttony : StatPart
	{
		private bool IsActive => GameComponent_CurseManager.Instance.activeCurseDefs.Contains(VFEP_DefOf.VFEP_CurseOfGluttony);

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && IsActive)
			{
				val *= 2;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && IsActive)
			{
				return VFEP_DefOf.VFEP_CurseOfGluttony.label + " - " + VFEP_DefOf.VFEP_CurseOfGluttony.description;
			}
			return null;
		}
	}
}
