using System.Linq;
using Verse;

namespace VFEPirates
{
    public static class CursesUtility
    {
        public static bool CursedStorytellerInCharge => Current.Game?.storyteller?.storytellerComps?.OfType<StorytellerComp_Cursed>().Any() ?? false;
    }
}
