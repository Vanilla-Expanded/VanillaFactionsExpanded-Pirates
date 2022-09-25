using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using VFECore;

namespace VFEPirates
{
    public class Apparel_Warcasket : Apparel
    {
        public Color? colorApparel;
        public override Color DrawColor => colorApparel ??= this.def.colorGenerator.NewRandomizedColor();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref colorApparel, "colorApparel");
        }

        public override void Tick()
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
