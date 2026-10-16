using System.Collections.Generic;
using UnityEngine;

// ========== 单个任务目标 ==========
[System.Serializable]
public class QuestGoalData
{
    public QuestGoalType type;
    public string targetId;
    public int requiredAmount;
}

// ========== 物品奖励 ==========
[System.Serializable]
public class ItemRewardData
{
    public string itemId;
    public int amount;
}

// ========== 任务奖励 ==========
[System.Serializable]
public class QuestRewardData
{
    public int gold;
    public List<ItemRewardData> items;
}

// ========== 主任务配置（ScriptableObject） ==========
[CreateAssetMenu(fileName = "新任务", menuName = "游戏数据/任务配置")]
public class QuestConfig : ScriptableObject
{
    [Header("基础信息")]
    public string questId;
    public string questName;
    [TextArea(3, 5)] public string description;
    public string category;

    [Header("前置任务")]
    public List<string> prerequisites;

    [Header("任务目标")]
    public List<QuestGoalData> goals;

    [Header("任务奖励")]
    public QuestRewardData reward;
}