using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
namespace VFEPirates
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class Pawn_GetInspectString_Patch
    {
        public static void Postfix(ref string __result, Pawn __instance)
        {
            var sb = new StringBuilder(__result);
            if (__instance.apparel != null)
            {
                foreach (var apparel in __instance.apparel.WornApparel)
                {
                    var comp = apparel.GetComp<CompRumSuit>();
                    if (comp != null)
                    {
                        sb.AppendLine("\n" + comp.CompInspectStringExtra());
                    }
                }
            }
            __result = sb.ToString().TrimEndNewlines();
        }
    }
    public class CompRumSuit : ThingComp
    {
        private int ticksUntilSpawn;

        public CompProperties_RumSuit PropsSpawner => (CompProperties_RumSuit)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                ResetCountdown();
            }
        }

        public override void CompTick()
        {
            TickInterval(1);
        }

        public override void CompTickRare()
        {
            TickInterval(250);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            ResetCountdown();
        }

        public Apparel Apparel => this.parent as Apparel;
        public Pawn Wearer => Apparel.Wearer;
        private void TickInterval(int interval)
        {
            if (Wearer is null)
            {
                return;
            }
            else if (Wearer.PositionHeld.Fogged(Wearer.MapHeld))
            {
                return;
            }
            if (ModCompatibility.DubsBadHygieneActive && Wearer != null)
            {
                ModCompatibility.FillBladderNeed(this.Wearer, 0.001f);
            }
            ticksUntilSpawn -= interval;
            CheckShouldSpawn();
        }

        private void CheckShouldSpawn()
        {
            if (ticksUntilSpawn <= 0)
            {
                ResetCountdown();
                TryDoSpawn();
            }
        }

        public bool TryDoSpawn()
        {
            if (Wearer.MapHeld is null)
            {
                return false;
            }
            if (PropsSpawner.spawnMaxAdjacent >= 0)
            {
                int num = 0;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c = Wearer.PositionHeld + GenAdj.AdjacentCellsAndInside[i];
                    if (!c.InBounds(Wearer.MapHeld))
                    {
                        continue;
                    }
                    List<Thing> thingList = c.GetThingList(Wearer.MapHeld);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j].def == PropsSpawner.thingToSpawn)
                        {
                            num += thingList[j].stackCount;
                            if (num >= PropsSpawner.spawnMaxAdjacent)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            if (TryFindSpawnCell(Wearer, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out var result))
            {
                Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
                thing.stackCount = PropsSpawner.spawnCount;
                if (thing == null)
                {
                    Log.Error("Could not spawn anything for " + Wearer);
                }
                if (PropsSpawner.inheritFaction && thing.Faction != Wearer.Faction)
                {
                    thing.SetFaction(Wearer.Faction);
                }
                GenPlace.TryPlaceThing(thing, result, Wearer.MapHeld, ThingPlaceMode.Direct, out var lastResultingThing);
                if (PropsSpawner.spawnForbidden)
                {
                    lastResultingThing.SetForbidden(value: true);
                }
                if (PropsSpawner.showMessageIfOwned && Wearer.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
                }

                VFEP_DefOf.VFEP_RumFinished.PlayOneShot(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld, false));
                return true;
            }
            return false;
        }

        public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(new TargetInfo(parent.PositionHeld, parent.MapHeld)).InRandomOrder())
            {
                if (!item.Walkable(parent.MapHeld))
                {
                    continue;
                }
                Building edifice = item.GetEdifice(parent.MapHeld);
                if (edifice != null && thingToSpawn.IsEdifice())
                {
                    continue;
                }
                Building_Door building_Door = edifice as Building_Door;
                if ((building_Door != null && !building_Door.FreePassage) || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.PositionHeld, item, parent.MapHeld)))
                {
                    continue;
                }
                bool flag = false;
                List<Thing> thingList = item.GetThingList(parent.MapHeld);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    result = item;
                    return true;
                }
            }
            result = IntVec3.Invalid;
            return false;
        }

        private void ResetCountdown()
        {
            ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
        }

        public override void PostExposeData()
        {
            string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
            Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
        }

      

        
        public override string CompInspectStringExtra()
        {
            Apparel apparel = parent as Apparel;

            if (apparel.Wearer == null)
            {
                return null;
            }
            if (PropsSpawner.writeTimeLeftToSpawn)
            {
                return "VFEP_NextSpawnedRumIn".Translate().Resolve() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
            }
            return null;
        }
    }
}
