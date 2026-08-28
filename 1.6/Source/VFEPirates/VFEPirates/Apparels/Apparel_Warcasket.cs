using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using VEF;

namespace VFEPirates
{
    public class Apparel_Warcasket : Apparel
    {
        // Older saves store the color here rather than in CompColorable
        public Color? colorApparel;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref colorApparel, "colorApparel");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && colorApparel.HasValue)
            {
                this.SetColor(colorApparel.Value);
                colorApparel = null;
            }
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.Wearer != null && ModCompatibility.DubsBadHygieneActive)
            {
                ModCompatibility.FillBladderNeed(this.Wearer, 0.001f);
                ModCompatibility.FillHygieneNeed(this.Wearer, 0.001f);
            }
        }
    }
}
