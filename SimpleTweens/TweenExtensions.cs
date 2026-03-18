using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleTweens
{
    public static partial class TweenExtensions
    {
        public static float CustomEvaluate(this AnimationCurve curve, ref float v)
        {
            return curve.Evaluate(v);
        }
        
        public static Tween TwPosition(this Transform transform, Vector3 target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(transform.position, target, duration, v => transform.position = v, ease,
                transform);
            return tween;
        }

        public static Tween TwLocalPosition(this Transform transform, Vector3 target, float duration,
            EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(transform.localPosition, target, duration, v => transform.localPosition = v,
                ease, transform);
            return tween;
        }

        public static Tween TwScale(this Transform transform, Vector3 target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(transform.localScale, target, duration, v => transform.localScale = v, ease,
                transform);
            return tween;
        }

        public static Tween TwAnchoredPosition(this RectTransform rectTransform, Vector2 target, float duration,
            EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(rectTransform.anchoredPosition, target, duration,
                v => rectTransform.anchoredPosition = v, ease, rectTransform);
            return tween;
        }

        public static Tween TwScale(this RectTransform rectTransform, Vector3 target, float duration,
            EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(rectTransform.localScale, target, duration, v => rectTransform.localScale = v,
                ease, rectTransform);
            return tween;
        }

        public static Tween TwPosition(this Rigidbody2D rigidbody, Vector2 target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.RunFixed(rigidbody.position, target, duration, rigidbody.MovePosition, ease,
                rigidbody);
            return tween;
        }

        public static Tween TwAlpha(this Image image, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(image.color.a, target, duration, v => image.color = image.color.WithA(v), ease,
                image);
            return tween;
        }

        public static Tween TwAlpha(this SpriteRenderer sprite, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(sprite.color.a, target, duration, v => sprite.color = sprite.color.WithA(v),
                ease, sprite);
            return tween;
        }

        public static Tween TwScale(this Image image, Vector3 target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(image.transform.localScale, target, duration,
                v => image.transform.localScale = v, ease, image);
            return tween;
        }

        public static Tween TwAlpha(this CanvasGroup canvasGroup, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(canvasGroup.alpha, target, duration, v => canvasGroup.alpha = v, ease,
                canvasGroup);
            return tween;
        }

        public static Tween TwTextAlpha(this TextMeshProUGUI text, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(text.alpha, target, duration, v => text.alpha = v, ease, text);
            return tween;
        }

        public static Tween TwTextAlpha(this TextMeshPro text, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(text.color.a, target, duration, v => text.color = new Color(text.color.r, text.color.g, text.color.b, v), ease, text);
            return tween;
        }

        public static Tween TwTextColor(this TextMeshPro text, Color target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(text.color, target, duration, v => text.color = v, ease, text);
            return tween;
        }

        public static Tween TwOrthographicSize(this Camera camera, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(camera.orthographicSize, target, duration, v => camera.orthographicSize = v,
                ease, camera);
            return tween;
        }

        public static Tween TwMaterialColor(this Material material, int hashID, Color target, float duration,
            EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(material.GetColor(hashID), target, duration, v => material.SetColor(hashID, v),
                ease, material);
            return tween;
        }

        public static Tween TwColor(this TextMeshProUGUI text, Color color, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(text.color, color, duration, v => text.color = v, ease, text);
            return tween;
        }

        public static Tween TwColor(this Image image, Color color, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(image.color, color, duration, v => image.color = v, ease, image);
            return tween;
        }

        public static Tween TwColor(this SpriteRenderer sprite, Color color, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(sprite.color, color, duration, v => sprite.color = v, ease, sprite);
            return tween;
        }

        public static Tween TwEnabled(this SpriteRenderer sprite, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(sprite.enabled ? 1f : 0f, 1f, duration, v => sprite.enabled = v > 0.999f, ease,
                sprite);
            return tween;
        }

        public static Tween TwDisabled(this SpriteRenderer sprite, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(sprite.enabled ? 1f : 0f, 0f, duration, v => sprite.enabled = v < 0.001f, ease,
                sprite);
            return tween;
        }

        public static Tween TwVolume(this AudioSource audio, float target, float duration, EaseProcedure ease)
        {
            var tween = TweenManager.Instance.Run(audio.volume, target, duration, v => audio.volume = v, ease, audio);
            return tween;
        }

        public static Tween TwPosition(this Transform transform, TweenAnimationCurveData<Vector3> curveData)
        {
            return transform.TwPosition(curveData.Value, curveData.Duration, curveData.Curve.CustomEvaluate)
                .SetDelay(curveData.Delay);
        }

        public static Tween TwLocalPosition(this Transform transform, TweenAnimationCurveData<Vector3> curveData)
        {
            return transform.TwLocalPosition(curveData.Value, curveData.Duration, curveData.Curve.CustomEvaluate)
                .SetDelay(curveData.Delay);
        }

        public static Tween TwPosition(this Transform transform, TweenAnimationData<Vector3> curveData)
        {
            return transform.TwPosition(curveData.Value, curveData.Duration, curveData.Ease.ToProcedure())
                .SetDelay(curveData.Delay);
        }

        public static Tween TwLocalPosition(this Transform transform, TweenAnimationData<Vector3> curveData)
        {
            return transform.TwLocalPosition(curveData.Value, curveData.Duration, curveData.Ease.ToProcedure())
                .SetDelay(curveData.Delay);
        }

        public static Tween TwScale(this Transform transform, TweenAnimationCurveData<Vector3> curveData)
        {
            return transform.TwScale(curveData.Value, curveData.Duration, curveData.Curve.CustomEvaluate)
                .SetDelay(curveData.Delay);
        }

        public static Tween TwScale(this Transform transform, TweenAnimationData<Vector3> curveData)
        {
            return transform.TwScale(curveData.Value, curveData.Duration, curveData.Ease.ToProcedure())
                .SetDelay(curveData.Delay);
        }

        public static Tween TwAnchoredPosition(this RectTransform rectTransform,
            TweenAnimationCurveData<Vector3> curveData)
        {
            return rectTransform.TwAnchoredPosition(curveData.Value, curveData.Duration, curveData.Curve.CustomEvaluate)
                .SetDelay(curveData.Delay);
        }

        public static Tween TwAnchoredPosition(this RectTransform rectTransform, TweenAnimationData<Vector3> curveData)
        {
            return rectTransform.TwAnchoredPosition(curveData.Value, curveData.Duration, curveData.Ease.ToProcedure())
                .SetDelay(curveData.Delay);
        }

        public static Tween TwScale(this RectTransform rectTransform, TweenAnimationCurveData<Vector3> curveData)
        {
            return rectTransform.TwScale(curveData.Value, curveData.Duration, curveData.Curve.CustomEvaluate)
                .SetDelay(curveData.Delay);
        }

        public static Tween TwScale(this RectTransform rectTransform, TweenAnimationData<Vector3> curveData)
        {
            return rectTransform.TwScale(curveData.Value, curveData.Duration, curveData.Ease.ToProcedure())
                .SetDelay(curveData.Delay);
        }
    }
}