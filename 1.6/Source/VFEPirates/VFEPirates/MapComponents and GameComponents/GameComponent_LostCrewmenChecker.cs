using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;


namespace VFEPirates
{
    public class GameComponent_LostCrewmenChecker : GameComponent
    {



        public int tickCounter = 0;
        public int tickInterval = 2000;
        public int crewMembersLost_backup = 0;


        public GameComponent_LostCrewmenChecker(Game game) : base()
        {

        }

        public override void FinalizeInit()
        {
            StaticCollectionsClass.crewMembersLost = crewMembersLost_backup;

            base.FinalizeInit();

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.crewMembersLost_backup, "crewMembersLost_backup", 0, true);
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounterLostCrewmen", 0, true);

        }

        public override void GameComponentTick()
        {


            tickCounter++;
            if ((tickCounter > tickInterval))
            {
                crewMembersLost_backup = StaticCollectionsClass.crewMembersLost;
                tickCounter = 0;
            }



        }


    }


}
