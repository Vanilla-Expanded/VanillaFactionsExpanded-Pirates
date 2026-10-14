using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfConfetti : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)), 
                prefix: AccessTools.Method(typeof(CurseOfConfetti), nameof(Prefix)),
                postfix: AccessTools.Method(typeof(CurseOfConfetti), nameof(Postfix)));
        }

        public static void Prefix(DamageInfo? dinfo, Pawn __instance, ref (Map map, IntVec3 pos) __state)
		{
            __state = (__instance.Map, __instance.Position);
		}

        public static void Postfix(DamageInfo? dinfo, (Map map, IntVec3 pos) __state)
        {
            if (__state.map != null)
            {
                FleckMaker.Static(__state.pos, __state.map, Confetti(Rand.RangeInclusive(0, 3)));
                FleckMaker.Static(__state.pos, __state.map, Confetti(Rand.RangeInclusive(0, 3)));
                VFEP_DefOf.VFEP_ConfettiExplosion.PlayOneShot(new TargetInfo(__state.pos, __state.map, false));
            }
        }

        public static FleckDef Confetti(int index)
		{
            return index switch
            {
                0 => VFEP_DefOf.VFEP_ConfettiA,
                1 => VFEP_DefOf.VFEP_ConfettiB,
                2 => VFEP_DefOf.VFEP_ConfettiC,
                3 => VFEP_DefOf.VFEP_ConfettiD,
                _ => VFEP_DefOf.VFEP_ConfettiA
            };
		}
    }
}
