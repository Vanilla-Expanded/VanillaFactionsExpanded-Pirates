using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEPirates
{
    public class WorkGiver_DoWelding : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(VFEP_DefOf.VFEP_WarcasketFoundry);
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (thing is Building_WarcasketFoundry {OccupantAliveAndPresent: true} foundry && pawn.CanReserveAndReach(thing, PathEndMode.Touch, MaxPathDanger(pawn)))
            {
                if (!foundry.ReadyForWelding(pawn, out var failReason))
                    JobFailReason.Is(failReason);
                else
                {
                    if (foundry.curWarcasketProject != null && foundry.curWarcasketProject.currentWorkAmountDone > 0)
                    {
                        var job = JobMaker.MakeJob(VFEP_DefOf.VFEP_DoWelding);
                        job.targetA = foundry;
                        return job;
                    }
                    else
                    {
                        var chosen = new List<ThingCount>();
                        var requiredIngredients = foundry.RequiredIngredients().ToList();
                        if (!WeldingUtility.TryFindBestFixedIngredients(requiredIngredients, pawn, foundry, chosen))
                            JobFailReason.Is("VFEM.MissingMaterials".Translate(string.Join(", ", requiredIngredients.Select(x => x.ToString()))));
                        else if (chosen.Any(x => !pawn.CanReserveAndReach(x.Thing, PathEndMode.ClosestTouch, MaxPathDanger(pawn))))
                            JobFailReason.Is("VFEM.MissingMaterials".Translate(string.Join(", ", requiredIngredients.Select(x => x.ToString()))));
                        else
                        {
                            var job = JobMaker.MakeJob(VFEP_DefOf.VFEP_DoWelding);
                            job.targetA = foundry;
                            job.targetQueueB = new List<LocalTargetInfo>(chosen.Count);
                            job.countQueue = new List<int>(chosen.Count);
                            for (var i = 0; i < chosen.Count; i++)
                            {
                                job.targetQueueB.Add(chosen[i].Thing);
                                job.countQueue.Add(chosen[i].Count);
                            }
                            return job;
                        }
                    }
                }
            }

            return null;
        }
    }
}