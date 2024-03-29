﻿using RimWorld;
using VanillaWeaponsExpandedLaser;
using Verse;

namespace VFEPirates
{
    public class Projectile_LaserEradicator : LaserBeam
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            GenExplosion.DoExplosion(this.Position, this.Map, 1.9f, DamageDefOf.Bomb, this.launcher);
            base.Impact(hitThing, blockedByShield);
        }
    }
}
