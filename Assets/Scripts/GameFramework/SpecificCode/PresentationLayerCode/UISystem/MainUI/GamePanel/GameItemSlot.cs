using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image iconImage;
    public Text quantityText;
    public Image highlightImage;
    public Text slotNumberText;

    private int slotIndex;
    private GamePanel gamePanel;

    void Awake()
    {
        // 查找组件
        if (iconImage == null)
        {
            Transform iconTrans = transform.Find("GenerateItemPoint");
            if (iconTrans == null) iconTrans = transform.Find("GenerateltemPoint");
            if (iconTrans != null) iconImage = iconTrans.GetComponent<Image>();
        }

        if (quantityText == null)
        {
            Transform numTrans = transform.Find("ItemNum");
            if (numTrans != null) quantityText = numTrans.GetComponent<Text>();
        }

        if (highlightImage == null)
        {
            Transform highTrans = transform.Find("highlight");
            if (highTrans != null) highlightImage = highTrans.GetComponent<Image>();
        }

        if (slotNumberText == null)
        {
            Transform slotNumTrans = transform.Find("Num");
            if (slotNumTrans != null) slotNumberText = slotNumTrans.GetComponent<Text>();
        }

        // 初始状态：隐藏图标和数量，隐藏高亮
        if (iconImage != null)
            iconImage.enabled = false;
        if (quantityText != null)
            quantityText.text = "";
        if (highlightImage != null)
            highlightImage.enabled = false;
    }

    public void Init(int index, GamePanel panel)
    {
        slotIndex = index;
        gamePanel = panel;

        if (slotNumberText != null)
        {
            if (slotIndex == 9)
                slotNumberText.text = "0";
            else
                slotNumberText.text = (slotIndex + 1).ToString();
        }

        // 初始状态：空格子
        UpdateSlot(null);
    }

    public void UpdateSlot(ItemSlot slot)
    {
        bool hasItem = slot != null && !string.IsNullOrEmpty(slot.itemId) && slot.quantity > 0;

        if (!hasItem)
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
        }
        else
        {
            ItemConfig config = InventoryManager.Instance?.GetItemConfig(slot.itemId);
            if (config != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = config.icon;
                    iconImage.enabled = true;
                }
                if (quantityText != null)
                {
                    quantityText.text = slot.quantity.ToString();
                    quantityText.enabled = true;
                }
            }
            else
            {
                if (iconImage != null) iconImage.enabled = false;
                if (quantityText != null) quantityText.text = "";
            }
        }
    }

    public void SetHighlight(bool highlight)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = highlight;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gamePanel != null)
        {
            gamePanel.SelectSlot(slotIndex);
        }
    }
}