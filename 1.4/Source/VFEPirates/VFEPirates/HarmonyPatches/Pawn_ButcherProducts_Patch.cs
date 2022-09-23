using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn), "Strip")]
    public static class Pawn_Strip_Patch
    {
        public static Pawn pawnsWithWarcasket;
        public static void Prefix(Pawn __instance)
        {
            if (__instance.IsWearingWarcasket())
            {
                pawnsWithWarcasket = __instance;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "ButcherProducts")]
    public static class Pawn_ButcherProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance)
        {
            if (Pawn_Strip_Patch.pawnsWithWarcasket == __instance)
            {
                yield return ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                Pawn_Strip_Patch.pawnsWithWarcasket = null;
            }
            foreach (var thing in __result)
            {
                yield return thing;
            }
        }
    }
}
