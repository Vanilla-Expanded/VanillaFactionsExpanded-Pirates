using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEPirates
{
    internal class CompProperties_Fueled : CompProperties_Refuelable
    {
        public CompProperties_Fueled() => this.compClass = typeof(Fueled);
    }

    internal class Fueled : CompRefuelable
    {
        public new bool ShouldAutoRefuelNow
        {
            get { return false; }
        }

        public new bool ShouldAutoRefuelNowIgnoringFuelPct
        {
            get { return false; } 
        }

        public new int GetFuelCountToFullyRefuel() { return 0; }

        public override void PostDraw() { }
    }
}
