using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEPirates
{
    internal class Building_CrashedShip : Building_TurretGun
    {
        int tick = 0;
        bool spawned = false;

        private List<Pawn> innerContainer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            for (int i = 0; i < 5; i++)
            {
                FleckMaker.ThrowDustPuff(this.TrueCenter(), this.Map, 2f);
                FleckMaker.ThrowFireGlow(this.TrueCenter(), this.Map, 2f);
                FleckMaker.ThrowHeatGlow(this.TrueCenter().ToIntVec3(), this.Map, 2f);
            }

            for (int i = 0; i < 10; i++)
            {
                RCellFinder.TryFindRandomCellNearWith(this.Position, c => c.Walkable(Map), Map, out IntVec3 r);
                if (Rand.Bool) FireUtility.TryStartFireIn(r, map, Rand.Range(0.1f, 0.925f));
                FilthMaker.TryMakeFilth(r, Map, ThingDefOf.Filth_Fuel, 1);
            }
        }

        public void AddPawnToShip(Pawn p)
        {
            if (innerContainer.NullOrEmpty()) innerContainer = new List<Pawn> { p };
            else innerContainer.Add(p);
        }

        public override void Tick()
        {
            base.Tick();
            if (tick >= 700 && !spawned)
            {
                Lord lord = LordMaker.MakeNewLord(this.Faction, new LordJob_AssaultColony(this.Faction), base.Map);
                RCellFinder.TryFindRandomCellNearWith(this.TrueCenter().ToIntVec3(), c => c.Walkable(Map), Map, out IntVec3 c, 3);
                innerContainer.ForEach(p =>
                {
                    GenSpawn.Spawn(p, c, Map);
                    lord.AddPawn(p);
                    RCellFinder.TryFindRandomCellNearWith(this.TrueCenter().ToIntVec3(), c => c.Walkable(Map), Map, out c, 3);
                });
                spawned = true;
            }
            else if (!spawned)
            {
                tick++;
            }
        }
    }
}
