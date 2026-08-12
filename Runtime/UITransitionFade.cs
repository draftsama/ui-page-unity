using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Modules.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Utilities
{
    public class UITransitionFade : MonoBehaviour
    {
        private static UITransitionFade _instance;

        private CanvasGroup canvasGroup;
        private Image image;

        public static UITransitionFade Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UITransitionFade>();
                    if (_instance == null)
                    {

                        //create canvas
                        var go = new GameObject("TransitionCanvas");
                        var canvas = go.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvas.sortingOrder = 1000;
                        go.AddComponent<CanvasScaler>();
                        go.AddComponent<GraphicRaycaster>();
                        var container = new GameObject("Container", typeof(RectTransform));
                        var contrainerRect = container.GetComponent<RectTransform>();
                        contrainerRect.SetParent(go.transform);

                        contrainerRect.anchorMin = Vector2.zero;
                        contrainerRect.anchorMax = Vector2.one;
                        contrainerRect.sizeDelta = Vector2.zero;
                        contrainerRect.anchoredPosition = Vector2.zero;

                        var fade = new GameObject("Fade", typeof(Image), typeof(CanvasGroup));
                        var fadeRect = fade.GetComponent<RectTransform>();
                        fadeRect.SetParent(container.transform);
                        fadeRect.anchorMin = Vector2.zero;
                        fadeRect.anchorMax = Vector2.one;
                        fadeRect.sizeDelta = Vector2.zero;
                        fadeRect.anchoredPosition = Vector2.zero;


                        _instance = fade.AddComponent<UITransitionFade>();

                        _instance.image = fade.GetComponent<Image>();
                        _instance.canvasGroup = fade.GetComponent<CanvasGroup>();
                        _instance.canvasGroup.alpha = 0;
                        _instance.canvasGroup.blocksRaycasts = true;
                        _instance.canvasGroup.interactable = true;

                    }
                }

                return _instance;
            }
        }

        public async UniTask FadeIn(int _milliseconds, Color _color, CancellationToken _token = default)
        {

            await UniTask.Yield();
            canvasGroup.alpha = 0;
            image.color = _color;
            await canvasGroup.DOFade(1f, _milliseconds / 1000f).SetUpdate(true).WithCancellation(_token);

        }
        public async UniTask FadeOut(int _milliseconds, Color _color, CancellationToken _token = default)
        {
            await UniTask.Yield();
            canvasGroup.alpha = 1;
            image.color = _color;
            await canvasGroup.DOFade(0f, _milliseconds / 1000f).SetUpdate(true).WithCancellation(_token);

        }



    }
}
