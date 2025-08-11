using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CT.UI
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private CanvasGroup loadingGroup;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text progressValue;
        [SerializeField] private float fadeDuration = 0.25f;

        bool _isLoading;
        string _nextScene;

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
        }

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
        /// Start loading scene
        /// </summary>
        /// <param name="sceneName"></param>
        public static void Load(string sceneName)
        {
            if (!Instance)
            {
                Debug.Log("SceneLoad is not in the scene, add the SceneLoader prefab");
                return;
            }
            Instance.QueueLoad(sceneName);
        }

        /// <summary>
        /// start loading coroutine and protect from multiple load
        /// </summary>
        /// <param name="sceneName"></param>
        private void QueueLoad(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;
            _nextScene = sceneName;
            StartCoroutine(LoadRoutine());
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
        private IEnumerator LoadRoutine()
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
    }

}