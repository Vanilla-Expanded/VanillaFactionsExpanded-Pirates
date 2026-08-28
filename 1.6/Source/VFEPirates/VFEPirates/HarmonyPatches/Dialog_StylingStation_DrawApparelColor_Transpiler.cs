using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Dialog_StylingStation), "DrawApparelColor")]
    public static class Dialog_StylingStation_DrawApparelColor_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var isLocked = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.IsLocked));
            var replacement = AccessTools.Method(typeof(Dialog_StylingStation_DrawApparelColor_Transpiler), nameof(IsLocked));
            var patched = false;
            foreach (var code in instructions)
            {
                if (code.Calls(isLocked))
                {
                    code.opcode = OpCodes.Call;
                    code.operand = replacement;
                    patched = true;
                }
                yield return code;
            }
            if (!patched)
            {
                Log.Warning("Dialog_StylingStation_DrawApparelColor_Transpiler: IsLocked call not found, warcaskets cannot be recolored");
            }
        }

        public static bool IsLocked(Pawn_ApparelTracker apparelTracker, Apparel apparel)
        {
            return !(apparel is Apparel_Warcasket) && apparelTracker.IsLocked(apparel);
        }
    }
}
