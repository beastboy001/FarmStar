using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("物品数据库")]
    public ItemConfig[] itemDatabase;  // 所有物品配置
    private Dictionary<string, ItemConfig> itemDict;

    [Header("背包设置")]
    public int maxSlots = 20;

    private InventoryData inventoryData;
    private int currentSlotIndex;  // 当前存档槽位

    // 事件系统
    public System.Action OnInventoryChanged;  // 背包变化事件
    public System.Action OnGoldChanged;       // 金币变化事件

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 建立物品字典
            itemDict = new Dictionary<string, ItemConfig>();
            foreach (var item in itemDatabase)
            {
                if (item != null && !itemDict.ContainsKey(item.itemId))
                {
                    itemDict[item.itemId] = item;
                }
            }
            Debug.Log($"InventoryManager 初始化完成，找到 {itemDict.Count} 个物品配置");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init(int slotIndex, InventoryData data)
    {
        Debug.Log("========== InventoryManager.Init ==========");
        Debug.Log($"slotIndex: {slotIndex}");
        Debug.Log($"data == null: {data == null}");

        currentSlotIndex = slotIndex;

        if (data != null && data.slots != null && data.slots.Count > 0)
        {
            inventoryData = data;
            Debug.Log($"加载存档背包，已有 {data.slots.Count} 个物品，金币：{data.gold}");

            // 确保槽位数量一致
            while (inventoryData.slots.Count < maxSlots)
            {
                inventoryData.slots.Add(new ItemSlot("", 0));
            }

            // 确保所有槽位索引正确
            for (int i = 0; i < inventoryData.slots.Count; i++)
            {
                inventoryData.slots[i].slotIndex = i;
            }
        }
        else
        {
            // 创建新背包
            CreateNewInventory();
        }

        Debug.Log($"背包初始化完成，{inventoryData.slots.Count}个槽位，金币：{inventoryData.gold}");

        // 打印前几个槽位的内容
        for (int i = 0; i < Mathf.Min(6, inventoryData.slots.Count); i++)
        {
            var slot = inventoryData.slots[i];
            Debug.Log($"槽位 {i}: {slot.itemId} x{slot.quantity}");
        }

        // 触发刷新事件
        OnInventoryChanged?.Invoke();
        OnGoldChanged?.Invoke();
    }

    /// <summary>
    /// 创建新背包的方法
    /// </summary>
    void CreateNewInventory()
    {
        inventoryData = new InventoryData
        {
            maxSlots = maxSlots,
            gold = 500,
            slots = new List<ItemSlot>()
        };

        // 先填满空格子
        for (int i = 0; i < maxSlots; i++)
        {
            inventoryData.slots.Add(new ItemSlot("", 0));
        }

        // 初始物品配置
        //SetSlot(0, "hoe", 1);           // 锄头
        //SetSlot(1, "axe", 1);           // 斧头
        //SetSlot(2, "pickaxe", 1);       // 镐子
        //SetSlot(3, "tomato_seed", 5);   // 番茄种子 x5
        //SetSlot(4, "corn_seed", 5);     // 玉米种子 x5
        //SetSlot(5, "wheat_seed", 5);    // 小麦种子 x5
        SetSlot(0, "tomato", 2);        // 番茄 x2
        SetSlot(1, "corn", 2);          // 玉米 x2
        SetSlot(2, "wheat", 2);         // 小麦 x2

        Debug.Log($"创建新背包，{inventoryData.slots.Count}个槽位，金币：{inventoryData.gold}");
    }
    /// <summary>
    /// 辅助方法：设置指定槽位的物品
    /// </summary>
    void SetSlot(int index, string itemId, int quantity)
    {
        if (index >= 0 && index < inventoryData.slots.Count)
        {
            inventoryData.slots[index].itemId = itemId;
            inventoryData.slots[index].quantity = quantity;
            inventoryData.slots[index].slotIndex = index;
        }
    }

    // ========== 物品操作 ==========

    /// <summary>
    /// 获取物品配置
    /// </summary>
    public ItemConfig GetItemConfig(string itemId)
    {
        if (itemDict.TryGetValue(itemId, out ItemConfig config))
            return config;
        Debug.LogWarning($"找不到物品配置：{itemId}");
        return null;
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    public bool AddItem(string itemId, int quantity)
    {
        if (quantity <= 0) return false;

        ItemConfig config = GetItemConfig(itemId);
        if (config == null) return false;

        int remaining = quantity;

        // 1. 先尝试堆叠到现有格子
        for (int i = 0; i < inventoryData.slots.Count; i++)
        {
            var slot = inventoryData.slots[i];
            if (slot.itemId == itemId && slot.quantity < config.maxStack)
            {
                int canAdd = config.maxStack - slot.quantity;
                int addAmount = Mathf.Min(canAdd, remaining);
                slot.quantity += addAmount;
                remaining -= addAmount;

                if (remaining <= 0)
                {
                    RefreshUI();
                    Debug.Log($"添加物品成功：{itemId} x{quantity}");
                    return true;
                }
            }
        }

        // 2. 找空格子
        for (int i = 0; i < inventoryData.slots.Count; i++)
        {
            var slot = inventoryData.slots[i];
            if (string.IsNullOrEmpty(slot.itemId) || slot.quantity <= 0)
            {
                int addAmount = Mathf.Min(config.maxStack, remaining);
                slot.itemId = itemId;
                slot.quantity = addAmount;
                slot.slotIndex = i;
                remaining -= addAmount;

                if (remaining <= 0)
                {
                    RefreshUI();
                    Debug.Log($"添加物品成功：{itemId} x{quantity}");
                    return true;
                }
            }
        }

        Debug.LogWarning($"背包已满，还有 {remaining} 个 {itemId} 无法添加");
        RefreshUI();
        return false;
    }
    /// <summary>
    /// 移除物品
    /// </summary>
    public bool RemoveItem(string itemId, int quantity)
    {
        if (quantity <= 0) return true;

        int remaining = quantity;

        // 从后往前移除
        for (int i = inventoryData.slots.Count - 1; i >= 0; i--)
        {
            var slot = inventoryData.slots[i];
            if (slot.itemId == itemId)
            {
                if (slot.quantity >= remaining)
                {
                    slot.quantity -= remaining;
                    if (slot.quantity <= 0)
                    {
                        slot.Clear();
                    }
                    RefreshUI();
                    Debug.Log($"移除物品成功：{itemId} x{quantity}");
                    return true;
                }
                else
                {
                    remaining -= slot.quantity;
                    slot.Clear();
                }
            }
        }

        Debug.LogWarning($"移除失败，物品不足");
        RefreshUI();
        return false;
    }

    /// <summary>
    /// 检查是否有足够物品
    /// </summary>
    public bool HasItem(string itemId, int quantity)
    {
        int total = 0;
        foreach (var slot in inventoryData.slots)
        {
            if (slot.itemId == itemId)
                total += slot.quantity;
        }
        return total >= quantity;
    }

    /// <summary>
    /// 获取物品总数量
    /// </summary>
    public int GetItemCount(string itemId)
    {
        int total = 0;
        foreach (var slot in inventoryData.slots)
        {
            if (slot.itemId == itemId)
                total += slot.quantity;
        }
        return total;
    }

    // ========== 槽位操作 ==========

    /// <summary>
    /// 交换两个槽位（支持空格子交换）
    /// </summary>
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= inventoryData.slots.Count ||
            indexB < 0 || indexB >= inventoryData.slots.Count)
        {
            Debug.LogWarning($"无效的槽位索引：A={indexA}, B={indexB}");
            return;
        }

        // 直接交换两个槽位
        var temp = inventoryData.slots[indexA];
        inventoryData.slots[indexA] = inventoryData.slots[indexB];
        inventoryData.slots[indexB] = temp;

        // 更新槽位索引
        inventoryData.slots[indexA].slotIndex = indexA;
        inventoryData.slots[indexB].slotIndex = indexB;

        Debug.Log($"交换槽位 {indexA} 和 {indexB}，A物品：{inventoryData.slots[indexA].itemId}，B物品：{inventoryData.slots[indexB].itemId}");

        RefreshUI();
    }

    /// <summary>
    /// 排序背包
    /// </summary>
    public void SortInventory()
    {
        // 按物品ID分组，先合并相同的物品
        Dictionary<string, int> itemGroups = new Dictionary<string, int>();

        foreach (var slot in inventoryData.slots)
        {
            if (!string.IsNullOrEmpty(slot.itemId))
            {
                if (itemGroups.ContainsKey(slot.itemId))
                    itemGroups[slot.itemId] += slot.quantity;
                else
                    itemGroups[slot.itemId] = slot.quantity;
            }
        }

        // 清空所有槽位
        foreach (var slot in inventoryData.slots)
        {
            slot.Clear();
        }

        // 按顺序重新填充
        int index = 0;
        foreach (var pair in itemGroups.OrderBy(p => p.Key))
        {
            string itemId = pair.Key;
            int total = pair.Value;
            ItemConfig config = GetItemConfig(itemId);
            int maxStack = config != null ? config.maxStack : 99;

            while (total > 0 && index < inventoryData.slots.Count)
            {
                int amount = Mathf.Min(maxStack, total);
                inventoryData.slots[index].itemId = itemId;
                inventoryData.slots[index].quantity = amount;
                inventoryData.slots[index].slotIndex = index;
                total -= amount;
                index++;
            }
        }

        // 确保所有槽位索引正确
        for (int i = 0; i < inventoryData.slots.Count; i++)
        {
            if (inventoryData.slots[i].slotIndex != i)
                inventoryData.slots[i].slotIndex = i;
        }

        Debug.Log("背包排序完成");
        RefreshUI();
    }

    // ========== 金币操作 ==========

    /// <summary>
    /// 获取金币
    /// </summary>
    public int GetGold()
    {
        return inventoryData != null ? inventoryData.gold : 0;
    }

    /// <summary>
    /// 增加金币
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        inventoryData.gold += amount;
        RefreshUI();
        OnGoldChanged?.Invoke();
        Debug.Log($"金币增加 {amount}，当前金币: {inventoryData.gold}");
    }

    /// <summary>
    /// 花费金币
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;

        if (inventoryData.gold >= amount)
        {
            inventoryData.gold -= amount;
            RefreshUI();
            OnGoldChanged?.Invoke();
            Debug.Log($"金币减少 {amount}，当前金币: {inventoryData.gold}");
            return true;
        }

        Debug.Log($"金币不足，需要 {amount}，当前 {inventoryData.gold}");
        return false;
    }

    // ========== 存档 ==========

    /// <summary>
    /// 获取背包数据
    /// </summary>
    public InventoryData GetInventoryData()
    {
        if (inventoryData == null)
        {
            Debug.LogWarning("inventoryData 为空，创建新数据");
            CreateNewInventory();
        }

        // 确保所有槽位的 slotIndex 正确
        for (int i = 0; i < inventoryData.slots.Count; i++)
        {
            inventoryData.slots[i].slotIndex = i;
        }
        return inventoryData;
    }

    /// <summary>
    /// 保存到 GameData
    /// </summary>
    public void SaveToGameData()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < SaveManager.gameDatas.Length)
        {
            GameData gameData = SaveManager.gameDatas[currentSlotIndex];
            if (gameData != null)
            {
                gameData.inventoryData = GetInventoryData();
                Debug.Log("背包数据已保存到 GameData");
            }
            else
            {
                Debug.LogWarning("GameData 为空，无法保存背包数据");
            }
        }
        else
        {
            Debug.LogWarning($"无效的存档槽位: {currentSlotIndex}");
        }
    }

    // ========== UI刷新 ==========

    /// <summary>
    /// 刷新UI（触发事件）
    /// </summary>
    void RefreshUI()
    {
        OnInventoryChanged?.Invoke();
        OnGoldChanged?.Invoke();
    }
}