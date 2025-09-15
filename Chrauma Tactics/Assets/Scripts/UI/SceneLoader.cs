using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using CT.Tools;

namespace CT.UI
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private CanvasGroup loadingGroup;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text progressValue;
        [SerializeField] private float fadeDuration = 0.25f;
        private NetworkSceneManager _hookedSM;
        private bool _isLoading;
        private string _nextScene;
        private bool _hooked;
        private bool _leaving = false;

        Coroutine _networkAnim;

        /// <summary>
        /// Singleton and loading screen setup
        /// </summary>
        void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            loadingGroup.alpha = 0f;
            SetInteractable(false);
            ResetProgress();

            TryHookNetSceneEvents();
            StartCoroutine(HookWhenReady());
        }

        void OnDestroy()
        {
            if (_hookedSM != null)
            {
                _hookedSM.OnLoadEventCompleted -= OnNetLoadCompleted;
                _hookedSM.OnSceneEvent -= OnNetSceneEvent;
            }

            _hookedSM = null;
            _hooked = false;
        }

        void TryHookNetSceneEvents()
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null) return;
            NetworkSceneManager sm = nm.SceneManager;
            if (sm == null) return;

            if (_hookedSM == sm && _hooked)
                return;

            if (_hookedSM != null)
            {
                _hookedSM.OnLoadEventCompleted -= OnNetLoadCompleted;
                _hookedSM.OnSceneEvent -= OnNetSceneEvent;
            }

            sm.OnLoadEventCompleted -= OnNetLoadCompleted;
            sm.OnLoadEventCompleted += OnNetLoadCompleted;

            sm.OnSceneEvent -= OnNetSceneEvent;
            sm.OnSceneEvent += OnNetSceneEvent;

            _hookedSM = sm;
            _hooked = true;
        }

        IEnumerator HookWhenReady()
        {
            // For client world in Multiplayer Play Mode
            while (true)
            {
                NetworkManager nm = NetworkManager.Singleton;
                if (nm != null && nm.SceneManager != null)
                {
                    TryHookNetSceneEvents();
                    yield break;
                }
                yield return null;
            }
        }

        #region  Offline / Local Loading
        /// <summary>
        /// Start loading scene
        /// </summary>
        /// <param name="sceneName"></param>
        public static void LoadOffline(string sceneName)
        {
            if (!Instance)
            {
                Debug.Log("SceneLoad is not in the scene, add the SceneLoader prefab");
                return;
            }
            Instance.QueueLocalLoad(sceneName);
        }

        /// <summary>
        /// start loading coroutine and protect from multiple load
        /// </summary>
        /// <param name="sceneName"></param>
        private void QueueLocalLoad(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;
            _nextScene = sceneName;
            StartCoroutine(LocalLoadRoutine());
        }

        /// <summary>
        /// Load scene animation
        /// Fade loading screen in
        /// load asynchronously the scene
        /// report progress to update slider and value
        /// activate scene when loading is done
        /// reset and fade out loading screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator LocalLoadRoutine()
        {
            yield return Fade(1f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Single);

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                /*this part is to smoth the progress bar up to 99*/
                float normalized = Mathf.InverseLerp(0f, 0.9f, operation.progress);
                float displayProgress = normalized * 0.99f;
                UpdateProgress(displayProgress);
                yield return null;
            }

            UpdateProgress(0.99f);
            yield return null;

            operation.allowSceneActivation = true;
            while (!operation.isDone)
                yield return null;

            UpdateProgress(1f);
            yield return null;

            yield return Fade(0f);

            _isLoading = false;
            ResetProgress();

        }
        #endregion
        #region Online / NGO load

        /// <summary>
        /// Start network loading visuals, called by NGO when starting a network scene load
        /// </summary>
        /// <param name="sceneName"></param>
        public static void BeginNetwork(string sceneName)
        {
            if (!Instance)
                return;
            Instance.StartNetworkVisuals(sceneName);
        }

        /// <summary>
        /// Stop network loading visuals, called by NGO when all clients have finished loading
        /// </summary>
        public static void CompleteNetwork()
        {
            if (!Instance)
                return;
            Instance.StopNetworkVisuals();
        }

        /// <summary>
        /// Start network loading screen animation
        /// </summary>
        /// <param name="sceneName"></param>
        void StartNetworkVisuals(string sceneName)
        {
            if (_isLoading)
                return;
            _isLoading = true;
            _nextScene = sceneName;
            if (_networkAnim != null)
                StopCoroutine(_networkAnim);
            _networkAnim = StartCoroutine(NetworkLoadVisuals());
        }

        /// <summary>
        /// Stop network loading screen animation
        /// </summary>
        void StopNetworkVisuals()
        {
            if (!_isLoading) return;
            if (_networkAnim != null) StopCoroutine(_networkAnim);
            StartCoroutine(NetworkCompleteRoutine());
        }

        /// <summary>
        /// Network loading animation
        /// </summary>
        /// <returns></returns>
        IEnumerator NetworkLoadVisuals()
        {
            yield return Fade(1f);
            float p = 0f;
            while (true)
            {
                p = Mathf.MoveTowards(p, 0.9f, Time.unscaledDeltaTime * 0.3f);
                UpdateProgress(p);
                yield return null;
            }
        }

        /// <summary>
        /// Network loading complete animation
        /// </summary>
        /// <returns></returns>
        IEnumerator NetworkCompleteRoutine()
        {
            UpdateProgress(1f);
            yield return null;
            yield return Fade(0f);
            _isLoading = false;
            ResetProgress();
        }

        /// <summary>
        /// NGO callback when all clients have finished loading the scene
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="clientsCompleted"></param>
        /// <param name="clientsTimedOut"></param>
        void OnNetLoadCompleted(string sceneName, LoadSceneMode mode,
                                System.Collections.Generic.List<ulong> clientsCompleted,
                                System.Collections.Generic.List<ulong> clientsTimedOut)
        {
            if (clientsTimedOut != null && clientsTimedOut.Count > 0)
                Debug.LogWarning($"Network load done with timeouts for {sceneName}.client timed out: {clientsTimedOut}");
            if (_isLoading)
                StopNetworkVisuals();
        }

        void OnNetSceneEvent(SceneEvent e)
        {
            /* When the server initiates a network LOAD for any scene, show visuals*/
            if (e.SceneEventType == SceneEventType.Load)
            {
                BeginNetwork(e.SceneName);
            }
        }
        #endregion

        /// <summary>
        /// Reset loading bar and value to 0;
        /// </summary>
        private void ResetProgress()
        {
            if (progressSlider)
                progressSlider.SetValueWithoutNotify(0f);
            if (progressValue)
                progressValue.text = "0%";
        }

        /// <summary>
        /// Upade loading value / bar visuals
        /// </summary>
        /// <param name="progressNbr"></param>
        void UpdateProgress(float progressNbr)
        {
            float p = Mathf.Clamp01(progressNbr);
            if (progressSlider)
                progressSlider.SetValueWithoutNotify(p);
            if (progressValue)
                progressValue.text = $"{Mathf.RoundToInt(p * 100f)}%";
        }

        /// <summary>
        /// Fade in or out the loading screen
        /// </summary>
        /// <param name="target">float alpha target</param>
        /// <returns></returns>
        private IEnumerator Fade(float target)
        {
            if (target > 0f)
                SetInteractable(true);

            float start = loadingGroup.alpha;
            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.unscaledDeltaTime;
                loadingGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);
                yield return null;
            }
            loadingGroup.alpha = target;

            if (Mathf.Approximately(target, 0f))
            {
                SetInteractable(false);
            }
        }

        /// <summary>
        /// allow canvas to be clicked or not
        /// </summary>
        /// <param name="isInteractable"></param>
        private void SetInteractable(bool isInteractable)
        {
            loadingGroup.blocksRaycasts = isInteractable;
            loadingGroup.interactable = isInteractable;
        }

        public void LeaveBattle()
        {
            if (_leaving)
                return;
            _leaving = true;

            StartCoroutine(Co_LeaveBattleRoutine());
        }

        private IEnumerator Co_LeaveBattleRoutine()
        {

            if (NetX.IsListening)
            {
                NetworkManager nm = NetX.NM;
                nm.Shutdown();
                yield return new WaitUntil(() => nm == null || !NetX.IsListening);
                yield return null;
            }
            LoadOffline("GameMenu");
        }
    }

}