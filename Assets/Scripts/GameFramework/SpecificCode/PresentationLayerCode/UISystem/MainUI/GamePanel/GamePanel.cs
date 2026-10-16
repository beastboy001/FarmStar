using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    [Header("顶部UI组件")]
    public Text goldText;
    public Text dayText;
    public Text seasonText;
    public Text timeText;

    [Header("物品栏设置")]
    public Transform itemsContainer;
    public int itemSlotCount = 10;

    [Header("NPC交互提示")]
    public GameObject interactTipPanel;
    public Text interactTipText;

    [Header("开场欢迎动画")]
    public Text WelcomeTitle;
    public Text PlayerFarmText;
    public float fadeInTime = 1f;
    public float displayTime = 2f;
    public float fadeOutTime = 1f;

    [Header("任务显示")]
    public Text TaskNameText;
    public Text TaskStateText;

    public static GamePanel Instance { get; private set; }
    private List<GameItemSlot> itemSlots = new List<GameItemSlot>();
    private int currentSelectedSlot = -1;
    private InventoryManager inventory;
    private bool isQuestListOpen = false;
    private bool isTaskUiOpen = false;
    private bool isBackpackOpen = false;

    // 时间系统变量
    private float realSecondsPerGameMinute = 0.8333f;
    private int currentHour = 6;
    private int currentMinute = 0;
    private float timeAccumulator = 0f;
    private int currentDay = 1;
    private int currentSeason = 0;
    private string[] seasonNames = { "春天", "夏天", "秋天", "冬天" };
    private bool isTimeRunning = true;
    private int currentSlotIndex = -1;
    private int lastGoldValue = -1;
    private bool isInitialized = false;
    private bool isWaitingForInventory = false;
    private int waitInventoryFrames = 0;
    private bool hasPlayedWelcome = false;
    private bool welcomeAnimationPending = false;

    private CanvasGroup welcomeGroup;
    private CanvasGroup farmNameGroup;

    public override void OnInit()
    {
        base.OnInit();
        Instance = this;
        FindUIComponents();
        InitializeItemSlots();
        TryGetInventory();
        InitWelcomeText();

        Debug.Log("GamePanel OnInit 完成");
    }

    private void InitWelcomeText()
    {
        if (WelcomeTitle != null)
        {
            welcomeGroup = WelcomeTitle.GetComponent<CanvasGroup>();
            if (welcomeGroup == null)
                welcomeGroup = WelcomeTitle.gameObject.AddComponent<CanvasGroup>();
            welcomeGroup.alpha = 0f;
            WelcomeTitle.gameObject.SetActive(false);
        }

        if (PlayerFarmText != null)
        {
            farmNameGroup = PlayerFarmText.GetComponent<CanvasGroup>();
            if (farmNameGroup == null)
                farmNameGroup = PlayerFarmText.gameObject.AddComponent<CanvasGroup>();
            farmNameGroup.alpha = 0f;
            PlayerFarmText.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayWelcomeAnimation()
    {
        string farmName = GetFarmName();

        if (PlayerFarmText != null)
            PlayerFarmText.text = $"<--{farmName}的农场-->";

        if (WelcomeTitle != null)
            WelcomeTitle.gameObject.SetActive(true);
        if (PlayerFarmText != null)
            PlayerFarmText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeInTime);
            if (welcomeGroup != null) welcomeGroup.alpha = alpha;
            if (farmNameGroup != null) farmNameGroup.alpha = alpha;
            yield return null;
        }

        if (welcomeGroup != null) welcomeGroup.alpha = 1f;
        if (farmNameGroup != null) farmNameGroup.alpha = 1f;

        yield return new WaitForSeconds(displayTime);

        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            if (welcomeGroup != null) welcomeGroup.alpha = alpha;
            if (farmNameGroup != null) farmNameGroup.alpha = alpha;
            yield return null;
        }

        if (welcomeGroup != null) welcomeGroup.alpha = 0f;
        if (farmNameGroup != null) farmNameGroup.alpha = 0f;

        if (WelcomeTitle != null)
            WelcomeTitle.gameObject.SetActive(false);
        if (PlayerFarmText != null)
            PlayerFarmText.gameObject.SetActive(false);
    }

    private string GetFarmName()
    {
        int slotIndex = ChoicePanel.currentSelectedSlot;
        if (slotIndex >= 0 && SaveManager.gameDatas[slotIndex] != null)
        {
            string name = SaveManager.gameDatas[slotIndex].playerData.farmName;
            if (!string.IsNullOrEmpty(name))
                return name;
        }
        return "我的";
    }

    public void ShowInteractTip(string npcName)
    {
        if (interactTipPanel != null)
        {
            if (interactTipText != null)
                interactTipText.text = $"与{npcName}对话";
            interactTipPanel.SetActive(true);
        }
    }

    public void HideInteractTip()
    {
        if (interactTipPanel != null)
            interactTipPanel.SetActive(false);
    }

    void FindUIComponents()
    {
        if (goldText == null)
        {
            Transform goldTrans = DeepFindTransform.DeepFindChild(transform, "GoldText");
            if (goldTrans != null) goldText = goldTrans.GetComponent<Text>();
        }

        if (dayText == null)
        {
            Transform dayTrans = DeepFindTransform.DeepFindChild(transform, "DayText");
            if (dayTrans != null) dayText = dayTrans.GetComponent<Text>();
        }

        if (seasonText == null)
        {
            Transform seasonTrans = DeepFindTransform.DeepFindChild(transform, "SeasonText");
            if (seasonTrans != null) seasonText = seasonTrans.GetComponent<Text>();
        }

        if (timeText == null)
        {
            Transform timeTrans = DeepFindTransform.DeepFindChild(transform, "TimeText");
            if (timeTrans != null) timeText = timeTrans.GetComponent<Text>();
        }

        if (itemsContainer == null)
        {
            Transform itemsTrans = DeepFindTransform.DeepFindChild(transform, "Items");
            if (itemsTrans != null) itemsContainer = itemsTrans;
        }

        if (interactTipPanel == null)
        {
            Transform jiaohu = DeepFindTransform.DeepFindChild(transform, "jiaohu");
            if (jiaohu != null)
            {
                interactTipPanel = jiaohu.gameObject;

                Transform tipText = DeepFindTransform.DeepFindChild(jiaohu, "TipText");
                if (tipText != null)
                {
                    interactTipText = tipText.GetComponent<Text>();
                    if (interactTipText != null)
                    {
                        interactTipText.text = "";
                    }
                }

                Debug.Log("动态找到交互提示面板: jiaohu");
            }
            else
            {
                Debug.LogWarning("未找到交互提示面板，请确保 GamePanel 下有名为 'jiaohu' 的子物体");
            }
        }

        if (interactTipPanel != null)
            interactTipPanel.SetActive(false);

        if (WelcomeTitle == null)
        {
            Transform welcomeTrans = DeepFindTransform.DeepFindChild(transform, "WelcomeTitle");
            if (welcomeTrans != null) WelcomeTitle = welcomeTrans.GetComponent<Text>();
        }

        if (PlayerFarmText == null)
        {
            Transform farmTextTrans = DeepFindTransform.DeepFindChild(transform, "PlayerFarmText");
            if (farmTextTrans != null) PlayerFarmText = farmTextTrans.GetComponent<Text>();
        }

        if (TaskNameText == null)
        {
            Transform taskNameTrans = DeepFindTransform.DeepFindChild(transform, "TaskNameText");
            if (taskNameTrans != null) TaskNameText = taskNameTrans.GetComponent<Text>();
        }

        if (TaskStateText == null)
        {
            Transform taskStateTrans = DeepFindTransform.DeepFindChild(transform, "TaskStateText");
            if (taskStateTrans != null) TaskStateText = taskStateTrans.GetComponent<Text>();
        }
    }

    void InitializeItemSlots()
    {
        if (itemsContainer == null)
        {
            Debug.LogError("物品栏容器为空！");
            return;
        }

        itemSlots.Clear();

        for (int i = 0; i < itemsContainer.childCount && i < itemSlotCount; i++)
        {
            Transform child = itemsContainer.GetChild(i);
            GameItemSlot slot = child.GetComponent<GameItemSlot>();
            if (slot == null)
                slot = child.gameObject.AddComponent<GameItemSlot>();

            slot.Init(i, this);
            itemSlots.Add(slot);
        }

        Debug.Log($"初始化物品栏完成，共 {itemSlots.Count} 个格子");
    }

    void TryGetInventory()
    {
        inventory = InventoryManager.Instance;

        if (inventory != null)
        {
            FinishInitialization();
            isWaitingForInventory = false;
            waitInventoryFrames = 0;
            return;
        }

        // 物体可能尚未激活，不能使用协程；在 Update 中轮询 InventoryManager
        isWaitingForInventory = true;
        waitInventoryFrames = 0;
    }

    void OnEnable()
    {
        if (!isInitialized && isWaitingForInventory)
        {
            inventory = InventoryManager.Instance;
            if (inventory != null)
            {
                FinishInitialization();
                isWaitingForInventory = false;
                waitInventoryFrames = 0;
            }
        }

        if (welcomeAnimationPending && gameObject.activeInHierarchy && !hasPlayedWelcome)
        {
            welcomeAnimationPending = false;
            hasPlayedWelcome = true;
            StartCoroutine(PlayWelcomeAnimation());
        }
    }

    void FinishInitialization()
    {
        if (isInitialized) return;

        Debug.Log("GamePanel: InventoryManager 已获取，完成初始化");

        inventory.OnInventoryChanged += OnInventoryChanged;
        inventory.OnGoldChanged += OnGoldChanged;

        currentSlotIndex = ChoicePanel.currentSelectedSlot;
        LoadTimeData();

        RefreshGold();
        RefreshDayAndSeason();
        RefreshTimeText();
        RefreshItemSlots();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestDataChanged += RefreshCurrentTaskDisplay;
        }
        RefreshCurrentTaskDisplay();

        isInitialized = true;
    }

    private Color GetStatusColor(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.CanAccept: return new Color(0.45f, 0.8f, 0);
            case QuestStatus.InProgress: return new Color(0.8f, 0.5f, 0);
            case QuestStatus.Completed: return new Color(0.4f, 0.65f, 0.9f);
            default: return Color.gray;
        }
    }

    private string GetStatusText(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.CanAccept: return "可接取";
            case QuestStatus.InProgress: return "进行中";
            case QuestStatus.Completed: return "已完成";
            case QuestStatus.Claimed: return "已领取";
            case QuestStatus.Locked: return "未解锁";
            default: return "";
        }
    }

    private void RefreshCurrentTaskDisplay()
    {
        if (TaskNameText == null || TaskStateText == null) return;

        var questManager = QuestManager.Instance;
        if (questManager == null)
        {
            TaskNameText.text = "暂无任务";
            TaskStateText.text = "";
            return;
        }

        var allQuests = questManager.GetAllQuests();
        if (allQuests == null || allQuests.Count == 0)
        {
            TaskNameText.text = "暂无任务";
            TaskStateText.text = "";
            return;
        }

        QuestConfig displayQuest = null;
        foreach (var quest in allQuests)
        {
            var status = questManager.GetQuestStatus(quest.questId);
            if (status == QuestStatus.InProgress)
            {
                displayQuest = quest;
                break;
            }
        }

        if (displayQuest == null)
        {
            foreach (var quest in allQuests)
            {
                var status = questManager.GetQuestStatus(quest.questId);
                if (status == QuestStatus.CanAccept)
                {
                    displayQuest = quest;
                    break;
                }
            }
        }

        if (displayQuest == null && allQuests.Count > 0)
        {
            displayQuest = allQuests[0];
        }

        if (displayQuest != null)
        {
            var status = questManager.GetQuestStatus(displayQuest.questId);
            TaskNameText.text = displayQuest.questName;
            TaskStateText.text = GetStatusText(status);
            TaskStateText.color = GetStatusColor(status);
        }
        else
        {
            TaskNameText.text = "暂无任务";
            TaskStateText.text = "";
        }
    }

    void OnInventoryChanged()
    {
        Debug.Log("收到背包变化事件，刷新物品栏");
        if (isInitialized)
        {
            RefreshItemSlots();
            RefreshGold();
        }
    }

    void OnGoldChanged()
    {
        if (isInitialized)
        {
            RefreshGold();
        }
    }

    void RefreshItemSlots()
    {
        if (inventory == null)
        {
            Debug.LogWarning($"RefreshItemSlots: inventory 为 null");
            return;
        }

        if (itemSlots.Count == 0) return;

        InventoryData data = inventory.GetInventoryData();
        if (data == null || data.slots == null)
        {
            Debug.LogWarning("背包数据为空");
            return;
        }

        for (int i = 0; i < itemSlotCount && i < itemSlots.Count; i++)
        {
            ItemSlot slot = null;
            if (i < data.slots.Count)
            {
                slot = data.slots[i];
            }

            if (itemSlots[i] != null)
            {
                itemSlots[i].UpdateSlot(slot);
            }
        }

        if (currentSelectedSlot >= 0 && currentSelectedSlot < itemSlots.Count)
        {
            SetSlotHighlight(currentSelectedSlot, true);
        }
    }

    void RefreshGold()
    {
        if (inventory == null || goldText == null) return;

        int currentGold = inventory.GetGold();
        if (lastGoldValue != currentGold)
        {
            goldText.text = currentGold.ToString();
            lastGoldValue = currentGold;
        }
    }

    void RefreshDayAndSeason()
    {
        if (dayText != null)
            dayText.text = $"第{currentDay}天";

        if (seasonText != null)
            seasonText.text = seasonNames[currentSeason];
    }

    void RefreshTimeText()
    {
        if (timeText != null)
        {
            string hourStr = currentHour.ToString("D2");
            string minuteStr = currentMinute.ToString("D2");
            timeText.text = $"{hourStr}:{minuteStr}";
        }
    }

    void LoadTimeData()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < SaveManager.gameDatas.Length)
        {
            GameData gameData = SaveManager.gameDatas[currentSlotIndex];
            if (gameData != null && gameData.timeData != null)
            {
                currentDay = gameData.timeData.day;
                currentSeason = gameData.timeData.season;
                currentHour = gameData.timeData.hour;
                currentMinute = gameData.timeData.minute;
                return;
            }
        }

        currentDay = 1;
        currentSeason = 0;
        currentHour = 6;
        currentMinute = 0;
    }

    void SaveTimeData()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < SaveManager.gameDatas.Length)
        {
            GameData gameData = SaveManager.gameDatas[currentSlotIndex];
            if (gameData == null)
            {
                gameData = new GameData();
                SaveManager.gameDatas[currentSlotIndex] = gameData;
            }

            if (gameData.timeData == null)
                gameData.timeData = new TimeData();

            gameData.timeData.day = currentDay;
            gameData.timeData.season = currentSeason;
            gameData.timeData.hour = currentHour;
            gameData.timeData.minute = currentMinute;
        }
    }

    void Update()
    {
        if (!isInitialized && isWaitingForInventory)
        {
            inventory = InventoryManager.Instance;
            if (inventory != null)
            {
                FinishInitialization();
                isWaitingForInventory = false;
                waitInventoryFrames = 0;
            }
            else
            {
                waitInventoryFrames++;
                if (waitInventoryFrames >= 60)
                {
                    Debug.LogWarning("GamePanel: 等待 InventoryManager 超时");
                    isWaitingForInventory = false;
                    waitInventoryFrames = 0;
                }
            }
        }

        if (!isTimeRunning) return;

        UpdateGameTime();
        CheckItemSlotShortcuts();
        CheckQuestPanelShortcut();

        if (isInitialized)
        {
            RefreshGold();
        }
    }

    void UpdateGameTime()
    {
        timeAccumulator += Time.deltaTime;

        while (timeAccumulator >= realSecondsPerGameMinute)
        {
            timeAccumulator -= realSecondsPerGameMinute;
            currentMinute += 10;

            if (currentMinute >= 60)
            {
                currentMinute = 0;
                currentHour++;

                if (currentHour >= 24)
                {
                    currentHour = 0;
                    NextDay();
                }
            }

            RefreshTimeText();
        }

        if (currentMinute % 30 == 0 && timeAccumulator < 0.1f)
        {
            SaveTimeData();
        }
    }

    void NextDay()
    {
        currentDay++;

        if (currentDay > 28)
        {
            currentDay = 1;
            currentSeason++;
            if (currentSeason >= 4) currentSeason = 0;
            RefreshDayAndSeason();
        }
        else
        {
            if (dayText != null) dayText.text = $"第{currentDay}天";
        }

        SaveTimeData();

        if (FarmManager.Instance != null)
            FarmManager.Instance.ManualSave();

        if (inventory != null)
            inventory.SaveToGameData();
    }

    void CheckItemSlotShortcuts()
    {
        if (isQuestListOpen) return;

        for (int i = 0; i < itemSlotCount; i++)
        {
            KeyCode key = GetKeyCodeForSlot(i);
            if (Input.GetKeyDown(key))
            {
                SelectSlot(i);
                break;
            }
        }
    }

    void CheckQuestPanelShortcut()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleMissionView();
        }
    }

    private void ToggleMissionView()
    {
        if (MainShopPanel.Instance == null)
            return;

        if (!isBackpackOpen)
        {
            OpenBackpackForUI();
        }

        if (!isTaskUiOpen)
        {
            isTaskUiOpen = true;
            MainShopPanel.Instance.gameObject.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            MainShopPanel.Instance.ShowBackpack();
            StartCoroutine(EnsureTaskUIReady());
        }
        else
        {
            isTaskUiOpen = false;
            MainShopPanel.Instance.ShowBackpack();
            SyncCursorForUI();
        }
    }

    private IEnumerator EnsureTaskUIReady()
    {
        yield return null;
        if (MainShopPanel.Instance != null && isTaskUiOpen)
            MainShopPanel.Instance.ShowTask();
    }

    public void NotifyBackpackOpened()
    {
        isBackpackOpen = true;
        SyncCursorForUI();
    }

    public void NotifyBackpackClosed()
    {
        isBackpackOpen = false;
        isTaskUiOpen = false;
    }

    private void OpenBackpackForUI()
    {
        if (MainShopPanel.Instance == null)
            return;

        if (!MainShopPanel.Instance.gameObject.activeSelf)
            MainShopPanel.Instance.gameObject.SetActive(true);

        MainShopPanel.Instance.ShowBackpack();
        isBackpackOpen = true;
        SyncCursorForUI();
    }

    private void SyncCursorForUI()
    {
        Cursor.visible = isBackpackOpen || isTaskUiOpen;
        Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    KeyCode GetKeyCodeForSlot(int slotIndex)
    {
        if (slotIndex == 9) return KeyCode.Alpha0;
        return KeyCode.Alpha1 + slotIndex;
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= itemSlots.Count) return;

        if (currentSelectedSlot >= 0 && currentSelectedSlot < itemSlots.Count)
        {
            SetSlotHighlight(currentSelectedSlot, false);
        }

        currentSelectedSlot = slotIndex;
        SetSlotHighlight(currentSelectedSlot, true);

        if (inventory != null && isInitialized)
        {
            InventoryData data = inventory.GetInventoryData();
            if (data != null && data.slots.Count > slotIndex)
            {
                ItemSlot slot = data.slots[slotIndex];
                PlayerFarmAction player = FindObjectOfType<PlayerFarmAction>();

                if (!string.IsNullOrEmpty(slot.itemId))
                {
                    ItemConfig config = inventory.GetItemConfig(slot.itemId);
                    if (config != null && player != null)
                    {
                        // 关键修改：使用 itemId 而不是 itemName
                        player.selectedItemId = config.itemId;
                        Debug.Log($"选中物品：{config.itemName}，ID：{config.itemId}，类型：{config.itemType}");
                    }
                }
                else
                {
                    if (player != null)
                    {
                        player.selectedItemId = "";
                    }
                }
            }
        }
    }

    void SetSlotHighlight(int slotIndex, bool highlight)
    {
        if (slotIndex >= 0 && slotIndex < itemSlots.Count && itemSlots[slotIndex] != null)
        {
            itemSlots[slotIndex].SetHighlight(highlight);
        }
    }

    public string GetSelectedItemId()
    {
        if (currentSelectedSlot >= 0 && inventory != null && isInitialized)
        {
            InventoryData data = inventory.GetInventoryData();
            if (data != null && data.slots.Count > currentSelectedSlot)
            {
                return data.slots[currentSelectedSlot].itemId;
            }
        }
        return "";
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        if (!isInitialized)
        {
            TryGetInventory();
        }
        else
        {
            currentSlotIndex = ChoicePanel.currentSelectedSlot;
            LoadTimeData();
            RefreshGold();
            RefreshDayAndSeason();
            RefreshTimeText();
            RefreshItemSlots();
            RefreshCurrentTaskDisplay();
        }

        isTimeRunning = true;

        if (currentSelectedSlot >= 0)
        {
            SetSlotHighlight(currentSelectedSlot, true);
        }

        if (!hasPlayedWelcome)
        {
            if (gameObject.activeInHierarchy)
            {
                welcomeAnimationPending = false;
                hasPlayedWelcome = true;
                StartCoroutine(PlayWelcomeAnimation());
            }
            else
                welcomeAnimationPending = true;
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        SaveTimeData();
        isTimeRunning = false;
        isQuestListOpen = false;
        isTaskUiOpen = false;
        isBackpackOpen = false;
        SyncCursorForUI();
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= OnInventoryChanged;
            inventory.OnGoldChanged -= OnGoldChanged;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestDataChanged -= RefreshCurrentTaskDisplay;
        }

        SaveTimeData();
    }

    public int GetCurrentGameHour()
    {
        return currentHour;
    }

    public int GetCurrentGameMinute()
    {
        return currentMinute;
    }

    /// <summary>
    /// 连续的一天内时间（0~24 小时），在两次 +10 分钟跳变之间线性推进，供天空/昼夜插值。
    /// </summary>
    public float GetTimeOfDayHoursSmooth()
    {
        float blockFrac = Mathf.Clamp01(timeAccumulator / realSecondsPerGameMinute);
        float totalMinutes = currentHour * 60f + currentMinute + 10f * blockFrac;
        totalMinutes %= 24f * 60f;
        if (totalMinutes < 0f)
            totalMinutes += 24f * 60f;
        return totalMinutes / 60f;
    }

    public int GetCurrentGameDay()
    {
        return currentDay;
    }

    public int GetCurrentSeason()
    {
        return currentSeason;
    }

    public float GetCurrentGameTimeInSeconds()
    {
        int totalMinutes = (currentDay - 1) * 24 * 60 + currentHour * 60 + currentMinute;
        float totalSeconds = totalMinutes * 60f;

        if (currentMinute % 10 == 0 && timeAccumulator < 0.05f)
        {
            Debug.Log($"当前游戏时间: 第{currentDay}天 {currentHour:D2}:{currentMinute:D2} = {totalSeconds}秒");
        }

        return totalSeconds;
    }
}