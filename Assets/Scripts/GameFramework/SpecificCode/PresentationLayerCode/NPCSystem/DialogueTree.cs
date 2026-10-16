using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "新对话树", menuName = "游戏数据/对话系统/对话树")]
public class DialogueTree : ScriptableObject
{
    [Header("对话树名称")]
    public string treeName;

    [Header("起始对话节点")]
    public DialogueNode startNode;

    [Header("任务相关节点映射（用于根据任务状态显示不同对话）")]
    public List<TaskDialogueNode> taskNodes;
}

[System.Serializable]
public class TaskDialogueNode
{
    public string questId;
    public DialogueNode node;
    public QuestTriggerType triggerType;
}