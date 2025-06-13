using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfInfestation : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(InfestationUtility), nameof(InfestationUtility.SpawnTunnels)), 
                prefix: AccessTools.Method(typeof(CurseOfInfestation), nameof(Infestation)));
        }

        public static void Infestation(ref int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null, IntVec3? overrideLoc = null, float? insectsPoints = null)
        {
            hiveCount *= 2;
        }
    }
}
