using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using Command_Ability = VFECore.Abilities.Command_Ability;

namespace VFEPirates
{
    public class Ability_GrapplingHook : Ability_ShootProjectile
    {
        public bool Loaded = true;

        public override TargetingParameters targetParams => Loaded ? base.targetParams : TargetingParameters.ForSelf(pawn);

        public override bool CanHitTarget(LocalTargetInfo target) =>
            base.CanHitTarget(target) && target.Thing is {def: {Fillage: FillCategory.Full}} or Plant {def: {plant: {IsTree: true}}};

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            if (Loaded)
            {
                var projectile = GenSpawn.Spawn(def.GetModExtension<AbilityExtension_Projectile>().projectile, pawn.Position, pawn.Map) as Projectile;
                if (projectile is AbilityProjectile abilityProjectile) abilityProjectile.ability = this;
                var target = targets[0].HasThing ? new LocalTargetInfo(targets[0].Thing) : new LocalTargetInfo(targets[0].Cell);
                projectile?.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
                if (projectile is Projectile_GrapplingHook hook) hook.UpdateDest();
                Loaded = false;
            }
            else
                Loaded = true;
        }

        public override void DoAction()
        {
            if (Event.current.button == 1 || Loaded)
                base.DoAction();
            else
                CreateCastJob((GlobalTargetInfo) pawn);
        }

        public override int GetCastTimeForPawn() => Loaded ? 0 : def.GetModExtension<AbilityExtension_GrapplingHook>().reloadTicks;
        public override float GetRangeForPawn() => Loaded ? base.GetRangeForPawn() : 0f;
        public override Gizmo GetGizmo() => new Command_Grapple(pawn, this);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Loaded, "loaded");
        }
    }

    public class Command_Grapple : Command_Ability
    {
        public Command_Grapple(Pawn pawn, Ability ability) : base(pawn, ability)
        {
            Setup();
        }

        public Ability_GrapplingHook Hook => ability as Ability_GrapplingHook;

        private void Setup()
        {
            icon = Hook.Loaded ? Hook.def.icon : Hook.def.GetModExtension<AbilityExtension_GrapplingHook>().ReloadIcon;
            defaultLabel = Hook.Loaded ? Hook.def.label : Hook.def.GetModExtension<AbilityExtension_GrapplingHook>().labelUnloaded;
        }
    }

    public class AbilityExtension_GrapplingHook : AbilityExtension_AbilityMod
    {
        public string labelUnloaded;
        private Texture2D reloadIcon;
        public string reloadIconPath;
        public int reloadTicks;
        public Texture2D ReloadIcon => reloadIcon ??= ContentFinder<Texture2D>.Get(reloadIconPath);
    }

    [StaticConstructorOnStartup]
    public class Projectile_GrapplingHook : AbilityProjectile
    {
        private static readonly Material RopeLineMat = MaterialPool.MatFrom("UI/Overlays/Rope", ShaderDatabase.Transparent, GenColor.FromBytes(99, 70, 41));
        private int ticksTillPull = -1;

        public Vector3 Origin => launcher.Spawned
            ? launcher.DrawPos
            : ThingOwnerUtility.SpawnedParentOrMe(launcher.ParentHolder)?.DrawPos ?? origin;

        public override Vector3 ExactPosition => ticksTillPull == -1 ? base.ExactPosition : destination;

        public void UpdateDest()
        {
            destination = usedTarget.Cell.ToVector3Shifted();
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksTillPull <= 0) return;
            ticksTillPull--;
            if (ticksTillPull <= 0) Pull();
        }

        public void Pull()
        {
            ticksTillPull = -2;
            var destCell = usedTarget.Thing.OccupiedRect().AdjacentCells.MinBy(cell => cell.DistanceTo(launcher.Position));
            var flyer = (PawnFlyer_Pulled) PawnFlyer.MakeFlyer(VFEP_DefOf.VFEP_GrapplingPawn, ability.pawn, destCell);
            flyer.Hook = this;
            GenSpawn.Spawn(flyer, destCell, Map);
        }

        public override void Draw()
        {
            GenDraw.DrawLineBetween(Origin, DrawPos, AltitudeLayer.PawnRope.AltitudeFor(), RopeLineMat, 0.05f);
            base.Draw();
        }

        protected override void Impact(Thing hitThing)
        {
            GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
            ticksTillPull = 10;
            landed = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksTillPull, "ticksTillPull");
        }
    }

    public class PawnFlyer_Pulled : PawnFlyer
    {
        protected Vector3 effectivePos;
        public Projectile_GrapplingHook Hook;
        private int positionLastComputedTick;

        public override Vector3 DrawPos
        {
            get
            {
                RecomputePosition();
                return effectivePos;
            }
        }

        protected override void RespawnPawn()
        {
            base.RespawnPawn();
            Hook.Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Hook, "hook");
        }

        public override void Tick()
        {
            base.Tick();
            if (FlyingPawn != null) RecomputePosition();
        }

        protected bool CheckRecompute()
        {
            if (positionLastComputedTick == ticksFlying) return false;

            positionLastComputedTick = ticksFlying;
            return true;
        }

        protected virtual void RecomputePosition()
        {
            if (CheckRecompute()) return;
            effectivePos = Vector3.Lerp(startVec, DestinationPos, ticksFlying / (float) ticksFlightTime);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            FlyingPawn.DrawAt(drawLoc, flip);
        }
    }
}