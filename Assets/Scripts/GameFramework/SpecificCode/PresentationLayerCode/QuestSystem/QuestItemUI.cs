using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuestItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Text questTypeText;
    public Text questStateText;
    public Text questNameText;
    public GameObject selectedHighlight;

    private string questId;
    private QuestManager questManager;

    void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    public void Init(string id)
    {
        questId = id;
        questManager = QuestManager.Instance;
        UpdateDisplay();
        SetSelected(false);
    }

    public void UpdateDisplay()
    {
        if (questManager == null) return;

        var quest = questManager.GetQuestById(questId);
        if (quest == null) return;

        QuestStatus status = questManager.GetQuestStatus(questId);

        if (questTypeText != null) questTypeText.text = quest.category;
        if (questNameText != null) questNameText.text = quest.questName;
        if (questStateText != null)
        {
            questStateText.text = GetStatusText(status);
            questStateText.color = GetStatusColor(status);
        }
    }

    private string GetStatusText(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.CanAccept: return "可接取";
            case QuestStatus.InProgress: return "进行中";
            case QuestStatus.Completed: return "已完成";
            case QuestStatus.Locked: return "未解锁";
            case QuestStatus.Claimed: return "已领奖";
            default: return "";
        }
    }

    private Color GetStatusColor(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.CanAccept: return new Color (0.45f, 0.8f,0);
            case QuestStatus.InProgress: return new Color(0.8f, 0.5f, 0);
            case QuestStatus.Completed: return new Color(0.4f, 0.65f, 0.9f);
            default: return Color.gray;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null) selectedHighlight.SetActive(selected);
    }

    public string GetQuestId() => questId;

    private void OnClick()
    {
        if (MainShopPanel.Instance != null)
            MainShopPanel.Instance.SelectQuest(questId);
    }

    public void OnPointerClick(PointerEventData eventData) => OnClick();
}