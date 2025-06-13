using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    public static class Pawn_GetDisabledWorkTypes_Patch
    {
        public static void Postfix(Pawn __instance, List<WorkTypeDef> __result)
        {
            if (__instance.story != null && __instance.IsSlave && __instance.IsWearingWarcasket())
            {
                for (int i = 0; i < __instance.story.traits.allTraits.Count; i++)
                {
                    foreach (WorkTypeDef disabledWorkType2 in __instance.story.traits.allTraits[i].GetDisabledWorkTypes())
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
