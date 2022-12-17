using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    public static class PawnRenderer_DrawHeadHair_Patch
    {
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			var codes = codeInstructions.ToList();
			var pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
			for (var i = 0; i < codes.Count; i++)
			{
				var code = codes[i];
				yield return code;
				if (code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb && lb.LocalIndex == 4)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
					yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawHeadHair_Patch), nameof(ShowHeadGearAlways)));

				}
			}
		}

		public static void ShowHeadGearAlways(Pawn pawn, ref bool flag4)
		{
			if (pawn.IsWearingWarcasket())
			{
                flag4 = false;
			}
		}
	}
}
