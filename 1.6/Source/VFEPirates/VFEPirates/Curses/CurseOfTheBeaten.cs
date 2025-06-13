﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfTheBeaten : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(FloatMenuMakerMap), "AddDraftedOrders"),
                transpiler: AccessTools.Method(typeof(CurseOfTheBeaten), nameof(Transpiler)));
            Patch(original: AccessTools.Method(typeof(WorkGiver_Tend), nameof(WorkGiver_Tend.HasJobOnThing)),
                postfix: AccessTools.Method(typeof(CurseOfTheBeaten), nameof(DontAutoTend)));
        }

        public static void DontAutoTend(Pawn pawn, Thing t, ref bool __result)
		{
            if (IsActive(VFEP_DefOf.VFEP_CurseOfTheBeaten) && 
                HealthAIUtility.FindBestMedicine(pawn, t as Pawn, false) is null)
			{
                __result = false;
			}
		}

        public static MethodInfo DecoratePrioritizedTaskInfo = AccessTools.Method(typeof(FloatMenuUtility),
            nameof(FloatMenuUtility.DecoratePrioritizedTask));
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo tendTargetField = AccessTools.Field(typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all)
                        .First(t => t.Name.Contains("c__DisplayClass11_12")), "tendTarget");
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.Calls(DecoratePrioritizedTaskInfo) && instructionList[i - 4].LoadsField(tendTargetField))
                {
                    FieldInfo medicineField = AccessTools.Field(typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all)
                        .First(t => t.Name.Contains("c__DisplayClass11_13")), "medicine");
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_S, operand: 40);
                    yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: medicineField);
                    yield return new CodeInstruction(opcode: OpCodes.Call, 
                        operand: AccessTools.Method(typeof(CurseOfTheBeaten), nameof(RevalidateFloatMenuOption)));
                }

            }
        }

        public static FloatMenuOption RevalidateFloatMenuOption(FloatMenuOption floatMenuOption, Thing medicine)
		{
            if (IsActive(VFEP_DefOf.VFEP_CurseOfTheBeaten) && medicine is null)
			{
                floatMenuOption.action = null;
                floatMenuOption.Label += $" ({VFEP_DefOf.VFEP_CurseOfTheBeaten.LabelCap} - {VFEP_DefOf.VFEP_CurseOfTheBeaten.description})";
            }
            return floatMenuOption;
		}
    }
}
