using UnityEngine;

[CreateAssetMenu(fileName = "新NPC", menuName = "游戏数据/NPC配置")]
public class NPCConfig : ScriptableObject
{
    [Header("基础信息")]
    public string npcId;
    public string npcName;
    public Sprite npcIcon;

    [Header("对话树")]
    public DialogueTree dialogueTree;

    [Header("交互设置")]
    public float interactRange = 2f;
}