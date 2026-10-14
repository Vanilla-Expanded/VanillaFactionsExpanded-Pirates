using HarmonyLib;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.GetHungerRateFactor))]
    public static class HediffSet_GetHungerRateFactor_Patch
    {
        public static void Postfix(ref float __result)
        {
            if (GameComponent_CurseManager.Instance.activeCurseDefs.Contains(VFEP_DefOf.VFEP_CurseOfGluttony))
            {
                __result *= 2f;
            }
        }
    }
}
