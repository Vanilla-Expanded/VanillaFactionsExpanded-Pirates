using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using VFECore;

namespace VFEPirates
{
    public class Verb_ShieldDetonation : Verb
    {
        public override bool Available()
        {
            return base.Available() && GetShieldComps.Any(x => x.Energy > 0);
        }
        public override bool IsMeleeAttack => true;
        public IEnumerable<CompShieldBubble> GetShieldComps
        {
            get
            {
                var apparels = CasterPawn?.apparel?.WornApparel;
                foreach (var apparel in apparels)
                {
                    var comp = apparel.GetComp<CompShieldBubble>();
                    if (comp != null)
                    {
                        yield return comp;
                    }
                }
            }
        }
        protected override bool TryCastShot()
        {
            var comps = GetShieldComps.ToList();
            var energyTotal = comps.Sum(x => x.Energy);
            GenExplosion.DoExplosion(Caster.Position, Caster.Map, energyTotal / 50f, DamageDefOf.Bomb, Caster, 
                (int)(energyTotal / 20f), weapon: EquipmentSource.def, ignoredThings: new List<Thing> { Caster});
            foreach (var comp in comps)
            {
                comp.Energy = 0;
            }
            return true;
        }
    }
}
