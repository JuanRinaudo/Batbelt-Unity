using System;

namespace SimpleTweens.Exceptions
{
    public class MissingTweenException : Exception
    {
        internal MissingTweenException(Tween tween) : base($"The tween {tween.Id} does not exist.")
        {
            
        }
    }
}