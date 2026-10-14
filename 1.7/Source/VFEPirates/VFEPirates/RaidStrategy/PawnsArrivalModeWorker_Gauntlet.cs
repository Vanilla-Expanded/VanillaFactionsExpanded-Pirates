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
                for (int pN = 0; pN < PawnsPerShip; pN++)
                {
                    if (pawns.Count > 0)
                    {
                        if (pawns[0].IsWorldPawn())
                        {
                            Find.WorldPawns.RemovePawn(pawns[0]);
                            pawns[0].psychicEntropy?.SetInitialPsyfocusLevel();
                        }
                        innerThing.AddPawnToShip(pawns[0]);
                        pawns.RemoveAt(0);
                    }
                }
                DropCellFinder.TryFindDropSpotNear(nextShipPos, target, out IntVec3 cell, false, true, true, VFEP_DefOf.VFEP_Ship_Black.Size, true);
                RCellFinder.TryFindRandomCellNearWith(cell, c => c.Walkable(target) && !c.Roofed(target), target, out nextShipPos);
                SkyfallerMaker.SpawnSkyfaller(randShip.Key, innerThing, cell, (Map)parms.target);
            }
        }
    }
}
