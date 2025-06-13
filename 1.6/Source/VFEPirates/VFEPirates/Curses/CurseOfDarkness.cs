using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.Noise;

namespace VFEPirates
{
    public class CurseOfDarkness : CurseWorker
    {
        public override void DoPatches()
        {
            Patch(typeof(DarklightUtility), "IsDarklightAt", prefix: AccessTools.Method(typeof(CurseOfDarkness), nameof(IsDarklightAtPrefix)));
            Patch(typeof(SkyManager), "CurrentSkyTarget", postfix: AccessTools.Method(typeof(CurseOfDarkness), nameof(CurrentSkyTargetPostfix)));
            Patch(AccessTools.TypeByName("Verse.SectionLayer_Zones"), "Regenerate", transpiler: AccessTools.Method(typeof(CurseOfDarkness), nameof(DrawLayerTranspiler)));
            Patch(AccessTools.Method(typeof(GlowGrid), "GetAccumulatedGlowAt", new System.Type[] { typeof(int), typeof(bool) }), postfix: AccessTools.Method(typeof(CurseOfDarkness), nameof(GetAccumulatedGlowAtPostfix)));
            Patch(typeof(Building), nameof(Building.SpawnSetup), postfix: AccessTools.Method(typeof(CurseOfDarkness), nameof(MarkGlowDirty)));
            Patch(typeof(Building), nameof(Building.DeSpawn), prefix: AccessTools.Method(typeof(CurseOfDarkness), nameof(MarkGlowDirty)));
        }

        public static void MarkGlowDirty(Building __instance)
        {
            __instance.Map.glowGrid.DirtyCell(__instance.Position);
        }

        public static bool IsDarklightAtPrefix(ref bool __result, IntVec3 position, Map map)
        {
            if (map.glowGrid.GroundGlowAt(position) <= 0)
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static void GetAccumulatedGlowAtPostfix(ref Color32 __result, GlowGrid __instance, Map ___map, int index, bool ignoreCavePlants = false)
        {
            var position = ___map.cellIndices.IndexToCell(index);
            if (position.Roofed(___map))
            {
                var glow = GlowAt(__result);
                if (glow <= 0.6f)
                {
                    __result = Color.Lerp(__result, Color.black, 1f - glow);
                }
            }
        }

        public static float GlowAt(Color32 accumulatedGlowAt)
        {
            if (accumulatedGlowAt.a == 1)
            {
                return 1f;
            }
            float b = (float)Mathf.Max(Mathf.Max(accumulatedGlowAt.r, accumulatedGlowAt.g), accumulatedGlowAt.b) / 255f * 3.6f;
            b = Mathf.Min(0.5f, b);
            return b;
        }

        public static void CurrentSkyTargetPostfix(ref SkyTarget __result)
        {
            __result.colors = new SkyColorSet(Color.Lerp(__result.colors.sky, Color.black, 1f - __result.glow), __result.colors.shadow, __result.colors.overlay, __result.colors.saturation);
        }
        public override void OnActivate()
        {
            base.OnActivate();
            RefreshEvererything();
        }

        public override void OnDisactivate()
        {
            base.OnDisactivate();
            RefreshEvererything();
        }

        public static IEnumerable<CodeInstruction> DrawLayerTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var hiddenField = AccessTools.Field(typeof(Zone), nameof(Zone.Hidden));
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (i > 1 && codes[i - 1].LoadsField(hiddenField) && codes[i].opcode == OpCodes.Brtrue)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CurseOfDarkness), nameof(CurseOfDarkness.ShouldShowZone)));
                    yield return new CodeInstruction(OpCodes.Brfalse, codes[i].operand);
                }
            }
        }

        public static bool ShouldShowZone(Zone zone)
        {
            if (zone.cells.TrueForAll(x => zone.Map.glowGrid.GroundGlowAt(x) <= 0))
            {
                return false;
            }
            return true;
        }

        public void RefreshEvererything()
        {
            foreach (var map in Find.Maps)
            {
                foreach (var cell in map.AllCells)
                {
                    map.glowGrid.DirtyCell(cell);
                }
            }
        }
    }
}
