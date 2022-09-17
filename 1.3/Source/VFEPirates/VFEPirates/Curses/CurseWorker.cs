using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static Verse.AsymmetricLinkData;

namespace VFEPirates
{
    public class CurseWorker
    {
        protected Dictionary<MethodBase, List<MethodInfo>> patchedMethods = new Dictionary<MethodBase, List<MethodInfo>>();
        public CurseWorker()
        {

        }
        public static bool IsActive(CurseDef def) => GameComponent_CurseManager.Instance.activeCurseDefs.Contains(def);
        public void Start()
        {
            patchedMethods ??= new Dictionary<MethodBase, List<MethodInfo>>();
            if (GameComponent_CurseManager.Instance.activeCurseTypes.Contains(GetType()))
            {
                if (!patchedMethods.Any())
                {
                    DoPatches();
                }
                OnActivate();
            }
            else
            {
                Disactivate();
            }
        }

        public void Disactivate()
        {
            if (patchedMethods.Any())
            {
                OnDisactivate();
                foreach (var kvp in patchedMethods)
                {
                    MethodBase method = kvp.Key;
                    foreach (var patch in kvp.Value)
                    {
                        VFEPiratesMod.harmony.Unpatch(method, patch);
                    }
                }
            }
            patchedMethods.Clear();
        }

        public virtual void OnActivate()
        {

        }

        public virtual void OnDisactivate()
        {

        }
        public virtual void DoPatches()
        {

        }

        public void Patch(Type type, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            Patch(AccessTools.Method(type, methodName), prefix, postfix, transpiler);
        }
        public void Patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            VFEPiratesMod.harmony.Patch(original, prefix != null ? new HarmonyMethod(prefix) : null, 
                postfix != null ? new HarmonyMethod(postfix) : null, transpiler != null ? new HarmonyMethod(transpiler) : null);
            var patches = new List<MethodInfo>();

            if (prefix != null) patches.Add(prefix);
            if (postfix != null) patches.Add(postfix);
            if (transpiler != null ) patches.Add(transpiler);

            if (patchedMethods.ContainsKey(original))
            {
                patchedMethods[original].AddRange(patches);
            }
            else
            {
                patchedMethods[original] = patches;
            }
        }
    }
}
