using Verse;

namespace VFEPirates
{
    public class Apparel_GuardianShoulders : Apparel_Warcasket
    {
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (Rand.Chance(0.25f))
            {
                return true;
            }
            else
            {
                return base.CheckPreAbsorbDamage(dinfo);
            }
        }
    }
}
