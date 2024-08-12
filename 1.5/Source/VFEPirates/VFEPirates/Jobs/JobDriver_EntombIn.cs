using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using VFEPirates.Buildings;

namespace VFEPirates
{
    public class JobDriver_GoToFoundry : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;
        public Building_WarcasketFoundry Foundry => TargetA.Thing as Building_WarcasketFoundry;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, Foundry.Position);
            yield return Toils_General.Do(delegate
            {
                var job = JobMaker.MakeJob(VFEP_DefOf.VFEP_EntombIn, Foundry);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }

    public class JobDriver_EntombIn : JobDriver
    {
        public Building_WarcasketFoundry Foundry => TargetA.Thing as Building_WarcasketFoundry;
        public override Vector3 ForcedBodyOffset => new Vector3(0, 0, 0.67f);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var project = new WarcasketProject(pawn, VFEP_DefOf.VFEP_Warcasket_Warcasket, VFEP_DefOf.VFEP_WarcasketShoulders_Warcasket, VFEP_DefOf.VFEP_WarcasketHelmet_Warcasket);
                var wasWearingWarcasket = pawn.IsWearingWarcasket();
                var previousApparels = this.pawn.apparel.WornApparel.ListFullCopy();
                foreach (var apparel in previousApparels)
                {
                    this.pawn.apparel.Remove(apparel);
                }
                Find.WindowStack.Add(new Dialog_WarcasketCustomization(project, pawn, onAccept: () =>
                {
                    Foundry.RegisterOccupant(pawn);
                    if (wasWearingWarcasket)
                        GenSpawn.Spawn(ThingDefOf.ChunkSlagSteel, pawn.Position, pawn.Map);
                    foreach (var apparel in pawn.apparel.WornApparel.ToList())
                        apparel.Destroy();
                    foreach (var apparel in previousApparels)
                        GenSpawn.Spawn(apparel, pawn.Position, pawn.Map);
                    Foundry.curWarcasketProject = project;
                }, onCancel: () =>
                {
                    Foundry.DeregisterOccupant();
                    foreach (var apparel in pawn.apparel.WornApparel.ToList())
                        apparel.Destroy();

                    foreach (var apparel in previousApparels)
                        pawn.apparel.Wear(apparel);
                    EndJobWith(JobCondition.Incompletable);
                }));
            };
            toil.tickAction = delegate { pawn.rotationTracker.FaceCell(pawn.Position + Rot4.South.FacingCell); };
            toil.socialMode = RandomSocialMode.Quiet;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            yield return toil;
        }
    }
}