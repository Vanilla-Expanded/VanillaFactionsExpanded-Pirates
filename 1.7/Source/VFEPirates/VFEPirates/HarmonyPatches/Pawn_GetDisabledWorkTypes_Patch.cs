using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    public static class Pawn_GetDisabledWorkTypes_Patch
    {
        // Vanilla lets slaves be forced past trait work disables. Warcasket and shellcasket cannot be forced past, so keep them.
        public static void Postfix(Pawn __instance, List<WorkTypeDef> __result)
        {
            if (__instance.story != null && __instance.IsSlave)
            {
                // Downstream warcasket mods may substitute a near-identical trait with its own disabled-work profile, so the apparel is the signal instead.
                var wearingWarcasket = __instance.IsWearingWarcasket();
                for (int i = 0; i < __instance.story.traits.allTraits.Count; i++)
                {
                    var trait = __instance.story.traits.allTraits[i];
                    if (!wearingWarcasket && trait.def != VFEP_DefOf.VFEP_Shellcasket)
                    {
                        continue;
                    }
                    foreach (WorkTypeDef disabledWorkType2 in trait.GetDisabledWorkTypes())
                    {
                        if (!__result.Contains(disabledWorkType2))
                        {
                            __result.Add(disabledWorkType2);
                        }
                    }
                }
            }
        }
    }
}
