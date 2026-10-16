using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-1000)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public const string MasterVolumeKey = "MasterVolume";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SFXVolume";
    public const float DefaultVolume = 0.75f;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("音量参数名称")]
    public string masterVolumeParam = "MasterVolume";
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";

    private float currentMasterVolume = DefaultVolume;
    private float currentMusicVolume = DefaultVolume;
    private float currentSFXVolume = DefaultVolume;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (Instance == this)
            ApplyVolumes();
    }

    /// <summary>
    /// 游戏启动时从存档加载并应用到 Mixer。
    /// </summary>
    public void InitializeVolumes()
    {
        LoadVolumes();
        ApplyVolumes();
    }

    /// <summary>
    /// 从 Mixer 读取当前实际音量，同步到内存（供设置界面显示，不会改声音）。
    /// </summary>
    public void SyncFromMixer()
    {
        if (TryGetMixerVolume(masterVolumeParam, out float master))
            currentMasterVolume = master;
        if (TryGetMixerVolume(musicVolumeParam, out float music))
            currentMusicVolume = music;
        if (TryGetMixerVolume(sfxVolumeParam, out float sfx))
            currentSFXVolume = sfx;
    }

    public void SetMasterVolume(float value)
    {
        currentMasterVolume = Mathf.Clamp01(value);
        ApplyVolume(masterVolumeParam, currentMasterVolume);
        PlayerPrefs.SetFloat(MasterVolumeKey, currentMasterVolume);
    }

    public void SetMusicVolume(float value)
    {
        currentMusicVolume = Mathf.Clamp01(value);
        ApplyVolume(musicVolumeParam, currentMusicVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, currentMusicVolume);
    }

    public void SetSFXVolume(float value)
    {
        currentSFXVolume = Mathf.Clamp01(value);
        ApplyVolume(sfxVolumeParam, currentSFXVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, currentSFXVolume);
    }

    public void SaveVolumes()
    {
        PlayerPrefs.Save();
    }

    public float GetMasterVolume() => currentMasterVolume;
    public float GetMusicVolume() => currentMusicVolume;
    public float GetSFXVolume() => currentSFXVolume;

    private void ApplyVolume(string paramName, float value)
    {
        if (audioMixer == null) return;

        float volumeDB = LinearToDecibels(value);
        audioMixer.SetFloat(paramName, volumeDB);
    }

    private void ApplyVolumes()
    {
        ApplyVolume(masterVolumeParam, currentMasterVolume);
        ApplyVolume(musicVolumeParam, currentMusicVolume);
        ApplyVolume(sfxVolumeParam, currentSFXVolume);
    }

    private void LoadVolumes()
    {
        currentMasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume));
        currentMusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume));
        currentSFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume));
    }

    private bool TryGetMixerVolume(string paramName, out float linear)
    {
        linear = DefaultVolume;
        if (audioMixer == null) return false;
        if (!audioMixer.GetFloat(paramName, out float db)) return false;

        linear = DecibelsToLinear(db);
        return true;
    }

    public static float LinearToDecibels(float linear)
    {
        return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
    }

    public static float DecibelsToLinear(float db)
    {
        if (db <= -80f) return 0f;
        return Mathf.Clamp01(Mathf.Pow(10f, db / 20f));
    }
}
