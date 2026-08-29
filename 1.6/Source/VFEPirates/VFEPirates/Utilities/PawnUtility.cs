using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEPirates
{
    public static class WarcasketUtility
    {
        public static bool IsWearingWarcasket(this Pawn pawn)
        {
            if (pawn?.apparel == null)
            {
                return false;
            }
            var wornApparel = pawn.apparel.WornApparel;
            for (var i = 0; i < wornApparel.Count; i++)
            {
                if (wornApparel[i] is Apparel_Warcasket)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
