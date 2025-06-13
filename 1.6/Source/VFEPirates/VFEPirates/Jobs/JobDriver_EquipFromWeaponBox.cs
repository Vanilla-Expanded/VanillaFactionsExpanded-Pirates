using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFEPirates
{
    public class JobDriver_EquipFromWeaponBox : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				var comp = this.TargetA.Thing.TryGetComp<CompWeaponBox>();
				comp.parent.Destroy();
				var weapon = ThingMaker.MakeThing(comp.Props.weaponToEquip) as ThingWithComps;
				pawn.equipment.MakeRoomFor(weapon);
				pawn.equipment.AddEquipment(weapon);
				if (weapon.def.soundInteract != null)
				{
					weapon.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}
	}
}
