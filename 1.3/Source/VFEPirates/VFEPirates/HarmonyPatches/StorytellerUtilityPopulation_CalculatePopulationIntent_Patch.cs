using HarmonyLib;
using RimWorld;

namespace VFEPirates
{
    [HarmonyPatch(typeof(StorytellerUtilityPopulation), "CalculatePopulationIntent")]
    public static class StorytellerUtilityPopulation_CalculatePopulationIntent_Patch
    {
        public static void Postfix(ref float __result)
        {
            if (CursesUtility.CursedStorytellerInCharge)
            {
                __result += GameComponent_CurseManager.Instance.activeCurseDefs.Count;
            }
        }
    }
}
