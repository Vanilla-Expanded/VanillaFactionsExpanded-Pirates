using RimWorld;
using VanillaWeaponsExpandedLaser;
using Verse;

namespace VFEPirates
{
    public class Projectile_LaserEradicator : LaserBeam
    {
        public override void Impact(Thing hitThing)
        {
            GenExplosion.DoExplosion(this.Position, this.Map, 1.9f, DamageDefOf.Bomb, this.launcher);
            base.Impact(hitThing);
        }
    }
}
