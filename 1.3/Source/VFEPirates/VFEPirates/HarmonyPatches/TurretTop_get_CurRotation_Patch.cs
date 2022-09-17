using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEPirates.HarmonyPatches
{
    //Don't rotate directional turrets. 
    [HarmonyPatch(typeof(TurretTop), "get_CurRotation")]
    class TurretTop_get_CurRotation_Patch
    {
        public static bool Prefix(ref Building_Turret ___parentTurret, ref int ___ticksUntilIdleTurn, ref float __result)
        {
            if(___parentTurret.AttackVerb is Verb_ShootCone)
            {
                var currentTarget = ___parentTurret.CurrentTarget;
                if (!currentTarget.IsValid)
                {
                    __result = ___parentTurret.Rotation.AsAngle;
                    return false;
                }
            }
            return true;
        }
    }
}
