using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    public static class JobGiver_OptimizeApparel_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn)
        {
            return !pawn.IsWearingWarcasket();
        }
    }
}
