using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
    public static class PawnRenderUtility_DrawEquipmentAiming_Patch
    {
        public static bool Prefix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
			var pawn = (eq.ParentHolder as Pawn_EquipmentTracker)?.pawn;
			if (pawn != null)
			{
                if (pawn.kindDef == VFEP_DefOf.VFEP_Mech_Spidermine || pawn.kindDef == VFEP_DefOf.VFEP_Mech_Wardrone)
                {
                    if (pawn.Dead || !pawn.Spawned || pawn.equipment == null || pawn.equipment.Primary == null
                        || (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon))
                    {
                        return false;
                    }
                    var pawnRotation = pawn.Rotation;
                    Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
                    if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
                    {
                        Vector3 vector = ((!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() :
                            stance_Busy.focusTarg.Thing.DrawPos);
                        float num = 0f;
                        if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (vector - pawn.DrawPos).AngleFlat();
                        }
                        drawLoc += new Vector3(0f, 0f, 0.1f).RotatedBy(num);
                        DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num);
                    }
                    else
                    {
                        if (pawnRotation == Rot4.South)
                        {
                            drawLoc += new Vector3(0f, 0f, 0.1f).RotatedBy(143f);
                            DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
                        }
                        else if (pawnRotation == Rot4.North)
                        {
                            drawLoc +=  new Vector3(0f, 0f, 0.1f).RotatedBy(143f);
                            DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
                        }
                        else if (pawnRotation == Rot4.East)
                        {
                            drawLoc += new Vector3(0f, 0f, 0.1f).RotatedBy(143f);
                            DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
                        }
                        else if (pawnRotation == Rot4.West)
                        {
                            drawLoc += new Vector3(0f, 0f, 0.1f).RotatedBy(217f);
                            DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 217f);
                        }
                    }
                    return false;
                }
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
			Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
			Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq), mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
		}

	}
}
