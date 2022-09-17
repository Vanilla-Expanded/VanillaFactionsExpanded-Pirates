using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VFEPirates
{
    public class BlastOffExtension : DefModExtension
    {
        public float fuelConsumption;
        public int maxLaunchDistance;
    }

    public class Ability_BlastOff : Ability
    {
        private CompTransporter cachedCompTransporter;
        private BlastOffExtension cachedExtension;

        private ThingWithComps transporter;
        public float FuelConsumption => Extension.fuelConsumption;
        public int MaxLaunchDistance => Extension.maxLaunchDistance;

        public CompTransporter Transporter => cachedCompTransporter ??= transporter.GetComp<CompTransporter>();
        public BlastOffExtension Extension => cachedExtension ??= def.GetModExtension<BlastOffExtension>();

        public override bool IsEnabledForPawn(out string reason)
        {
            if (holder.TryGetComp<CompReloadable>().RemainingCharges < FuelConsumption)
            {
                reason = "VFEP.NotEnoughFuel".Translate();
                return false;
            }

            reason = null;
            return true;
        }

        public override bool CanHitTargetTile(GlobalTargetInfo target) => target.WorldObject is MapParent {HasMap: true};

        public override bool ValidateTargetTile(GlobalTargetInfo target, bool showMessages = false)
        {
            if (!base.ValidateTargetTile(target, showMessages)) return false;

            if (!target.IsValid)
            {
                if (showMessages) Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            var num = Find.WorldGrid.TraversalDistanceBetween(pawn.Tile, target.Tile);
            if (MaxLaunchDistance > 0 && num > MaxLaunchDistance)
            {
                if (showMessages) Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (Find.World.Impassable(target.Tile))
            {
                if (showMessages) Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        protected override Texture2D MouseAttachment(GlobalTargetInfo target) =>
            TravelingPawn.MakeReadableTextureInstance(PortraitsCache.Get(pawn, new Vector2(50, 50), Rot4.South));

        protected override string WorldTargetingLabel(GlobalTargetInfo target)
        {
            if (!target.IsValid) return null;
            var num = Find.WorldGrid.TraversalDistanceBetween(pawn.Tile, target.Tile);
            if (MaxLaunchDistance > 0 && num > MaxLaunchDistance)
            {
                GUI.color = ColorLibrary.RedReadable;
                return "TransportPodDestinationBeyondMaximumRange".Translate();
            }

            return string.Empty;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var destinationTile = targets[0].Tile;
            var comp = holder.TryGetComp<CompReloadable>();
            for (var i = 0; i < FuelConsumption; i++) comp.UsedOnce();
            var map = pawn.Map;
            transporter = ThingMaker.MakeThing(ThingDefOf.TransportPod) as ThingWithComps;
            GenSpawn.Spawn(transporter, pawn.Position, map);
            pawn.DeSpawn();
            Transporter.innerContainer.TryAdd(pawn);
            TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
            Transporter.TryRemoveLord(map);
            var groupID = Transporter.groupID;
            var compTransporter = Transporter;
            var directlyHeldThings = compTransporter.GetDirectlyHeldThings();
            var activeDropPawn = (ActiveDropPawn) ThingMaker.MakeThing(VFEP_DefOf.VFEP_ActiveDropPawn);
            activeDropPawn.pawn = pawn;
            activeDropPawn.Contents = new ActiveDropPodInfo();
            activeDropPawn.Contents.despawnPodBeforeSpawningThing = true;
            activeDropPawn.Contents.openDelay = 0;
            activeDropPawn.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
            var obj = (PawnLeaving) SkyfallerMaker.MakeSkyfaller(VFEP_DefOf.VFEP_PawnLeaving, activeDropPawn);
            obj.pawn = pawn;
            obj.groupID = groupID;
            obj.destinationTile = destinationTile;
            obj.worldObjectDef = VFEP_DefOf.VFEP_TravelingPawn;
            compTransporter.CleanUpLoadingVars(map);
            compTransporter.parent.Destroy();
            GenSpawn.Spawn(obj, compTransporter.parent.Position, map);
            CameraJumper.TryHideWorld();
            transporter = null;
            cachedCompTransporter = null;
        }
    }

    public class PawnIncoming : DropPodIncoming
    {
        private Effecter flightEffecter;
        public Pawn pawn;
        public override Graphic Graphic => pawn.Graphic;

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var num = 0f;
            if (def.skyfaller.rotateGraphicTowardsDirection) num = angle;
            if (def.skyfaller.angleCurve != null) angle = def.skyfaller.angleCurve.Evaluate(TimeInAnimation);
            if (def.skyfaller.rotationCurve != null) num += def.skyfaller.rotationCurve.Evaluate(TimeInAnimation);
            if (def.skyfaller.xPositionCurve != null) drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
            if (def.skyfaller.zPositionCurve != null) drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
            pawn.Drawer.renderer.RenderPawnAt(drawLoc, pawn.Rotation);
            DrawDropSpotShadow();
        }

        public override void Tick()
        {
            if (flightEffecter == null && def.pawnFlyer.flightEffecterDef != null)
            {
                flightEffecter = def.pawnFlyer.flightEffecterDef.Spawn();
                flightEffecter.Trigger(this, TargetInfo.Invalid);
            }
            else
                flightEffecter?.EffectTick(this, TargetInfo.Invalid);

            base.Tick();
        }

        protected override void Impact()
        {
            base.Impact();
            GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, 15, DamageDefOf.Bomb, pawn, 15, ignoredThings: Gen.YieldSingle<Thing>(pawn).ToList());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        }
    }

    public class PawnLeaving : FlyShipLeaving
    {
        private static readonly List<Thing> tmpActiveDropPods = new();

        private Effecter flightEffecter;
        public Pawn pawn;
        public override Graphic Graphic => pawn.Graphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ticksToDiscard = 500;
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var num = 0f;
            if (def.skyfaller.rotateGraphicTowardsDirection) num = angle;
            if (def.skyfaller.angleCurve != null) angle = def.skyfaller.angleCurve.Evaluate(TimeInAnimation);
            if (def.skyfaller.rotationCurve != null) num += def.skyfaller.rotationCurve.Evaluate(TimeInAnimation);
            if (def.skyfaller.xPositionCurve != null) drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
            if (def.skyfaller.zPositionCurve != null) drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
            pawn.Drawer.renderer.RenderPawnAt(drawLoc, pawn.Rotation);
            DrawDropSpotShadow();
        }

        public override void Tick()
        {
            if (flightEffecter == null && def.pawnFlyer.flightEffecterDef != null)
            {
                flightEffecter = def.pawnFlyer.flightEffecterDef.Spawn();
                flightEffecter.Trigger(this, TargetInfo.Invalid);
            }
            else
                flightEffecter?.EffectTick(this, TargetInfo.Invalid);

            base.Tick();
        }

        protected override void LeaveMap()
        {
            var alreadyLeftField = Traverse.Create(this).Field<bool>("alreadyLeft");
            if (alreadyLeftField.Value || !createWorldObject)
            {
                if (Contents != null)
                {
                    foreach (var item in Contents.innerContainer)
                    {
                        Pawn pawn;
                        if ((pawn = item as Pawn) != null) pawn.ExitMap(false, Rot4.Invalid);
                    }

                    Contents.innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.QuestLogic);
                }

                base.LeaveMap();
                return;
            }

            if (groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + groupID);
                Destroy();
                return;
            }

            if (destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + destinationTile);
                Destroy();
                return;
            }

            var lord = TransporterUtility.FindLord(groupID, Map);
            if (lord != null) Map.lordManager.RemoveLord(lord);
            var travelingPawn = (TravelingPawn) WorldObjectMaker.MakeWorldObject(worldObjectDef);
            travelingPawn.pawn = this.pawn;
            travelingPawn.Tile = Map.Tile;
            travelingPawn.SetFaction(Faction.OfPlayer);
            travelingPawn.destinationTile = destinationTile;
            travelingPawn.arrivalAction = arrivalAction;
            Find.WorldObjects.Add(travelingPawn);
            tmpActiveDropPods.Clear();
            tmpActiveDropPods.AddRange(Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
            for (var i = 0; i < tmpActiveDropPods.Count; i++)
            {
                var flyShipLeaving = tmpActiveDropPods[i] as FlyShipLeaving;
                if (flyShipLeaving != null && flyShipLeaving.groupID == groupID)
                {
                    Traverse.Create(this).Field("alreadyLeft").SetValue(true);
                    travelingPawn.AddPod(flyShipLeaving.Contents, true);
                    flyShipLeaving.Contents = null;
                    flyShipLeaving.Destroy();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        }
    }


    public class ActiveDropPawn : ActiveDropPod
    {
        public Pawn pawn;
        public override Graphic Graphic => pawn.Graphic;

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            pawn.Drawer.renderer.RenderPawnAt(DrawPos, Rot4.South);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pawn, "pawn");
        }
    }

    public class TravelingPawn : TravelingTransportPods
    {
        [TweakValue("00", 0, 1)] public static float drawTest = 0.083f;
        public Pawn pawn;
        public Texture2D PawnTexture => MakeReadableTextureInstance(PortraitsCache.Get(pawn, new Vector2(50, 50), Rot4.South));
        public override Material Material => MaterialPool.MatFrom(PawnTexture);
        public override string Label => pawn.Label;
        public override string LabelShort => pawn.LabelShort;
        public override string LabelShortCap => pawn.LabelShortCap;

        public override void Draw()
        {
            var averageTileSize = Find.WorldGrid.averageTileSize;
            var pos = DrawPos;
            var size = 0.7f * averageTileSize;
            var altOffset = drawTest;
            var material = Material;
            var normalized = pos.normalized;
            var vector = normalized;
            var q = Quaternion.LookRotation(Vector3.up, vector);
            var s = new Vector3(size, 1f, size);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(pos + normalized * altOffset, q, s);
            var layer = WorldCameraManager.WorldLayer;
            Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer);
        }

        public static Texture2D MakeReadableTextureInstance(RenderTexture source)
        {
            var temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            temporary.name = "MakeReadableTexture_Temp";
            Graphics.Blit(source, temporary);
            var active = RenderTexture.active;
            RenderTexture.active = temporary;
            var texture2D = new Texture2D(source.width, source.height);
            texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            return texture2D;
        }

        public void ArrivedOverride()
        {
            var arrivedField = Traverse.Create(this).Field("arrived");
            var podsField = Traverse.Create(this).Field("pods");
            if (arrivedField.GetValue<bool>()) return;
            arrivedField.SetValue(true);
            if (arrivalAction == null || !arrivalAction.StillValid(podsField.GetValue<List<ActiveDropPodInfo>>(), destinationTile))
            {
                arrivalAction = null;
                var maps = Find.Maps;
                for (var i = 0; i < maps.Count; i++)
                    if (maps[i].Tile == destinationTile)
                    {
                        arrivalAction = new PawnArrivalAction_LandInSpecificCell(maps[i].Parent, pawn);
                        break;
                    }

                if (arrivalAction == null)
                {
                    if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(podsField.GetValue<List<ActiveDropPodInfo>>(), destinationTile))
                        arrivalAction = new TransportPodsArrivalAction_FormCaravan();
                    else
                    {
                        var caravans = Find.WorldObjects.Caravans;
                        for (var j = 0; j < caravans.Count; j++)
                            if (caravans[j].Tile == destinationTile &&
                                TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(podsField.GetValue<List<ActiveDropPodInfo>>(), caravans[j]))
                            {
                                arrivalAction = new TransportPodsArrivalAction_GiveToCaravan(caravans[j]);
                                break;
                            }
                    }
                }
            }

            if (arrivalAction != null && arrivalAction.ShouldUseLongEvent(podsField.GetValue<List<ActiveDropPodInfo>>(), destinationTile))
                LongEventHandler.QueueLongEvent(delegate { AccessTools.Method(typeof(TravelingTransportPods), "DoArrivalAction").Invoke(this, null); },
                    "GeneratingMapForNewEncounter", false, null);
            else
                AccessTools.Method(typeof(TravelingTransportPods), "DoArrivalAction").Invoke(this, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        }

        [HarmonyPatch(typeof(TravelingTransportPods), "Arrived")]
        public static class TravelingTransportPods_Arrived
        {
            public static bool Prefix(TravelingTransportPods __instance)
            {
                if (__instance is TravelingPawn tp)
                {
                    tp.ArrivedOverride();
                    return false;
                }

                return true;
            }
        }
    }

    public class PawnArrivalAction_LandInSpecificCell : TransportPodsArrivalAction_LandInSpecificCell
    {
        private MapParent mapParent;

        private Pawn pawn;

        public PawnArrivalAction_LandInSpecificCell()
        {
        }

        public PawnArrivalAction_LandInSpecificCell(MapParent mapParent, Pawn pawn)
        {
            this.mapParent = mapParent;
            this.pawn = pawn;
        }

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            var lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
            DropTravelingPawns(pods, mapParent.Map.Center, mapParent.Map);
            Messages.Message("VFEP.MessagePawnArrived".Translate(lookTarget.Named("PAWN")), lookTarget, MessageTypeDefOf.TaskCompletion);
        }

        public void DropTravelingPawns(List<ActiveDropPodInfo> dropPods, IntVec3 near, Map map)
        {
            TransportPodsArrivalActionUtility.RemovePawnsFromWorldPawns(dropPods);
            for (var i = 0; i < dropPods.Count; i++)
            {
                DropCellFinder.TryFindDropSpotNear(near, map, out var result, false, true);
                MakePawnDropAt(result, map, dropPods[i]);
            }
        }

        public void MakePawnDropAt(IntVec3 c, Map map, ActiveDropPodInfo info)
        {
            var activeDropPod = (ActiveDropPawn) ThingMaker.MakeThing(VFEP_DefOf.VFEP_ActiveDropPawn);
            activeDropPod.pawn = this.pawn;
            activeDropPod.Contents = info;
            var pawnIncoming = SkyfallerMaker.SpawnSkyfaller(VFEP_DefOf.VFEP_PawnIncoming, activeDropPod, c, map) as PawnIncoming;
            pawnIncoming.pawn = activeDropPod.Contents.innerContainer.First() as Pawn;
            foreach (var item in activeDropPod.Contents.innerContainer)
            {
                Pawn pawn;
                if ((pawn = item as Pawn) != null && pawn.IsWorldPawn())
                {
                    Find.WorldPawns.RemovePawn(pawn);
                    pawn.psychicEntropy?.SetInitialPsyfocusLevel();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref mapParent, "mapParent");
        }
    }
}