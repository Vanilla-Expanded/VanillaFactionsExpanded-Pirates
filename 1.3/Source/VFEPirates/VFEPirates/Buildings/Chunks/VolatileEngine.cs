using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace VFEPirates
{
    public class CompProperties_VolatileEngine : CompProperties
    {
        public SoundDef soundAmbient;

        public CompProperties_VolatileEngine() => this.compClass = typeof(CompVolatileEngine);
    }

    internal class CompVolatileEngine : ThingComp
    {
        protected CompGlower compGlower;
        private Sustainer sustainerProducingPower;
        int ticksCount = 1;
        int totalCount = 0;
        int glowRad = 0;
        float volumeFactor = 0.1f;

        private CompProperties_VolatileEngine Props => (CompProperties_VolatileEngine)this.props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksCount, "ticksCount", 1);
            Scribe_Values.Look(ref totalCount, "totalCount", 0);
            Scribe_Values.Look(ref glowRad, "glowRad", 0);
            Scribe_Values.Look(ref volumeFactor, "volumeFactor", 0.1f);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compGlower = parent.GetComp<CompGlower>();
            compGlower.Props.glowRadius = glowRad;
            compGlower.UpdateLit(parent.Map);
            parent.Map.glowGrid.MarkGlowGridDirty(parent.Position);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (sustainerProducingPower == null || sustainerProducingPower.Ended)
                return;
            sustainerProducingPower.End();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned)
            {
                if (this.Props.soundAmbient != null)
                {
                    if (sustainerProducingPower == null || sustainerProducingPower.Ended) sustainerProducingPower = Props.soundAmbient.TrySpawnSustainer(SoundInfo.InMap((TargetInfo)parent));
                    sustainerProducingPower.Maintain();
                    sustainerProducingPower.info.volumeFactor = volumeFactor;
                }

                if (ticksCount % 2500 == 0)
                {
                    glowRad++;
                    compGlower.Props.glowRadius = glowRad;
                    compGlower.UpdateLit(parent.Map);
                    parent.Map.glowGrid.MarkGlowGridDirty(parent.Position);
                    volumeFactor += 0.05f;
                    sustainerProducingPower.End();
                    totalCount++;
                }
                ticksCount++;

                if (totalCount == 6)
                {
                    parent.GetComp<CompExplosive>().StartWick();
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            int tickL = (6 * 2500) - ticksCount;
            if (tickL > 0)
            {
                string str = "VFEP.ExplodeIn".Translate() + " " + tickL.ToStringTicksToPeriod(false) + "\n" + base.CompInspectStringExtra();
                return str.TrimEndNewlines();
            }
            return base.CompInspectStringExtra();
        }
    }
}
