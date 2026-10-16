using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainShopPanel : BasePanel
{
    [Header("========== 标签按钮 ==========")]
    public Button BackpackTab;
    public Button ShopTab;
    public Button TaskTab;
    public Button ExitButton;

    [Header("========== 视图 ==========")]
    public GameObject BackpackView;
    public GameObject Shopview;
    public GameObject Missionview;

    [Header("========== 背包分类按钮 ==========")]
    public Button ShowAllButton;
    public Button ShowCropButton;
    public Button ShowToolButton;

    [Header("========== 背包组件 ==========")]
    public Transform ItemGrid;
    public GameObject ItemSlotPrefab;
    public Button SortButton;

    [Header("========== 物品详情面板 ==========")]
    public GameObject propDetails;
    public Image ItemImage;
    public Text Itemname;
    public Text PropDetailsText;
    public Text AccessText;
    public Text ItemResidue;

    [Header("========== 商店组件 ==========")]
    public Transform shopItemGrid;
    public GameObject shopItemPrefab;
    public Text purchaseAmountText;
    public Text sellAmountText;
    public Button purchaseButton;
    public Button sellButton;
    public Image shopItemImage;
    public Text shopItemname;

    [Header("========== 任务面板组件 ==========")]
    public Text missionNameText;
    public Text targetPreviewText;
    public Transform prizeContainer;
    public GameObject prizeItemPrefab;
    public Sprite goldIcon;
    public Button pickupButton;
    public Button acquireButton;

    [Header("========== 通用 ==========")]
    public Text goldText;

    private InventoryManager inventory;
    private ShopManager shop;
    private ItemType? currentFilter = null;
    private List<ItemSlotUI> slotUIs = new List<ItemSlotUI>();
    private List<ShopItemUI> shopItemUIs = new List<ShopItemUI>();
    private ItemConfig currentDisplayItem;
    private ShopItemUI currentSelectedShopItem;
    private PlayerFarmAction playerAction;

    private QuestManager questManager;
    private string selectedQuestId;
    private bool isPanelInitialized = false;

    public static MainShopPanel Instance { get; private set; }

    public override void OnInit()
    {
        if (isPanelInitialized) return;
        isPanelInitialized = true;

        base.OnInit();
        FindComponents();
        RegisterEvents();
        ShowBackpack();

        if (propDetails != null)
            propDetails.SetActive(false);
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        Instance = this;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        inventory = InventoryManager.Instance;
        shop = ShopManager.Instance;
        questManager = QuestManager.Instance;

        if (inventory != null)
            inventory.OnInventoryChanged += RefreshBackpack;

        RefreshBackpack();
        RefreshShop();
        RefreshGold();
        AutoSelectFirstItem();

        if (Missionview != null && Missionview.activeSelf)
        {
            if (QuestListPanel.Instance != null)
            {
                QuestListPanel.Instance.RefreshAllDisplay();
                if (questManager != null)
                    questManager.OnQuestDataChanged += OnQuestDataChangedHandler;
            }
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        if (playerAction != null)
            playerAction.OnBackpackClosed();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("MainShopPanel OnClose: cursor hidden");

        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshBackpack;

        if (questManager != null)
            questManager.OnQuestDataChanged -= OnQuestDataChangedHandler;
    }

    // ========== 事件处理器 ==========
    private void OnQuestDataChangedHandler()
    {
        // 刷新任务列表的显示
        if (QuestListPanel.Instance != null)
            QuestListPanel.Instance.RefreshAllDisplay();

        // 检查当前选中的任务是否已变化（比如完成了）
        if (!string.IsNullOrEmpty(selectedQuestId))
        {
            var quest = questManager?.GetQuestById(selectedQuestId);
            if (quest != null)
            {
                // 刷新当前选中任务的显示
                UpdateButtonStates(quest);
                UpdateRewardDisplay(quest);
            }
        }

        // ========== 新增：如果当前没有选中任何任务，或者当前选中的任务已经完成，则自动选中第一个未完成任务 ==========
        var allQuests = questManager?.GetAllQuests();
        if (allQuests != null && allQuests.Count > 0)
        {
            // 优先选中状态为 Completed 但未领奖的任务
            QuestConfig pendingCompleteQuest = null;
            foreach (var q in allQuests)
            {
                var status = questManager.GetQuestStatus(q.questId);
                if (status == QuestStatus.Completed)
                {
                    pendingCompleteQuest = q;
                    break;
                }
            }

            if (pendingCompleteQuest != null)
            {
                // 自动选中已完成的任务
                SelectQuest(pendingCompleteQuest.questId);
                Debug.Log($"自动选中已完成任务: {pendingCompleteQuest.questName}");
            }
            else if (string.IsNullOrEmpty(selectedQuestId))
            {
                // 如果没有选中的任务，选中第一个
                SelectQuest(allQuests[0].questId);
            }
        }
    }
    public void FindComponents()
    {
        BackpackTab = DeepFindTransform.DeepFindChild(transform, "BackpackTab")?.GetComponent<Button>();
        ShopTab = DeepFindTransform.DeepFindChild(transform, "ShopTab")?.GetComponent<Button>();
        TaskTab = DeepFindTransform.DeepFindChild(transform, "TaskTab")?.GetComponent<Button>();
        ExitButton = DeepFindTransform.DeepFindChild(transform, "ExitButton")?.GetComponent<Button>();

        ShowAllButton = DeepFindTransform.DeepFindChild(transform, "ShowAllButton")?.GetComponent<Button>();
        ShowCropButton = DeepFindTransform.DeepFindChild(transform, "ShowCropButton")?.GetComponent<Button>();
        ShowToolButton = DeepFindTransform.DeepFindChild(transform, "ShowToolButton")?.GetComponent<Button>();

        BackpackView = DeepFindTransform.DeepFindChild(transform, "BackpackView")?.gameObject;
        Shopview = DeepFindTransform.DeepFindChild(transform, "Shopview")?.gameObject;
        Missionview = DeepFindTransform.DeepFindChild(transform, "Missionview")?.gameObject;

        SortButton = DeepFindTransform.DeepFindChild(transform, "SortButton")?.GetComponent<Button>();
        ItemGrid = DeepFindTransform.DeepFindChild(transform, "ItemGrid");
        ItemSlotPrefab = GameResSystem.GetRes<GameObject>("UI/BagPanel/ItemSlotUI");

        propDetails = DeepFindTransform.DeepFindChild(transform, "propDetails")?.gameObject;
        ItemImage = DeepFindTransform.DeepFindChild(transform, "ItemImage")?.GetComponent<Image>();
        Itemname = DeepFindTransform.DeepFindChild(transform, "Itemname")?.GetComponent<Text>();
        PropDetailsText = DeepFindTransform.DeepFindChild(transform, "PropDetailsText")?.GetComponent<Text>();
        AccessText = DeepFindTransform.DeepFindChild(transform, "AccessText")?.GetComponent<Text>();
        ItemResidue = DeepFindTransform.DeepFindChild(transform, "ItemResidue")?.GetComponent<Text>();

        shopItemGrid = DeepFindTransform.DeepFindChild(transform, "shopItemGrid");
        shopItemPrefab = GameResSystem.GetRes<GameObject>("UI/BagPanel/ShopItemUI");
        purchaseAmountText = DeepFindTransform.DeepFindChild(transform, "purchaseamount")?.GetComponent<Text>();
        sellAmountText = DeepFindTransform.DeepFindChild(transform, "sellamount")?.GetComponent<Text>();
        sellButton = DeepFindTransform.DeepFindChild(transform, "SellButton")?.GetComponent<Button>();
        purchaseButton = DeepFindTransform.DeepFindChild(transform, "PurchaseButton")?.GetComponent<Button>();
        shopItemImage = DeepFindTransform.DeepFindChild(transform, "shopItemImage")?.GetComponent<Image>();
        shopItemname = DeepFindTransform.DeepFindChild(transform, "shopItemname")?.GetComponent<Text>();

        goldText = DeepFindTransform.DeepFindChild(transform, "goldtext")?.GetComponent<Text>();

        missionNameText = DeepFindTransform.DeepFindChild(transform, "missionName")?.GetComponent<Text>();
        targetPreviewText = DeepFindTransform.DeepFindChild(transform, "TargetText")?.GetComponent<Text>();
        Transform rewardPreview = DeepFindTransform.DeepFindChild(transform, "RewardPreview");
        if (rewardPreview != null)
        {
            prizeContainer = rewardPreview.Find("prizepoint");
            if (prizeContainer == null) prizeContainer = rewardPreview;
        }
        prizeItemPrefab = GameResSystem.GetRes<GameObject>("UI/QuestSystem/PrizeItem");
        pickupButton = DeepFindTransform.DeepFindChild(transform, "PickupButton")?.GetComponent<Button>();
        acquireButton = DeepFindTransform.DeepFindChild(transform, "AcquireButton")?.GetComponent<Button>();

        playerAction = FindObjectOfType<PlayerFarmAction>();
    }

    private void RegisterEvents()
    {
        if (BackpackTab != null) BackpackTab.onClick.AddListener(ShowBackpack);
        if (ShopTab != null) ShopTab.onClick.AddListener(ShowShop);
        if (TaskTab != null) TaskTab.onClick.AddListener(ShowTask);
        if (ExitButton != null) ExitButton.onClick.AddListener(OnexitButton);
        if (ShowAllButton != null) ShowAllButton.onClick.AddListener(OnShowAll);
        if (ShowCropButton != null) ShowCropButton.onClick.AddListener(OnShowCrop);
        if (ShowToolButton != null) ShowToolButton.onClick.AddListener(OnShowTool);
        if (SortButton != null) SortButton.onClick.AddListener(OnSort);
        if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchase);
        if (sellButton != null) sellButton.onClick.AddListener(OnSell);
        if (pickupButton != null) pickupButton.onClick.AddListener(OnPickupQuest);
        if (acquireButton != null) acquireButton.onClick.AddListener(OnAcquireReward);
    }

    public void ShowBackpack()
    {
        if (BackpackView == null || Shopview == null || Missionview == null) return;
        BackpackView.SetActive(true);
        Shopview.SetActive(false);
        Missionview.SetActive(false);
        if (BackpackTab != null) BackpackTab.interactable = false;
        if (ShopTab != null) ShopTab.interactable = true;
        if (TaskTab != null) TaskTab.interactable = true;
    }

    public void ShowShop()
    {
        if (BackpackView == null || Shopview == null || Missionview == null) return;
        BackpackView.SetActive(false);
        Shopview.SetActive(true);
        Missionview.SetActive(false);
        if (BackpackTab != null) BackpackTab.interactable = true;
        if (ShopTab != null) ShopTab.interactable = false;
        if (TaskTab != null) TaskTab.interactable = true;
    }

    public void ShowTask()
    {
        if (BackpackView == null || Shopview == null || Missionview == null) return;
        BackpackView.SetActive(false);
        Shopview.SetActive(false);
        Missionview.SetActive(true);
        if (BackpackTab != null) BackpackTab.interactable = true;
        if (ShopTab != null) ShopTab.interactable = true;
        if (TaskTab != null) TaskTab.interactable = false;

        if (QuestListPanel.Instance != null)
        {
            QuestListPanel.Instance.RefreshTaskPanel();

            var allQuests = questManager?.GetAllQuests();
            if (allQuests != null && allQuests.Count > 0)
            {
                SelectQuest(allQuests[0].questId);
            }
        }

        RefreshGold();
    }

    public void OpenTaskViewDirectly()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        OnShow();
        ShowTask();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var gamePanel = GamePanel.Instance;
        if (gamePanel != null)
            gamePanel.NotifyBackpackOpened();
    }

    public void OnexitButton() => OnClose();

    void AutoSelectFirstItem()
    {
        if (slotUIs == null || slotUIs.Count == 0) return;
        if (slotUIs[0] != null && slotUIs[0].HasItem())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            slotUIs[0].OnPointerClick(pointerData);
        }
    }

    void RefreshGold()
    {
        if (goldText != null && inventory != null)
            goldText.text = inventory.GetGold().ToString();
    }

    void RefreshBackpack()
    {
        if (ItemGrid == null || inventory == null || ItemSlotPrefab == null) return;
        InventoryData data = inventory.GetInventoryData();
        if (data == null || data.slots == null) return;
        foreach (var slot in slotUIs) if (slot != null) Destroy(slot.gameObject);
        slotUIs.Clear();
        for (int i = 0; i < data.slots.Count; i++)
        {
            var slot = data.slots[i];
            if (slot.slotIndex != i) slot.slotIndex = i;
            bool shouldShow = true;
            if (currentFilter.HasValue && !string.IsNullOrEmpty(slot.itemId))
            {
                ItemConfig config = inventory.GetItemConfig(slot.itemId);
                shouldShow = (config != null && config.itemType == currentFilter.Value);
            }
            if (shouldShow)
            {
                GameObject slotObj = Instantiate(ItemSlotPrefab, ItemGrid);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
                if (slotUI != null) slotUI.Init(slot, this);
                slotUIs.Add(slotUI);
            }
        }
        RefreshGold();
    }

    void RefreshShop()
    {
        if (shopItemGrid == null || shop == null || shopItemPrefab == null) return;
        foreach (var item in shopItemUIs) if (item != null) Destroy(item.gameObject);
        shopItemUIs.Clear();
        foreach (var shopItem in shop.shopData.items)
        {
            var info = shop.GetShopItemInfo(shopItem.itemId);
            if (info.config == null) continue;
            GameObject itemObj = Instantiate(shopItemPrefab, shopItemGrid);
            ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();
            if (itemUI != null) itemUI.Init(info.config, info.stock, info.price, this);
            shopItemUIs.Add(itemUI);
        }
        RefreshGold();
    }

    public void OnShowAll() { currentFilter = null; RefreshBackpack(); AutoSelectFirstItem(); }
    public void OnShowCrop() { currentFilter = ItemType.Crop; RefreshBackpack(); AutoSelectFirstItem(); }
    public void OnShowTool() { currentFilter = ItemType.Tool; RefreshBackpack(); AutoSelectFirstItem(); }
    public void OnSort() { if (inventory != null) { inventory.SortInventory(); RefreshBackpack(); AutoSelectFirstItem(); } }

    public void OnPurchase()
    {
        if (currentDisplayItem == null) return;
        ShopManager.Instance.BuyItem(currentDisplayItem.itemId, 1);
        RefreshShop(); RefreshGold();
        if (currentDisplayItem != null && ItemResidue != null)
            ItemResidue.text = $"拥有: {inventory.GetItemCount(currentDisplayItem.itemId)}";
    }

    public void OnSell()
    {
        if (currentDisplayItem == null) return;
        ShopManager.Instance.SellItem(currentDisplayItem.itemId, 1);
        RefreshBackpack(); RefreshGold();
        if (currentDisplayItem != null && ItemResidue != null)
            ItemResidue.text = $"拥有: {inventory.GetItemCount(currentDisplayItem.itemId)}";
    }

    public void ShowItemDetails(ItemSlot slot, ItemConfig config)
    {
        if (propDetails == null) return;
        currentDisplayItem = config;
        propDetails.SetActive(true);
        if (ItemImage != null && config.icon != null) ItemImage.sprite = config.icon;
        if (Itemname != null) Itemname.text = config.itemName;
        if (PropDetailsText != null) PropDetailsText.text = config.description;
        if (AccessText != null) AccessText.text = config.acquisitionMethod;
        if (ItemResidue != null && inventory != null)
            ItemResidue.text = $"拥有: {inventory.GetItemCount(config.itemId)}";
        foreach (var slotUI in slotUIs) if (slotUI != null) slotUI.SetSelected(false);
    }

    public void ShowShopItemDetails(ShopItemUI shopItem, ItemConfig config, int stock, int price)
    {
        if (Shopview == null) return;
        currentDisplayItem = config;
        currentSelectedShopItem = shopItem;
        Shopview.SetActive(true);
        if (shopItemImage != null && config.icon != null) shopItemImage.sprite = config.icon;
        if (shopItemname != null) shopItemname.text = config.itemName;
        Transform residueTrans = DeepFindTransform.DeepFindChild(Shopview.transform, "ItemResidue");
        if (residueTrans != null)
        {
            Text residueText = residueTrans.GetComponent<Text>();
            if (residueText != null && inventory != null)
                residueText.text = $"拥有: {inventory.GetItemCount(config.itemId)}";
        }
        Transform purchaseAmountTrans = DeepFindTransform.DeepFindChild(Shopview.transform, "purchaseamount");
        if (purchaseAmountTrans != null)
        {
            Text purchaseText = purchaseAmountTrans.GetComponent<Text>();
            if (purchaseText != null) purchaseText.text = price.ToString();
        }
        Transform sellAmountTrans = DeepFindTransform.DeepFindChild(Shopview.transform, "sellamount");
        if (sellAmountTrans != null)
        {
            Text sellText = sellAmountTrans.GetComponent<Text>();
            if (sellText != null) sellText.text = config.sellPrice.ToString();
        }
        Canvas.ForceUpdateCanvases();
        foreach (var item in shopItemUIs) if (item != null) item.SetSelected(false);
        if (shopItem != null) shopItem.SetSelected(true);
    }

    public void HideItemDetails() { if (propDetails != null) propDetails.SetActive(false); }

    // ==================== 任务系统方法 ====================

    public void SelectQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return;
        if (questManager == null) questManager = QuestManager.Instance;
        if (questManager == null) return;

        selectedQuestId = questId;
        var quest = questManager.GetQuestById(questId);
        if (quest == null) return;

        UpdateQuestDisplay(quest);
        if (QuestListPanel.Instance != null)
            QuestListPanel.Instance.SetSelectedQuest(questId);
    }

    private void UpdateQuestDisplay(QuestConfig quest)
    {
        if (quest == null) return;
        if (missionNameText != null) missionNameText.text = quest.questName;
        if (targetPreviewText != null) targetPreviewText.text = quest.description;
        UpdateRewardDisplay(quest);
        UpdateButtonStates(quest);
    }

    private void UpdateRewardDisplay(QuestConfig quest)
    {
        if (prizeContainer == null || prizeItemPrefab == null) return;
        for (int i = prizeContainer.childCount - 1; i >= 0; i--)
            Destroy(prizeContainer.GetChild(i).gameObject);

        if (quest.reward == null) return;

        if (quest.reward.gold > 0)
        {
            GameObject item = Instantiate(prizeItemPrefab, prizeContainer);
            RewardItemUI rewardUI = item.GetComponent<RewardItemUI>();
            if (rewardUI == null) rewardUI = item.AddComponent<RewardItemUI>();
            Sprite icon = Resources.Load<Sprite>("UI/QuestSystem/Icons/gold");
            if (icon == null) icon = goldIcon;
            rewardUI.Init(icon, quest.reward.gold);
        }

        if (quest.reward.items != null)
        {
            foreach (var reward in quest.reward.items)
            {
                if (reward == null || reward.amount <= 0) continue;
                GameObject item = Instantiate(prizeItemPrefab, prizeContainer);
                RewardItemUI rewardUI = item.GetComponent<RewardItemUI>();
                if (rewardUI == null) rewardUI = item.AddComponent<RewardItemUI>();
                Sprite itemIcon = LoadItemIcon(reward.itemId);
                rewardUI.Init(itemIcon, reward.amount);
            }
        }
    }

    private Sprite LoadItemIcon(string itemId)
    {
        string iconPath = $"UI/QuestSystem/Icons/{itemId}";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        if (icon != null) return icon;

        if (InventoryManager.Instance != null)
        {
            ItemConfig config = InventoryManager.Instance.GetItemConfig(itemId);
            if (config != null && config.icon != null)
                return config.icon;
        }
        return null;
    }

    private void UpdateButtonStates(QuestConfig quest)
    {
        if (questManager == null) return;
        QuestStatus status = questManager.GetQuestStatus(quest.questId);

        // 更新 pickupButton（接取任务按钮）
        if (pickupButton != null)
        {
            var text = pickupButton.GetComponentInChildren<Text>();
            switch (status)
            {
                case QuestStatus.CanAccept:
                    if (text != null) text.text = "接取任务";
                    pickupButton.interactable = true;
                    break;
                case QuestStatus.InProgress:
                    if (text != null) text.text = "任务进行中";
                    pickupButton.interactable = false;
                    break;
                case QuestStatus.Completed:
                    if (text != null) text.text = "已完成";
                    pickupButton.interactable = false;
                    break;
                case QuestStatus.Claimed:
                    if (text != null) text.text = "已领取";
                    pickupButton.interactable = false;
                    break;
                case QuestStatus.Locked:
                    if (text != null) text.text = "未解锁";
                    pickupButton.interactable = false;
                    break;
                default:
                    if (text != null) text.text = "任务进行中";
                    pickupButton.interactable = false;
                    break;
            }
        }

        // 更新 acquireButton（领取奖励按钮）
        if (acquireButton != null)
        {
            var text = acquireButton.GetComponentInChildren<Text>();
            if (status == QuestStatus.Completed)
            {
                if (text != null) text.text = "领取奖励";
                acquireButton.interactable = true;
            }
            else
            {
                if (text != null) text.text = "领取奖励";
                acquireButton.interactable = false;
            }
        }
    }

    private void OnPickupQuest()
    {
        if (string.IsNullOrEmpty(selectedQuestId)) return;
        if (questManager == null) questManager = QuestManager.Instance;
        if (questManager == null) return;

        if (questManager.GetQuestStatus(selectedQuestId) == QuestStatus.CanAccept)
        {
            questManager.AcceptQuest(selectedQuestId);
            if (QuestListPanel.Instance != null)
                QuestListPanel.Instance.RefreshAllDisplay();

            // 刷新当前任务的按钮状态
            var quest = questManager.GetQuestById(selectedQuestId);
            if (quest != null) UpdateButtonStates(quest);
        }
    }

    private void OnAcquireReward()
    {
        if (string.IsNullOrEmpty(selectedQuestId)) return;
        if (questManager == null) questManager = QuestManager.Instance;
        if (questManager == null) return;

        if (questManager.GetQuestStatus(selectedQuestId) == QuestStatus.Completed)
        {
            questManager.ClaimReward(selectedQuestId);
            RefreshGold();
            if (QuestListPanel.Instance != null)
                QuestListPanel.Instance.RefreshAllDisplay();

            // 刷新当前任务的按钮状态
            var quest = questManager.GetQuestById(selectedQuestId);
            if (quest != null) UpdateButtonStates(quest);
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshBackpack;
        if (questManager != null)
            questManager.OnQuestDataChanged -= OnQuestDataChangedHandler;
    }
}