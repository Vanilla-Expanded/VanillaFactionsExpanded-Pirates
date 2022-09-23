using System;
using Verse;
using RimWorld;

namespace VFEPirates
{
	public class Thought_Precept_Camaraderie_Exalted : ThoughtWorker_Precept_Social
	{
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return false;
			}
			if (StaticCollectionsClass.crewMembersLost == 0)
            {
				return false;
			}

			else if (StaticCollectionsClass.crewMembersLost >= 20)
			{
				return ThoughtState.ActiveAtStage(19); 
			}

			return ThoughtState.ActiveAtStage(StaticCollectionsClass.crewMembersLost-1);
			


		}

		

	}
}