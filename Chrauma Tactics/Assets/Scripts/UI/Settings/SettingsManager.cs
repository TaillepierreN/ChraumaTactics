using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Mixer & Sliders")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Mixer Exposed Param Names (dB)")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string bgmParam = "BGMVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [Header("Defaults (linear 0..1)")]
    [Range(0f, 1f)][SerializeField] private float defaultMaster = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultBGM = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultSFX = 1f;

    [Header("PlayerPrefs keys")]
    private const string KEY_MASTER = "vol_master";
    private const string KEY_BGM = "vol_bgm";
    private const string KEY_SFX = "vol_sfx";

    void Awake()
    {
        SetupSlider(masterSlider);
        SetupSlider(bgmSlider);
        SetupSlider(sfxSlider);

        float master = PlayerPrefs.HasKey(KEY_MASTER) ? PlayerPrefs.GetFloat(KEY_MASTER) : defaultMaster;
        float bgm = PlayerPrefs.HasKey(KEY_BGM) ? PlayerPrefs.GetFloat(KEY_BGM) : defaultBGM;
        float sfx = PlayerPrefs.HasKey(KEY_SFX) ? PlayerPrefs.GetFloat(KEY_SFX) : defaultSFX;

        ApplyAll(master, bgm, sfx);

        masterSlider.SetValueWithoutNotify(master);
        bgmSlider.SetValueWithoutNotify(bgm);
        sfxSlider.SetValueWithoutNotify(sfx);
    }

    private void ApplyAll(float master, float bgm, float sfx)
    {
        ApplyLinear(masterParam, master);
        ApplyLinear(bgmParam, bgm);
        ApplyLinear(sfxParam, sfx);
    }

    private void OnEnable()
    {
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnMasterChanged(float value)
    {
        ApplyLinear(masterParam, value);
        PlayerPrefs.SetFloat(KEY_MASTER, value);
    }

    private void OnBGMChanged(float value)
    {
        ApplyLinear(bgmParam, value);
        PlayerPrefs.SetFloat(KEY_BGM, value);
    }

    private void OnSFXChanged(float value)
    {
        ApplyLinear(sfxParam, value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }

    void OnActiveSceneChanged(Scene _, Scene __)
    {
        StartCoroutine(ReapplyAtEndOfFrame());
    }

    IEnumerator ReapplyAtEndOfFrame()
    {
        yield return null;
        float master = masterSlider ? masterSlider.value : PlayerPrefs.GetFloat(KEY_MASTER, defaultMaster);
        float bgm = bgmSlider ? bgmSlider.value : PlayerPrefs.GetFloat(KEY_BGM, defaultBGM);
        float sfx = sfxSlider ? sfxSlider.value : PlayerPrefs.GetFloat(KEY_SFX, defaultSFX);
        ApplyAll(master, bgm, sfx);
    }

    private void ApplyLinear(string param, float linear)
    {
        if (linear <= 0.0001f)
        {
            mixer.SetFloat(param, -80f);
            return;
        }
        float dB = Mathf.Log10(linear) * 20f;
        if (dB < -80f)
            dB = -80f;

        mixer.SetFloat(param, dB);
    }

    private void SetupSlider(Slider slider)
    {
        if (!slider)
            return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }
}
