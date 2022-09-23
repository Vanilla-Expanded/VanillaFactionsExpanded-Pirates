using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public class PawnRenderer_DrawEquipment_Patch
    {
        public static bool Prefix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            if (___pawn.kindDef == VFEP_DefOf.VFEP_Mech_Spidermine || ___pawn.kindDef == VFEP_DefOf.VFEP_Mech_Wardrone)
            {
				if (___pawn.Dead || !___pawn.Spawned || ___pawn.equipment == null || ___pawn.equipment.Primary == null 
					|| (___pawn.CurJob != null && ___pawn.CurJob.def.neverShowWeapon))
				{
					return false;
				}
				Vector3 drawLoc = new Vector3(0f, 0.03474903f, 0f);
				Stance_Busy stance_Busy = ___pawn.stances.curStance as Stance_Busy;
				if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid && (flags & PawnRenderFlags.NeverAimWeapon) == 0)
				{
					Vector3 vector = ((!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : 
						stance_Busy.focusTarg.Thing.DrawPos);
					float num = 0f;
					if ((vector - ___pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
					{
						num = (vector - ___pawn.DrawPos).AngleFlat();
					}
					drawLoc += rootLoc + new Vector3(0f, 0f, 0.1f).RotatedBy(num);
					DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc, num);
				}
				else
				{
					if (pawnRotation == Rot4.South)
					{
						drawLoc += rootLoc + new Vector3(0f, 0f, 0.1f).RotatedBy(143f);
						DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc, 143f);
					}
					else if (pawnRotation == Rot4.North)
					{
						drawLoc += rootLoc + new Vector3(0f, 0f, 0.1f).RotatedBy(143f);
						DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc, 143f);
					}
					else if (pawnRotation == Rot4.East)
					{
						drawLoc += rootLoc + new Vector3(0f, 0f, 0.1f).RotatedBy(143f);
						DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc, 143f);
					}
					else if (pawnRotation == Rot4.West)
					{
						drawLoc += rootLoc + new Vector3(0f, 0f, 0.1f).RotatedBy(217f);
						DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc, 217f);
					}
				}
				return false;
			}
			return true;

		}

		public static void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			Mesh mesh = null;
			float num = aimAngle - 90f;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			else if (aimAngle > 200f && aimAngle < 340f)
			{
				mesh = MeshPool.plane10Flip;
				num -= 180f;
				num -= eq.def.equippedAngleOffset;
			}
			else
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			num %= 360f;
			CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
			if (compEquippable != null)
			{
				EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
				drawLoc += drawOffset;
				num += angleOffset;
			}
			Material material = null;
			Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
			Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq), mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
		}

	}
}
