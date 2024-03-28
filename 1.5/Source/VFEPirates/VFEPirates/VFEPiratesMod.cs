using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    public class VFEPiratesMod : Mod
    {
        public static Harmony harmony;
        public static VFEPiratesSettings settings;

        public static List<WarcasketDef> allArmorDefs = new List<WarcasketDef>();
        public static List<WarcasketDef> allShoulderPadsDefs = new List<WarcasketDef>();
        public static List<WarcasketDef> allHelmetDefs = new List<WarcasketDef>();

        public VFEPiratesMod(ModContentPack modContentPack) : base(modContentPack)
        {
            settings = GetSettings<VFEPiratesSettings>();
            harmony = new Harmony("VFEPirates.Mod");
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard lst = new();
            lst.Begin(inRect);
            lst.CheckboxLabeled("Disable gauntlet raids:", ref settings.disableGauntlet);
            lst.End();
        }

        public override string SettingsCategory() => "Vanilla Factions Expanded Pirates";
    }

    public class VFEPiratesSettings : ModSettings
    {
        public bool disableGauntlet = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disableGauntlet, "disableGauntlet");
        }
    }
}
