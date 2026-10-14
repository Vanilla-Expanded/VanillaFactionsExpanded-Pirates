using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEPirates
{
	[StaticConstructorOnStartup]
	public static class ModCompatibility
	{
		static ModCompatibility()
        {
			DubsBadHygieneActive = ModsConfig.IsActive("Dubwise.DubsBadHygiene") || ModsConfig.IsActive("Dubwise.DubsBadHygiene.Lite");
		}

		public static void FillBladderNeed(Pawn pawn, float value) 
		{
			var need = pawn?.needs?.TryGetNeed<DubsBadHygiene.Need_Bladder>();
			if (need != null)
            {
				need.CurLevel += value;
            }
		}

		public static void FillHygieneNeed(Pawn pawn, float value)
		{
			var need = pawn?.needs?.TryGetNeed<DubsBadHygiene.Need_Hygiene>();
			if (need != null)
			{
				need.CurLevel += value;
			}
		}

		public static bool DubsBadHygieneActive;
	}

}
