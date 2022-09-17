using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEPirates
{
    public class JobGiver_GoToNearestEnemyAndExplode : JobGiver_AIFightEnemy
	{
		private float targetKeepRadius = 30;
		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget != null)
            {
                dest = enemyTarget.Position;
                return true;
            }
            else
            {
                dest = IntVec3.Invalid;
                return false;
            }
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            return 3000000;
        }
		protected override Job TryGiveJob(Pawn pawn)
		{
			UpdateEnemyTarget(pawn);
			Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget == null)
			{
				if (pawn.health.hediffSet.GetFirstHediffOfDef(VFEP_DefOf.VFEP_KillItself) == null)
                {
					var hediff = HediffMaker.MakeHediff(VFEP_DefOf.VFEP_KillItself, pawn) as Hediff_KillItself;
					pawn.health.AddHediff(hediff);
				}
                return null;
			}
			Job job = JobMaker.MakeJob(VFEP_DefOf.VFEP_GotoAndExplode, enemyTarget);
			job.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
			job.checkOverrideOnExpire = true;
			return job;
		}

		protected override void UpdateEnemyTarget(Pawn pawn)
		{
			Thing thing = pawn.mindState.enemyTarget;
			if (thing != null && (thing.Destroyed || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly) 
				|| (float)(pawn.Position - thing.Position).LengthHorizontalSquared > targetKeepRadius * targetKeepRadius 
				|| ((IAttackTarget)thing).ThreatDisabled(pawn)))
			{
				thing = null;
			}
			if (thing == null)
			{
				thing = FindAttackTarget(pawn);
			}
			pawn.mindState.enemyTarget = thing;
		}
		
		protected override Thing FindAttackTarget(Pawn pawn)
		{
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => ExtraTargetValidator(pawn, x), 0f, targetKeepRadius, 
				GetFlagPosition(pawn), GetFlagRadius(pawn));
		}
	}
}
