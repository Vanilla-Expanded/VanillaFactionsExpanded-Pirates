using RimWorld;

namespace VFEPirates
{
    public class RaidStrategyWorker_Gauntlet : RaidStrategyWorker_ImmediateAttack
    {
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            string fDefName = parms.faction.def.defName;
            if (VFEPiratesMod.settings.disableGauntlet)
                return false;

            return base.CanUseWith(parms, groupKind) && ((Faction.OfPirates != null && fDefName == Faction.OfPirates.def.defName) || fDefName == "VFEP_Junkers" || fDefName == "VFEP_Mercenaries");
        }
    }
}
