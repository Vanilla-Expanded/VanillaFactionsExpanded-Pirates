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

        private CompShieldBubble shieldComp;
        CompShieldBubble ShieldComp
        {
            get
            {
                if (shieldComp is null)
                {
                    shieldComp = this.TryGetComp<CompShieldBubble>();
                }
                return shieldComp;
            }
        }
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            var shield = ShieldComp;
            if (shield != null)
            {
                return shield.AbsorbingDamage(dinfo, out bool absorbed);
            }
            else
            {
                return base.CheckPreAbsorbDamage(dinfo);
            }
        }
        public override void DrawWornExtras()
        {
            base.DrawWornExtras();
            Comps_PostDraw();
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
        public override bool AllowVerbCast(Verb verb)
        {
            var comp = ShieldComp;
            if (comp != null)
            {
                if (verb.IsMeleeAttack && comp.Props.dontAllowMeleeAttack)
                {
                    return false;
                }
                else if (!verb.IsMeleeAttack && comp.Props.dontAllowRangedAttack)
                {
                    return false;
                }
            }
            return base.AllowVerbCast(verb);
        }
        public override void PostPostMake()
        {
            base.PostPostMake();
            var comp = ShieldComp;
            if (comp != null && comp.Props.chargeFullyWhenMade)
            {
                comp.Energy = comp.EnergyMax;
            }
        }
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (Gizmo wornGizmo in base.GetWornGizmos())
            {
                yield return wornGizmo;
            }
            if (ShieldComp != null && Find.Selector.SingleSelectedThing == base.Wearer && base.Wearer.IsColonistPlayerControlled)
            {
                Gizmo_EnergyCompShieldStatus gizmo_EnergyShieldStatus = new Gizmo_EnergyCompShieldStatus();
                gizmo_EnergyShieldStatus.shield = ShieldComp;
                yield return gizmo_EnergyShieldStatus;
            }
        }
    }
}
