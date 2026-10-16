using System.Collections.Generic;
using UnityEngine;

// 商店商品项
[System.Serializable]
public class ShopItem
{
    public string itemId;      // 物品ID
    public int stock;           // 库存（-1表示无限）
    public int customPrice;     // 自定义价格（0表示使用物品默认价格）
}

[System.Serializable]
public class ShopData
{
    public List<ShopItem> items = new List<ShopItem>();
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("商店配置")]
    public ShopData shopData;

    private InventoryManager inventory;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        inventory = InventoryManager.Instance;
    }

    // ========== 购买逻辑 ==========

    /// <summary>
    /// 购买物品
    /// </summary>
    // 修改 BuyItem 方法
    public bool BuyItem(string itemId, int quantity)
    {
        // 1. 获取物品配置
        ItemConfig config = inventory.GetItemConfig(itemId);
        if (config == null)
        {
            Debug.LogError($"找不到物品配置：{itemId}");
            return false;
        }

        // 2. 检查商店库存
        ShopItem shopItem = shopData.items.Find(i => i.itemId == itemId);
        if (shopItem == null)
        {
            Debug.Log($"商店没有该物品：{itemId}");
            return false;
        }

        if (shopItem.stock != -1 && shopItem.stock < quantity)
        {
            Debug.Log($"库存不足，当前库存：{shopItem.stock}");
            return false;
        }

        // 3. 计算价格
        int pricePerUnit = shopItem.customPrice > 0 ? shopItem.customPrice : config.buyPrice;
        int totalPrice = pricePerUnit * quantity;

        // 4. 扣金币（会触发 OnGoldChanged 事件）
        if (!inventory.SpendGold(totalPrice))
        {
            Debug.Log($"金币不足，需要 {totalPrice}，当前 {inventory.GetGold()}");
            return false;
        }

        // 5. 添加物品到背包（会触发 OnInventoryChanged 事件）
        if (!inventory.AddItem(itemId, quantity))
        {
            // 添加失败，退还金币
            inventory.AddGold(totalPrice);
            Debug.Log("背包已满，购买失败");
            return false;
        }

        // 6. 减少商店库存
        if (shopItem.stock != -1)
        {
            shopItem.stock -= quantity;
        }

        Debug.Log($"购买成功：{itemId} x{quantity}，花费 {totalPrice} 金币");
        return true;
    }

    // 修改 SellItem 方法
    public bool SellItem(string itemId, int quantity)
    {
        if (!inventory.HasItem(itemId, quantity))
        {
            Debug.Log($"物品不足");
            return false;
        }

        ItemConfig config = inventory.GetItemConfig(itemId);
        if (config == null) return false;

        int totalPrice = config.sellPrice * quantity;

        // 移除物品（会触发 OnInventoryChanged 事件）
        if (inventory.RemoveItem(itemId, quantity))
        {
            // 增加金币（会触发 OnGoldChanged 事件）
            inventory.AddGold(totalPrice);
            Debug.Log($"出售 {itemId} x{quantity}，获得 {totalPrice} 金币");
            return true;
        }

        return false;
    }
    
    /// <summary>
         /// 获取商品信息（用于UI显示）
         /// </summary>
    public (ItemConfig config, int stock, int price) GetShopItemInfo(string itemId)
    {
        ItemConfig config = inventory.GetItemConfig(itemId);
        if (config == null) return (null, 0, 0);

        ShopItem shopItem = shopData.items.Find(i => i.itemId == itemId);
        int stock = shopItem != null ? shopItem.stock : 0;
        int price = shopItem != null && shopItem.customPrice > 0 ? shopItem.customPrice : config.buyPrice;

        return (config, stock, price);
    }

}