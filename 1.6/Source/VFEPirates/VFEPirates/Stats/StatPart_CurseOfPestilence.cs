using RimWorld;

namespace VFEPirates
{
    public class StatPart_CurseOfPestilence : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && IsActive)
			{
				val /= 2f;
			}
		}
		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && IsActive)
            {
				return VFEP_DefOf.VFEP_CurseOfPestilence.label + " - " + VFEP_DefOf.VFEP_CurseOfPestilence.description;
			}
			return null;
		}

		private bool IsActive => GameComponent_CurseManager.Instance.activeCurseDefs.Contains(VFEP_DefOf.VFEP_CurseOfPestilence);
	}
}
