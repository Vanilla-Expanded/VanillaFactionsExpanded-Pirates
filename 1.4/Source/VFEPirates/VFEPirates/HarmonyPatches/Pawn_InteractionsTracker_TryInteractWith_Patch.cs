using HarmonyLib;
using RimWorld;
using Verse;


namespace VFEPirates
{

    [HarmonyPatch(typeof(Pawn_InteractionsTracker))]
    [HarmonyPatch("TryInteractWith")]
    public static class VFEPirates_Pawn_InteractionsTracker_TryInteractWith_Patch
    {
        [HarmonyPostfix]
        static void AddInteractionThoughts(Pawn ___pawn, bool __result)
        {
            if (ModsConfig.IdeologyActive) {
                if (__result && ___pawn?.Ideo?.HasPrecept(VFEP_DefOf.VFEP_Camaraderie_Respected) == true)
                {
                    ___pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(VFEP_DefOf.VFEP_Camaraderie_Respected_Memory, null, ___pawn?.Ideo?.GetPrecept(VFEP_DefOf.VFEP_Camaraderie_Respected));

                }
                if (__result && ___pawn?.Ideo?.HasPrecept(VFEP_DefOf.VFEP_Camaraderie_Exalted) == true)
                {
                    ___pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(VFEP_DefOf.VFEP_Camaraderie_Exalted_Memory, null, ___pawn?.Ideo?.GetPrecept(VFEP_DefOf.VFEP_Camaraderie_Exalted));

                }

            }
            
        }
    }








}
