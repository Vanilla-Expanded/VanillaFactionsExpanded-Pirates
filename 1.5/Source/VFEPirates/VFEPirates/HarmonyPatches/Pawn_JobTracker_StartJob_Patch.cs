using HarmonyLib;
using Verse;
using Verse.AI;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public class Pawn_JobTracker_StartJob_Patch
    {
        private static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
        {
            if (___pawn.CurJobDef == VFEP_DefOf.VFEP_EntombIn)
            {
                return false;
            }
            return true;
        }
    }
}
