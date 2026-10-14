using RimWorld;
using Verse;
using Verse.AI;

namespace VFEPirates;

public class JobGiver_StandIdle : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        return JobMaker.MakeJob(JobDefOf.Wait, 60);
    }
}