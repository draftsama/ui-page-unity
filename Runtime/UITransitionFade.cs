using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Draft
{
    public class UITransitionFade : MonoBehaviour
    {
        private static readonly Dictionary<Transform, UITransitionFade> s_Fades = new Dictionary<Transform, UITransitionFade>();

        private CanvasGroup canvasGroup;
        private Image image;
        private Transform owner;

        /// <summary>
        /// Returns the fade overlay living under <paramref name="_parent"/>, i.e. in the same local space as the pages.
        /// </summary>
        public static UITransitionFade GetFor(Transform _parent)
        {
            if (s_Fades.TryGetValue(_parent, out var existing) && existing != null)
                return existing;

            var fade = new GameObject("TransitionFade", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(LayoutElement));
            var rect = (RectTransform)fade.transform;
            rect.SetParent(_parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            fade.GetComponent<LayoutElement>().ignoreLayout = true;

            var instance = fade.AddComponent<UITransitionFade>();
            instance.owner = _parent;
            instance.image = fade.GetComponent<Image>();
            instance.canvasGroup = fade.GetComponent<CanvasGroup>();
            instance.canvasGroup.alpha = 0;
            instance.canvasGroup.blocksRaycasts = false;
            instance.canvasGroup.interactable = false;

            s_Fades[_parent] = instance;
            return instance;
        }

        private void OnDestroy()
        {
            if (owner != null && s_Fades.TryGetValue(owner, out var current) && current == this)
                s_Fades.Remove(owner);
        }

        public async UniTask FadeIn(int _milliseconds, Color _color, CancellationToken _token = default)
        {

            await UniTask.Yield();
            transform.SetAsLastSibling();
            canvasGroup.alpha = 0;
            image.color = _color;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            await canvasGroup.DOFade(1f, _milliseconds / 1000f).SetUpdate(true).WithCancellation(_token);

        }
        public async UniTask FadeOut(int _milliseconds, Color _color, CancellationToken _token = default)
        {
            await UniTask.Yield();
            transform.SetAsLastSibling();
            canvasGroup.alpha = 1;
            image.color = _color;
            await canvasGroup.DOFade(0f, _milliseconds / 1000f).SetUpdate(true).WithCancellation(_token);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

        }



    }
}
