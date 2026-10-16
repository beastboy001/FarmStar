using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MaskPanel : BasePanel
{
    private Image backImage;
    private CanvasGroup canvasGroup;

    public override void OnInit()
    {
        base.OnInit();
        backImage = GetComponent<Image>();
        if (backImage == null) backImage = gameObject.AddComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = timer;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        InputPanel.ClosePanelsAfterMask();
        OnClose();
        UISystem.Instance?.OpenPanel<AnimationPanel>();
    }
}