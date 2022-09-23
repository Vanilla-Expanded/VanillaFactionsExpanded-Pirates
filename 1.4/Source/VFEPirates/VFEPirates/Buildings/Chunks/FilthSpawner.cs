using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEPirates
{
    public class CompProperties_FilthSpawner : CompProperties
    {
        public ThingDef filthDef;
        public int spawnCountOnSpawn = 1;
        public int spawnRadius = 3;
        public int totalFilthSpawn = 40;
        public int spawnEachXticks = 300;

        public CompProperties_FilthSpawner() => this.compClass = typeof(CompFilthSpawner);
    }

    internal class CompFilthSpawner : ThingComp
    {
        int ticksCount = 0;
        int leftToSpawn;

        private CompProperties_FilthSpawner Props => (CompProperties_FilthSpawner)this.props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksCount, "ticksCount", 0);
            Scribe_Values.Look(ref leftToSpawn, "leftToSpawn", Props.totalFilthSpawn);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            leftToSpawn = Props.totalFilthSpawn;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ticksCount % Props.spawnEachXticks == 0 && parent.Spawned)
            {
                for (int i = 0; i < Props.spawnCountOnSpawn && leftToSpawn > 0; i++)
                {
                    if (RCellFinder.TryFindRandomCellNearWith(parent.Position, c => c.Walkable(parent.Map) && FilthMaker.CanMakeFilth(c, parent.Map, Props.filthDef), parent.Map, out IntVec3 c, 2, Props.spawnRadius) &&
                        FilthMaker.TryMakeFilth(c, parent.Map, Props.filthDef, 1))
                    {
                        leftToSpawn--;
                    }
                }

                if (leftToSpawn == 0) parent.AllComps.Remove(this);
            }
            ticksCount++;
        }
    }
}