using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfSteel : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.PropertyGetter(typeof(DamageInfo), nameof(DamageInfo.Amount)), postfix: AccessTools.Method(typeof(CurseOfSteel), nameof(Postfix)));
        }

        public static void Postfix(ref float __result)
        {
            __result *= 2;
        }
    }
}
