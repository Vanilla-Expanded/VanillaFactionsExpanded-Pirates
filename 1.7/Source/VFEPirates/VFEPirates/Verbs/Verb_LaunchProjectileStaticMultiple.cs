using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFEPirates
{
    public class VerbProps_MultipleProjectiles : VerbProperties
    {
        public int projectileCount;
    }

    [StaticConstructorOnStartup]
	public class Verb_LaunchProjectileStaticMultiple : Verb_LaunchProjectileStatic
    {
        public static Texture2D texture;
        public override Texture2D UIIcon
        {
            get
            {
                if (verbProps.commandIcon != null)
                {
                    if (texture == null)
                    {
                        texture = ContentFinder<Texture2D>.Get(verbProps.commandIcon);
                    }
                    return texture;
                }
                if (EquipmentSource != null)
                {
                    return EquipmentSource.def.uiIcon;
                }
                return BaseContent.BadTex;
            }
        }
        public VerbProps_MultipleProjectiles VerbProps => this.verbProps as VerbProps_MultipleProjectiles;
        protected override bool TryCastShot()
        {
            bool result = false;
            for (var i = 0; i < VerbProps.projectileCount; i++)
            {
                if (Available())
                {
                    result = base.TryCastShot();
                }
            }
            return result;
        }

    }
}
