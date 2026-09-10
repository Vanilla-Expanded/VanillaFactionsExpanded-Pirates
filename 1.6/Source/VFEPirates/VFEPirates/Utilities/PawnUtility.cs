using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using VEF.Apparels;

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

        // VEF only skips traitsOnEquip/traitsOnUnequip while both of these flags are set
        public static void WithoutApparelTraits(Action action)
        {
            var prev = ApparelExtensionUtilities.doNotRunTraitsPatch;
            var prevLegacy = VanillaExpandedFramework_Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch;
            ApparelExtensionUtilities.doNotRunTraitsPatch = true;
            VanillaExpandedFramework_Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch = true;
            try
            {
                action();
            }
            finally
            {
                ApparelExtensionUtilities.doNotRunTraitsPatch = prev;
                VanillaExpandedFramework_Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch = prevLegacy;
            }
        }
    }
}
