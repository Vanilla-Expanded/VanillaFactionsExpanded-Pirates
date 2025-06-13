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
            if (pawn.apparel != null)
            {
                return pawn.apparel.WornApparel.Any(x => x is Apparel_Warcasket);
            }
            return false;
        }
    }
}
