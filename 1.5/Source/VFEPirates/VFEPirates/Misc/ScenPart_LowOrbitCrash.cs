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
    internal class Chunks
    {
        internal static readonly Dictionary<ThingDef, ThingDef> chunks = new()
        {
            { VFEP_DefOf.VFEP_ShipChunkDebris, VFEP_DefOf.VFEP_IShipChunkDebris },
            { VFEP_DefOf.VFEP_ShipChunkBattery, VFEP_DefOf.VFEP_IShipChunkBattery },
            { VFEP_DefOf.VFEP_ShipChunkCryptosleepCasket, VFEP_DefOf.VFEP_IShipChunkCryptosleepCasket },
            { VFEP_DefOf.VFEP_ShipChunkVolatileEngine, VFEP_DefOf.VFEP_IShipChunkVolatileEngine },
            { VFEP_DefOf.VFEP_ShipChunkFuelTank, VFEP_DefOf.VFEP_IShipChunkFuelTank },
            { VFEP_DefOf.VFEP_ShipChunkGauntletTurret, VFEP_DefOf.VFEP_IShipChunkGauntletTurret },
            { VFEP_DefOf.VFEP_ShipChunkReactor, VFEP_DefOf.VFEP_IShipChunkReactor },
            { VFEP_DefOf.VFEP_ShipMedicalCrate, VFEP_DefOf.VFEP_IShipMedicalCrate },
            { VFEP_DefOf.VFEP_ShipNutrientPasteCrate, VFEP_DefOf.VFEP_IShipNutrientPasteCrate }
        };
    }

    internal class ScenPart_LowOrbitCrash : ScenPart
    {
        public override void GenerateIntoMap(Map map)
        {
            if (Find.GameInitData == null)
                return;

            Pawn p = Find.GameInitData.startingAndOptionalPawns.Last();
            Find.GameInitData.startingAndOptionalPawns.Remove(p);

            this.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, CreateThingGroups());

            foreach (var chunk in Chunks.chunks)
            {
                TryFindShipChunkDropCell(chunk.Value, MapGenerator.PlayerStartSpot, map, out IntVec3 spawnPos);
                Thing inner = ThingMaker.MakeThing(chunk.Key);
                inner.SetFactionDirect(Faction.OfPlayer);

                if (inner is Building_CryptosleepCasket bc)
                {
                    bc.TryAcceptThing(p);
                    SkyfallerMaker.SpawnSkyfaller(chunk.Value, bc, spawnPos, map);
                }
                else if (inner is Building_Battery bb && bb.GetComp<CompPowerBattery>() is CompPowerBattery cbb)
                {
                    cbb.SetStoredEnergyPct(1);
                    SkyfallerMaker.SpawnSkyfaller(chunk.Value, bb, spawnPos, map);
                }
                else
                {
                    SkyfallerMaker.SpawnSkyfaller(chunk.Value, inner, spawnPos, map);
                }
            }
        }

        private bool TryFindShipChunkDropCell(ThingDef chunk, IntVec3 nearLoc, Map map, out IntVec3 pos) => CellFinderLoose.TryFindSkyfallerCell(chunk, map, out pos, nearLoc: nearLoc, nearLocMaxDist: 15);

        // Pod utils
        private List<List<Thing>> CreateThingGroups()
        {
            List<List<Thing>> thingsGroups = new();
            foreach (Pawn pawn in Find.GameInitData.startingAndOptionalPawns)
                thingsGroups.Add(new List<Thing>() { pawn });

            List<Thing> thingList = new();
            foreach (ScenPart allPart in Find.Scenario.AllParts)
                thingList.AddRange(allPart.PlayerStartingThings());

            int index = 0;
            foreach (Thing thing in thingList)
            {
                if (thing.def.CanHaveFaction) thing.SetFactionDirect(Faction.OfPlayer);

                thingsGroups[index].Add(thing);

                ++index;
                if (index >= thingsGroups.Count) index = 0;
            }

            return thingsGroups;
        }

        private void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110)
        {
            foreach (List<Thing> thingsGroup in thingsGroups)
            {
                if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out IntVec3 result, false, false) && (false || !DropCellFinder.TryFindDropSpotNear(dropCenter, map, out result, false, true)))
                {
                    Log.Warning("DropThingsNear failed to find a place to drop " + thingsGroup.FirstOrDefault() + " near " + dropCenter + ". Dropping on random square instead.");
                    result = CellFinderLoose.RandomCellWith(c => c.Walkable(map), map);
                }

                for (int index = 0; index < thingsGroup.Count; ++index)
                    thingsGroup[index].SetForbidden(true, false);

                ActiveDropPodInfo info = new();
                foreach (Thing thing in thingsGroup)
                    info.innerContainer.TryAdd(thing);
                info.openDelay = openDelay;
                info.leaveSlag = true;
                this.MakeDropPodAt(result, map, info);
            }
        }

        private void MakeDropPodAt(IntVec3 c, Map map, ActiveDropPodInfo info)
        {
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
            activeDropPod.Contents = info;
            SkyfallerMaker.SpawnSkyfaller(VFEP_DefOf.VFEP_SideDropPod, activeDropPod, c, map);
            foreach (Thing thing in activeDropPod.Contents.innerContainer)
            {
                if (thing is Pawn p1 && p1.IsWorldPawn())
                {
                    Find.WorldPawns.RemovePawn(p1);
                    p1.psychicEntropy?.SetInitialPsyfocusLevel();
                }
            }
        }

        public override void PostMapGenerate(Map map)
        {
            if (Find.GameInitData == null)
                return;
            PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.CrashedTogether);
        }
    }
}
