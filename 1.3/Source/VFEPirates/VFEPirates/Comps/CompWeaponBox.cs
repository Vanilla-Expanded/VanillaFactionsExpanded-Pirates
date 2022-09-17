using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Runtime.Remoting.Lifetime;
using Verse.AI;
using HeavyWeapons;

namespace VFEPirates
{
    public class CompProperties_WeaponBox : CompProperties
    {
        public ThingDef weaponToEquip;
        public CompProperties_WeaponBox()
        {
            this.compClass = typeof(CompWeaponBox);
        }
    }
    public class CompWeaponBox : ThingComp
    {
        public CompProperties_WeaponBox Props => this.props as CompProperties_WeaponBox;
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.CompFloatMenuOptions(selPawn))
            {
                yield return opt;
            }
            if (this.parent.Faction == Faction.OfPlayer)
            {
                var options = Props.weaponToEquip.GetModExtension<HeavyWeapon>();
                if (options != null && options.isHeavy && !Patch_FloatMenuMakerMap.AddHumanlikeOrders_Fix.CanEquip(selPawn, options))
                {
                    yield return new FloatMenuOption("CannotEquip".Translate(Props.weaponToEquip.label) + " (" + options.disableOptionLabelKey.Translate(selPawn.LabelShort) + ")", null);
                }
                else
                {
                    string label = "Equip".Translate(Props.weaponToEquip.label);
                    if (Props.weaponToEquip.IsRangedWeapon && selPawn.story != null && selPawn.story.traits.HasTrait(TraitDefOf.Brawler))
                    {
                        label += " " + "EquipWarningBrawler".Translate();
                    }
                    yield return new FloatMenuOption(label, delegate
                    {
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VFEP_DefOf.VFEP_EquipFromWeaponBox, this.parent), JobTag.Misc);
                        FleckMaker.Static(this.parent.DrawPos, this.parent.MapHeld, FleckDefOf.FeedbackEquip);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                    });
                }

            }
        }
    }
}