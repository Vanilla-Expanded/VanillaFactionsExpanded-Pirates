using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(CompApparelVerbOwner), "CreateVerbTargetCommand")]
    public static class CompReloadable_CreateVerbTargetCommand_Patch
    {
        public static void Postfix(ref Command_VerbTarget __result, Verb verb)
        {
            if (verb is Verb_DroneDeployment || verb is Verb_Spidermine)
            {
                __result.defaultIconColor = Color.white;
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
}
