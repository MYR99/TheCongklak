using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip seedPickup;
    public AudioClip seedDrop;
    public AudioClip extraTurn;
    public AudioClip buttonClick;
    public AudioClip gameWin;
    public AudioClip gameLose;
    public AudioClip seedCapture;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void StartMusic()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) { sfxSource.PlayOneShot(clip); }
    }
    
    public void SetMusicVolume(float volume) { musicSource.volume = volume; }
    public void SetSFXVolume(float volume) { sfxSource.volume = volume; }
    public void ToggleSound(bool isEnabled) { musicSource.mute = !isEnabled; sfxSource.mute = !isEnabled; }
}