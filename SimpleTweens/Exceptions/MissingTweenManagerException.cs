using System;

namespace SimpleTweens.Exceptions
{
    public class MissingTweenManagerException : Exception
    {
        internal MissingTweenManagerException(Tween tween) : base($"Tween {tween.Id} does not have a valid {nameof(TweenManager)}. Has it been destroyed?")
        {
            
        }
        
        internal MissingTweenManagerException(TweenGroup tweenGroup) : base($"Tween {tweenGroup.Id} does not have a valid {nameof(TweenManager)}. Has it been destroyed?")
        {
            
        }
    }
}