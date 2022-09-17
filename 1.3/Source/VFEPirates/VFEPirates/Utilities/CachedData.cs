using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    public static class CachedData
    {
		public delegate Vector3 GetBodyPos(PawnRenderer pawnRenderer, Vector3 drawLoc, out bool showBody);
		public static readonly GetBodyPos getBodyPos = AccessTools.MethodDelegate<GetBodyPos>(AccessTools.Method(typeof(PawnRenderer), "GetBodyPos"));

		public delegate bool TryFindBestIngredientsInSet_NoMixHelper(List<Thing> availableThings, List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, Bill bill = null);
		public static readonly TryFindBestIngredientsInSet_NoMixHelper tryFindBestIngredientsInSet_NoMixHelper =
			AccessTools.MethodDelegate<TryFindBestIngredientsInSet_NoMixHelper>(AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestIngredientsInSet_NoMixHelper"));
	}
}
