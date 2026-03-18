using System;
using SimpleTweens.Exceptions;
using JetBrains.Annotations;

namespace SimpleTweens
{
    /// <summary>
    /// The tween handle.
    /// </summary>
    [PublicAPI]
    public readonly struct Tween
    {
        internal readonly long Id;
        private static long _idIncrementer;
        private readonly TweenManager _tweenManager;

        /// <summary>
        /// Is this tween alive? The tween can be alive even ifs been paused.
        /// </summary>
        public bool IsAlive => _tweenManager && _tweenManager.IsTweenActive(this);
        public bool HasStarted => _tweenManager && _tweenManager.HasStarted(this);

        internal Tween(TweenManager tweenManager)
        {
            Id = _idIncrementer++;
            _tweenManager = tweenManager;
        }

        /// <summary>
        /// Starts this tween. This acts as an unpause.
        /// </summary>
        /// <exception cref="MissingTweenException">Occurrs if this tween doesn't exist or no longer exists.</exception>
        /// <exception cref="MissingTweenManagerException">Occurrs if this tween is misconfigured or the TweenManager that created it has been destroyed.</exception>
        public void Start()
        {
            ThrowIfInvalid();
            _tweenManager.PlayTween(this);
        }

        /// <summary>
        /// Pauses this tween.
        /// </summary>
        /// <exception cref="MissingTweenException">Occurrs if this tween doesn't exist or no longer exists.</exception>
        /// <exception cref="MissingTweenManagerException">Occurrs if this tween is misconfigured or the TweenManager that created it has been destroyed.</exception>
        public void Pause()
        {
            ThrowIfInvalid();
            _tweenManager.PauseTween(this);
        }

        /// <summary>
        /// Restarts this tween.
        /// </summary>
        /// <exception cref="MissingTweenException">Occurrs if this tween doesn't exist or no longer exists.</exception>
        /// <exception cref="MissingTweenManagerException">Occurrs if this tween is misconfigured or the TweenManager that created it has been destroyed.</exception>
        public void Restart()
        {
            ThrowIfInvalid();
            _tweenManager.ResetTween(this);
        }

        public void TryCancel()
        {
            if (CheckIfInvalid())
                return;
            _tweenManager.CancelTween(this);
        }
        
        /// <summary>
        /// Cancel's this tween.
        /// </summary>
        /// <exception cref="MissingTweenException">Occurrs if this tween doesn't exist or no longer exists.</exception>
        /// <exception cref="MissingTweenManagerException">Occurrs if this tween is misconfigured or the TweenManager that created it has been destroyed.</exception>
        public void Cancel()
        {
            ThrowIfInvalid(false);
            _tweenManager.CancelTween(this);
        }
        
        public void TryComplete(bool invokeComplete = true)
        {
            if (CheckIfInvalid())
                return;
            _tweenManager.CompleteTween(this, invokeComplete);
        }

        /// <summary>
        /// Completes the tween immediately.
        /// </summary>
        public void Complete(bool invokeComplete = true)
        {
            ThrowIfInvalid(false);
            _tweenManager.CompleteTween(this, invokeComplete);
        }

        public Tween AddOnCancel(Action cancel)
        {
            ThrowIfInvalid();
            _tweenManager.AddOnCancel(this, cancel);
            return this;
        }

        public Tween AddOnComplete(Action complete)
        {
            ThrowIfInvalid();
            _tweenManager.AddOnComplete(this, complete);
            return this;
        }

        public Tween AddOnStart(Action start)
        {
            ThrowIfInvalid();
            _tweenManager.AddOnStart(this, start);
            return this;
        }

        public Tween SetLoops(int loops, LoopType type)
        {
            _tweenManager.SetLoops(this, loops, type);
            return this;
        }

        public Tween SetProgress(float progress)
        {
            _tweenManager.SetProgress(this, progress);
            return this;
        }

        public Tween SetNormalizedProgress(float normalizedProgress)
        {
            _tweenManager.SetNormalizedProgress(this, normalizedProgress);
            return this;
        }

        public Tween SetIgnoreTimescale(bool ignoreTimescale)
        {
            _tweenManager.SetIgnoreTimescale(this, ignoreTimescale);
            return this;
        }

        public Tween SetDelay(float delay)
        {
            _tweenManager.SetDelay(this, delay);
            return this;
        }

        bool CheckIfInvalid(bool checkIfActive = true)
        {
            return !_tweenManager || (checkIfActive && !_tweenManager.IsTweenActive(this));
        }

        void ThrowIfInvalid(bool checkIfActive = true)
        {
            if (!_tweenManager)
                throw new MissingTweenManagerException(this);

            if (checkIfActive && !_tweenManager.IsTweenActive(this))
                throw new MissingTweenException(this);
        }

#if AURATWEEN_UNITASK_SUPPORT
        public Cysharp.Threading.Tasks.UniTask.Awaiter GetAwaiter()
        {
            return CompletionTask().GetAwaiter();
        }

        private async Cysharp.Threading.Tasks.UniTask CompletionTask()
        {
            while (IsAlive)
                await Cysharp.Threading.Tasks.UniTask.Yield();
        }
#endif
    }
}