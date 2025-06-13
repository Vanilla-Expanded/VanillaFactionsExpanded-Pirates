using HarmonyLib;
using RimWorld;

namespace VFEPirates
{
    [HarmonyPatch(typeof(Page_SelectStoryteller))]
    [HarmonyPatch("CanDoNext")]
    public static class Page_SelectStoryteller_CanDoNext_Patch
    {
        [HarmonyPostfix]
        public static void ChangeNext(Page_SelectStoryteller __instance)
        {
            if (CursesUtility.CursedStorytellerInCharge && __instance.next is not Page_ChooseCurses)
            {
                var next = __instance.next;
                next.prev = __instance.next = new Page_ChooseCurses {prev = __instance, next = next};
            }
            else if (__instance.next is Page_ChooseCurses {next: var next})
            {
                GameComponent_CurseManager.Instance.Notify_StorytellerChanged();
                __instance.next = next;
            }
        }
    }
}