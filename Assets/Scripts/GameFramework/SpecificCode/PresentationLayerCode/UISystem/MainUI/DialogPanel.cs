using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class DialogPanel : BasePanel
{
    [Header("对话面板UI组件")]
    public Text npcNameText;
    public Text dialogueText;
    public Button continueButton;
    public Transform answerGeneration;
    public GameObject answerPrefab;

    private List<GameObject> currentAnswerButtons = new List<GameObject>();
    private DialogueNode currentNode;
    private NPCInteractable currentNPC;
    private NPCConfig currentConfig;

    private PlayerMoveControl2 playerMove;
    private CinemachineFreeLook freeLookCamera;
    private bool wasCursorVisible;
    private CursorLockMode wasCursorLockMode;
    private GamePanel cachedGamePanel;
    private PlayerFarmAction playerFarmAction;  // 新增

    public override void OnInit()
    {
        base.OnInit();
        FindUIComponents();
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClick);
    }

    private void FindUIComponents()
    {
        if (npcNameText == null)
        {
            Transform npcNameTrans = DeepFindTransform.DeepFindChild(transform, "NPCName");
            if (npcNameTrans != null) npcNameText = npcNameTrans.GetComponent<Text>();
        }
        if (dialogueText == null)
        {
            Transform dialogueTrans = DeepFindTransform.DeepFindChild(transform, "DialogueText");
            if (dialogueTrans != null) dialogueText = dialogueTrans.GetComponent<Text>();
        }
        if (continueButton == null)
        {
            Transform continueTrans = DeepFindTransform.DeepFindChild(transform, "ContinueButton");
            if (continueTrans != null) continueButton = continueTrans.GetComponent<Button>();
        }
        if (answerGeneration == null)
        {
            Transform answerGenTrans = DeepFindTransform.DeepFindChild(transform, "AnswerGeneration");
            if (answerGenTrans != null) answerGeneration = answerGenTrans;
        }
        if (answerPrefab == null)
            answerPrefab = Resources.Load<GameObject>("UI/DialogPanel/answer");

        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (answerGeneration != null) answerGeneration.gameObject.SetActive(false);
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        // 获取玩家脚本并禁用画画动作
        playerFarmAction = FindObjectOfType<PlayerFarmAction>();
        if (playerFarmAction != null)
        {
            playerFarmAction.SetDialogOpen(true);
        }

        if (objs.Length >= 3 && objs[0] is DialogueNode && objs[1] is NPCInteractable && objs[2] is NPCConfig)
        {
            currentNode = (DialogueNode)objs[0];
            currentNPC = (NPCInteractable)objs[1];
            currentConfig = (NPCConfig)objs[2];

            cachedGamePanel = FindObjectOfType<GamePanel>();
            if (cachedGamePanel != null) cachedGamePanel.gameObject.SetActive(false);

            PausePlayer(true);

            if (npcNameText != null && currentConfig != null)
                npcNameText.text = currentConfig.npcName;

            ShowCurrentNode();
        }
    }

    private string GetPlayerName()
    {
        int slotIndex = ChoicePanel.currentSelectedSlot;
        if (slotIndex >= 0 && SaveManager.gameDatas[slotIndex] != null)
        {
            string name = SaveManager.gameDatas[slotIndex].playerData.playerName;
            if (!string.IsNullOrEmpty(name)) return name;
        }
        return "玩家";
    }

    private string ReplacePlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("{玩家姓名}", GetPlayerName()).Replace("{PlayerName}", GetPlayerName());
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            CloseDialog();
            return;
        }

        if (dialogueText != null)
            dialogueText.text = ReplacePlaceholders(currentNode.npcLine);

        if (currentNode.options != null && currentNode.options.Count > 0)
            ShowAnswerButtons();
        else
            ShowContinueButton();
    }

    private void ShowContinueButton()
    {
        if (answerGeneration != null) answerGeneration.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        ClearAnswerButtons();
    }

    private void ShowAnswerButtons()
    {
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (answerGeneration == null || answerPrefab == null)
        {
            ShowContinueButton();
            return;
        }

        answerGeneration.gameObject.SetActive(true);
        ClearAnswerButtons();

        for (int i = 0; i < currentNode.options.Count && i < 2; i++)
        {
            var option = currentNode.options[i];
            if (option == null) continue;

            GameObject btnObj = Instantiate(answerPrefab, answerGeneration);
            Button btn = btnObj.GetComponent<Button>();
            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = option.optionText;

            AddButtonClickEffect(btn, option);
            currentAnswerButtons.Add(btnObj);
        }
    }

    private void AddButtonClickEffect(Button button, PlayerOption option)
    {
        Vector3 originalScale = button.transform.localScale;
        button.onClick.AddListener(() =>
        {
            SetAllButtonsInteractable(false);
            StartCoroutine(ButtonClickAnimation(button, originalScale, option));
        });
    }

    private IEnumerator ButtonClickAnimation(Button button, Vector3 originalScale, PlayerOption option)
    {
        float duration = 0.1f;
        float elapsed = 0;
        Vector3 targetScale = originalScale * 0.85f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            button.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            button.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        button.transform.localScale = originalScale;
        yield return new WaitForSeconds(0.15f);
        ProcessOption(option);
    }

    private void ProcessOption(PlayerOption option)
    {
        // 处理任务触发
        if (!string.IsNullOrEmpty(option.questId) && QuestManager.Instance != null)
        {
            var status = QuestManager.Instance.GetQuestStatus(option.questId);

            if (option.questTrigger == QuestTriggerType.Accept)
            {
                if (status == QuestStatus.CanAccept)
                    QuestManager.Instance.AcceptQuest(option.questId);
            }
            else if (option.questTrigger == QuestTriggerType.Complete)
            {
                // 完成任务（对话类型）
                QuestManager.Instance.UpdateProgress(option.questId, QuestGoalType.TalkToNPC, currentConfig.npcName, 1);
            }
        }

        // 跳转或关闭
        if (option.nextNode != null)
        {
            currentNode = option.nextNode;
            ShowCurrentNode();
        }
        else
        {
            CloseDialog();
        }
    }

    private void SetAllButtonsInteractable(bool interactable)
    {
        foreach (var btnObj in currentAnswerButtons)
        {
            if (btnObj != null)
            {
                Button btn = btnObj.GetComponent<Button>();
                if (btn != null) btn.interactable = interactable;
            }
        }
    }

    private void OnContinueClick()
    {
        if (currentNode != null && currentNode.nextNode != null)
        {
            currentNode = currentNode.nextNode;
            ShowCurrentNode();
        }
        else
        {
            CloseDialog();
        }
    }

    private void ClearAnswerButtons()
    {
        foreach (var btn in currentAnswerButtons)
            if (btn != null) Destroy(btn);
        currentAnswerButtons.Clear();
    }

    private void CloseDialog()
    {
        // 恢复玩家画画动作
        if (playerFarmAction != null)
        {
            playerFarmAction.SetDialogOpen(false);
        }

        PausePlayer(false);
        if (cachedGamePanel != null) cachedGamePanel.gameObject.SetActive(true);
        OnClose();
        if (currentNPC != null) currentNPC.OnDialogClosed();
    }

    private void PausePlayer(bool pause)
    {
        if (playerMove == null) playerMove = FindObjectOfType<PlayerMoveControl2>();
        if (playerMove != null) playerMove.enabled = !pause;

        if (freeLookCamera == null) freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        if (freeLookCamera != null) freeLookCamera.enabled = !pause;

        if (pause)
        {
            wasCursorVisible = Cursor.visible;
            wasCursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = wasCursorVisible;
            Cursor.lockState = wasCursorLockMode;
        }
    }
}