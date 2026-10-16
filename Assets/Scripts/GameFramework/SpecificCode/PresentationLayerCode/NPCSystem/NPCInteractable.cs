using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NPCInteractable : MonoBehaviour
{
    [Header("NPC配置")]
    public NPCConfig npcConfig;

    [Header("头顶名字Canvas")]
    public GameObject nameCanvas;
    public Text nameText;

    private Transform player;
    private bool isPlayerInRange = false;
    private HashSet<string> triggeredTaskNodes = new HashSet<string>();

    private GamePanel GetGamePanel()
    {
        return FindObjectOfType<GamePanel>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (nameText != null && npcConfig != null)
            nameText.text = npcConfig.npcName;

        if (nameCanvas != null)
            nameCanvas.SetActive(true);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= (npcConfig != null ? npcConfig.interactRange : 2f);

        if (wasInRange != isPlayerInRange)
        {
            GamePanel gamePanel = GetGamePanel();
            if (gamePanel != null)
            {
                if (isPlayerInRange)
                    gamePanel.ShowInteractTip(npcConfig.npcName);
                else
                    gamePanel.HideInteractTip();
            }
        }

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        Debug.Log($"NPCInteractable.Interact() 被调用，NPC: {gameObject.name}");

        if (npcConfig == null)
        {
            Debug.LogError($"NPC {gameObject.name} 没有配置 NPCConfig");
            return;
        }

        if (npcConfig.dialogueTree == null)
        {
            Debug.LogError($"NPC {gameObject.name} 的对话树为空");
            return;
        }

        DialogueNode startNode = GetStartNode();
        Debug.Log($"获取到的起始节点: {(startNode != null ? startNode.name : "null")}");

        if (startNode != null && UISystem.Instance != null)
        {
            GamePanel gamePanel = GetGamePanel();
            if (gamePanel != null)
                gamePanel.HideInteractTip();

            Debug.Log("准备打开 DialogPanel");
            UISystem.Instance.OpenPanel<DialogPanel>(startNode, this, npcConfig);
        }
        else
        {
            Debug.LogError("无法打开对话面板：startNode 或 UISystem.Instance 为空");
        }
    }

    private DialogueNode GetStartNode()
    {
        if (npcConfig == null || npcConfig.dialogueTree == null)
            return null;

        var tree = npcConfig.dialogueTree;
        var questManager = QuestManager.Instance;

        if (questManager == null)
            return tree.startNode;

        // ========== 修复：使用 questId 而不是整个 QuestData 对象 ==========

        // 1. 优先检查是否有已完成待领奖的任务（状态为 Completed）
        foreach (var taskNode in tree.taskNodes)
        {
            if (taskNode == null || taskNode.node == null) continue;

            // 修复：直接使用 taskNode.questId（string），不需要 Find
            var status = questManager.GetQuestStatus(taskNode.questId);

            if (taskNode.triggerType == QuestTriggerType.Complete && status == QuestStatus.Completed)
            {
                Debug.Log($"找到已完成待领奖任务节点: {taskNode.questId} -> {taskNode.node.name}");
                return taskNode.node;
            }
        }

        // 2. 检查进行中的任务（状态为 InProgress）
        foreach (var taskNode in tree.taskNodes)
        {
            if (taskNode == null || taskNode.node == null) continue;

            var status = questManager.GetQuestStatus(taskNode.questId);

            if (taskNode.triggerType == QuestTriggerType.Accept && status == QuestStatus.InProgress)
            {
                Debug.Log($"找到进行中任务节点: {taskNode.questId} -> {taskNode.node.name}");
                return taskNode.node;
            }
        }

        // 3. 检查可接取的任务（状态为 CanAccept）
        foreach (var taskNode in tree.taskNodes)
        {
            if (taskNode == null || taskNode.node == null) continue;

            var status = questManager.GetQuestStatus(taskNode.questId);

            if (taskNode.triggerType == QuestTriggerType.Accept && status == QuestStatus.CanAccept)
            {
                Debug.Log($"找到可接取任务节点: {taskNode.questId} -> {taskNode.node.name}");
                return taskNode.node;
            }
        }

        // 4. 检查是否所有任务都已领奖（状态为 Claimed）
        bool allClaimed = true;
        foreach (var taskNode in tree.taskNodes)
        {
            if (taskNode == null || string.IsNullOrEmpty(taskNode.questId) || taskNode.questId == "all_completed") continue;

            var status = questManager.GetQuestStatus(taskNode.questId);
            if (status != QuestStatus.Claimed)
            {
                allClaimed = false;
                break;
            }
        }

        if (allClaimed)
        {
            var finalNode = tree.taskNodes.Find(n => n.questId == "all_completed");
            if (finalNode != null && finalNode.node != null)
            {
                Debug.Log($"所有任务已完成，显示自由探索节点: {finalNode.node.name}");
                return finalNode.node;
            }
        }

        // 5. 默认返回起始节点
        Debug.Log($"没有找到特殊节点，返回起始节点: {tree.startNode.name}");
        return tree.startNode;
    }

    public void MarkTaskNodeTriggered(DialogueNode node)
    {
        if (node != null)
            triggeredTaskNodes.Add(node.name);
    }

    public void OnDialogClosed()
    {
        GamePanel gamePanel = GetGamePanel();
        if (gamePanel != null && isPlayerInRange)
        {
            gamePanel.ShowInteractTip(npcConfig.npcName);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (npcConfig != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, npcConfig.interactRange);
        }
    }
}