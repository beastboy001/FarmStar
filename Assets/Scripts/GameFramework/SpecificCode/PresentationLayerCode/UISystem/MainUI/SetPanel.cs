using UnityEngine;
using UnityEngine.UI;

public class SetPanel : BasePanel
{
    [Header("UI组件")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectSlider;

    public Button continueButton;
    public Button exitButton;

    [Header("音量文字显示")]
    public Text masterVolumeText;
    public Text musicVolumeText;
    public Text soundEffectText;

    private AudioManager audioManager;
    private bool keepCursorVisibleOnClose = false;

    public override void OnInit()
    {
        base.OnInit();
        FindComponents();
        RegisterEvents();
    }

    private void FindComponents()
    {
        if (masterVolumeSlider == null)
        {
            Transform master = DeepFindTransform.DeepFindChild(transform, "MasterVolumeSlider");
            if (master != null) masterVolumeSlider = master.GetComponent<Slider>();
        }

        if (musicVolumeSlider == null)
        {
            Transform music = DeepFindTransform.DeepFindChild(transform, "MusicSlider");
            if (music != null) musicVolumeSlider = music.GetComponent<Slider>();
        }

        if (soundEffectSlider == null)
        {
            Transform sfx = DeepFindTransform.DeepFindChild(transform, "SoundEffectSlider");
            if (sfx != null) soundEffectSlider = sfx.GetComponent<Slider>();
        }

        if (continueButton == null)
        {
            Transform btn = DeepFindTransform.DeepFindChild(transform, "ContinueButton");
            if (btn != null) continueButton = btn.GetComponent<Button>();
        }

        if (exitButton == null)
        {
            Transform btn = DeepFindTransform.DeepFindChild(transform, "ExitButton");
            if (btn != null) exitButton = btn.GetComponent<Button>();
        }

        if (masterVolumeText == null)
        {
            Transform text = DeepFindTransform.DeepFindChild(transform, "MasterVolumeText");
            if (text != null) masterVolumeText = text.GetComponent<Text>();
        }

        if (musicVolumeText == null)
        {
            Transform text = DeepFindTransform.DeepFindChild(transform, "MusicVolumeText");
            if (text != null) musicVolumeText = text.GetComponent<Text>();
        }

        if (soundEffectText == null)
        {
            Transform text = DeepFindTransform.DeepFindChild(transform, "SoundEffectText");
            if (text != null) soundEffectText = text.GetComponent<Text>();
        }
    }

    private void RegisterEvents()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (soundEffectSlider != null)
            soundEffectSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButton);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButton);
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(value * 100) + "%";

        if (audioManager != null)
            audioManager.SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100) + "%";

        if (audioManager != null)
            audioManager.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (soundEffectText != null)
            soundEffectText.text = Mathf.RoundToInt(value * 100) + "%";

        if (audioManager != null)
            audioManager.SetSFXVolume(value);
    }

    private void OnContinueButton()
    {
        if (audioManager != null)
            audioManager.SaveVolumes();

        CloseSetPanel();
    }

    private void CloseSetPanel()
    {
        PlayerFarmAction player = FindObjectOfType<PlayerFarmAction>();
        if (player != null && !keepCursorVisibleOnClose)
            player.OnSetPanelClosed();

        OnClose();

        if (keepCursorVisibleOnClose)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnExitButton()
    {
        Debug.Log("退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        keepCursorVisibleOnClose = false;
        if (objs != null && objs.Length > 0 && objs[0] is bool)
            keepCursorVisibleOnClose = (bool)objs[0];

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        audioManager = AudioManager.Instance;
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();

        float master = AudioManager.DefaultVolume;
        float music = AudioManager.DefaultVolume;
        float sfx = AudioManager.DefaultVolume;

        if (audioManager != null)
        {
            // 只读取当前实际音量来更新 UI，不要重新 Apply，避免打开设置时声音突变
            audioManager.SyncFromMixer();
            master = audioManager.GetMasterVolume();
            music = audioManager.GetMusicVolume();
            sfx = audioManager.GetSFXVolume();
        }
        else
        {
            master = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, AudioManager.DefaultVolume);
            music = PlayerPrefs.GetFloat(AudioManager.MusicVolumeKey, AudioManager.DefaultVolume);
            sfx = PlayerPrefs.GetFloat(AudioManager.SfxVolumeKey, AudioManager.DefaultVolume);
        }

        SyncSliderUI(masterVolumeSlider, masterVolumeText, master);
        SyncSliderUI(musicVolumeSlider, musicVolumeText, music);
        SyncSliderUI(soundEffectSlider, soundEffectText, sfx);
    }

    private void SyncSliderUI(Slider slider, Text label, float value)
    {
        if (slider == null) return;
        value = Mathf.Clamp01(value);
        slider.SetValueWithoutNotify(value);
        if (label != null)
            label.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (soundEffectSlider != null)
            soundEffectSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueButton);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitButton);
    }
}