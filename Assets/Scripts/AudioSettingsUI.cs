using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    void Start()
    {
        if (AudioManager.Instance == null) return;

        // 🔥 carrega valores atuais
        masterSlider.value = AudioManager.Instance.masterVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        musicSlider.value = AudioManager.Instance.musicVolume;

        // 🔥 conecta eventos
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
    }

    void OnMasterChanged(float value)
    {
        AudioManager.Instance.SetMaster(value);
    }

    void OnSFXChanged(float value)
    {
        AudioManager.Instance.SetSFX(value);
    }

    void OnMusicChanged(float value)
    {
        AudioManager.Instance.SetMusic(value);
    }
}