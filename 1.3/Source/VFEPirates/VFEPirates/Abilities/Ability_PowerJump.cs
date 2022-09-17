using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using Command_Ability = VFECore.Abilities.Command_Ability;

namespace VFEPirates
{
    public class PowerJumpExtension : DefModExtension
    {
        public int detonationDamageAmount;
        public float fuelConsumption;
        public SoundDef powerJumpSoundActivated;
    }

    public class Ability_PowerJump : Ability
    {
        public float Range => pawn.GetStatValue(VFEP_DefOf.VFEP_PowerJumpRange);
        public float FuelConsumption => def.GetModExtension<PowerJumpExtension>().fuelConsumption;
        public float DetonationRadius => pawn.GetStatValue(VFEP_DefOf.VFEP_PowerJumpDetonationRadius);
        public int DetonationDamageAmount => def.GetModExtension<PowerJumpExtension>().detonationDamageAmount;
        public SoundDef PowerJumpSoundActivated => def.GetModExtension<PowerJumpExtension>().powerJumpSoundActivated;

        public override bool CanHitTarget(LocalTargetInfo target) => target.Cell.WalkableBy(pawn.Map, pawn) && target.Cell.DistanceTo(pawn.Position) <= Range;

        public override float GetRangeForPawn() => Range;

        public override Gizmo GetGizmo()
        {
            var action = new Command_Ability(pawn, this);
            if (holder.TryGetComp<CompReloadable>().RemainingCharges < FuelConsumption) action.Disable("VFEP.NotEnoughFuel".Translate());
            return action;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var map = pawn.Map;
            var position = pawn.Position;
            var comp = holder.TryGetComp<CompReloadable>();
            for (var i = 0; i < FuelConsumption; i++) comp.UsedOnce();

            var sound = PowerJumpSoundActivated;
            if (sound != null) sound.PlayOneShot(new TargetInfo(position, map));

            var destCell = targets[0].Cell;
            AbilityPawnFlyer flyer = (PawnFlyer_PowerJump) PawnFlyer.MakeFlyer(VFEP_DefOf.VFEP_PowerJumpPawn, pawn, destCell);
            flyer.ability = this;
            flyer.target = destCell.ToVector3();
            GenSpawn.Spawn(flyer, destCell, map);
            GenExplosion.DoExplosion(position, map, DetonationRadius, DamageDefOf.Bomb, pawn, DetonationDamageAmount, ignoredThings: new List<Thing> {pawn});
        }
    }

    public class PawnFlyer_PowerJump : AbilityPawnFlyer
    {
        private Effecter flightEffecter;

        public override Vector3 DrawPos => position;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var a = Mathf.Max(Traverse.Create(this).Field<float>("flightDistance").Value, 1f) / FlyingPawn.GetStatValue(VFEP_DefOf.VFEP_FlightSpeed);
                a = Mathf.Max(a, def.pawnFlyer.flightDurationMin);
                ticksFlightTime = a.SecondsToTicks();
                ticksFlying = 0;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var vector = position;
            vector.y += 10;
            FlyingPawn.Drawer.renderer.RenderPawnAt(vector, direction);
        }

        public override void Tick()
        {
            if (flightEffecter == null && def.pawnFlyer.flightEffecterDef != null)
            {
                flightEffecter = def.pawnFlyer.flightEffecterDef.Spawn();
                flightEffecter.Trigger(this, TargetInfo.Invalid);
            }
            else
                flightEffecter?.EffectTick(this, TargetInfo.Invalid);

            base.Tick();
        }

        protected override void RespawnPawn()
        {
            var pawn = FlyingPawn;
            base.RespawnPawn();
            var powerJumpAbility = ability as Ability_PowerJump;
            GenExplosion.DoExplosion(Position, Map, powerJumpAbility.DetonationRadius, DamageDefOf.Bomb, pawn, powerJumpAbility.DetonationDamageAmount,
                ignoredThings: new List<Thing> {pawn});
        }
    }
}