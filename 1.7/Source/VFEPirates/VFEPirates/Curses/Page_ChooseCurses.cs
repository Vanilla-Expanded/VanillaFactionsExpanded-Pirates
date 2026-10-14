using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    public class Page_ChooseCurses : Page
    {
        public const float CURSE_SIZE = 160;
        public const float CURSE_PADDING = 20f;
        private Vector2 scrollPos;

        public Page_ChooseCurses()
        {
            if (Current.ProgramState == ProgramState.Playing) doCloseButton = true;
        }

        public int CursesPerRow => CURSE_PADDING * (6 + 2) + CURSE_SIZE * 6 + 25 >= UI.screenWidth - 20f ? 4 : 6;

        public override Vector2 InitialSize => new(CURSE_PADDING * (CursesPerRow + 2) + CURSE_SIZE * CursesPerRow + 25, UI.screenHeight - 200);

        public override void PostOpen()
        {
            base.PostOpen();
            GameComponent_CurseManager.Instance.Notify_StorytellerChanged();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 45f), "VFEP.ChooseCurses".Translate());
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(0, 50f, inRect.width, 25f), "VFEP.ChooseCurses.Desc".Translate());
            var rect = GetMainRect(inRect);
            rect.y += 20f;
            rect.height -= 20f;
            var count = Mathf.CeilToInt(DefDatabase<CurseDef>.AllDefsListForReading.Count / (float) CursesPerRow);
            var i = 0;
            Widgets.BeginScrollView(rect, ref scrollPos, new Rect(0, 0, rect.width - 25f, count * CURSE_SIZE + (count + 1) * CURSE_PADDING));
            var curseRect = new Rect(CURSE_PADDING, CURSE_PADDING, CURSE_SIZE, CURSE_SIZE);
            foreach (var curse in DefDatabase<CurseDef>.AllDefs)
            {
                DoCurse(curseRect, curse);
                curseRect.x += CURSE_SIZE + CURSE_PADDING;
                i++;
                if (i >= CursesPerRow)
                {
                    curseRect.x = CURSE_PADDING;
                    curseRect.y += CURSE_SIZE + CURSE_PADDING;
                    i = 0;
                }
            }

            Widgets.EndScrollView();
            var configRect = new Rect(Current.ProgramState == ProgramState.Playing ? 0f : inRect.x + inRect.width / 2f - 100f, inRect.yMax - 50f, 200f, 50f);
            Text.Font = GameFont.Small;
            Widgets.Label(configRect.TopHalf(), "VFEP.DesiredPopMax".Translate(GameComponent_CurseManager.Instance.DesiredPopulationMax(Find.Storyteller.def)));
            Widgets.Label(configRect.BottomHalf(), "VFEP.DaysBigThreats".Translate(GameComponent_CurseManager.Instance.MaxThreatBigIntervalDays
                (Find.Storyteller.def.comps.OfType<StorytellerCompProperties_RandomMain>().First())));
            Text.Font = font;
            Text.Anchor = anchor;
            if (Current.ProgramState != ProgramState.Playing) DoBottomButtons(inRect);
        }

        public void DoCurse(Rect rect, CurseDef curse)
        {
            var imgRect = new Rect(rect.x + 30, rect.y, 100, 100);
            Widgets.DrawLightHighlight(imgRect);
            Widgets.DrawTextureFitted(imgRect.ContractedBy(10f), curse.Icon, 1f);
            Widgets.DrawTextureFitted(new Rect(imgRect.x + 80, imgRect.y + 80, 20, 20),
                GameComponent_CurseManager.Instance.activeCurseDefs.Contains(curse) ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, 1f);
            Text.Font = GameFont.Small;
            var labelRect = new Rect(rect.x - 10, rect.y + 100, rect.width + 10, 20);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect, curse.label);
            Text.Font = GameFont.Tiny;
            var descRect = new Rect(rect.x, labelRect.y + 20, rect.width, 50);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(descRect, curse.description);
            if (Widgets.ButtonInvisible(rect))
            {
                if (GameComponent_CurseManager.Instance.activeCurseDefs.Contains(curse))
                {
                    GameComponent_CurseManager.Instance.Remove(curse);
                    curse.Worker.Disactivate();
                }
                else
                {
                    GameComponent_CurseManager.Instance.Add(curse);
                    curse.Worker.Start();
                }
            }
        }
    }
}