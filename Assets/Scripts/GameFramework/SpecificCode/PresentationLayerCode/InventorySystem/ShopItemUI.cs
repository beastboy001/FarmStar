using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image iconImage;              // 商品图片
    public GameObject selectedIndicator; // 高亮效果（选中时隐藏）

    private ItemConfig itemConfig;
    private int stock;
    private int price;
    private MainShopPanel mainPanel;

    public void Init(ItemConfig config, int itemStock, int itemPrice, MainShopPanel panel)
    {
        itemConfig = config;
        stock = itemStock;
        price = itemPrice;
        mainPanel = panel;

        if (iconImage != null && config.icon != null)
            iconImage.sprite = config.icon;

        // 默认显示高亮（未选中状态）
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        // 【关键】选中时隐藏高亮，未选中时显示高亮
        if (selectedIndicator != null)
            selectedIndicator.SetActive(!selected);
    }

    public void Buy(int quantity)
    {
        if (itemConfig != null)
        {
            ShopManager.Instance.BuyItem(itemConfig.itemId, quantity);
        }
    }

    public string GetItemId()
    {
        return itemConfig != null ? itemConfig.itemId : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (mainPanel != null && itemConfig != null)
        {
            Debug.Log($"点击商店物品: {itemConfig.itemName}");
            mainPanel.ShowShopItemDetails(this, itemConfig, stock, price);
        }
    }
}