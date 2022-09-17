using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfTheBoomalopes : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)), prefix: AccessTools.Method(typeof(CurseOfTheBoomalopes), nameof(Prefix)));
        }

        public static void Prefix(DamageInfo? dinfo, Pawn __instance, Hediff exactCulprit = null)
        {
            if (__instance.RaceProps.Animal)
            {
                float radius;
                if (__instance.ageTracker.CurLifeStageIndex == 0)
                {
                    radius = 1.9f;
                }
                else if (__instance.ageTracker.CurLifeStageIndex == 1)
                {
                    radius = 2.9f;
                }
                else
                {
                    radius = 4.9f;
                }
                GenExplosion.DoExplosion(__instance.Position, __instance.Map, radius, DamageDefOf.Flame, __instance, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
            }
        }
    }
}
