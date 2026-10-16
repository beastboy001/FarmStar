using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AnimationPanel : BasePanel
{
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Button skipButton;
    private CanvasGroup blackFadeGroup;
    private float cachedMusicVolume = -1f;
    private Coroutine fadeCoroutine;

    public override void OnInit()
    {
        base.OnInit();

        // 动态查找 RawImage（用于显示视频）
        Transform rawImageTrans = DeepFindTransform.DeepFindChild(transform, "RawImage");
        if (rawImageTrans != null)
        {
            rawImage = rawImageTrans.GetComponent<RawImage>();
            // 获取或添加 VideoPlayer 组件
            videoPlayer = rawImageTrans.GetComponent<VideoPlayer>();
            if (videoPlayer == null)
                videoPlayer = rawImageTrans.gameObject.AddComponent<VideoPlayer>();
        }
        else
        {
            Debug.LogError("AnimationPanel: 找不到 RawImage");
        }

        // 动态查找跳过按钮 ExitButton
        Transform btnTrans = DeepFindTransform.DeepFindChild(transform, "ExitButton");
        if (btnTrans != null)
        {
            skipButton = btnTrans.GetComponent<Button>();
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkip);
        }
        else
        {
            Debug.LogWarning("AnimationPanel: 找不到 ExitButton，跳过功能不可用");
        }

        // 动态查找黑幕遮罩 BlackFade
        Transform blackFadeTrans = DeepFindTransform.DeepFindChild(transform, "BlackFade");
        if (blackFadeTrans != null)
        {
            blackFadeGroup = blackFadeTrans.GetComponent<CanvasGroup>();
            if (blackFadeGroup == null)
                blackFadeGroup = blackFadeTrans.gameObject.AddComponent<CanvasGroup>();
            blackFadeGroup.alpha = 0f;
            blackFadeGroup.blocksRaycasts = false;
            blackFadeGroup.interactable = false;
            blackFadeTrans.gameObject.SetActive(false);
        }

        // 配置 VideoPlayer（如果还没有设置目标纹理）
        if (videoPlayer != null && rawImage != null)
        {
            // 设置 VideoPlayer 输出到 RawImage
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = null; // 让 VideoPlayer 自动创建 RenderTexture
                                              // 注意：需要提前在 Inspector 或代码中指定 videoClip
                                              // 这里假设视频资源路径为 "Videos/Opening" 或在 Inspector 中已设置
                                              // 如果没有设置，可以通过代码加载：videoPlayer.clip = Resources.Load<VideoClip>("Videos/Opening");

            videoPlayer.isLooping = false;
            videoPlayer.playOnAwake = false;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        MuteBackgroundMusic();

        if (videoPlayer != null)
        {
            // 准备并播放视频
            if (videoPlayer.clip == null)
            {
                Debug.LogError("AnimationPanel: 未设置视频片段，请检查 VideoPlayer 的 VideoClip");
                // 没有视频直接跳过
                CloseAndOpenLoadPanel();
                return;
            }
            videoPlayer.Prepare();
        }
        else
        {
            CloseAndOpenLoadPanel();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // 视频准备完成，开始播放
        if (rawImage != null && vp.texture != null)
            rawImage.texture = vp.texture;
        vp.Play();
    }

    private void OnSkip()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
        CloseAndOpenLoadPanel();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        CloseAndOpenLoadPanel();
    }

    private void CloseAndOpenLoadPanel()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        RestoreBackgroundMusic();
        fadeCoroutine = StartCoroutine(CloseWithBlackFade());
    }

    private IEnumerator CloseWithBlackFade()
    {
        yield return FadeBlack(0f, 1f, 0.35f);

        OnClose();
        UISystem.Instance?.OpenPanel<LoadPanel>();

        yield return FadeBlack(1f, 0f, 0.35f);
        if (blackFadeGroup != null)
            blackFadeGroup.gameObject.SetActive(false);

        fadeCoroutine = null;
    }

    private IEnumerator FadeBlack(float from, float to, float duration)
    {
        if (blackFadeGroup == null)
            yield break;

        blackFadeGroup.gameObject.SetActive(true);
        blackFadeGroup.blocksRaycasts = true;
        blackFadeGroup.interactable = false;
        blackFadeGroup.alpha = from;

        if (duration <= 0f)
        {
            blackFadeGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            blackFadeGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        blackFadeGroup.alpha = to;
    }

    private void MuteBackgroundMusic()
    {
        if (AudioManager.Instance == null) return;

        cachedMusicVolume = AudioManager.Instance.GetMusicVolume();
        AudioManager.Instance.SetMusicVolume(0f);
    }

    private void RestoreBackgroundMusic()
    {
        if (AudioManager.Instance == null) return;
        if (cachedMusicVolume < 0f) return;

        AudioManager.Instance.SetMusicVolume(cachedMusicVolume);
        cachedMusicVolume = -1f;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnSkip);

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        RestoreBackgroundMusic();
    }
}