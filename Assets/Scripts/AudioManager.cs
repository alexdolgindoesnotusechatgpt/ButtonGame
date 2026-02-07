using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSfx;

    void Awake()
    {
        // Singleton pattern to keep this alive between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayTitleMusic()
    {
        PlayMusic(titleMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    public void PlayClickSfx()
    {
        if (sfxSource != null && buttonClickSfx != null)
            sfxSource.PlayOneShot(buttonClickSfx);
    }

    // Helper to switch music tracks smoothly
    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        
        // Don't restart if it's already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}