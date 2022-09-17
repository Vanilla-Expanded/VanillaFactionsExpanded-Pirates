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
    public static class Building_TurretGun_DrawExtraSelectionOverlays_Patch
    {
        public static void Postfix(Building_TurretGun __instance)
        {
            //Draw cone instead of radius ring for directional turret
            if((__instance.AttackVerb is Verb_ShootCone verb_ShootCone))
            {
                verb_ShootCone.DrawHighlight(verb_ShootCone.CurrentTarget);
            }
        }
    }
}
