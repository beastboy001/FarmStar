using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tile : MonoBehaviour
{
    [Header("索引")]
    public int index;

    [Header("坐标")]
    public int gridX;
    public int gridY;

    [Header("组件")]
    public Transform itemHolder;

    [Header("杂物设置")]
    public ObstacleType obstacleType;

    [Header("当前状态")]
    public TileState currentState = TileState.Obstacle;

    [Header("UI组件")]
    public GameObject progressCanvas;
    public Slider progressBar;
    public Text showState;

    [Header("UI位置设置")]
    public float uiOffset = 2.0f;

    [Header("收获提示字体")]
    public Font harvestFont;

    [Header("清除杂物获得金币")]
    public int weedGoldReward = 5;
    public int rockGoldReward = 10;
    public int treeGoldReward = 20;

    private GameObject currentItem;
    private MeshRenderer myRenderer;
    private GameObject harvestMessage;
    public CropData cropData = null;
    private bool isPlayerNear = false;

    [System.Serializable]
    public class CropData
    {
        public string cropId;
        public float remainingTime;
        public float totalGrowTime;
        public bool isMature;

        public CropData(string id, float growSeconds)
        {
            cropId = id;
            remainingTime = growSeconds;
            totalGrowTime = growSeconds;
            isMature = false;
        }

        public CropData(string id, float remaining, float total)
        {
            cropId = id;
            remainingTime = remaining;
            totalGrowTime = total;
            isMature = false;
        }
    }

    void Awake()
    {
        myRenderer = GetComponent<MeshRenderer>();

        if (itemHolder == null)
            itemHolder = transform.Find("ItemHolder");

        if (progressCanvas == null && itemHolder != null)
        {
            progressCanvas = itemHolder.Find("ProgressCanvas")?.gameObject;
        }

        if (progressBar == null && progressCanvas != null)
        {
            Transform progressBarTrans = progressCanvas.transform.Find("ProgressBar");
            if (progressBarTrans != null)
                progressBar = progressBarTrans.GetComponent<Slider>();
        }

        if (showState == null && progressCanvas != null)
        {
            Transform showStateTrans = progressCanvas.transform.Find("ShowState");
            if (showStateTrans != null)
                showState = showStateTrans.GetComponent<Text>();
        }

        if (progressCanvas != null && progressCanvas.GetComponent<BillboardCanvas>() == null)
            progressCanvas.AddComponent<BillboardCanvas>();

        if (progressCanvas != null)
            progressCanvas.SetActive(false);

        if (progressBar != null)
            progressBar.gameObject.SetActive(false);
        if (showState != null)
            showState.gameObject.SetActive(false);

        cropData = null;
    }

    void Start()
    {
        if (obstacleType == ObstacleType.None && currentState == TileState.Obstacle)
        {
            currentState = TileState.Empty;
        }

        UpdateVisual();
        UpdateColor();

        if (cropData != null && cropData.isMature)
        {
            ShowMatureUI();
        }
    }

    void LateUpdate()
    {
        if (progressCanvas != null && progressCanvas.activeSelf)
        {
            AdjustUIPosition();
            UpdateUIDisplay();
        }

        if (cropData != null && cropData.isMature && progressCanvas != null && !progressCanvas.activeSelf)
        {
            ShowMatureUI();
        }
    }

    void Update()
    {
        if (cropData != null && !cropData.isMature && !string.IsNullOrEmpty(cropData.cropId))
        {
            cropData.remainingTime -= Time.deltaTime;

            if (cropData.remainingTime <= 0 && !cropData.isMature)
            {
                cropData.isMature = true;
                cropData.remainingTime = 0;
                OnCropMature();
            }
        }
    }

    void UpdateUIDisplay()
    {
        if (cropData == null || string.IsNullOrEmpty(cropData.cropId))
        {
            if (currentState == TileState.Obstacle)
            {
                if (progressBar != null) progressBar.gameObject.SetActive(true);
                if (showState != null) showState.gameObject.SetActive(false);
            }
            else
            {
                if (progressBar != null) progressBar.gameObject.SetActive(false);
                if (showState != null) showState.gameObject.SetActive(false);
            }
            return;
        }

        if (cropData.isMature)
        {
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (showState != null)
            {
                showState.text = "待采摘";
                showState.color = new Color(1, 0.9f, 0.8f);
                showState.gameObject.SetActive(true);
            }
        }
        else
        {
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (showState != null)
            {
                float remaining = cropData.remainingTime;
                if (remaining < 0) remaining = 0;
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60f);
                showState.text = $"{minutes:D2}:{seconds:D2}";
                showState.color = Color.yellow;
                showState.gameObject.SetActive(true);
            }
        }
    }

    void SpawnObstacle(ObstacleType type)
    {
        if (currentItem != null) Destroy(currentItem);
        if (itemHolder == null) return;

        GameObject prefab = null;
        switch (type)
        {
            case ObstacleType.Tree: prefab = FarmManager.Instance?.treePrefab; break;
            case ObstacleType.Rock: prefab = FarmManager.Instance?.rockPrefab; break;
            case ObstacleType.Weed: prefab = FarmManager.Instance?.weedPrefab; break;
            case ObstacleType.None: return;
        }

        if (prefab != null)
        {
            currentItem = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            currentItem.transform.SetParent(itemHolder, true);
            currentItem.transform.position = itemHolder.position;
            currentItem.transform.rotation = itemHolder.rotation;
        }
    }

    void UpdateColor()
    {
        if (myRenderer == null) return;

        switch (currentState)
        {
            case TileState.Obstacle:
            case TileState.Empty:
                myRenderer.material.color = new Color(0.6f, 0.7f, 0.3f);
                break;
            case TileState.Plowed:
            case TileState.Planted:
                myRenderer.material.color = new Color(0.6f, 0.4f, 0.2f);
                break;
        }
    }

    void AdjustUIPosition()
    {
        if (progressCanvas == null || itemHolder == null) return;

        float heightOffset = uiOffset;
        if (currentItem != null)
        {
            Renderer itemRenderer = currentItem.GetComponent<Renderer>();
            if (itemRenderer != null)
            {
                heightOffset += itemRenderer.bounds.size.y / 2;
            }
        }

        progressCanvas.transform.position = itemHolder.position + Vector3.up * heightOffset;
    }

    public void ShowUI()
    {
        isPlayerNear = true;
        if (progressCanvas == null) return;

        if (!progressCanvas.activeSelf)
        {
            AdjustUIPosition();
            progressCanvas.SetActive(true);
        }
        UpdateUIDisplay();
    }

    public void ShowMatureUI()
    {
        if (progressCanvas == null) return;
        AdjustUIPosition();
        progressCanvas.SetActive(true);

        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (showState != null)
        {
            showState.text = "待采摘";
            showState.color = Color.red;
            showState.gameObject.SetActive(true);
        }
    }

    public void HideUI()
    {
        isPlayerNear = false;
        if (cropData != null && cropData.isMature) return;
        if (progressCanvas != null && progressCanvas.activeSelf)
        {
            progressCanvas.SetActive(false);
        }
    }

    public void HideCanvas()
    {
        if (progressCanvas != null && progressCanvas.activeSelf)
        {
            progressCanvas.SetActive(false);
        }
    }

    public void SetProgress(float progress)
    {
        if (progressBar != null && progressBar.gameObject.activeSelf)
        {
            progressBar.value = progress;
        }
    }

    public void ResetProgress()
    {
        if (progressBar != null && progressBar.gameObject.activeSelf)
        {
            progressBar.value = 0;
        }
    }

    public void ShowProgressBar() => ShowUI();
    public void ShowStageText() => ShowUI();

    /// <summary>
    /// 获取清除所需时间（根据工具效率）
    /// </summary>
    public float GetClearTimeWithTool(ObstacleType obstacleType, ToolType toolType, float baseTime)
    {
        float time = baseTime;
        if (InventoryManager.Instance == null) return time;

        switch (obstacleType)
        {
            case ObstacleType.Tree:
                if (toolType == ToolType.Axe)
                {
                    ItemConfig axeConfig = InventoryManager.Instance.GetItemConfig("axe");
                    if (axeConfig != null && axeConfig.treeEfficiency > 0)
                    {
                        time = baseTime / axeConfig.treeEfficiency;
                        Debug.Log($"斧头效率: {axeConfig.treeEfficiency}x, 时间: {baseTime:F1}秒 -> {time:F1}秒");
                    }
                }
                break;
            case ObstacleType.Rock:
                if (toolType == ToolType.Pickaxe)
                {
                    ItemConfig pickaxeConfig = InventoryManager.Instance.GetItemConfig("pickaxe");
                    if (pickaxeConfig != null && pickaxeConfig.rockEfficiency > 0)
                    {
                        time = baseTime / pickaxeConfig.rockEfficiency;
                        Debug.Log($"镐子效率: {pickaxeConfig.rockEfficiency}x, 时间: {baseTime:F1}秒 -> {time:F1}秒");
                    }
                }
                break;
            case ObstacleType.Weed:
                if (toolType == ToolType.Scythe)
                {
                    ItemConfig scytheConfig = InventoryManager.Instance.GetItemConfig("scythe");
                    if (scytheConfig != null && scytheConfig.weedEfficiency > 0)
                    {
                        time = baseTime / scytheConfig.weedEfficiency;
                        Debug.Log($"镰刀效率: {scytheConfig.weedEfficiency}x, 时间: {baseTime:F1}秒 -> {time:F1}秒");
                    }
                }
                break;
        }
        return Mathf.Max(0.1f, time);
    }
    /// <summary>
    /// 检查是否有合适的工具可以清除当前杂物（只定义一次）
    /// </summary>
    public bool CanClearWithTool(ToolType toolType)
    {
        switch (obstacleType)
        {
            case ObstacleType.Tree: return toolType == ToolType.Axe;
            case ObstacleType.Rock: return toolType == ToolType.Pickaxe;
            case ObstacleType.Weed: return toolType == ToolType.Scythe;
            default: return true;
        }
    }

    /// <summary>
    /// 清除杂物并获得金币
    /// </summary>
    public void ClearObstacle()
    {
        if (currentItem != null) Destroy(currentItem);

        // 根据杂物类型获得金币
        int goldReward = 0;
        string obstacleName = "";

        switch (obstacleType)
        {
            case ObstacleType.Weed:
                goldReward = weedGoldReward;
                obstacleName = "杂草";
                break;
            case ObstacleType.Rock:
                goldReward = rockGoldReward;
                obstacleName = "石头";
                break;
            case ObstacleType.Tree:
                goldReward = treeGoldReward;
                obstacleName = "树木";
                break;
        }

        // 添加金币
        if (goldReward > 0 && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddGold(goldReward);
            ShowGoldMessage(goldReward);
            Debug.Log($"清除{obstacleName}，获得 {goldReward} 金币");
        }

        obstacleType = ObstacleType.None;
        currentState = TileState.Empty;
        UpdateColor();
        HideCanvas();
    }

    /// <summary>
    /// 显示获得金币飘字
    /// </summary>
    void ShowGoldMessage(int amount)
    {
        GameObject msgCanvas = new GameObject("GoldMessage");
        msgCanvas.transform.position = itemHolder.position + Vector3.up * 2f;

        Canvas canvas = msgCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = msgCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);

        CanvasScaler scaler = msgCanvas.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        msgCanvas.AddComponent<BillboardCanvas>();

        GameObject textObj = new GameObject("GoldText");
        textObj.transform.SetParent(msgCanvas.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition3D = Vector3.zero;
        textRect.sizeDelta = new Vector2(2, 1);
        textRect.localScale = Vector3.one;

        Text text = textObj.AddComponent<Text>();

        if (harvestFont != null)
            text.font = harvestFont;
        else
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        text.text = $"+{amount}💰";
        text.fontSize = 50;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.yellow;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(8, 8);

        StartCoroutine(AnimateGoldMessage(msgCanvas.transform, text));
    }

    IEnumerator AnimateGoldMessage(Transform msgTransform, Text text)
    {
        float duration = 1.2f;
        float elapsed = 0;

        Vector3 startPos = msgTransform.position;
        Vector3 endPos = startPos + Vector3.up * 1.5f;

        Color startColor = Color.yellow;
        Color endColor = new Color(1, 0.8f, 0, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            msgTransform.position = Vector3.Lerp(startPos, endPos, t);

            if (text != null)
            {
                text.color = Color.Lerp(startColor, endColor, t);
            }

            yield return null;
        }

        Destroy(msgTransform.gameObject);
    }

    void OnCropMature()
    {
        if (cropData == null || string.IsNullOrEmpty(cropData.cropId)) return;
        UpdateVisual();
        ShowMatureUI();
        Debug.Log($"作物成熟！{cropData.cropId}");

        FarmManager.Instance?.SaveFarmData();
    }

    void ShowCropModel()
    {
        if (cropData == null || string.IsNullOrEmpty(cropData.cropId)) return;

        CropConfig config = FarmManager.Instance.GetCropConfig(cropData.cropId);
        if (config == null) return;

        GameObject prefab = null;
        if (cropData.isMature)
        {
            prefab = config.maturePrefab;
        }
        else
        {
            float progress = 1f - (cropData.remainingTime / cropData.totalGrowTime);
            if (progress < 0.33f) prefab = config.seedPrefab;
            else if (progress < 0.66f) prefab = config.growingPrefab;
            else prefab = config.maturePrefab;
        }

        if (prefab != null)
        {
            currentItem = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            currentItem.transform.SetParent(itemHolder, true);
            currentItem.transform.position = itemHolder.position;
            currentItem.transform.rotation = itemHolder.rotation;
        }
    }

    public void UpdateVisual()
    {
        if (currentItem != null) Destroy(currentItem);

        switch (currentState)
        {
            case TileState.Obstacle:
                SpawnObstacle(obstacleType);
                break;
            case TileState.Planted:
                if (cropData != null && !string.IsNullOrEmpty(cropData.cropId))
                {
                    ShowCropModel();
                }
                break;
        }
        UpdateColor();
    }

    public bool ShouldForceShow() => cropData != null && cropData.isMature;

    public void Plow()
    {
        currentState = TileState.Plowed;
        UpdateColor();
    }

    public void Plant(string cropId)
    {
        CropConfig config = FarmManager.Instance.GetCropConfig(cropId);
        if (config == null)
        {
            Debug.LogError($"找不到作物配置：{cropId}");
            return;
        }

        currentState = TileState.Planted;
        cropData = new CropData(cropId, config.growSeconds);
        UpdateVisual();
        UpdateColor();
        Debug.Log($"种植 {config.cropName}，需要 {config.growSeconds} 秒成熟");

        if (isPlayerNear && progressCanvas != null) ShowUI();
        FarmManager.Instance?.SaveFarmData();
    }

    public bool IsMature() => cropData != null && cropData.isMature;

    void ShowHarvestMessage(int amount)
    {
        if (harvestMessage != null) Destroy(harvestMessage);

        GameObject msgCanvas = new GameObject("HarvestMessage");
        msgCanvas.transform.position = itemHolder.position + Vector3.up * 2f;

        Canvas canvas = msgCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = msgCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);

        CanvasScaler scaler = msgCanvas.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        msgCanvas.AddComponent<BillboardCanvas>();

        GameObject textObj = new GameObject("HarvestText");
        textObj.transform.SetParent(msgCanvas.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition3D = Vector3.zero;
        textRect.sizeDelta = new Vector2(2, 1);
        textRect.localScale = Vector3.one;

        Text text = textObj.AddComponent<Text>();

        if (harvestFont != null)
            text.font = harvestFont;
        else
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        text.text = $"+{amount}";
        text.fontSize = 60;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.yellow;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(10, 10);

        harvestMessage = msgCanvas;

        StartCoroutine(AnimateHarvestMessage(msgCanvas.transform, text));
    }

    IEnumerator AnimateHarvestMessage(Transform msgTransform, Text text)
    {
        float duration = 1.2f;
        float elapsed = 0;

        Vector3 startPos = msgTransform.position;
        Vector3 endPos = startPos + Vector3.up * 1.5f;

        Color startColor = Color.yellow;
        Color endColor = new Color(1, 1, 0, 0);

        Vector3 originalScale = msgTransform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            msgTransform.position = Vector3.Lerp(startPos, endPos, t);

            if (text != null)
            {
                text.color = Color.Lerp(startColor, endColor, t);
            }

            msgTransform.localScale = originalScale;

            yield return null;
        }

        Destroy(msgTransform.gameObject);
        harvestMessage = null;
    }

    public void Harvest()
    {
        if (cropData == null || !cropData.isMature) return;

        CropConfig config = FarmManager.Instance.GetCropConfig(cropData.cropId);
        if (config != null)
        {
            int amount = Random.Range(config.minHarvest, config.maxHarvest + 1);
            ShowHarvestMessage(amount);
            InventoryManager.Instance?.AddItem(config.harvestItemId, amount);
            Debug.Log($"收获 {config.cropName} x{amount}");

            // ========== 更新任务进度 ==========
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.UpdateProgress(null, QuestGoalType.HarvestCrop, cropData.cropId, amount);
            }
        }

        if (currentItem != null) Destroy(currentItem);
        cropData = null;
        currentState = TileState.Plowed;
        UpdateColor();
        UpdateVisual();

        if (progressCanvas != null) progressCanvas.SetActive(false);
        isPlayerNear = false;

        FarmManager.Instance?.SaveFarmData();
    }
}