using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static float MasterVolume { get; private set; } = 1.0f;

    private const string MASTER_VOLUME_KEY = "MasterVolume";

    private void Awake() {
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1));
    }

    public void QuitButton(){
        Application.Quit();
    }

    public static void SetMasterVolume(float value){
        MasterVolume = value;
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
    }
}
