using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(CompReloadable), "CreateVerbTargetCommand")]
    public static class CompReloadable_CreateVerbTargetCommand_Patch
    {
        public static void Postfix(ref Command_Reloadable __result, Thing gear, Verb verb)
        {
            if (verb is Verb_DroneDeployment || verb is Verb_Spidermine)
            {
                __result.defaultIconColor = Color.white;
                __result.overrideColor = Color.white;
            }
            if (verb is Verb_Spidermine)
            {
                __result.defaultDesc = "VFEP.DeploySpidermineDesc".Translate();
            }
            if (verb is Verb_DroneDeployment)
            {
                __result.defaultDesc = "VFEP.DeployWarDrone".Translate();
            }
        }
    }

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
