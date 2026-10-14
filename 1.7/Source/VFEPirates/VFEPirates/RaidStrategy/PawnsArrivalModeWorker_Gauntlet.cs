using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEPirates
{
    internal class PawnsArrivalModeWorker_Gauntlet : PawnsArrivalModeWorker
    {
        private static readonly int PawnsPerShip = 8;

        private static readonly Dictionary<ThingDef, ThingDef> shipAssoc = new()
        {
            { VFEP_DefOf.VFEP_Ship_Black, VFEP_DefOf.VFEP_CrashedShip_Black },
            { VFEP_DefOf.VFEP_Ship_Orange, VFEP_DefOf.VFEP_CrashedShip_Orange },
            { VFEP_DefOf.VFEP_Ship_Green, VFEP_DefOf.VFEP_CrashedShip_Green },
            { VFEP_DefOf.VFEP_Ship_Red, VFEP_DefOf.VFEP_CrashedShip_Red }
        };

        // ResolveRaidStrategy hands parms.raidStrategy to TryRandomElementByWeight as its out parameter, so
        // it holds the running candidate while later strategies are still being filtered, and the base
        // check against its arriveModes rejects this mode. Only the gauntlet strategy lists it, so skip that.
        public override bool CanUseWith(IncidentParms parms)
        {
            RaidStrategyDef raidStrategy = parms.raidStrategy;
            parms.raidStrategy = null;
            try
            {
                return base.CanUseWith(parms);
            }
            finally
            {
                parms.raidStrategy = raidStrategy;
            }
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!DropCellFinder.TryFindRaidDropCenterClose(out parms.spawnCenter, map))
            {
                parms.spawnCenter = map.Center;
            }
            return true;
        }

        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            Map target = (Map)parms.target;
            int shipCount = pawns.Count / PawnsPerShip;
            int pawnsLeft = pawns.Count % PawnsPerShip;

            if (pawnsLeft > 0) shipCount++;

            RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, c => c.Walkable(target) && !c.Roofed(target), target, out IntVec3 nextShipPos);
            for (int i = 0; i < shipCount; i++)
            {
                var randShip = shipAssoc.RandomElement();

                Building_CrashedShip innerThing = (Building_CrashedShip)ThingMaker.MakeThing(randShip.Value);
                innerThing.SetFactionDirect(pawns[0].Faction);
                for (int pN = i * PawnsPerShip; pN < (i + 1) * PawnsPerShip && pN < pawns.Count; pN++)
                {
                    Pawn pawn = pawns[pN];
                    if (pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(pawn);
                        pawn.psychicEntropy?.SetInitialPsyfocusLevel();
                    }
                    innerThing.AddPawnToShip(pawn);
                }
                DropCellFinder.TryFindDropSpotNear(nextShipPos, target, out IntVec3 cell, false, true, true, VFEP_DefOf.VFEP_Ship_Black.Size, true);
                RCellFinder.TryFindRandomCellNearWith(cell, c => c.Walkable(target) && !c.Roofed(target), target, out nextShipPos);
                SkyfallerMaker.SpawnSkyfaller(randShip.Key, innerThing, cell, (Map)parms.target);
            }
        }
    }
}
