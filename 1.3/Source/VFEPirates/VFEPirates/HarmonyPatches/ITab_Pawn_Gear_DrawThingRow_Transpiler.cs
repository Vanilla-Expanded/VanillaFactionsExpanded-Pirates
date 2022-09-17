using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow")]
    public static class ITab_Pawn_Gear_DrawThingRow_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].OperandIs("DropThingLocked"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ITab_Pawn_Gear_DrawThingRow_Transpiler), nameof(DropThingTooltip)));
                    i += 2;
                }
                yield return codes[i];
            }
        }

        public static TaggedString DropThingTooltip(Apparel apparel)
        {
            if (apparel is Apparel_Warcasket)
            {
                return "VFEP.WarcasketCannotBeRemoved".Translate();
            }
            return "DropThingLocked".Translate();
        }
    }
}
