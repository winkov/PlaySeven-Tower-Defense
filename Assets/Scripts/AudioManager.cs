using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Volumes")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.6f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 🔥 CARREGA CONFIG
        LoadSettings();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        float finalVolume = masterVolume * sfxVolume * volumeMultiplier;
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    // =========================
    // SAVE / LOAD
    // =========================

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MASTER_VOL", masterVolume);
        PlayerPrefs.SetFloat("SFX_VOL", sfxVolume);
        PlayerPrefs.SetFloat("MUSIC_VOL", musicVolume);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MASTER_VOL", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_VOL", 0.6f);
        musicVolume = PlayerPrefs.GetFloat("MUSIC_VOL", 0.5f);
    }

    // =========================
    // SETTERS (usados pelo UI)
    // =========================

    public void SetMaster(float value)
    {
        masterVolume = value;
        SaveSettings();
    }

    public void SetSFX(float value)
    {
        sfxVolume = value;
        SaveSettings();
    }

    public void SetMusic(float value)
    {
        musicVolume = value;
        SaveSettings();
    }
}