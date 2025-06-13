using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VFEPirates
{

    [HarmonyPatch(typeof(StorytellerComp_RandomMain), "ChooseRandomCategory")]
    public static class StorytellerComp_RandomMain_ChooseRandomCategory_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var maxThreatBigIntervalDaysField = AccessTools.Field(typeof(StorytellerCompProperties_RandomMain), nameof(StorytellerCompProperties_RandomMain.maxThreatBigIntervalDays));
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(maxThreatBigIntervalDaysField))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StorytellerComp_RandomMain_ChooseRandomCategory_Transpiler), nameof(GetMaxThreatBigIntervalDaysField)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static float GetMaxThreatBigIntervalDaysField(StorytellerCompProperties_RandomMain props)
        {
            if (CursesUtility.CursedStorytellerInCharge)
            {
                return GameComponent_CurseManager.Instance.MaxThreatBigIntervalDays(props);
            }
            return props.maxThreatBigIntervalDays;
        }
    }
}









