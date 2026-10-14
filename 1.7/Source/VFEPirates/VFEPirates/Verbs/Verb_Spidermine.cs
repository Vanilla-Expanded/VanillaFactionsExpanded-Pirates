using UnityEngine;
using Verse;

namespace VFEPirates
{
    public class Verb_Spidermine : Verb_LaunchProjectileStatic
    {
        private Texture2D commandIconCached;
        public override Texture2D UIIcon
        {
            get
            {
                if (verbProps.commandIcon != null)
                {
                    if (commandIconCached == null)
                    {
                        commandIconCached = ContentFinder<Texture2D>.Get(verbProps.commandIcon);
                    }
                    return commandIconCached;
                }
                if (EquipmentSource != null)
                {
                    return EquipmentSource.def.uiIcon;
                }
                return BaseContent.BadTex;
            }
        }

        public override bool IsMeleeAttack => true;
    }
}
