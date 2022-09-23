using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
	public class CurseOfGreed : CurseWorker
	{
        public override void DoPatches()
        {
            Patch(typeof(Mineable), "TrySpawnYield", transpiler: AccessTools.Method(typeof(CurseOfGreed), nameof(Transpiler)));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            var mineableThingField = AccessTools.Field(typeof(BuildingProperties), nameof(BuildingProperties.mineableThing));
            var makeThingMethod = AccessTools.Method(typeof(ThingMaker), "MakeThing");
            for (int i = 0; i < codes.Count; i++)
			{
                if (i < codes.Count - 6 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 3].LoadsField(mineableThingField) && codes[i + 5].Calls(makeThingMethod))
                {
                    yield return new CodeInstruction(opcode: OpCodes.Ldsfld, operand: AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.Gold))).MoveLabelsFrom(codes[i]);
                    i += 4;
				}
                yield return codes[i];
			}
        }
    }
}
