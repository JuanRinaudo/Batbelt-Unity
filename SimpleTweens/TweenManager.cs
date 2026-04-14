using System;
using System.Collections.Generic;
using SimpleTweens.Exceptions;
using SimpleTweens.Internal;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace SimpleTweens
{
    [PublicAPI]
    [DefaultExecutionOrder(-5000)] // We want this to execute earlier. Our current design means if someone spawns a tween before .Start(), an exception will be thrown.
    public class TweenManager : MonoBehaviour
    {        
        [SerializeField]
        int _defaultTweenCapacity = 100;
        
        // We store the contexts in both a list and dictionary.
        // The list is so we can loop over every active context without
        // allocating any garbage. The dictionary is so we can have O(1) perf
        // when looking for contexts from the individual tweens.
        List<TweenContext> _activeContexts;
        List<TweenContext> _fixedActiveContexts;
        Dictionary<long, TweenContext> _activeContextLookup;
        
        ObjectPool<TweenContext>? _contextPool;
        
        public static TweenManager Instance { get; private set; }

        private void Awake()
        {
            if(Instance != null)
            {
                Debug.LogWarning("Multiple TweenManagers detected in the scene. There should only be one. Destroying the newest one.");
                Destroy(this);
                return;
            }

            Instance = this;
            
            var collectionChecks = false;
#if UNITY_EDITOR
            collectionChecks = true;
#endif
            _activeContexts = new List<TweenContext>(_defaultTweenCapacity);
            _fixedActiveContexts = new List<TweenContext>(_defaultTweenCapacity);
            _activeContextLookup = new Dictionary<long, TweenContext>(_defaultTweenCapacity);
            _contextPool = new ObjectPool<TweenContext>(() => new TweenContext(), ClearContext, ClearContext, ClearContext, collectionChecks, _defaultTweenCapacity, int.MaxValue);
            
            // Warm up the pool
            var contexts = new TweenContext[_defaultTweenCapacity];
            
            for (int i = 0; i < contexts.Length; i++)
                contexts[i] = _contextPool.Get();
            
            foreach (var ctx in contexts)
                _contextPool.Release(ctx);
        }
        void OnDestroy()
        {
            _contextPool?.Dispose();
        }
        
        [CanBeNull]
        internal List<TweenContext> GetTweenContextList(TweenContext ctx)
        {
            return ctx.FixedUpdate ? _fixedActiveContexts : _activeContexts;
        }
        
        /// <summary>
        /// Sets the capacity of this <see cref="TweenManager"/>. This must be called before this component starts.
        /// </summary>
        /// <remarks>
        /// If you're instantiating the <see cref="TweenManager"/> programatically and need to set the capacity, you can use .SetActive(false)
        /// on the target <see cref="GameObject"/>, add the <see cref="TweenManager"/> to that <see cref="GameObject"/>, and then call <see cref="SetCapacity"/>.
        /// </remarks>
        /// <param name="size">The size of the default tween capacity.</param>
        public void SetCapacity(int size)
        {
            if (0 > size)
                size = 0;
            
            _defaultTweenCapacity = size;
        }

        /// <summary>
        /// Run a tween based on the provided options.
        /// </summary>
        /// <param name="options">The options of the tween.</param>
        /// <returns></returns>
        /// <exception cref="UninitializedTweenManagerException">Occurrs when this TweenManager has not been initialized. Make sure the component is active and enabled.</exception>
        public Tween Run(TweenOptions options)
        {
            if (_contextPool == null)
                throw new UninitializedTweenManagerException();
            
            var handle = new Tween(this);
            var ctx = _contextPool.Get();
            ctx.Id = handle.Id;
            ctx.Updater = options.Updater;
            ctx.Delay = 0;
            ctx.Duration = options.Duration;
            ctx.Lifetime = options.Lifetime;
            ctx.Procedure = options.Procedure;
            ctx.Owner = options.Owner;
            ctx.Loops = 0;
            ctx.IgnoreTimescale = false;
            ctx.LoopType = LoopType.Restart;
            ctx.Inverted = false;
            AddContext(ctx);
            return handle;
        }
        
        public Tween DelayedCall(float delay, Action callback, Object owner = null)
        {
            var tween = Run(new TweenOptions
            {
                Duration = 0.1f,
                FixedUpdate = false,
                Procedure = Easer.Linear,
                Updater = _ => { },
                Lifetime = owner ? () => owner : null,
                Owner = owner
            });
            tween.SetDelay(delay);
            tween.AddOnStart(callback);
            return tween;
        }
        
        public TweenGroup CreateGroup()
        {
            return new TweenGroup(this);
        }
        
        public TweenGroup CreateGroup(List<Tween> tweens)
        {
            return new TweenGroup(this, tweens);
        }

        internal bool IsTweenActive(Tween tween)
        {
            return _activeContextLookup != null && _activeContextLookup.ContainsKey(tween.Id);
        }

        internal bool IsTweenPaused(Tween tween)
        {
            return _activeContextLookup != null && _activeContextLookup.ContainsKey(tween.Id) && _activeContextLookup[tween.Id].Paused;
        }

        internal bool HasStarted(Tween tween)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return false;
            
            return ctx.Started;
        }
        
        internal void PlayTween(Tween tween)
        {
            var ctx = GetContext(tween);
            ctx!.Paused = false;
        }
        
        internal void PauseTween(Tween tween)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;
            
            ctx.Paused = true;
        }
        
        internal void ResetTween(Tween tween)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.Progress = 0f;
        }
        
        internal void CancelTween(Tween tween)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.WantsToCancel = true;
        }
        
        internal void CompleteTween(Tween tween, bool invokeComplete = true)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            CompleteTween(ctx, invokeComplete);
        }
        
        internal void CompleteTween(TweenContext ctx, bool invokeComplete = true)
        {
            var endValue = ctx.Inverted ? 0f : 1f;
            ctx.Updater?.Invoke(ctx.Procedure!(ref endValue)); // Force the updater to be "1" to re-evaluate its value in case we go over.
            if(invokeComplete)
                ctx.OnComplete?.Invoke();
            
            _activeContextLookup.Remove(ctx.Id);
            GetTweenContextList(ctx)?.Remove(ctx);
            _contextPool.Release(ctx);
        }
        
        internal void AddOnCancel(Tween tween, Action cancel)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.OnCancel += cancel;
        }

        internal void AddOnComplete(Tween tween, Action complete)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.OnComplete += complete;
        }

        internal void AddOnStart(Tween tween, Action start)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.OnStart += start;
        }

        internal void SetLoops(Tween tween, int loops, LoopType type)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.Loops = loops;
            ctx.LoopType = type;
        }
        
        public void SetProgress(Tween tween, float progress)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.Progress = progress;
        }

        public void SetNormalizedProgress(Tween tween, float normalizedProgress)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.Progress = normalizedProgress * ctx.Duration;
        }

        internal void SetIgnoreTimescale(Tween tween, bool ignoreTimescale)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.IgnoreTimescale = ignoreTimescale;
        }

        internal void SetDelay(Tween tween, float delay)
        {
            var ctx = GetContext(tween);
            if (ctx is null)
                return;

            ctx.Delay = delay;
        }

        public void CancelAll(Object target)
        {
            if(_activeContexts == null || _fixedActiveContexts == null)
                return;
            
            for (int i = 0; i < _activeContexts.Count; i++)
                if (_activeContexts[i].Owner == target)
                    _activeContexts[i].WantsToCancel = true;
            
            for (int i = 0; i < _fixedActiveContexts.Count; i++)
                if (_fixedActiveContexts[i].Owner == target)
                    _fixedActiveContexts[i].WantsToCancel = true;
        }

        public void CompleteAll(Object target, bool invokeComplete = true)
        {
            if(_activeContexts == null || _fixedActiveContexts == null)
                return;
            
            for (int i = 0; i < _activeContexts.Count; i++)
                if (_activeContexts[i].Owner == target)
                    CompleteTween(_activeContexts[i], invokeComplete);
            
            for (int i = 0; i < _fixedActiveContexts.Count; i++)
                if (_fixedActiveContexts[i].Owner == target)
                    CompleteTween(_fixedActiveContexts[i], invokeComplete);
        }

        TweenContext? GetContext(Tween tween) => _activeContextLookup != null && _activeContextLookup.TryGetValue(tween.Id, out var ctx) ? ctx : null;
        
        void AddContext(TweenContext ctx)
        {
            if (_activeContexts is null || _activeContextLookup is null)
                return;
            
            _activeContexts.Add(ctx);
            _activeContextLookup[ctx.Id] = ctx;
        }
        
        void AddFixedContext(TweenContext ctx)
        {
            if (_fixedActiveContexts is null || _activeContextLookup is null)
                return;
            
            _fixedActiveContexts.Add(ctx);
            _activeContextLookup[ctx.Id] = ctx;
        }

        void Update()
        {
            // BatCore.Log($"Active tweens: {_contextPool?.CountActive ?? 0}");
            
            if (_contextPool is null || _activeContexts is null || _activeContextLookup is null)
                return;
            
            // Iterate over every active context in reverse order
            // so we can remove them if they become if they're invalid.
            for (int i = _activeContexts.Count - 1; i >= 0; i--)
            {
                var ctx = _activeContexts[i];
                
                var time = ctx.IgnoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
                
                // Do not progress the tween if its paused.
                if (!ctx.Paused)
                    ctx.Progress += time;

                if(ctx.Progress >= ctx.Delay && !ctx.Started) {
                    ctx.Started = true;
                    ctx.OnStart?.Invoke();
                }
                
                var lifetimeExpired = ctx.Lifetime != null && !ctx.Lifetime();
                if (ctx.WantsToCancel || lifetimeExpired)
                {
                    // We only want to invoke the cancellation event if it was intended.
                    if (!lifetimeExpired)
                        ctx.OnCancel?.Invoke();
                    
                    _activeContextLookup.Remove(ctx.Id);
                    _activeContexts.Remove(ctx);
                    _contextPool.Release(ctx);
                    continue;
                }

                var easer = ctx.Procedure;                
                // Check if the tween has been completed.
                if (ctx.Progress >= (ctx.Duration + ctx.Delay) || ctx.Duration == 0)
                {
                    var endValue = ctx.Inverted ? 0f : 1f;
                    ctx.Updater?.Invoke(easer!(ref endValue)); // Force the updater to be "1" to re-evaluate its value in case we go over.
                    ctx.OnComplete?.Invoke();
                    
                    if(ctx.Loops == 0) {
                        // Tween was completed. Remove from active, invoke necessary events, and cleanup.
                        _activeContextLookup.Remove(ctx.Id);
                        _activeContexts.Remove(ctx);
                        _contextPool.Release(ctx);
                    }
                    else {
                        if(ctx.Loops > 0)
                            ctx.Loops--;

                        if(ctx.LoopType == LoopType.Restart) {
                            ctx.Progress = 0;
                        }
                        else if(ctx.LoopType == LoopType.Yoyo) {
                            ctx.Progress = 0;
                            ctx.Inverted = !ctx.Inverted;
                        }
                    }
                    continue;
                }

                var progress = Mathf.Max((ctx.Progress - ctx.Delay) / ctx.Duration, 0);
                if(ctx.Inverted)
                    progress = 1 - progress;

                ctx.Updater?.Invoke(easer!(ref progress)); 
            }
        }
        

        void FixedUpdate()
        {
            // BatCore.Log($"Active tweens: {_contextPool?.CountActive ?? 0}");
            
            if (_contextPool is null || _fixedActiveContexts is null || _activeContextLookup is null)
                return;
            
            // Iterate over every active context in reverse order
            // so we can remove them if they become if they're invalid.
            for (int i = _fixedActiveContexts.Count - 1; i >= 0; i--)
            {
                var ctx = _fixedActiveContexts[i];
                
                var time = ctx.IgnoreTimescale ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;
                
                // Do not progress the tween if its paused.
                if (!ctx.Paused)
                    ctx.Progress += time;

                var lifetimeExpired = ctx.Lifetime != null && !ctx.Lifetime();
                if (ctx.WantsToCancel || lifetimeExpired)
                {
                    // We only want to invoke the cancellation event if it was intended.
                    if (!lifetimeExpired)
                        ctx.OnCancel?.Invoke();
                    
                    _activeContextLookup.Remove(ctx.Id);
                    _fixedActiveContexts.Remove(ctx);
                    _contextPool.Release(ctx);
                    continue;
                }

                var easer = ctx.Procedure;                
                // Check if the tween has been completed.
                if (ctx.Progress >= (ctx.Duration + ctx.Delay) || ctx.Duration == 0)
                {
                    var endValue = ctx.Inverted ? 0f : 1f;
                    ctx.Updater?.Invoke(easer!(ref endValue)); // Force the updater to be "1" to re-evaluate its value in case we go over.
                    ctx.OnComplete?.Invoke();
                    
                    if(ctx.Loops == 0) {
                        // Tween was completed. Remove from active, invoke necessary events, and cleanup.
                        _activeContextLookup.Remove(ctx.Id);
                        _fixedActiveContexts.Remove(ctx);
                        _contextPool.Release(ctx);
                    }
                    else {
                        if(ctx.Loops > 0)
                            ctx.Loops--;

                        if(ctx.LoopType == LoopType.Restart) {
                            ctx.Progress = 0;
                        }
                        else if(ctx.LoopType == LoopType.Yoyo) {
                            ctx.Progress = 0;
                            ctx.Inverted = !ctx.Inverted;
                        }
                    }
                    continue;
                }

                var progress = Mathf.Max((ctx.Progress - ctx.Delay) / ctx.Duration, 0);
                if(ctx.Inverted)
                    progress = 1 - progress;

                if(progress > 0 && !ctx.Started) {
                    ctx.Started = true;
                    ctx.OnStart?.Invoke();
                }

                ctx.Updater?.Invoke(easer!(ref progress)); 
            }
        }
        
        static void ClearContext(TweenContext ctx)
        {
            ctx.Id = -1;
            ctx.Progress = 0;
            ctx.WantsToCancel = false;
            ctx.Paused = false;
            ctx.Started = false;
            ctx.OnStart = null;
            ctx.OnCancel = null;
            ctx.OnComplete = null;
            ctx.Updater = null!;
            ctx.Delay = 0;
            ctx.Duration = 0;
            ctx.Procedure = null!;
            ctx.Lifetime = null;
            ctx.Loops = 0;
            ctx.LoopType = LoopType.Restart;
            ctx.Inverted = false;
            ctx.IgnoreTimescale = false;
        }

        static void ClearGroup(TweenGroup group)
        {
            group.Restore();
        }
    }
}