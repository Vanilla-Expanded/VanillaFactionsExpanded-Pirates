using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEPirates
{
    [StaticConstructorOnStartup]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        static PawnGenerator_GeneratePawn_Patch()
        {
            VFEPiratesMod.harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] {typeof(PawnGenerationRequest) }),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PawnGenerator_GeneratePawn_Patch), nameof(Postfix))));
        }
        public static void Postfix(Pawn __result)
        {
            if (__result.IsWearingWarcasket())
            {
                for (int i = __result.apparel.WornApparel.Count - 1; i >= 0; i--)
                {
                    if (__result.apparel.WornApparel[i] is not Apparel_Warcasket)
                    {
                        __result.apparel.Remove(__result.apparel.WornApparel[i]);
                    }
                }
                CheckApparels(__result, VFEPiratesMod.allArmorDefs);
                CheckApparels(__result, VFEPiratesMod.allShoulderPadsDefs);
                CheckApparels(__result, VFEPiratesMod.allHelmetDefs);
                __result.apparel.LockAll();
            }
            void CheckApparels(Pawn pawn, List<WarcasketDef> apparels)
            {
                if (!pawn.apparel.WornApparel.Any(x => apparels.Contains(x.def)))
                {
                    var armorDef = apparels.RandomElement();
                    var armor = ThingMaker.MakeThing(armorDef, GenStuff.RandomStuffFor(armorDef)) as Apparel;
                    pawn.apparel.Wear(armor, false, true);
                }
            }
        }
    }
}
