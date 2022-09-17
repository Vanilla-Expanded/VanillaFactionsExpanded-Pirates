using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Verse;
using VFECore;
using VFECore.UItils;

namespace VFEPirates.Buildings
{
    public class Dialog_WarcasketCustomization : Window
    {
        private static readonly Color DisplayBGColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
        private readonly List<WarcasketDef> armors;
        private readonly List<WarcasketDef> helmets;
        private readonly Action onAccept;
        private readonly Action onCancel;
        private readonly Pawn pawn;
        private readonly WarcasketProject project;
        private readonly List<WarcasketDef> shoulders;
        private Vector2 scrollPos;
        public Dialog_WarcasketCustomization(WarcasketProject project, Pawn pawn, Action onAccept, Action onCancel)
        {
            armors = VFEPiratesMod.allArmorDefs.Where(def => def.IsResearchFinished).ToList();
            shoulders = VFEPiratesMod.allShoulderPadsDefs.Where(def => def.IsResearchFinished).ToList();
            helmets = VFEPiratesMod.allHelmetDefs.Where(def => def.IsResearchFinished).ToList();
            this.onAccept = onAccept;
            this.onCancel = onCancel;
            this.pawn = pawn;
            this.project = project;
            forcePause = true;
            Notify_SettingsChanged();
        }
        protected override float Margin => 5f;
        public override Vector2 InitialSize => new(955f, UI.screenHeight - 150f);
        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            onAccept();
            Close();
        }

        public override void OnCancelKeyPressed()
        {
            base.OnCancelKeyPressed();
            onCancel();
            Close();
        }

        public override void PostOpen()
        {
            base.PostOpen();

            if (armors.NullOrEmpty() || shoulders.NullOrEmpty() || helmets.NullOrEmpty())
            {
                if (PawnUtility.ShouldSendNotificationAbout(pawn))
                    Messages.Message("VFEP.NoWarcaskets".Translate(), new LookTargets(Gen.YieldSingle(pawn)), MessageTypeDefOf.RejectInput);
                onCancel();
                Close();
            }
        }

        private void Notify_SettingsChanged()
        {
            Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch = true;
            project.ApplyOn(pawn);
            Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch = false;
            project.totalWorkAmount = project.armorDef.GetStatValueAbstract(StatDefOf.WorkToMake) + project.shoulderPadsDef.GetStatValueAbstract(StatDefOf.WorkToMake) +
                                      project.helmetDef.GetStatValueAbstract(StatDefOf.WorkToMake);
            PortraitsCache.SetDirty(pawn);
        }
        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            inRect.xMax -= 20f;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(inRect.TakeTopPart(40f), "VFEP.WarcasketCustom".Translate());
            inRect.yMin += 5f;
            var bottomRect = inRect.TakeBottomPart(200f);
            var viewRect = new Rect(0, 0, inRect.width - 20f, 650f);
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
            var displayRect = viewRect.TakeTopPart(200f);
            DoDisplay(displayRect.TakeRightPart(300f).ContractedBy(5f), Rot4.East);
            DoDisplay(displayRect.TakeRightPart(300f).ContractedBy(5f), Rot4.North);
            DoDisplay(displayRect.TakeRightPart(300f).ContractedBy(5f), Rot4.South);
            viewRect.yMin += 5f;
            Text.Anchor = TextAnchor.UpperLeft;
            var partsRect = viewRect.TakeTopPart(450);
            DoPartsSelect(partsRect.TakeRightPart(300f).ContractedBy(5f), "VFEP.Helmet".Translate(), helmets, project.helmetDef, def =>
            {
                project.helmetDef = def;
                Notify_SettingsChanged();
            }, ref project.colorHelmet);
            DoPartsSelect(partsRect.TakeRightPart(300f).ContractedBy(5f), "VFEP.Shoulderpads".Translate(), shoulders, project.shoulderPadsDef, def =>
            {
                project.shoulderPadsDef = def;
                Notify_SettingsChanged();
            }, ref project.colorShoulderPads);
            DoPartsSelect(partsRect.TakeRightPart(300f).ContractedBy(5f), "VFEP.ArmorFrame".Translate(), armors, project.armorDef, def =>
            {
                project.armorDef = def;
                Notify_SettingsChanged();
            }, ref project.colorArmor, true);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.EndScrollView();
            var infoRect = bottomRect.TakeTopPart(80f);
            var workRect = infoRect.TakeRightPart(150f);
            Text.Font = GameFont.Medium;
            Widgets.Label(workRect.TopHalf(), "VFEP.WorkAmount".Translate());
            workRect = workRect.BottomHalf().ContractedBy(2f);
            Widgets.DrawHighlight(workRect);
            workRect.xMin += 10f;
            Text.Font = GameFont.Small;
            Widgets.Label(workRect, ((int)project.totalWorkAmount).ToStringTicksToPeriod());
            Text.Font = GameFont.Medium;
            Widgets.Label(infoRect.TopHalf(), "VFEP.TotalCost".Translate());
            Text.Font = GameFont.Small;
            infoRect = infoRect.BottomHalf().ContractedBy(2f);
            Widgets.DrawHighlight(infoRect);
            bottomRect.xMin += 5f;
            infoRect.xMin += 10f;
            Widgets.Label(infoRect, project.RequiredIngredients().Join(ing => ing.ToString().Trim('(', ')')));
            var textRect = bottomRect.LeftPartPixels(bottomRect.width - 300f);
            var buttonsRect = bottomRect.RightPartPixels(290f);
            var y = (buttonsRect.height - 60f) / 2;
            buttonsRect.yMin += y;
            buttonsRect.yMax -= y;
            if (Widgets.ButtonText(buttonsRect.LeftHalf().ContractedBy(5f), "VFEP.Cancel".Translate())) OnCancelKeyPressed();
            if (Widgets.ButtonText(buttonsRect.RightHalf().ContractedBy(5f), "Accept".Translate())) OnAcceptKeyPressed();
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            textRect.yMin += 10f;
            Widgets.Label(textRect, "VFEP.WarcasketText".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = font;
            Text.Anchor = anchor;
        }

        public void DoPartsSelect(Rect inRect, string label, List<WarcasketDef> options, WarcasketDef current, Action<WarcasketDef> setCurrent, ref Color currentColor,
            bool doResearchText = false)
        {
            inRect = inRect.ContractedBy(3f);
            Text.Font = GameFont.Small;
            Widgets.Label(inRect.TakeTopPart(20f), label);
            inRect.y += 2f;
            DoSelection(inRect.TakeTopPart(24f), options, current, setCurrent);
            if (doResearchText)
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                var researchTextRect = inRect.TakeTopPart(25f);
                researchTextRect.width *= 3;
                Widgets.Label(researchTextRect, "VFEP.ResearchText".Translate().Colorize(ColoredText.SubtleGrayColor));
                Text.Font = GameFont.Small;
            }
            else
                inRect.y += 25f;

            var infoRect = inRect.TakeTopPart(30f);
            Widgets.InfoCardButton(infoRect.x, infoRect.y, current);
            infoRect.x += 30f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(infoRect, current.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            inRect.y += 5f;
            Widgets.Label(inRect.TakeTopPart(100f), current.shortDescription);
            inRect.y += 5f;
            Widgets.Label(inRect.TakeTopPart(40f), "VFEP.ResourceCost".Translate() + " " + current.costList.Join(cost => cost.LabelCap));
            inRect.y += 5f;
            if (Widgets.ColorSelector(inRect.TakeTopPart(150f), ref currentColor, StaticStartup.colors)) Notify_SettingsChanged();
        }

        public void DoSelection(Rect selectionRect, List<WarcasketDef> options, WarcasketDef current, Action<WarcasketDef> setCurrent)
        {
            var index = options.IndexOf(current);
            var oldIndex = index;
            const float buttonWidth = 30f;
            var width = selectionRect.width - buttonWidth * 2f;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rightButton = selectionRect.TakeRightPart(buttonWidth);
            Widgets.DrawHighlightIfMouseover(rightButton);
            if (Mouse.IsOver(rightButton)) GUI.color = GenUI.MouseoverColor;
            Widgets.DrawAtlas(rightButton, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Widgets.Label(rightButton.ContractedBy(3f), ">");
            if (Widgets.ButtonInvisible(rightButton))
            {
                index++;
                if (index >= options.Count) index = 0;
            }

            var selectMiddleRect = selectionRect.TakeRightPart(width);
            Widgets.DrawHighlightIfMouseover(selectMiddleRect);
            if (Mouse.IsOver(selectMiddleRect)) GUI.color = GenUI.MouseoverColor;
            Widgets.DrawAtlas(selectMiddleRect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Widgets.Label(selectMiddleRect.ContractedBy(3f), current.LabelCap);
            if (Widgets.ButtonInvisible(selectMiddleRect) && options.Count > 1)
                Find.WindowStack.Add(new FloatMenu(options.Except(current).Select(opt => new FloatMenuOption(opt.LabelCap, () => setCurrent(opt))).ToList()));

            if (Mouse.IsOver(selectionRect)) GUI.color = GenUI.MouseoverColor;
            Widgets.DrawAtlas(selectionRect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Widgets.Label(selectionRect.ContractedBy(3f), "<");
            if (Widgets.ButtonInvisible(selectionRect))
            {
                index--;
                if (index < 0) index = options.Count - 1;
            }

            if (index != oldIndex) setCurrent(options[index]);
        }

        public void DoDisplay(Rect inRect, Rot4 rot)
        {
            Widgets.DrawBox(inRect, 1, Texture2D.whiteTexture);
            Widgets.DrawBoxSolid(inRect, DisplayBGColor);
            inRect = inRect.ContractedBy(3f);
            var pawnTexture = GetPawnTexture(pawn, inRect.size, rot);
            GUI.DrawTexture(inRect, pawnTexture);
        }

        public RenderTexture GetPawnTexture(Pawn pawn, Vector2 size, Rot4 rotation, Vector3 cameraOffset = default(Vector3), float cameraZoom = 1f, bool supersample = true, bool compensateForUIScale = true, bool renderHeadgear = true, bool renderClothes = true, Dictionary<Apparel, Color> overrideApparelColors = null, Color? overrideHairColor = null, bool stylingStation = false)
        {
            if (supersample)
            {
                size *= 1.25f;
            }
            if (compensateForUIScale)
            {
                size *= Prefs.UIScale;
            }
            float angle = 0f;
            Vector3 positionOffset = default(Vector3);
            if (pawn.Dead || pawn.Downed)
            {
                angle = 85f;
                positionOffset.x -= 0.18f;
                positionOffset.z -= 0.18f;
            }
            RenderTexture renderTexture = NewRenderTexture(size);
            Find.PawnCacheRenderer.RenderPawn(pawn, renderTexture, cameraOffset, cameraZoom, angle, rotation, pawn.health.hediffSet.HasHead, 
                renderBody: true, renderHeadgear, renderClothes, portrait: false, positionOffset, overrideApparelColors, overrideHairColor, stylingStation);
            return renderTexture;
        }

        private RenderTexture pawnTexture;
        private RenderTexture NewRenderTexture(Vector2 size)
        {
            if (pawnTexture is null)
            {
                pawnTexture = new RenderTexture((int)size.x, (int)size.y, 24)
                {
                    name = "Portrait",
                    useMipMap = false,
                    filterMode = FilterMode.Bilinear
                };
            }
            return pawnTexture; 
        }
    }
}