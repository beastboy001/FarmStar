using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI组件")]
    public Image iconImage;              // 物品图片
    public Text quantityText;             // 物品数量
    public GameObject selectedIndicator;  // 高亮效果（选中时隐藏）

    private ItemSlot slotData;
    private ItemConfig itemConfig;
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private MainShopPanel mainPanel;

    private GameObject dragIcon;
    private CanvasGroup canvasGroup;
    private Vector2 originalIconSize;

    // 记录拖拽开始时的槽位索引
    private int dragStartSlotIndex = -1;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentCanvas = GetComponentInParent<Canvas>();

        if (iconImage != null)
            originalIconSize = iconImage.rectTransform.sizeDelta;
    }

    public void Init(ItemSlot slot, MainShopPanel panel)
    {
        slotData = slot;
        mainPanel = panel;

        if (slot == null || string.IsNullOrEmpty(slot.itemId))
        {
            if (iconImage != null)
                iconImage.gameObject.SetActive(false);
            if (quantityText != null)
                quantityText.text = "";
        }
        else
        {
            itemConfig = InventoryManager.Instance.GetItemConfig(slot.itemId);
            if (itemConfig != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = itemConfig.icon;
                    iconImage.gameObject.SetActive(true);
                }
                if (quantityText != null)
                {
                    quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
                }
            }
        }
        SetSelected(false);
    }

    public bool HasItem()
    {
        return slotData != null && !string.IsNullOrEmpty(slotData.itemId);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(!selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (mainPanel != null && itemConfig != null)
            {
                mainPanel.ShowItemDetails(slotData, itemConfig);
                SetSelected(true);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotData == null || string.IsNullOrEmpty(slotData.itemId)) return;

        // 【关键】记录拖拽开始时的槽位索引
        dragStartSlotIndex = slotData.slotIndex;

        // 创建拖拽图标
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(parentCanvas.transform);
        dragIcon.transform.SetAsLastSibling();

        Image img = dragIcon.AddComponent<Image>();
        img.sprite = iconImage.sprite;
        img.raycastTarget = false;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = originalIconSize;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);

        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;

        Debug.Log($"开始拖拽：槽位 {dragStartSlotIndex}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );
            dragIcon.transform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 【关键】获取目标槽位
        int targetSlotIndex = -1;

        if (eventData.pointerCurrentRaycast.isValid)
        {
            GameObject target = eventData.pointerCurrentRaycast.gameObject;
            if (target != null)
            {
                ItemSlotUI targetSlot = target.GetComponentInParent<ItemSlotUI>();
                if (targetSlot != null && targetSlot != this)
                {
                    targetSlotIndex = targetSlot.slotData.slotIndex;
                    Debug.Log($"拖拽到目标槽位：{targetSlotIndex}");
                }
            }
        }

        // 执行交换
        if (dragStartSlotIndex >= 0 && targetSlotIndex >= 0 && dragStartSlotIndex != targetSlotIndex)
        {
            InventoryManager.Instance.SwapSlots(dragStartSlotIndex, targetSlotIndex);
            Debug.Log($"交换槽位 {dragStartSlotIndex} 和 {targetSlotIndex}");
        }

        // 重置拖拽状态
        dragStartSlotIndex = -1;
    }
}