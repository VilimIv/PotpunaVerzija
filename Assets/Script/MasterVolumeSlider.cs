using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MasterVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private TMP_Text volumeDisplay;

    private void Start() {
        masterVolumeSlider.SetValueWithoutNotify(SettingsManager.MasterVolume);
        mainAudioMixer.SetFloat("MasterVolume", masterVolumeSlider.value);
        volumeDisplay.text = (masterVolumeSlider.value * 100).ToString("F0");
    }

    public void OnValueUpdate(float value){
        SettingsManager.SetMasterVolume(value);
        mainAudioMixer.SetFloat("MasterVolume", value);
        volumeDisplay.text = (value * 100).ToString("F0");
    }
}
