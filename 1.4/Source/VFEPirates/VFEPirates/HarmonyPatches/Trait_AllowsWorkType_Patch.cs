using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Trait), "AllowsWorkType")]
    public static class Trait_AllowsWorkType_Patch
    {
        public static void Postfix(Trait __instance, WorkTypeDef workDef, ref bool __result)
        {
            if (__instance.def == VFEP_DefOf.VFEP_WarcasketTrait && (workDef == WorkTypeDefOf.Hauling || workDef == WorkTypeDefOf.Hunting))
            {
                __result = true;
            }
        }
    }
}
