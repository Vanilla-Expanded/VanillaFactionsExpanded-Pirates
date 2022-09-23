using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Verse.Sound;
using Verse.AI.Group;
using Verse.AI;


namespace VFEPirates
{
	public class RitualOutcomeEffectWorker_SingingShanties : RitualOutcomeEffectWorker_FromQuality
	{

		public List<ThoughtDef> listOfThoughtsToRemove = new List<ThoughtDef> { ThoughtDefOf.PawnWithGoodOpinionDied,VFEP_DefOf.MySonDied, VFEP_DefOf.MyDaughterDied,
		VFEP_DefOf.MyHusbandDied,VFEP_DefOf.MyWifeDied,VFEP_DefOf.MyFianceDied,VFEP_DefOf.MyFianceeDied,VFEP_DefOf.MyLoverDied,VFEP_DefOf.MyBrotherDied,
		VFEP_DefOf.MySisterDied,VFEP_DefOf.MyGrandchildDied,VFEP_DefOf.MyFatherDied,VFEP_DefOf.MyMotherDied,VFEP_DefOf.MyNieceDied,VFEP_DefOf.MyNephewDied,
		VFEP_DefOf.MyHalfSiblingDied,VFEP_DefOf.MyAuntDied,VFEP_DefOf.MyUncleDied,VFEP_DefOf.MyGrandparentDied,VFEP_DefOf.MyCousinDied,VFEP_DefOf.MyKinDied	};

		public RitualOutcomeEffectWorker_SingingShanties()
		{
		}

		public RitualOutcomeEffectWorker_SingingShanties(RitualOutcomeEffectDef def) : base(def)
		{
		}

		public override bool SupportsAttachableOutcomeEffect
		{
			get
			{
				return false;
			}
		}

		public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
		{

			float quality = base.GetQuality(jobRitual, progress);
			OutcomeChance outcome = this.GetOutcome(quality, jobRitual);
			LookTargets lookTargets = jobRitual.selectedTarget;
			string text = null;
			if (jobRitual.Ritual != null)
			{
				this.ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out text, ref lookTargets);
			}
			
			foreach (Pawn pawn in totalPresence.Keys)
			{

				base.GiveMemoryToPawn(pawn, outcome.memory, jobRitual);

			}
			
			
			if (outcome.Positive)
			{
				Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_AncientComplex, StorytellerUtility.DefaultThreatPointsNow(jobRitual.Map));
				QuestUtility.SendLetterQuestAvailable(quest);
			}

			if (outcome.positivityIndex == 2)
			{

				foreach (Pawn pawn in totalPresence.Keys)
				{
					foreach (ThoughtDef thought in listOfThoughtsToRemove) {
						pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(thought);

					}
				}

			}



			string text2 = outcome.description.Formatted(jobRitual.Ritual.Label).CapitalizeFirst() + "\n\n" + this.OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
			string text3 = this.def.OutcomeMoodBreakdown(outcome);
			if (!text3.NullOrEmpty())
			{
				text2 = text2 + "\n\n" + text3;
			}
			
			if (text != null)
			{
				text2 = text2 + "\n\n" + text;
			}
			string text4;
			this.ApplyDevelopmentPoints(jobRitual.Ritual, outcome, out text4);
			if (text4 != null)
			{
				text2 = text2 + "\n\n" + text4;
			}
			Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), text2, outcome.Positive ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, lookTargets, null, null, null, null);




		}


	}
}
