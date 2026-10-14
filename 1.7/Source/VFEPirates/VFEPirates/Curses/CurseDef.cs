using System;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    public class CurseDef : Def
    {
        [Unsaved] private Texture2D icon;

        public string iconPath;
        public Type workerClass = typeof(CurseWorker);

        [Unsaved] private CurseWorker workerInt;

        public CurseWorker Worker
        {
            get
            {
                if (workerInt == null) workerInt = (CurseWorker) Activator.CreateInstance(workerClass ?? typeof(CurseWorker));
                return workerInt;
            }
        }

        public Texture2D Icon => icon ??= ContentFinder<Texture2D>.Get(iconPath);
    }
}