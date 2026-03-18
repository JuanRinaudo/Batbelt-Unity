using System;
using Object = UnityEngine.Object;

namespace SimpleTweens.Internal
{
    internal sealed class TweenContext
    {
        public long Id;

        public float Progress;

        public bool WantsToCancel;

        public bool Started;
        
        public bool Paused;

        public bool HasLifetime;

        public bool IgnoreTimescale;
        
        public bool FixedUpdate;
        
        public float Delay;

        public float Duration;
        
        public Action? OnCancel;
        
        public Action? OnStart;

        public Action? OnComplete;
        
        public Func<bool>? Lifetime;
        
        public Action<float>? Updater;
        
        public EaseProcedure? Procedure;

        public Object? Owner;

        public int Loops;

        public bool Inverted;

        public LoopType LoopType;
    }
}