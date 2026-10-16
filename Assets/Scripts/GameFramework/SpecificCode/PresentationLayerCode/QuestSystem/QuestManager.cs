using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("任务配置（手动拖入）")]
    public List<QuestConfig> allQuestConfigs;

    private Dictionary<string, QuestConfig> questConfigDict = new Dictionary<string, QuestConfig>();
    private Dictionary<string, QuestStatus> questStatus = new Dictionary<string, QuestStatus>();
    private Dictionary<string, int> questProgress = new Dictionary<string, int>();

    public event Action OnQuestDataChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadQuestConfigs();
    }

    void Start()
    {
        LoadFromGameData();
    }

    private void LoadQuestConfigs()
    {
        // 从手动拖入的 List 中加载
        if (allQuestConfigs == null || allQuestConfigs.Count == 0)
        {
            Debug.LogWarning("没有拖入任务配置，请将 QuestConfig 拖到 QuestManager 的 allQuestConfigs 列表中");
            return;
        }

        questConfigDict.Clear();
        foreach (var config in allQuestConfigs)
        {
            if (config != null && !questConfigDict.ContainsKey(config.questId))
            {
                questConfigDict[config.questId] = config;
            }
        }

        Debug.Log($"加载 {questConfigDict.Count} 个任务配置（来自 ScriptableObject）");
    }

    // ========== 公共接口 ==========

    public List<QuestConfig> GetAllQuests() => questConfigDict.Values.ToList();

    public QuestConfig GetQuestById(string questId)
    {
        return questConfigDict.ContainsKey(questId) ? questConfigDict[questId] : null;
    }

    public QuestStatus GetQuestStatus(string questId)
    {
        if (!questConfigDict.ContainsKey(questId)) return QuestStatus.Locked;
        if (questStatus.ContainsKey(questId)) return questStatus[questId];

        var quest = questConfigDict[questId];
        if (quest.prerequisites != null && quest.prerequisites.Count > 0)
        {
            foreach (string prereqId in quest.prerequisites)
            {
                var prereqStatus = GetQuestStatus(prereqId);
                if (prereqStatus != QuestStatus.Claimed && prereqStatus != QuestStatus.Completed)
                    return QuestStatus.Locked;
            }
        }
        return QuestStatus.CanAccept;
    }

    public string GetQuestProgressText(string questId)
    {
        if (!questConfigDict.ContainsKey(questId)) return "";
        if (!questStatus.ContainsKey(questId)) return "";

        var status = questStatus[questId];
        if (status == QuestStatus.Completed) return "✅ 任务完成，点击领取奖励";
        if (status == QuestStatus.Claimed) return "✓ 奖励已领取";
        if (status != QuestStatus.InProgress) return "";

        var quest = questConfigDict[questId];
        string text = "";
        for (int i = 0; i < quest.goals.Count; i++)
        {
            var goal = quest.goals[i];
            int current = GetGoalProgress(questId, i);
            text += GetGoalDisplayText(goal) + $" {current}/{goal.requiredAmount}";
            if (i < quest.goals.Count - 1) text += "\n";
        }
        return text;
    }

    public QuestRewardData GetQuestReward(string questId)
    {
        return questConfigDict.ContainsKey(questId) ? questConfigDict[questId].reward : null;
    }

    // ========== 内部方法 ==========

    private int GetGoalProgress(string questId, int goalIndex)
    {
        string key = $"{questId}_{goalIndex}";
        return questProgress.ContainsKey(key) ? questProgress[key] : 0;
    }

    private void SetGoalProgress(string questId, int goalIndex, int value)
    {
        string key = $"{questId}_{goalIndex}";
        questProgress[key] = value;
    }

    private string GetGoalDisplayText(QuestGoalData goal)
    {
        switch (goal.type)
        {
            case QuestGoalType.TalkToNPC: return $"与 {goal.targetId} 对话";
            case QuestGoalType.CollectItem: return $"收集 {goal.targetId}";
            case QuestGoalType.HarvestCrop: return $"收获 {goal.targetId}";
            case QuestGoalType.ReachLocation: return $"前往 {goal.targetId}";
            case QuestGoalType.ClearObstacle: return $"清除 {goal.targetId}";
            default: return "";
        }
    }

    public bool AcceptQuest(string questId)
    {
        if (!questConfigDict.ContainsKey(questId)) return false;
        if (questStatus.ContainsKey(questId)) return false;
        if (GetQuestStatus(questId) != QuestStatus.CanAccept) return false;

        questStatus[questId] = QuestStatus.InProgress;
        var quest = questConfigDict[questId];
        for (int i = 0; i < quest.goals.Count; i++)
            SetGoalProgress(questId, i, 0);

        OnQuestDataChanged?.Invoke();
        Debug.Log($"✅ 接取任务：{quest.questName}");
        return true;
    }

    public void UpdateProgress(string questId, QuestGoalType type, string targetId, int amount = 1)
    {
        // 如果 questId 为空，遍历所有进行中的任务
        if (string.IsNullOrEmpty(questId))
        {
            var inProgress = questStatus.Where(kv => kv.Value == QuestStatus.InProgress).Select(kv => kv.Key).ToList();
            foreach (var qid in inProgress)
                UpdateProgress(qid, type, targetId, amount);
            return;
        }

        if (!questConfigDict.ContainsKey(questId)) return;
        if (!questStatus.ContainsKey(questId)) return;
        if (questStatus[questId] != QuestStatus.InProgress) return;

        var quest = questConfigDict[questId];
        bool updated = false;

        for (int i = 0; i < quest.goals.Count; i++)
        {
            var goal = quest.goals[i];
            // ========== 关键：这里比较 targetId ==========
            if (goal.type == type && goal.targetId == targetId)
            {
                int current = GetGoalProgress(questId, i);
                int newProgress = Mathf.Min(goal.requiredAmount, current + amount);
                if (newProgress != current)
                {
                    SetGoalProgress(questId, i, newProgress);
                    updated = true;
                }
            }
        }

        if (!updated) return;

        // 检查是否所有目标都完成
        bool allCompleted = true;
        for (int i = 0; i < quest.goals.Count; i++)
        {
            if (GetGoalProgress(questId, i) < quest.goals[i].requiredAmount)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted && questStatus[questId] == QuestStatus.InProgress)
        {
            questStatus[questId] = QuestStatus.Completed;
            Debug.Log($"🎉 任务完成：{quest.questName}");
        }

        OnQuestDataChanged?.Invoke();
    }

    public bool ClaimReward(string questId)
    {
        if (!questConfigDict.ContainsKey(questId)) return false;
        if (!questStatus.ContainsKey(questId)) return false;
        if (questStatus[questId] != QuestStatus.Completed) return false;

        var quest = questConfigDict[questId];
        if (quest.reward != null)
        {
            if (quest.reward.gold > 0 && InventoryManager.Instance != null)
                InventoryManager.Instance.AddGold(quest.reward.gold);
            if (quest.reward.items != null)
            {
                foreach (var item in quest.reward.items)
                    InventoryManager.Instance?.AddItem(item.itemId, item.amount);
            }
        }

        questStatus[questId] = QuestStatus.Claimed;
        OnQuestDataChanged?.Invoke();
        Debug.Log($"💰 领取奖励：{quest.questName}");
        return true;
    }

    // ========== 存档相关 ==========

    public void SaveToGameData()
    {
        int slotIndex = ChoicePanel.currentSelectedSlot;
        if (slotIndex >= 0 && slotIndex < SaveManager.gameDatas.Length)
        {
            var gameData = SaveManager.gameDatas[slotIndex];
            if (gameData != null && gameData.questSaveData != null)
            {
                gameData.questSaveData.FromDictionaries(questStatus, questProgress);
            }
        }
    }

    public void LoadFromGameData()
    {
        int slotIndex = ChoicePanel.currentSelectedSlot;
        if (slotIndex >= 0 && slotIndex < SaveManager.gameDatas.Length)
        {
            var gameData = SaveManager.gameDatas[slotIndex];
            if (gameData?.questSaveData != null)
            {
                gameData.questSaveData.ToDictionaries(out var statusDict, out var progressDict);
                LoadQuestData(statusDict, progressDict);
            }
        }
    }

    public void LoadQuestData(Dictionary<string, QuestStatus> statusDict, Dictionary<string, int> progressDict)
    {
        if (statusDict != null)
        {
            questStatus.Clear();
            foreach (var kv in statusDict)
                questStatus[kv.Key] = kv.Value;
        }

        if (progressDict != null)
        {
            questProgress.Clear();
            foreach (var kv in progressDict)
                questProgress[kv.Key] = kv.Value;
        }

        OnQuestDataChanged?.Invoke();
        Debug.Log($"任务数据已加载，共 {questStatus.Count} 个任务记录");
    }

    public void ClearAllQuestData()
    {
        questStatus.Clear();
        questProgress.Clear();
        int slotIndex = ChoicePanel.currentSelectedSlot;
        if (slotIndex >= 0 && slotIndex < SaveManager.gameDatas.Length)
        {
            var gameData = SaveManager.gameDatas[slotIndex];
            if (gameData != null && gameData.questSaveData != null)
                gameData.questSaveData.Clear();
        }
        OnQuestDataChanged?.Invoke();
    }
}