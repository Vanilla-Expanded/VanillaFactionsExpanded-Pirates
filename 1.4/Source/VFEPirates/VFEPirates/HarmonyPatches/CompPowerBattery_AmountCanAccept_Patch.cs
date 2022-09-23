using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEPirates
{
    [HarmonyPatch(typeof(CompPowerBattery))]
    [HarmonyPatch("AmountCanAccept", MethodType.Getter)]
    public class CompPowerBattery_AmountCanAccept_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref CompPowerBattery __instance, ref float __result)
        {
            if (__instance.parent.def.defName == "VFEP_ShipChunkBattery")
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }
}
