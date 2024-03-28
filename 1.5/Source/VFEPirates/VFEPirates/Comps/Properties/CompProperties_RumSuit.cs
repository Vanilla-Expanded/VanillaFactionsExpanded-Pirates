using RimWorld;
using Verse;
namespace VFEPirates
{
    public class CompProperties_RumSuit : CompProperties
    {
        public ThingDef thingToSpawn;

        public int spawnCount = 1;

        public IntRange spawnIntervalRange = new IntRange(100, 100);

        public int spawnMaxAdjacent = -1;

        public bool spawnForbidden;

        public bool writeTimeLeftToSpawn;

        public bool showMessageIfOwned;

        public string saveKeysPrefix;

        public bool inheritFaction;

        public CompProperties_RumSuit()
        {
            compClass = typeof(CompRumSuit);
        }
    }
}