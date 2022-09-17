using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VFEPirates
{
    public static class WeldingUtility
    {
		private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

		private static List<Thing> relevantThings = new List<Thing>();

		private static HashSet<Thing> processedThings = new HashSet<Thing>();

		private static List<Thing> newRelevantThings = new List<Thing>();

		public static bool TryFindBestFixedIngredients(List<IngredientCount> ingredients, Pawn pawn, Thing ingredientDestination, List<ThingCount> chosen, float searchRadius = 999f)
		{
			return TryFindBestIngredientsHelper((Thing t) => ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t)), 
				(List<Thing> foundThings) => CachedData.tryFindBestIngredientsInSet_NoMixHelper(foundThings, ingredients, chosen, ingredientDestination.Position, alreadySorted: false), 
				ingredients, pawn, ingredientDestination, chosen, searchRadius);
		}
		public static bool TryFindBestIngredientsHelper(Predicate<Thing> thingValidator, Predicate<List<Thing>> foundAllIngredientsAndChoose, List<IngredientCount> ingredients, Pawn pawn, Thing billGiver, List<ThingCount> chosen, float searchRadius)
		{
			chosen.Clear();
			newRelevantThings.Clear();
			if (ingredients.Count == 0)
			{
				return true;
			}
			IntVec3 billGiverRootCell = billGiver.Position;
			Region rootReg = billGiverRootCell.GetRegion(pawn.Map);
			if (rootReg == null)
			{
				return false;
			}
			relevantThings.Clear();
			processedThings.Clear();
			bool foundAll = false;
			float radiusSq = searchRadius * searchRadius;
			Predicate<Thing> baseValidator = (Thing t) => t.Spawned && !t.IsForbidden(pawn) && (float)(t.Position - billGiver.Position).LengthHorizontalSquared < radiusSq 
				&& thingValidator(t) && pawn.CanReserve(t);
			TraverseParms traverseParams = TraverseParms.For(pawn);
			RegionEntryPredicate entryCondition = null;
			if (Math.Abs(999f - searchRadius) >= 1f)
			{
				entryCondition = delegate (Region from, Region r)
				{
					if (!r.Allows(traverseParams, isDestination: false))
					{
						return false;
					}
					CellRect extentsClose = r.extentsClose;
					int num = Math.Abs(billGiver.Position.x - Math.Max(extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)));
					if ((float)num > searchRadius)
					{
						return false;
					}
					int num2 = Math.Abs(billGiver.Position.z - Math.Max(extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)));
					return !((float)num2 > searchRadius) && (float)(num * num + num2 * num2) <= radiusSq;
				};
			}
			else
			{
				entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
			}
			int adjacentRegionsAvailable = rootReg.Neighbors.Count((Region region) => entryCondition(rootReg, region));
			int regionsProcessed = 0;
			processedThings.AddRange(relevantThings);
			RegionProcessor regionProcessor = delegate (Region r)
			{
				List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing = list[i];
					if (!processedThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn) && baseValidator(thing))
					{
						newRelevantThings.Add(thing);
						processedThings.Add(thing);
					}
				}
				regionsProcessed++;
				if (newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
				{
					relevantThings.AddRange(newRelevantThings);
					newRelevantThings.Clear();
					if (foundAllIngredientsAndChoose(relevantThings))
					{
						foundAll = true;
						return true;
					}
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
			relevantThings.Clear();
			newRelevantThings.Clear();
			processedThings.Clear();
			return foundAll;
		}
	}
}
