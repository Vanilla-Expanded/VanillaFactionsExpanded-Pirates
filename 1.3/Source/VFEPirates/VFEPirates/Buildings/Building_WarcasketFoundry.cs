using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using VFEPirates.Buildings;

namespace VFEPirates
{
    [StaticConstructorOnStartup]
    public class Building_WarcasketFoundry : Building
    {
        public static readonly Material Frame = MaterialPool.MatFrom("Things/Building/WarcasketFoundry/WarcasketFrame");

        [TweakValue("00", 0, 10)] public static float yOffset = 4.5f;
        [TweakValue("00", 0, 10)] public static float zOffset = 0.7f;

        [TweakValue("00", 0, 10)] public static float ySize = 2f;
        [TweakValue("00", 0, 10)] public static float zSize = 2f;

        [TweakValue("00", 0, 1)] public static float BarSizeX = 0.815f;
        [TweakValue("00", 0, 1)] public static float BarSizeY = 0.175f;


        [TweakValue("00", 0, 1)] public static float BatOffsetX = 0;
        [TweakValue("00", 0, 1)] public static float BatOffsetZ = 0.25f;
        [TweakValue("00", 0, 6)] public static float BatOffsetY = 5f;

        private static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));
        private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        public CompPowerTrader compPower;

        public WarcasketProject curWarcasketProject;
        public Pawn occupant;

        public float weldingProgress;

        public bool OccupantAliveAndPresent => occupant != null && occupant.Position == Position && !occupant.Downed && !occupant.Dead;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.TryGetComp<CompPowerTrader>();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var g in base.GetFloatMenuOptions(selPawn)) yield return g;
            if (Faction == Faction.OfPlayer)
                if (occupant is null)
                {
                    var reason = CannotUseNowReason(selPawn);
                    if (reason != null)
                        yield return new FloatMenuOption("VFEPirates.CannotEntomb".Translate(reason), null);
                    else
                    {
                        var jobDef = VFEP_DefOf.VFEP_EntombIn;
                        string label = "VFEPirates.EntombInWarcasket".Translate();
                        if (selPawn.WorkTagIsDisabled(WorkTags.Violent))
                        {
                            label += "VFEP.EntombInIncapableOfViolenceWarning".Translate(selPawn.Named("PAWN"));
                        }
                        Action action = delegate
                        {
                            var job = JobMaker.MakeJob(jobDef, this);
                            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            Messages.Message("VFEPirates.EntombingWarning".Translate(), this, MessageTypeDefOf.CautionInput);
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), selPawn, this);
                    }
                }
        }

        public string CannotUseNowReason(Pawn selPawn)
        {
            if (!compPower.PowerOn) return "NoPower".Translate();
            if (!VFEPiratesMod.allShoulderPadsDefs.Any(d => d.IsResearchFinished) || !VFEPiratesMod.allShoulderPadsDefs.Any(d => d.IsResearchFinished) ||
                !VFEPiratesMod.allShoulderPadsDefs.Any(d => d.IsResearchFinished))
                return "VFEP.NoWarcaskets".Translate();
            return null;
        }

        public IEnumerable<IngredientCount> RequiredIngredients() => curWarcasketProject.RequiredIngredients();

        public bool ReadyForWelding(Pawn crafter, out string failReason)
        {
            failReason = null;
            if (!compPower.PowerOn) failReason = "NoPower".Translate();
            if (!OccupantAliveAndPresent) return false;
            if (curWarcasketProject is null) return false;
            return failReason is null;
        }

        public override void Tick()
        {
            base.Tick();
            if (!OccupantAliveAndPresent && occupant != null) DeregisterOccupant();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos()) yield return g;
            if (Prefs.DevMode)
                if (occupant != null && curWarcasketProject != null)
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Finish welding",
                        action = FinishWarCasketProject
                    };
        }

        public void FinishWarCasketProject()
        {
            curWarcasketProject.ApplyOn(occupant);
            curWarcasketProject = null;
            DeregisterOccupant();
            compPower.powerOutputInt = -50;
        }

        public void RegisterOccupant(Pawn pawn)
        {
            occupant = pawn;
        }

        public void DeregisterOccupant()
        {
            occupant?.jobs?.StopAll();
            occupant = null;
        }

        public override void Draw()
        {
            base.Draw();
            if (occupant != null && curWarcasketProject != null)
            {
                var pos = this.TrueCenter();
                pos.y += yOffset;
                pos.z += zOffset;
                var quat = Quaternion.identity;
                var matrix = default(Matrix4x4);
                matrix.SetTRS(pos, quat, new Vector3(ySize, 1, zSize));
                Graphics.DrawMesh(MeshPool.plane10, matrix, Frame, 0);

                var r = default(GenDraw.FillableBarRequest);
                r.center = DrawPos + new Vector3(BatOffsetX, BatOffsetY, BatOffsetZ);
                r.size = new Vector2(BarSizeX, BarSizeY);
                r.fillPercent = curWarcasketProject.currentWorkAmountDone / curWarcasketProject.totalWorkAmount;
                r.filledMat = BatteryBarFilledMat;
                r.unfilledMat = BatteryBarUnfilledMat;
                r.margin = 0.15f;
                var rotation = Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                r.rotation = rotation;
                GenDraw.DrawFillableBar(r);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref occupant, "occupant");
            Scribe_Deep.Look(ref curWarcasketProject, "curWarcasketProject");
        }
    }
}