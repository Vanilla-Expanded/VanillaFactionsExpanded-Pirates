using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VFEPirates
{
    public class Hediff_SiegeMode : HediffWithComps
    {
        public Ability_SiegeMode ability;

        public bool remove;
        public override bool ShouldRemove => remove;

        public override void Tick()
        {
            base.Tick();
            if (pawn.pather.Moving)
            {
                remove = true;
                ability.enabled = false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remove, "remove");
            Scribe_References.Look(ref ability, "ability");
        }
    }

    [StaticConstructorOnStartup]
    public class CommandAbilityToggle : Command_Toggle
    {
        public static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public Ability ability;

        public Pawn pawn;

        public CommandAbilityToggle(Pawn pawn, Ability ability)
        {
            this.pawn = pawn;
            this.ability = ability;

            defaultLabel = ability.def.LabelCap;
            defaultDesc = ability.GetDescriptionForPawn();
            icon = ability.def.icon;
            disabled = !ability.IsEnabledForPawn(out var reason);
            disabledReason = reason.Colorize(Color.red);
            order = 10f + (ability.def.requiredHediff?.hediffDef?.index ?? 0) + (ability.def.requiredHediff?.minimumLevel ?? 0);
            //this.shrinkable     = true;
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUIInt(butRect, parms);
            if (disabled && ability.cooldown > Find.TickManager.TicksGame)
                GUI.DrawTexture(
                    butRect.RightPartPixels(butRect.width * ((float) (ability.cooldown - Find.TickManager.TicksGame) / ability.GetCooldownForPawn())),
                    CooldownTex);
            return result;
        }
    }

    public class SiegeModeExtension : DefModExtension
    {
        public HediffDef siegeModeHediff;
        public SoundDef soundDisabled;
        public SoundDef soundEnabled;
    }

    public class Ability_SiegeMode : Ability
    {
        public bool enabled;

        public override Gizmo GetGizmo()
        {
            if (enabled)
                return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "VFEP.SiegeModeOff".Translate(),
                    toggleAction = delegate
                    {
                        enabled = false;
                        DoAction();
                    },
                    isActive = () => enabled = true,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/SiegeModeOff"),
                    turnOnSound = def.GetModExtension<SiegeModeExtension>().soundEnabled,
                    turnOffSound = def.GetModExtension<SiegeModeExtension>().soundDisabled
                };
            return new CommandAbilityToggle(pawn, this)
            {
                defaultLabel = "VFEP.SiegeModeOn".Translate(),
                toggleAction = delegate
                {
                    enabled = true;
                    DoAction();
                },
                isActive = () => enabled = false,
                icon = ContentFinder<Texture2D>.Get("UI/Abilities/SiegeModeOn"),
                turnOnSound = def.GetModExtension<SiegeModeExtension>().soundEnabled,
                turnOffSound = def.GetModExtension<SiegeModeExtension>().soundDisabled
            };
        }

        public override void ApplyHediffs(params GlobalTargetInfo[] targets)
        {
            base.ApplyHediffs(targets);
            if (targets[0].Thing is Pawn p)
            {
                var extension = def.GetModExtension<SiegeModeExtension>();
                if (enabled)
                {
                    var hediff = HediffMaker.MakeHediff(extension.siegeModeHediff, p) as Hediff_SiegeMode;
                    hediff.ability = this;
                    p.health.AddHediff(hediff);
                }
                else
                {
                    var hediff = p.health.hediffSet.GetFirstHediffOfDef(extension.siegeModeHediff);
                    if (hediff != null) p.health.RemoveHediff(hediff);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enabled, "enabled");
        }
    }
}