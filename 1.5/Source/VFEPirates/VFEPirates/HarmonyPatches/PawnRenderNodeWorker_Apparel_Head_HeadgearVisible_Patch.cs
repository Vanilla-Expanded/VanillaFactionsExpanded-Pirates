using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "HeadgearVisible")]
    public static class PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch
    {
		public static void Postfix(PawnDrawParms parms, ref bool __result)
		{
			if (parms.pawn.IsWearingWarcasket())
			{
                __result = true;
			}
		}
	}
}
