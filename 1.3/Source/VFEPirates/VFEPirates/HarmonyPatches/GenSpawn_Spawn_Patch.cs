using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VFEPirates
{
    [HarmonyPatch(typeof(GenSpawn), "Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
    public static class GenSpawn_Spawn_Patch
    {
        public static bool Prefix(ref Thing newThing, ref WipeMode wipeMode, bool respawningAfterLoad)
        {
            if (newThing?.def is WarcasketDef)
            {
                return false;
            }
            return true;
        }
    }
}
