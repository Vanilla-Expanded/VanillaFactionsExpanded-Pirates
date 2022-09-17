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
            Patch(typeof(GlowGrid), "RecalculateAllGlow", postfix: AccessTools.Method(typeof(CurseOfDarkness), nameof(RecalculateAllGlowPostfix)));
            Patch(typeof(Building), nameof(Building.SpawnSetup), postfix: AccessTools.Method(typeof(CurseOfDarkness), nameof(MarkGlowDirty)));
            Patch(typeof(Building), nameof(Building.DeSpawn), prefix: AccessTools.Method(typeof(CurseOfDarkness), nameof(MarkGlowDirty)));
            Patch(typeof(SkyManager), "CurrentSkyTarget", postfix: AccessTools.Method(typeof(CurseOfDarkness), nameof(CurrentSkyTargetPostfix)));
            Patch(AccessTools.TypeByName("Verse.SectionLayer_Zones"), "Regenerate", transpiler: AccessTools.Method(typeof(CurseOfDarkness), nameof(DrawLayerTranspiler)));
        }
        public static bool IsDarklightAtPrefix(ref bool __result, IntVec3 position, Map map)
        {
            if (map.glowGrid.GameGlowAt(position) <= 0)
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static void RecalculateAllGlowPostfix(GlowGrid __instance, Map ___map)
        {
            int numGridCells = ___map.cellIndices.NumGridCells;
            for (int i = 0; i < numGridCells; i++)
            {
                var position = ___map.cellIndices.IndexToCell(i);
                if (position.Roofed(___map))
                {
                    var glow = ___map.glowGrid.GameGlowAt(position);
                    if (glow <= 0.6f)
                    {
                        __instance.glowGrid[i] = Color.Lerp(__instance.glowGrid[i], Color.black, 1f - glow);
                        __instance.glowGridNoCavePlants[i] = Color.Lerp(__instance.glowGridNoCavePlants[i], Color.black, 1f - glow);
                    }
                }
            }
            foreach (var zone in ___map.zoneManager.AllZones)
            {
                ___map.mapDrawer.MapMeshDirty(zone.cells[0], MapMeshFlag.Zone);
            }
        }

        public static void MarkGlowDirty(Building __instance)
        {
            __instance.Map.glowGrid.MarkGlowGridDirty(__instance.Position);
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
            var hiddenField = AccessTools.Field(typeof(Zone), nameof(Zone.hidden));
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
            if (zone.cells.TrueForAll(x => zone.Map.glowGrid.GameGlowAt(x) <= 0))
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
                    map.glowGrid.MarkGlowGridDirty(cell);
                }
            }
        }
    }
}
