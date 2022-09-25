using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfInstaKill : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(Pawn_HealthTracker), "MakeDowned"), 
                prefix: AccessTools.Method(typeof(CurseOfInstaKill), nameof(InstaKill)));
            Patch(original: AccessTools.Method(typeof(HealthUtility), nameof(HealthUtility.DamageUntilDowned)),
                prefix: AccessTools.Method(typeof(CurseOfInstaKill), nameof(InstaKillHealthUtil)));
        }

        public static bool InstaKill(DamageInfo? dinfo, Hediff hediff, Pawn ___pawn)
        {
            ___pawn.Kill(dinfo, hediff);
            return false;
        }

        public static bool InstaKillHealthUtil(Pawn p, bool allowBleedingWounds = true)
		{
            p.Kill(null);
            return false;
		}
    }
}
