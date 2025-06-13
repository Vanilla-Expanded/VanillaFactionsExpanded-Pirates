using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfTheEyePatch : CurseWorker
    {
        public override void DoPatches()
        {
            //Patch(original: AccessTools.Method(typeof(ShotReport), nameof(ShotReport.HitFactorFromShooter), parameters: new Type[] { typeof(float), typeof(float) }), 
            //    prefix: AccessTools.Method(typeof(CurseOfTheEyePatch), nameof(HalfAccuracy)));
            //Patch(original: AccessTools.Method(typeof(ShotReport), nameof(ShotReport.GetTextReadout)),
            //    postfix: AccessTools.Method(typeof(CurseOfTheEyePatch), nameof(HalfAccuracyReport)));
        }

        public static void HalfAccuracy(ref float accRating, float distance)
        {
            accRating /= 2;
        }

        public static void HalfAccuracyReport(ref string __result, float ___forcedMissRadius)
		{
            if (___forcedMissRadius <= 0.5f)
			{
                __result += $"\n  {VFEP_DefOf.VFEP_CurseOfTheEyePatch.label} - {VFEP_DefOf.VFEP_CurseOfTheEyePatch.description}";
			}
		}
    }
}
