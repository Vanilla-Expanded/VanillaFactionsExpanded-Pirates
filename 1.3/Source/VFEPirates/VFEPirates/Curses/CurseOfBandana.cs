using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfBandana : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Constructor(typeof(Stance_Busy), parameters: new Type[] { typeof(int), typeof(LocalTargetInfo), typeof(Verb) }), 
                postfix: AccessTools.Method(typeof(CurseOfBandana), nameof(CooldownTick)));
        }

        public static void CooldownTick(Stance_Busy __instance, int ticks, LocalTargetInfo focusTarg, Verb verb, ref int ___ticksLeft)
        {
            if (__instance is Stance_Cooldown)
			{
                ___ticksLeft = Mathf.CeilToInt(0.1f * ticks);
            }
        }
    }
}
