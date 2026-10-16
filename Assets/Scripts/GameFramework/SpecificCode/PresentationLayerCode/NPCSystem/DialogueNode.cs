using System.Collections.Generic;
using UnityEngine;

// 任务触发类型
public enum QuestTriggerType
{
    None,       // 无任务触发
    Accept,     // 接取任务
    Complete    // 完成任务（领奖）
}

[CreateAssetMenu(fileName = "新对话节点", menuName = "游戏数据/对话系统/对话节点")]
public class DialogueNode : ScriptableObject
{
    [Header("NPC说的话")]
    [TextArea(3, 5)]
    public string npcLine;

    [Header("玩家选项（最多2个）")]
    public List<PlayerOption> options;

    [Header("是否结束对话")]
    public bool endDialogue = false;

    [Header("下一个对话节点（当没有选项时自动跳转）")]
    public DialogueNode nextNode;
}

[System.Serializable]
public class PlayerOption
{
    [Header("选项文字")]
    public string optionText;

    [Header("下一个对话节点（设为null则结束对话）")]
    public DialogueNode nextNode;

    [Header("任务触发（可选）")]
    public string questId;
    public QuestTriggerType questTrigger;
}