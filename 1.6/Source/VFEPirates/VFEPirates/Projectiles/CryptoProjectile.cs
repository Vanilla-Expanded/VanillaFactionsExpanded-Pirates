﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;
using VEF.Weapons;

namespace VFEPirates
{
	public class CryptoProjectile : ExpandableProjectile
	{
        public override void DoDamage(IntVec3 pos)
		{
			base.DoDamage(pos);
			try
			{
				if (pos != this.launcher.Position && this.launcher.Map != null && GenGrid.InBounds(pos, this.launcher.Map))
				{
					Map.snowGrid.AddDepth(pos, 0.5f);
					var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
					for (int num = list.Count - 1; num >= 0; num--)
					{
						if (IsDamagable(list[num]))
						{
							this.customImpact = true;
							base.Impact(list[num]);
							this.customImpact = false;
							if (list[num] is Pawn pawn)
                            {
								HealthUtility.AdjustSeverity(pawn, HediffDefOf.Hypothermia, Rand.Range(0.05f, 0.25f));
								HealthUtility.AdjustSeverity(pawn, VFEP_DefOf.VFEP_HypothermicSlowdown, 0.3f);
							}
						}
					}
				}
			}
			catch { };
		}

        public override bool IsDamagable(Thing t)
        {
            return t is Pawn && base.IsDamagable(t) || t.def == ThingDefOf.Fire;
        }
    }
}
