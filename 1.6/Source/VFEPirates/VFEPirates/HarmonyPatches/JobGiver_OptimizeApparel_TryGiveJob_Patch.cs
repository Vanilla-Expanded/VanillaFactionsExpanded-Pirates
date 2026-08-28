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
            if (ModsConfig.IdeologyActive && JobGiver_OptimizeApparel.TryCreateRecolorJob(pawn, out var job))
            {
                __result = job;
            }
            return false;
        }
    }
}
