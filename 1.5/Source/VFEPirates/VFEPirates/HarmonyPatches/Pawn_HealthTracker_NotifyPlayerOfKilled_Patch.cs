using HarmonyLib;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
    public static class Pawn_HealthTracker_NotifyPlayerOfKilled_Patch
    {
        public static bool Prefix(Pawn ___pawn)
        {
            if (___pawn.kindDef == VFEP_DefOf.VFEP_Mech_Wardrone || ___pawn.kindDef == VFEP_DefOf.VFEP_Mech_Spidermine)
            {
                return false;
            }
            return true;
        }
    }
}
