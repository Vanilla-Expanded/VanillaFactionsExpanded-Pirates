using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfFire : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.PropertyGetter(typeof(FireWatcher), nameof(FireWatcher.LargeFireDangerPresent)), 
                postfix: AccessTools.Method(typeof(CurseOfFire), nameof(Postfix)));
        }

        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }
}
