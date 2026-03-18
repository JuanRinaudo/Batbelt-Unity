using System;
using System.Collections.Generic;
using SimpleTweens.Exceptions;

namespace SimpleTweens
{
    public class TweenGroup
    {
        public List<Tween> Tweens;

        public Action? OnCancel;
        public Action? OnStart;
        public Action? OnComplete;

        public long Id;

        static long _idIncrementer;
        readonly TweenManager _tweenManager;

        int _completedCounter;

        public bool IsAlive
        {
            get
            {
                foreach (var t in Tweens)
                    if (t.IsAlive)
                        return true;
                return false;
            }
        }

        public bool HasStarted
        {
            get
            {
                foreach (var t in Tweens)
                    if (t.HasStarted)
                        return true;
                return false;
            }
        }

        public TweenGroup(TweenManager tweenManager)
        {
            _tweenManager = tweenManager;
            Tweens = new List<Tween>();
        }

        public TweenGroup(TweenManager tweenManager, List<Tween> tweens)
        {
            _tweenManager = tweenManager;
            Tweens = tweens;
        }

        public void Add(Tween tween)
        {
            Tweens.Add(tween);
            tween.AddOnComplete(OnTweenComplete);
        }

        public void Add(List<Tween> tweens)
        {
            foreach (var tween in tweens)
            {
                Tweens.Add(tween);
                tween.AddOnComplete(OnTweenComplete);
            }
        }

        void OnTweenComplete()
        {
            _completedCounter++;
            if (_completedCounter == Tweens.Count)
                GroupCompleted();
        }

        void GroupCompleted()
        {
            OnComplete?.Invoke();
            Clear();
        }

        public void Complete(bool invokeComplete = true)
        {
            for (var index = 0; Tweens != null && index < Tweens.Count; index++)
            {
                var t = Tweens[index];
                t.Complete(invokeComplete);
            }
        }

        public void Cancel()
        {
            OnCancel?.Invoke();
            foreach (var t in Tweens)
                t.Cancel();

            Clear();
        }

        public void Start()
        {
            OnStart?.Invoke();
            foreach (var t in Tweens)
                t.Start();
        }

        public void Restart()
        {
            foreach (var t in Tweens)
                t.Restart();
        }

        public void Pause()
        {
            foreach (var t in Tweens)
                t.Pause();
        }

        public TweenGroup AddOnCancel(Action cancel)
        {
            ThrowIfInvalid();
            OnCancel += cancel;
            return this;
        }

        public TweenGroup AddOnComplete(Action complete)
        {
            ThrowIfInvalid();
            OnComplete += complete;
            return this;
        }

        public TweenGroup AddOnStart(Action start)
        {
            ThrowIfInvalid();
            OnStart += start;
            return this;
        }

        void ThrowIfInvalid()
        {
            if (!_tweenManager)
                throw new MissingTweenManagerException(this);
        }

        public void Restore()
        {
            Id = _idIncrementer++;
            _completedCounter = 0;
            Tweens?.Clear();
        }

        public void Clear()
        {
            Id = -1;
            _completedCounter = 0;
            Tweens?.Clear();
        }
    }
}