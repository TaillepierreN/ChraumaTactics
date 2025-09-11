using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioClip[] _menuMusic;
    [SerializeField] private AudioClip[] _battleMusic;
    [SerializeField] private AudioSource _audioSource;
    public enum TypePlaying
    {
        Menu,
        Battle,
        None
    }
    private TypePlaying currentlyPlaying = TypePlaying.None;
    public static BackgroundMusic Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void Start()
    {
        StartMenuMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            StartBattleMusic();
        }
        else
        {
            StartMenuMusic();
        }
    }

    private void StartBattleMusic()
    {
        if (currentlyPlaying == TypePlaying.Battle)
            return;
        _audioSource.clip = _battleMusic[UnityEngine.Random.Range(0, _battleMusic.Length)];
        _audioSource.Play();
        currentlyPlaying = TypePlaying.Battle;
    }
    private void StartMenuMusic()
    {
        if (currentlyPlaying == TypePlaying.Menu)
            return;
        _audioSource.clip = _menuMusic[UnityEngine.Random.Range(0, _menuMusic.Length)];
        _audioSource.Play();
        currentlyPlaying = TypePlaying.Menu;
    }


}
