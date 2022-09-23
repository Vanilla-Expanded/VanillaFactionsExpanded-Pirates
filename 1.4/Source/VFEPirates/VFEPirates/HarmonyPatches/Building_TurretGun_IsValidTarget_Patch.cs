using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEPirates.HarmonyPatches
{
    [HarmonyPatch(typeof(Building_TurretGun), "IsValidTarget")]
    class Building_TurretGun_IsValidTarget_Patch
    {
        public static void Postfix(Thing t, Building_TurretGun __instance, ref bool __result)
        {
            if(__instance.AttackVerb is Verb_ShootCone verbShootCone)
            {
                __result &= verbShootCone.InCone(t.Position, verbShootCone.caster.Position, verbShootCone.Caster.Rotation, verbShootCone.VerbProps.coneAngle);
            }
        }
    }
}
