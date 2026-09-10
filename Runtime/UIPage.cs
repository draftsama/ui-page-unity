using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine.Events;

namespace Draft
{

    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public class UIPage : MonoBehaviour
    {
        private static readonly Dictionary<string, List<UIPage>> s_PageRegistry = new();
        private static readonly HashSet<string> s_TransitioningGroups = new();

        /// <summary>
        /// Clears static page state before the first scene loads on every play-mode entry and in
        /// player builds. Runs even when Unity 6's Enter Play Mode Options have domain reload
        /// disabled, which is the exact configuration where a stale registry from the previous
        /// session would otherwise carry destroyed pages into the next one.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_PageRegistry.Clear();
            s_TransitioningGroups.Clear();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeEditorHooks()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// Repairs state left behind when play mode stops. Fires after Unity restores the
        /// serialized scene. Only pages that were actually mid-transition are reset to their
        /// default baseline - a page the developer deliberately previewed at edit time keeps its
        /// authored state. The repair is in-memory only: it never marks the scene, an object, or
        /// a prefab dirty, and never records an undo entry.
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            if (_state != PlayModeStateChange.EnteredEditMode) return;

            s_PageRegistry.Clear();
            s_TransitioningGroups.Clear();

            foreach (var page in Object.FindObjectsByType<UIPage>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var wasTransitioning = page.m_IsTransitionPage;
                page.m_IsTransitionPage = false;

                if (wasTransitioning)
                    page.SetShow(page.m_IsDefault);
            }
        }
#endif

        [SerializeField][HideInInspector] private string m_GroupName = "Default";
        public string GroupName => m_GroupName;
        [SerializeField][HideInInspector] private bool m_IsDefault;
        public bool IsDefault => m_IsDefault;
        [SerializeField][HideInInspector] private bool m_IsOpened;
        public bool IsOpened => m_IsOpened;
        [SerializeField][HideInInspector] private bool m_IsTransitionPage;
        public bool IsTransitionPage => m_IsTransitionPage;
        [SerializeField][HideInInspector] public TransitionInfo m_TransitionInfo;



        #region Event 

        [HideInInspector]public UnityEvent OnBeginShowPageEvent;
        [HideInInspector]public UnityEvent OnEndShowPageEvent;
        [HideInInspector]public UnityEvent OnBeginHidePageEvent;
        [HideInInspector]public UnityEvent OnEndHidePageEvent;

        #endregion

        private CanvasGroup m_CanvasGroupCache;

        public CanvasGroup m_CanvasGroup
        {
            get
            {
                if (!m_CanvasGroupCache) m_CanvasGroupCache = GetComponent<CanvasGroup>();
                return m_CanvasGroupCache;
            }
        }

        public RectTransform m_RectTransform { get; private set; }

        /// <summary>
        /// Resolves the CanvasGroup, recovering a stale cached reference automatically.
        /// Returns false and logs a clear error if no CanvasGroup exists on this GameObject -
        /// never creates, attaches, or destroys a component as part of recovery.
        /// </summary>
        private bool TryGetCanvasGroup(out CanvasGroup _canvasGroup)
        {
            _canvasGroup = m_CanvasGroup;
            if (_canvasGroup) return true;

            Debug.LogError($"UIPage '{name}' in group '{m_GroupName}' has no CanvasGroup - it was removed or destroyed externally. Re-add a CanvasGroup component to restore show/hide behaviour.", this);
            return false;
        }


        /// <summary>
        /// Make sure to call base.Awake() in derived classes
        /// </summary>
        protected virtual void Awake()
        {
            m_IsTransitionPage = false;
            m_CanvasGroupCache = GetComponent<CanvasGroup>();
            m_RectTransform = GetComponent<RectTransform>();
            // Register into group
            if (!s_PageRegistry.TryGetValue(m_GroupName, out var list))
            {
                list = new List<UIPage>();
                s_PageRegistry[m_GroupName] = list;
            }
            if (!list.Contains(this))
                list.Add(this);

            // Prune destroyed carry-over entries (Unity Object equality treats these as null) so a
            // page destroyed in a previous play session cannot influence this session's scan.
            list.RemoveAll(_ => _ == null);

            // Show only if default AND no other page is already open in this group
            var hasOpened = list.Any(_ => _ != null && _ != this && _.m_IsOpened);
            var shouldShow = m_IsDefault && !hasOpened;

            SetShow(shouldShow);
            if (shouldShow)
                foreach (var pe in GetComponents<IPageShowEnd>())
                    pe.OnEndShowPage();
        }

        protected virtual void OnDestroy()
        {
            if (!s_PageRegistry.TryGetValue(m_GroupName, out var list)) return;
            list.Remove(this);

            // If this page was open, try to show the default page instead
            if (m_IsOpened)
            {
                var defaultPage = list.FirstOrDefault(_ => _ != null && _.m_IsDefault);
                if (defaultPage) defaultPage.SetShow(true);
            }
        }


        public void SetShow(bool _isShow)
        {
            if (!TryGetCanvasGroup(out var canvasGroup)) return;

            canvasGroup.alpha = _isShow ? 1 : 0;
            canvasGroup.interactable = _isShow;
            canvasGroup.blocksRaycasts = _isShow;
            m_IsOpened = _isShow;

        }
        public void OpenPage()
        {
            var token = this.GetCancellationTokenOnDestroy();
            OpenPage(_overrideTransitionInfo: null, _token: token);
        }

        public async UniTask OpenPageAsync(TransitionInfo _overrideTransitionInfo = null, CancellationToken _token = default)
        {
            if (m_IsOpened || m_IsTransitionPage) return;
            if (_token == default)
                _token = this.GetCancellationTokenOnDestroy();
            await TransitionPageAsync(this, _overrideTransitionInfo, _token);

        }


        public void OpenPage(TransitionInfo _overrideTransitionInfo = null, CancellationToken _token = default)
        {
            if (m_IsOpened || m_IsTransitionPage) return;
            if (_token == default)
                _token = this.GetCancellationTokenOnDestroy();
            OpenPageAsync(_overrideTransitionInfo, _token).Forget();

        }


        public async UniTask ShowPageAsync(int _milliseconds, bool _isShow, CancellationToken _token = default)
        {
            var targetAlpha = _isShow ? 1f : 0f;
            if (!TryGetCanvasGroup(out var canvasGroup)) return;


            if (_isShow)
            {
                foreach (var pe in GetComponents<IPageShowBegin>())
                    pe.OnBeginShowPage();
                OnBeginShowPageEvent?.Invoke();
            }
            else
            {
                foreach (var pe in GetComponents<IPageHideBegin>())
                    pe.OnBeginHidePage();
                OnBeginHidePageEvent?.Invoke();
            }


            await canvasGroup.DOFade(targetAlpha, _milliseconds / 1000f).SetUpdate(true).WithCancellation(_token);


            if (_isShow)
            {
                foreach (var pe in GetComponents<IPageShowEnd>())
                    pe.OnEndShowPage();
                OnEndShowPageEvent?.Invoke();
            }
            else
            {
                foreach (var pe in GetComponents<IPageHideEnd>())
                    pe.OnEndHidePage();
                OnEndHidePageEvent?.Invoke();
            }

            m_IsOpened = _isShow;
        }

        public void SetDefault(bool _value = true)
        {
            if (m_IsDefault == _value) return;
            m_IsDefault = _value;

            if (_value)
            {
                foreach (var p in GetPages(m_GroupName))
                    if (p != this) p.SetDefault(false);
            }
        }

        public void SetGroupName(string _groupName)
        {
            if (m_GroupName == _groupName) return;

            // Unregister from old group
            if (s_PageRegistry.TryGetValue(m_GroupName, out var oldList))
                oldList.Remove(this);

            m_GroupName = _groupName;

            // Register into new group
            if (!s_PageRegistry.TryGetValue(m_GroupName, out var newList))
            {
                newList = new List<UIPage>();
                s_PageRegistry[m_GroupName] = newList;
            }
            if (!newList.Contains(this))
                newList.Add(this);
        }


        #region Static Methods

        public static T GetPage<T>(string _groupName = "Default") where T : UIPage
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var pages = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
                return pages.FirstOrDefault(_ => _.m_GroupName == _groupName);
            }
#endif
            if (!s_PageRegistry.TryGetValue(_groupName, out var list)) return null;
            return list.OfType<T>().FirstOrDefault();
        }


        public static UIPage GetPageByName(string _name, string _groupName = "Default")
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var pages = Object.FindObjectsByType<UIPage>(FindObjectsSortMode.None);
                return pages.FirstOrDefault(_ => _.m_GroupName == _groupName && _.name == _name);
            }
#endif
            if (!s_PageRegistry.TryGetValue(_groupName, out var list))
            {
                Debug.LogError($"UIPage.GetPageByName: group '{_groupName}' has no registered pages.");
                return null;
            }

            var page = list.FirstOrDefault(_ => _ != null && _.name == _name);
            if (!page)
                Debug.LogError($"UIPage.GetPageByName: no page named '{_name}' in group '{_groupName}'.");

            return page;
        }




        public static UIPage GetCurrentPage(string _groupName = "Default")
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return Object.FindObjectsByType<UIPage>(FindObjectsSortMode.None)
                    .FirstOrDefault(_ => _.m_GroupName == _groupName && _.m_IsOpened);
#endif
            if (!s_PageRegistry.TryGetValue(_groupName, out var list)) return null;
            return list.FirstOrDefault(_ => _ != null && _.m_IsOpened);
        }

        public static UIPage[] GetPages(string _groupName = "Default")
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return Object.FindObjectsByType<UIPage>(FindObjectsSortMode.None)
                    .Where(_ => _.m_GroupName == _groupName).ToArray();
#endif
            if (!s_PageRegistry.TryGetValue(_groupName, out var list)) return Array.Empty<UIPage>();
            return list.Where(_ => _ != null).ToArray();
        }

        public static bool ResetUIPagesWithoutNotify(string _groupName)
        {
            var uiPages = GetPages(_groupName);

            foreach (var p in uiPages)
            {
                p.SetShow(p.m_IsDefault);

            }

            return true;
        }

        public static async UniTask TransitionPageAsync(UIPage _target, TransitionInfo _overrideTransition = null, CancellationToken _token = default)
        {
            if (!_target) return;
            if (_target.m_IsTransitionPage) return;

            var current = UIPage.GetCurrentPage(_target.m_GroupName);
            // Debug.Log($"TransitionPageAsync current:{current}  - target:{_target}");

            if (current == null)
            {
                _target.m_RectTransform.SetAsLastSibling();
                _target.SetShow(true);

                foreach (var pe in _target.GetComponents<IPageShowBegin>())
                    pe.OnBeginShowPage();
                _target.OnBeginShowPageEvent?.Invoke();

                foreach (var pe in _target.GetComponents<IPageShowEnd>())
                    pe.OnEndShowPage();
                _target.OnEndShowPageEvent?.Invoke();

                return;
            }

            await TransitionPageAsync(current, _target, _overrideTransition, _token);
        }



        public static async UniTask TransitionPageAsync(UIPage _current, UIPage _target, TransitionInfo _overrideTransitionInfo = null, CancellationToken _token = default)
        {

            var transitionInfo = _overrideTransitionInfo ?? _target.m_TransitionInfo;


            if (_current == null || _target == null || _current == _target || _current.m_IsTransitionPage || _target.m_IsTransitionPage)
                return;

            var targetGroup = _target.m_GroupName;
            var currentGroup = _current.m_GroupName;

            if (!s_TransitioningGroups.Add(targetGroup)) return;
            if (currentGroup != targetGroup && !s_TransitioningGroups.Add(currentGroup))
            {
                s_TransitioningGroups.Remove(targetGroup);
                return;
            }

            try
            {
                _current.m_IsTransitionPage = true;
                _target.m_IsTransitionPage = true;

                if (!_current.TryGetCanvasGroup(out var currentCanvasGroup) || !_target.TryGetCanvasGroup(out var targetCanvasGroup)) return;

                currentCanvasGroup.blocksRaycasts = false;
                targetCanvasGroup.blocksRaycasts = false;

                // Render target above every sibling under its parent - not just above _current -
                // so it reaches the true top of the group regardless of how many pages share the parent.
                _target.m_RectTransform.SetAsLastSibling();


                foreach (var pe in _target.GetComponents<IPageShowBegin>())
                    pe.OnBeginShowPage();
                _target.OnBeginShowPageEvent?.Invoke();

                foreach (var pe in _current.GetComponents<IPageHideBegin>())
                    pe.OnBeginHidePage();
                _current.OnBeginHidePageEvent?.Invoke();

                if (transitionInfo.m_Type == TransitionInfo.TransitionType.Fade)
                {
                    var duration = transitionInfo.m_Duration * 0.5f;


                    await UITransitionFade.Instance.FadeIn((int)duration, transitionInfo.m_FadeColor, _token);
                    _current.SetShow(false);
                    _target.SetShow(true);
                     _current.m_RectTransform.anchoredPosition = Vector2.zero;
                    _target.m_RectTransform.anchoredPosition = Vector2.zero;
                    await UITransitionFade.Instance.FadeOut((int)duration, transitionInfo.m_FadeColor, _token);


                }
                else if (transitionInfo.m_Type == TransitionInfo.TransitionType.CrossFade)
                {
                    _current.m_RectTransform.anchoredPosition = Vector2.zero;
                    _target.m_RectTransform.anchoredPosition = Vector2.zero;

                    currentCanvasGroup.interactable = false;
                    currentCanvasGroup.blocksRaycasts = false;

                    targetCanvasGroup.interactable = false;
                    targetCanvasGroup.blocksRaycasts = false;

                    await UniTask.WhenAll(
                        currentCanvasGroup.DOFade(0f, transitionInfo.m_Duration / 1000f).SetUpdate(true).WithCancellation(_token),
                        targetCanvasGroup.DOFade(1f, transitionInfo.m_Duration / 1000f).SetUpdate(true).WithCancellation(_token)
                    );
                    
                    
                    _current.SetShow(false);
                    _target.SetShow(true);



                }
                else if (transitionInfo.m_Type == TransitionInfo.TransitionType.Slide)
                {
                    var duration = Mathf.FloorToInt(transitionInfo.m_Duration * 0.5f);
                    _target.m_RectTransform.anchoredPosition = transitionInfo.m_StartPosition;
                    targetCanvasGroup.alpha = 1;
               

                    await UniTask.WhenAll(
                                    _current.m_RectTransform
                                        .DOAnchorPos(transitionInfo.m_EndPosition - transitionInfo.m_StartPosition, duration / 1000f)
                                        .SetEase(transitionInfo.m_Ease)
                                        .SetUpdate(true)
                                        .WithCancellation(_token),
                                    _target.m_RectTransform
                                        .DOAnchorPos(transitionInfo.m_EndPosition, duration / 1000f)
                                        .SetEase(transitionInfo.m_Ease)
                                        .SetUpdate(true)
                                        .WithCancellation(_token)
                                 );

                    _current.SetShow(false);
                    _target.SetShow(true);

                }

                foreach (var pe in _target.GetComponents<IPageShowEnd>())
                    pe.OnEndShowPage();
                _target.OnEndShowPageEvent?.Invoke();

                foreach (var pe in _current.GetComponents<IPageHideEnd>())
                    pe.OnEndHidePage();
                _current.OnEndHidePageEvent?.Invoke();
            }
            finally
            {
                if (_current) _current.m_IsTransitionPage = false;
                if (_target) _target.m_IsTransitionPage = false;

                s_TransitioningGroups.Remove(targetGroup);
                if (currentGroup != targetGroup) s_TransitioningGroups.Remove(currentGroup);
            }

        }

        #endregion


    }


    public interface IPageShowBegin
    {
        void OnBeginShowPage();
    }

    public interface IPageShowEnd
    {
        void OnEndShowPage();
    }

    public interface IPageHideBegin
    {
        void OnBeginHidePage();
    }

    public interface IPageHideEnd
    {
        void OnEndHidePage();
    }

}


