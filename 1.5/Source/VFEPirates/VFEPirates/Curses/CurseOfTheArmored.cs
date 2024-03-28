using System;
using System.Text;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEPirates
{
    public class CurseOfTheArmored : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(original: AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration), parameters: new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver) }), 
                postfix: AccessTools.Method(typeof(CurseOfTheArmored), nameof(AdjustedArmorPenetrationThing)));
            Patch(original: AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration), parameters: new Type[] { typeof(Tool), typeof(Pawn), typeof(ThingDef), typeof(ThingDef), typeof(HediffComp_VerbGiver) }),
                postfix: AccessTools.Method(typeof(CurseOfTheArmored), nameof(AdjustedArmorPenetrationThingDef)));
            Patch(original: AccessTools.Method(typeof(ExtraDamage), nameof(ExtraDamage.AdjustedArmorPenetration), parameters: new Type[] { }),
                postfix: AccessTools.Method(typeof(CurseOfTheArmored), nameof(GetArmorPenetrationBase)));
            Patch(original: AccessTools.Method(typeof(ExtraDamage), nameof(ExtraDamage.AdjustedArmorPenetration), parameters: new Type[] { typeof(Verb), typeof(Pawn) }),
                postfix: AccessTools.Method(typeof(CurseOfTheArmored), nameof(GetArmorPenetrationVerb)));
            Patch(original: AccessTools.Method(typeof(ProjectileProperties), nameof(ProjectileProperties.GetArmorPenetration), parameters: new Type[] { typeof(float), typeof(StringBuilder) }), 
                postfix: AccessTools.Method(typeof(CurseOfTheArmored), nameof(GetArmorPenetrationMultiplier)));
        }

        public static void AdjustedArmorPenetrationThing(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource, ref float __result)
        {
            __result /= 2;
        }

        public static void AdjustedArmorPenetrationThingDef(Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff, HediffComp_VerbGiver hediffCompSource, ref float __result)
        {
            __result /= 2;
        }

        public static void GetArmorPenetrationBase(ref float __result)
        {
            __result /= 2;
        }

        public static void GetArmorPenetrationVerb(Verb verb, Pawn caster, ref float __result)
        {
            __result /= 2;
        }

        public static void GetArmorPenetrationMultiplier(float weaponDamageMultiplier, StringBuilder explanation, ref float __result)
        {
            __result /= 2;
        }
    }
}
