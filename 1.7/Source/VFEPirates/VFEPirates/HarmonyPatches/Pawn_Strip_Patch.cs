using HarmonyLib;
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
}
