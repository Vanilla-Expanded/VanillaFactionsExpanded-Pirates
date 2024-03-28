using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VFEPirates
{
	public class StatWorker_ShootingAccuracy_CurseOfTheEyePatch : StatWorker_ShootingAccuracy
	{
		private bool IsActive => GameComponent_CurseManager.Instance.activeCurseDefs.Contains(VFEP_DefOf.VFEP_CurseOfTheEyePatch);

		public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
		{
			base.FinalizeValue(req, ref val, applyPostProcess);

			if (IsActive)
			{
				val /= 2;
			}
		}

		public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));

			if (IsActive)
			{
				stringBuilder.AppendLine(VFEP_DefOf.VFEP_CurseOfTheEyePatch.label + " - " + VFEP_DefOf.VFEP_CurseOfTheEyePatch.description);
			}

			return stringBuilder.ToString();
		}
	}
}
