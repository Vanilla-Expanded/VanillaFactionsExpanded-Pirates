using Verse;

namespace VFEPirates
{
    public class Hediff_KillItself : HediffWithComps
    {
        public int ticksToKill = -1;
		public bool killInstant;
        public override void Tick()
        {
            base.Tick();
            if (killInstant)
            {   
                //Log.Message("Killing " + pawn.LabelShort + " instantly");
                //Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                this.pawn.Kill(null);
                return;
			}
			if (pawn.mindState.enemyTarget != null)
            {
				ticksToKill = -1;
			}
            else if (ticksToKill == -1)
            {
				ticksToKill = Find.TickManager.TicksGame + 180;
            }

			if (ticksToKill > 0 && Find.TickManager.TicksGame >= ticksToKill)
            {
                //Log.Message("Killing " + pawn.LabelShort + " after " + (ticksToKill) + " ticks");
                //Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                this.pawn.Kill(null);
			}
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref ticksToKill, "ticksToKill", -1);
			Scribe_Values.Look(ref killInstant, "killInstant", false);
		}
	}
}
