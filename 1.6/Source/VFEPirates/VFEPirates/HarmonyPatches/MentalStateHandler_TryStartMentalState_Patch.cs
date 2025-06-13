using HarmonyLib;
using Verse;
using Verse.AI;

namespace VFEPirates
{
    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState", null)]
    public static class MentalStateHandler_TryStartMentalState_Patch
    {
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            if (___pawn?.Map != null && ___pawn.Position.GetFirstBuilding(___pawn.Map) is Building_WarcasketFoundry foundry && foundry.occupant == ___pawn)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
