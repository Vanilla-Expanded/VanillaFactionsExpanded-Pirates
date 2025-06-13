using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfAging : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.BiologicalTicksPerTick)), 
                postfix: AccessTools.Method(typeof(CurseOfAging), nameof(AgeMultiplier)));
        }
        public static void AgeMultiplier(ref float __result)
        {
            __result *= 10f;
        }
    }
}
