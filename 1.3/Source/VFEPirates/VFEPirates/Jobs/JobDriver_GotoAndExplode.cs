using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEPirates
{
    public class JobDriver_GotoAndExplode : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			LocalTargetInfo lookAtTarget = job.GetTarget(TargetIndex.B);
			Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			toil.AddPreTickAction(delegate
			{
				if (job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position))
				{
					TryExitMap();
				}
			});
			toil.FailOn(() => job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn));
			if (lookAtTarget.IsValid)
			{
				toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
				{
					pawn.rotationTracker.FaceCell(lookAtTarget.Cell);
				});
				toil.handlingFacing = true;
			}
			yield return toil;
			Toil toil2 = new Toil();
			toil2.initAction = delegate
			{
				if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == base.TargetA.Cell)
				{
					pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
				}
				if (!job.ritualTag.NullOrEmpty())
				{
					(pawn.GetLord()?.LordJob as LordJob_Ritual)?.AddTagForPawn(pawn, job.ritualTag);
				}
				if (job.exitMapOnArrival && (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position)))
				{
					TryExitMap();
				}
				var hediff = HediffMaker.MakeHediff(VFEP_DefOf.VFEP_KillItself, pawn) as Hediff_KillItself;
				hediff.killInstant = true;
				Log.Message("Kill instant: " + TargetA);
				pawn.health.AddHediff(hediff);
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}

		private void TryExitMap()
		{
			if (!job.failIfCantJoinOrCreateCaravan || CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn))
			{
				pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
			}
		}
	}
}
