using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEPirates
{
    public class JobDriver_DoWelding : JobDriver
	{
		public Building_WarcasketFoundry Foundry => TargetA.Thing as Building_WarcasketFoundry;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			foreach (var target in job.GetTargetQueue(TargetIndex.B))
            {
				pawn.Map.physicalInteractionReservationManager.Reserve(pawn, job, target.Thing);
            }
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			AddEndCondition(delegate
			{
				Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
				return thing.Spawned && thing is Building_WarcasketFoundry foundry && foundry.ReadyForWelding(pawn, out _) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			this.FailOnBurningImmobile(TargetIndex.A);
			Toil gotoFoundry = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch);
			if (job.targetQueueB != null)
            {
				yield return Toils_Jump.JumpIf(gotoFoundry, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
				foreach (Toil item in CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C))
				{
					yield return item;
				}
			}
			yield return gotoFoundry;
			yield return new Toil
			{
				initAction = () => job.SetTarget(TargetIndex.C, Foundry.occupant)
			};
			yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.ClosestTouch);
			yield return new Toil
			{
				initAction = delegate
				{
					if (job.targetQueueB != null && job.placedThings != null)
                    {
						for (var i = job.placedThings.Count - 1; i >= 0; i--)
						{
							job.placedThings[i].thing?.Destroy();
						}
						pawn.Map.physicalInteractionReservationManager.ReleaseClaimedBy(pawn, job);
						job.placedThings = null;
					}
				},
				tickAction = delegate
                {
					Foundry.compPower.powerOutputInt = -350;
					Foundry.curWarcasketProject.DoWork(pawn.GetStatValue(StatDefOf.WorkSpeedGlobal), out bool workDone);
					if (workDone)
                    {
						Foundry.FinishWarCasketProject();
						this.EndJobWith(JobCondition.Succeeded);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			}.WithEffect(() => EffecterDefOf.ConstructMetal, TargetIndex.A);
		}

		public IEnumerable<Toil> CollectIngredientsToils(TargetIndex ingredientInd, TargetIndex billGiverInd, TargetIndex ingredientPlaceCellInd, bool subtractNumTakenFromJobCount = false, bool failIfStackCountLessThanJobCount = true)
		{
			Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientInd);
			yield return extract;
			Toil getToHaulTarget = Toils_Goto.GotoThing(ingredientInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(ingredientInd).FailOnSomeonePhysicallyInteracting(ingredientInd);
			yield return getToHaulTarget;
			yield return Toils_Haul.StartCarryThing(ingredientInd, putRemainderInQueue: true, subtractNumTakenFromJobCount, failIfStackCountLessThanJobCount);
			yield return JobDriver_DoBill.JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
			yield return Toils_Goto.GotoThing(billGiverInd, PathEndMode.OnCell).FailOnDestroyedOrNull(ingredientInd);
			Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(billGiverInd, ingredientInd, ingredientPlaceCellInd);
			yield return findPlaceTarget;
			yield return PlaceHauledThingInCell(ingredientPlaceCellInd, findPlaceTarget, storageMode: false);
			yield return Toils_Jump.JumpIfHaveTargetInQueue(ingredientInd, extract);
		}

		public static Toil PlaceHauledThingInCell(TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode, bool tryStoreInSameStorageIfSpotCantHoldWholeStack = false)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				IntVec3 cell = curJob.GetTarget(cellInd).Cell;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(string.Concat(actor, " tried to place hauled thing in cell but is not hauling anything."));
				}
				else
				{
					SlotGroup slotGroup = actor.Map.haulDestinationManager.SlotGroupAt(cell);
					if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carryTracker.CarriedThing))
					{
						actor.Map.designationManager.TryRemoveDesignationOn(actor.carryTracker.CarriedThing, DesignationDefOf.Haul);
					}
					Action<Thing, int> placedAction = delegate (Thing th, int added)
					{
						if (curJob.placedThings == null)
						{
							curJob.placedThings = new List<ThingCountClass>();
						}
						ThingCountClass thingCountClass = curJob.placedThings.Find((ThingCountClass x) => x.thing == th);
						if (thingCountClass != null)
						{
							thingCountClass.Count += added;
						}
						else
						{
							curJob.placedThings.Add(new ThingCountClass(th, added));
						}
					};

					if (!actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out var _, placedAction))
					{
						if (storageMode)
						{
							if (nextToilOnPlaceFailOrIncomplete != null && ((tryStoreInSameStorageIfSpotCantHoldWholeStack && StoreUtility.TryFindBestBetterStoreCellForIn(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, cell.GetSlotGroup(actor.Map), out var foundCell)) || StoreUtility.TryFindBestBetterStoreCellFor(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell)))
							{
								if (actor.CanReserve(foundCell))
								{
									actor.Reserve(foundCell, actor.CurJob);
								}
								actor.CurJob.SetTarget(cellInd, foundCell);
								actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
							}
							else
							{
								Job job = HaulAIUtility.HaulAsideJobFor(actor, actor.carryTracker.CarriedThing);
								if (job != null)
								{
									curJob.targetA = job.targetA;
									curJob.targetB = job.targetB;
									curJob.targetC = job.targetC;
									curJob.count = job.count;
									curJob.haulOpportunisticDuplicates = job.haulOpportunisticDuplicates;
									curJob.haulMode = job.haulMode;
									actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
								}
								else
								{
									Log.Error(string.Concat("Incomplete haul for ", actor, ": Could not find anywhere to put ", actor.carryTracker.CarriedThing, " near ", actor.Position, ". Destroying. This should never happen!"));
									actor.carryTracker.CarriedThing.Destroy();
								}
							}
						}
						else if (nextToilOnPlaceFailOrIncomplete != null)
						{
							actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
						}
					}
				}
			};
			return toil;
		}
	}
}
