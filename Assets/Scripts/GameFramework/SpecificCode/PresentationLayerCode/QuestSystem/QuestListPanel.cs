using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QuestListPanel : MonoBehaviour
{
    [Header("任务项预制体")]
    public GameObject questItemPrefab;

    private List<QuestItemUI> questItems = new List<QuestItemUI>();
    private bool initialized = false;
    private VerticalLayoutGroup layoutGroup;
    private RectTransform contentRect;
    private bool isRefreshing = false;

    public static QuestListPanel Instance { get; private set; }

    void Awake()
    {
        Debug.Log("=== QuestListPanel.Awake() ===");
        Instance = this;
        layoutGroup = GetComponent<VerticalLayoutGroup>();
        contentRect = GetComponent<RectTransform>();

        ContentSizeFitter sizeFitter = GetComponent<ContentSizeFitter>();
        if (sizeFitter != null) Destroy(sizeFitter);
    }

    public void RefreshTaskPanel()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        Debug.Log("=== QuestListPanel.RefreshTaskPanel() ===");
        StartCoroutine(InitializeAndRefresh());
    }

    private IEnumerator InitializeAndRefresh()
    {
        // 等待 QuestManager 准备好
        int waitCount = 0;
        while (QuestManager.Instance == null)
        {
            waitCount++;
            if (waitCount > 50)
            {
                Debug.LogError("QuestManager 初始化超时");
                isRefreshing = false;
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }

        // 等待任务配置加载
        waitCount = 0;
        var allQuests = QuestManager.Instance.GetAllQuests();
        while ((allQuests == null || allQuests.Count == 0) && waitCount < 50)
        {
            waitCount++;
            yield return new WaitForSeconds(0.1f);
            allQuests = QuestManager.Instance.GetAllQuests();
        }

        if (allQuests == null || allQuests.Count == 0)
        {
            Debug.LogError("没有加载到任务配置");
            isRefreshing = false;
            yield break;
        }

        Debug.Log($"获取到 {allQuests.Count} 个任务配置");

        // 重要：先彻底清空所有子物体，防止重复
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        questItems.Clear();

        if (questItemPrefab == null)
        {
            Debug.LogError("questItemPrefab 为空！请拖入 QuestItem 预制体");
            isRefreshing = false;
            yield break;
        }

        // 创建任务项
        foreach (var quest in allQuests)
        {
            Debug.Log($"创建任务UI: {quest.questName} (ID: {quest.questId})");
            GameObject obj = Instantiate(questItemPrefab, transform);
            QuestItemUI itemUI = obj.GetComponent<QuestItemUI>();
            if (itemUI != null)
            {
                itemUI.Init(quest.questId);
                questItems.Add(itemUI);
            }
        }

        Debug.Log($"共创建 {questItems.Count} 个任务UI");

        // 强制刷新布局
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
        }

        // 计算并设置 Content 高度
        float totalHeight = 0;
        float spacing = layoutGroup != null ? layoutGroup.spacing : 8;
        foreach (var item in questItems)
        {
            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null)
            {
                totalHeight += rect.rect.height;
                totalHeight += spacing;
            }
        }
        if (totalHeight > 0) totalHeight -= spacing;
        if (totalHeight < 100) totalHeight = 200;

        if (contentRect != null)
        {
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            contentRect.anchoredPosition = new Vector2(0, 0);
            Debug.Log($"Content 高度设置为 {totalHeight}");
        }

        Canvas.ForceUpdateCanvases();

        initialized = true;
        isRefreshing = false;
        Debug.Log("任务列表初始化完成");

        // 订阅任务数据变化事件
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestDataChanged += RefreshAllDisplay;
    }

    public void RefreshAllDisplay()
    {
        if (!initialized) return;
        foreach (var item in questItems)
            item.UpdateDisplay();
    }

    public void SetSelectedQuest(string questId)
    {
        foreach (var item in questItems)
            item.SetSelected(item.GetQuestId() == questId);
    }
}