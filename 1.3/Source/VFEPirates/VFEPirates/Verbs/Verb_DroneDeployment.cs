using RimWorld;
using Verse;

namespace VFEPirates
{
    public class Verb_DroneDeployment : Verb
    {
        protected override bool TryCastShot()
        {
            ReleaseDrones(base.ReloadableCompSource);
            return true;
        }
        public override bool IsMeleeAttack => true;
        public static void ReleaseDrones(CompReloadable comp)
        {
            if (comp != null && comp.CanBeUsed)
            {
                Pawn wearer = comp.Wearer;
                var dronesToReleaseCount = 3;
                if (wearer.apparel.WornApparel.Any(x => x.def == VFEP_DefOf.VFEP_WarcasketHelmet_Controller))
                {
                    dronesToReleaseCount += 1;
                }
                for (var i = 0; i < dronesToReleaseCount; i++)
                {
                    var drone = PawnGenerator.GeneratePawn(VFEP_DefOf.VFEP_Mech_Wardrone, wearer.Faction);
                    drone.relations = new Pawn_RelationsTracker(drone);
                    GenSpawn.Spawn(drone, wearer.Position, wearer.Map);
                }
                comp.UsedOnce();
            }
        }
    }
}
