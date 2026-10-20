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
    internal class Building_CrashedShip : Building_TurretGun, IThingHolder
    {
        int tick = 0;
        bool spawned = false;

        private ThingOwner<Pawn> innerContainer;

        public Building_CrashedShip()
        {
            innerContainer = new ThingOwner<Pawn>(this, false);
        }

        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref tick, "tick");
            Scribe_Values.Look(ref spawned, "spawned");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) return;

            for (int i = 0; i < 5; i++)
            {
                FleckMaker.ThrowDustPuff(this.TrueCenter(), this.Map, 2f);
                FleckMaker.ThrowFireGlow(this.TrueCenter(), this.Map, 2f);
                FleckMaker.ThrowHeatGlow(this.TrueCenter().ToIntVec3(), this.Map, 2f);
            }

            for (int i = 0; i < 10; i++)
            {
                RCellFinder.TryFindRandomCellNearWith(this.Position, c => c.Walkable(Map), Map, out IntVec3 r);
                if (Rand.Bool) FireUtility.TryStartFireIn(r, map, Rand.Range(0.1f, 0.925f), this);
                FilthMaker.TryMakeFilth(r, Map, ThingDefOf.Filth_Fuel, 1);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!spawned && Spawned) SpawnPawns();
            innerContainer.ClearAndDestroyContents();
            base.Destroy(mode);
        }

        public void AddPawnToShip(Pawn p)
        {
            innerContainer.TryAdd(p);
        }

        protected override void Tick()
        {
            base.Tick();
            if (tick >= 700 && !spawned)
            {
                SpawnPawns();
            }
            else if (!spawned)
            {
                tick++;
            }
        }

        // Raiders arriving through the raid pipeline already have a lord from the strategy worker.
        private void SpawnPawns()
        {
            spawned = true;
            Lord lord = null;
            for (int i = innerContainer.Count - 1; i >= 0; i--)
            {
                Pawn p = innerContainer[i];
                if (!innerContainer.TryDrop(p, this.Position, Map, ThingPlaceMode.Near, out _)) continue;
                if (p.GetLord() == null)
                {
                    lord ??= LordMaker.MakeNewLord(this.Faction, new LordJob_AssaultColony(this.Faction), base.Map);
                    lord.AddPawn(p);
                }
            }
        }
    }
}
