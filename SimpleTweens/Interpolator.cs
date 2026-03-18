using JetBrains.Annotations;

namespace SimpleTweens
{
    [PublicAPI]
    public delegate T Interpolator<T>(ref T start, ref T end, ref float time);
}