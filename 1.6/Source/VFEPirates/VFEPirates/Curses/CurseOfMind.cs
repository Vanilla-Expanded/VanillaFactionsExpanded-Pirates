using HarmonyLib;
using RimWorld;

namespace VFEPirates
{
    public class CurseOfMind : CurseWorker
    {
        public override void DoPatches()
        {
			Patch(typeof(Thought), "MoodOffset", postfix: AccessTools.Method(typeof(CurseOfMind), nameof(Postfix)));
        }
		public static void Postfix(ref float __result)
		{
            __result *= 2f;
        }
    }
}
