using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFEPirates.Buildings;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(Building_TurretGun), "DrawExtraSelectionOverlays")]
    public static class Building_TurretGun_DrawExtraSelectionOverlays_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool flag = false; //only apply patch to first occurence of DrawRadiusRing
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand as MethodInfo == typeof(GenDraw).GetMethod("DrawRadiusRing", new Type[] { typeof(IntVec3), typeof(float) }) && !flag)
                {
                    flag = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Building_TurretGun_DrawExtraSelectionOverlays_Transpiler), nameof(DrawConeForDirectionalTurret)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static void DrawConeForDirectionalTurret(IntVec3 center, float radius, Building_TurretGun instance)
        {
            //Don't draw radius ring for directional turrets
            if(!(instance.AttackVerb is Verb_ShootCone))
            {
                GenDraw.DrawRadiusRing(instance.Position, radius);
            }
        }
    }
}
