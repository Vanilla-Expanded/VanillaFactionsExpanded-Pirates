using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    public static class CachedData
    {
		public delegate bool TryFindBestIngredientsInSet_NoMixHelper(List<Thing> availableThings, List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients, Bill bill = null);

        public static readonly TryFindBestIngredientsInSet_NoMixHelper tryFindBestIngredientsInSet_NoMixHelper =
			AccessTools.MethodDelegate<TryFindBestIngredientsInSet_NoMixHelper>(AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestIngredientsInSet_NoMixHelper"));
	}
}
