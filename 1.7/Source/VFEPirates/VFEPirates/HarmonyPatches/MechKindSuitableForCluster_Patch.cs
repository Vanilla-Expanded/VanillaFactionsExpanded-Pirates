using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(MechClusterGenerator), nameof(MechClusterGenerator.MechKindSuitableForCluster))]
    public class MechKindSuitableForCluster_Patch
    {
        public static void Postfix(PawnKindDef __0, ref bool __result)
        {
            if (__0 == VFEP_DefOf.VFEP_Mech_Spidermine || __0 == VFEP_DefOf.VFEP_Mech_Wardrone)
            {
                __result = false;
            }
        }
    }
}
