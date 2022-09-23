using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    internal class IncomingSmoker : Skyfaller
    {
        public override void Tick()
        {
            base.Tick();
            if (Spawned && ticksToImpact > 30)
            {
                ThrowBlackSmoke(this.DrawPos, this.Map, 4);
            }
        }

        private void ThrowBlackSmoke(Vector3 loc, Map map, float size)
        {
            if (!loc.ShouldSpawnMotesAt(map))
                return;

            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, Rand.Range(1.5f, 2.5f) * size);
            dataStatic.rotationRate = Rand.Range(-30f, 30f);
            dataStatic.velocityAngle = Rand.Range(30, 40);
            dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
            dataStatic.instanceColor = Color.black;
            map.flecks.CreateFleck(dataStatic);
        }
    }
}
