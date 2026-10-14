using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEPirates
{
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    public static class JobGiver_OptimizeApparel_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (!pawn.IsWearingWarcasket())
            {
                return true;
            }
            if (ModsConfig.IdeologyActive && Find.TickManager.TicksGame >= pawn.mindState.nextApparelOptimizeTick)
            {
                if (JobGiver_OptimizeApparel.TryCreateRecolorJob(pawn, out var job))
                {
                    __result = job;
                }
                else
                {
                    // Vanilla's SetNextOptimizeTick, skipped along with the rest of the method
                    pawn.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame + Rand.Range(6000, 9000);
                }
            }
            return false;
        }
    }
}
