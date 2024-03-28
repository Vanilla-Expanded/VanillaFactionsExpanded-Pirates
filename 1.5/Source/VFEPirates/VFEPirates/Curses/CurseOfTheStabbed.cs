using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfTheStabbed : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(Verb_MeleeAttack), "TryCastShot"),
                transpiler: AccessTools.Method(typeof(CurseOfTheStabbed), nameof(Transpiler)));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.Calls(AccessTools.Method(typeof(Pawn_StanceTracker), nameof(Pawn_StanceTracker.StaggerFor))))
                {
                    yield return new CodeInstruction(opcode: OpCodes.Ldc_I4_2);
                    yield return new CodeInstruction(opcode: OpCodes.Div);
                }

                yield return instruction;
            }
        }
    }
}
