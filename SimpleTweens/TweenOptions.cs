using System;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace SimpleTweens
{
    [PublicAPI]
    public ref struct TweenOptions
    {
        public float Duration;
        public bool FixedUpdate;
        public Func<bool>? Lifetime;
        public Action<float> Updater;
        public EaseProcedure Procedure;
        public Object? Owner;
    }
}