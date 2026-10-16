using System;
using System.Collections.Generic;
using UnityEngine;

// ========== 任务状态枚举 ==========
public enum QuestStatus
{
    Locked = 0,
    CanAccept = 1,
    InProgress = 2,
    Completed = 3,
    Claimed = 4
}

// ========== 任务目标类型 ==========
public enum QuestGoalType
{
    TalkToNPC = 0,
    CollectItem = 1,
    HarvestCrop = 2,
    ReachLocation = 3,
    ClearObstacle = 4
}

// ========== 任务存档数据 ==========
[Serializable]
public class QuestSaveData
{
    public List<string> questIds = new List<string>();
    public List<int> statuses = new List<int>();
    public List<string> progressKeys = new List<string>();
    public List<int> progressValues = new List<int>();

    public void ToDictionaries(out Dictionary<string, QuestStatus> statusDict, out Dictionary<string, int> progressDict)
    {
        statusDict = new Dictionary<string, QuestStatus>();
        progressDict = new Dictionary<string, int>();

        for (int i = 0; i < questIds.Count && i < statuses.Count; i++)
            statusDict[questIds[i]] = (QuestStatus)statuses[i];

        for (int i = 0; i < progressKeys.Count && i < progressValues.Count; i++)
            progressDict[progressKeys[i]] = progressValues[i];
    }

    public void FromDictionaries(Dictionary<string, QuestStatus> statusDict, Dictionary<string, int> progressDict)
    {
        questIds.Clear();
        statuses.Clear();
        progressKeys.Clear();
        progressValues.Clear();

        foreach (var kv in statusDict)
        {
            questIds.Add(kv.Key);
            statuses.Add((int)kv.Value);
        }
        foreach (var kv in progressDict)
        {
            progressKeys.Add(kv.Key);
            progressValues.Add(kv.Value);
        }
    }

    public void Clear()
    {
        questIds.Clear();
        statuses.Clear();
        progressKeys.Clear();
        progressValues.Clear();
    }
}