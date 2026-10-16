using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadPanel : BasePanel
{
    public Slider processViewSli;
    public Text progressText;  // 可选，显示百分比

    public override void OnInit()
    {
        // 自动查找组件（若未手动拖拽）
        if (processViewSli == null)
            processViewSli = transform.Find("processViewSli")?.GetComponent<Slider>();
        if (progressText == null)
            progressText = transform.Find("progressText")?.GetComponent<Text>();
        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        int sceneIndex = objs.Length > 0 ? (int)objs[0] : 1;

        // 如果传入的是 2，自动改为 1（因为 Build Settings 里只有 0 和 1）
        if (sceneIndex == 2)
        {
            Debug.LogWarning("场景索引 2 不存在，自动改为索引 1（GameScene）");
            sceneIndex = 1;
        }

        StartCoroutine(StartLoading(sceneIndex));
    }

    private IEnumerator StartLoading(int sceneIndex)
    {
        // 开始异步加载，禁止自动激活场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        // 模拟一个平滑的进度显示（同时监听真实 progress）
        float displayProgress = 0f;
        float targetProgress = 0f;

        while (!asyncLoad.isDone)
        {
            // 真实加载进度（最大到 0.9，之后需要手动完成）
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            targetProgress = realProgress;

            // 平滑移动 displayProgress 向 targetProgress
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 0.5f);

            // 更新 UI
            processViewSli.value = displayProgress;
            if (progressText != null)
                progressText.text = Mathf.RoundToInt(displayProgress * 100) + "%";

            // 当真实进度达到 0.9（即加载完成）且显示进度也到达 0.9 时，结束循环
            if (realProgress >= 0.99f && displayProgress >= 0.99f)
                break;

            yield return null;
        }

        // 最后冲刺到 100%
        while (displayProgress < 1f)
        {
            displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 1f);
            processViewSli.value = displayProgress;
            if (progressText != null)
                progressText.text = Mathf.RoundToInt(displayProgress * 100) + "%";
            yield return null;
        }

        // 激活场景
        asyncLoad.allowSceneActivation = true;

        // 等待一帧，确保场景切换完成
        yield return null;

        // 关闭加载面板，打开游戏主面板
        OnClose();
        UISystem.Instance?.OpenPanel<GamePanel>();
    }
}