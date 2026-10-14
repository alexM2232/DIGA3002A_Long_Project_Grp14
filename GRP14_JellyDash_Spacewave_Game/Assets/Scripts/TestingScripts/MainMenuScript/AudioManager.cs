
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip mainMenuMusic;
    public AudioClip jellyDashMusic;
    public AudioClip spaceWavesMusic;
    public AudioClip winMusic;

    [Header("Sound Effects")]
    public AudioClip checkpointSFX;
    public AudioClip damageSFX;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length == 0)
            musicSource = gameObject.AddComponent<AudioSource>();
        else
            musicSource = sources[0];

        if (sources.Length < 2)
            sfxSource = gameObject.AddComponent<AudioSource>();
        else
            sfxSource = sources[1];

        musicSource.playOnAwake = false;
        musicSource.loop = true;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Music clip is missing.");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayJellyDashMusic()
    {
        PlayMusic(jellyDashMusic);
    }

    public void PlaySpaceWavesMusic()
    {
        PlayMusic(spaceWavesMusic);
    }

    public void PlayWinMusic()
    {
        if (winMusic == null)
        {
            Debug.LogWarning("AudioManager: Win music is missing.");
            return;
        }

        musicSource.Stop();
        musicSource.clip = winMusic;
        musicSource.loop = false;
        musicSource.Play();
    }

    public void PlayCheckpointSFX()
    {
        if (checkpointSFX != null)
            sfxSource.PlayOneShot(checkpointSFX);
    }

    public void PlayDamageSFX()
    {
        if (damageSFX != null)
            sfxSource.PlayOneShot(damageSFX);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
