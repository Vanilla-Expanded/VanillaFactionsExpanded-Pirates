using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Unlock")]
    public static class Pawn_ApparelTracker_Unlock_Patch
    {
        public static bool Prefix(Apparel apparel)
        {
            if (apparel is Apparel_Warcasket)
            {
                return false;
            }
            return true;
        }
    }
}
