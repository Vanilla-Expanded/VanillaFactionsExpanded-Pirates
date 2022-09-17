using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using Verse;
using System.Linq;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn), "AnythingToStrip")]
    public static class Pawn_AnythingToStrip_Patch
    {
        public static bool Prefix(Pawn __instance)
        {
            if (__instance.IsWearingWarcasket())
            {
                return false;
            }
            return true;
        }
    }
}
