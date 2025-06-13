using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEPirates
{
    public class CurseCollection : IExposable
    {
        public CurseCollection()
        {
            curses = new List<CurseDef>();
        }

        public CurseCollection(CurseDef curseDef)
        {
            curses = new List<CurseDef> { curseDef };
        }
        public List<CurseDef> curses;
        public void ExposeData()
        {
            Scribe_Collections.Look(ref curses, "curses");
            if (curses is null)
            {
                curses ??= new List<CurseDef>();
            }
        }
    }
    public class GameComponent_CurseManager : GameComponent
    {
        public static GameComponent_CurseManager Instance;
        public HashSet<CurseDef> activeCurseDefs = new();

        private Dictionary<StorytellerDef, CurseCollection> activeCurses = new();

        public HashSet<Type> activeCurseTypes = new();

        public GameComponent_CurseManager(Game game)
        {
            Init();
        }

        public float DesiredPopulationMax(StorytellerDef storytellerDef)
        {
            return storytellerDef.populationIntentFactorFromPopCurve.Where(x => x.y >= 0).MinBy(x => x.y).x + activeCurseDefs.Count;
        }

        public float MaxThreatBigIntervalDays(StorytellerCompProperties_RandomMain props)
        {
            return props.maxThreatBigIntervalDays + activeCurseDefs.Count;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Init();
            InitializeCurses();
        }

        public void Init()
        {
            Instance = this;
            activeCurses ??= new Dictionary<StorytellerDef, CurseCollection>();
            activeCurseTypes ??= new HashSet<Type>();
            activeCurseDefs ??= new HashSet<CurseDef>();
            UpdateLists();
        }

        public void Add(CurseDef curseDef)
        {
            if (!activeCurses.ContainsKey(Find.Storyteller.def))
                activeCurses[Find.Storyteller.def] = new CurseCollection(curseDef);
            else
                activeCurses[Find.Storyteller.def].curses.Add(curseDef);
            UpdateLists();
        }

        public void Remove(CurseDef curseDef)
        {
            if (activeCurses.ContainsKey(Find.Storyteller.def)) activeCurses[Find.Storyteller.def].curses.Remove(curseDef);
            UpdateLists();
        }

        public void Notify_StorytellerChanged()
        {
            if (!activeCurses.ContainsKey(Find.Storyteller.def))
                activeCurses[Find.Storyteller.def] = new CurseCollection();
            UpdateLists();
        }

        private void UpdateLists()
        {
            if (Find.Storyteller?.def != null && activeCurses.ContainsKey(Find.Storyteller.def))
            {
                activeCurseTypes = activeCurses[Find.Storyteller.def].curses.Select(x => x.workerClass).ToHashSet();
                activeCurseDefs = activeCurses[Find.Storyteller.def].curses.ToHashSet();
            }
        }

        private void InitializeCurses()
        {
            foreach (var def in DefDatabase<CurseDef>.AllDefs)
            {
                try
                {
                    def.Worker.Start();
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception thrown while initializing curses. Worker = \"{def.Worker}\" Exception = {ex}");
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeCurses, "activeCurses", LookMode.Def, LookMode.Deep);
            Init();
        }
    }
}