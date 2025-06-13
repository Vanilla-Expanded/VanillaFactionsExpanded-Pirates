using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEPirates
{

    [HarmonyPatch(typeof(TaleUtility))]
    [HarmonyPatch("Notify_PawnDied")]
    public static class VFEPirates_TaleUtility_Notify_PawnDied_Patch
    {
        [HarmonyPostfix]
        static void NotifyCrewmanDied(Pawn victim)
        {
           

            if (ModsConfig.IdeologyActive&&victim?.RaceProps?.Humanlike == true && victim.IsColonist)
            { 
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(VFEP_DefOf.VFEP_CrewmanDied));
                StaticCollectionsClass.crewMembersLost++;
            }
        }
    }


}









