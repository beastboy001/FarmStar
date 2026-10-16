using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class PlayerFarmAction : MonoBehaviour
{
    [Header("????????")]
    public float interactRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode plantKey = KeyCode.Mouse0;
    public KeyCode plowKey = KeyCode.Mouse1;

    [Header("??????????????")]
    public float weedClearTime = 1f;
    public float rockClearTime = 3f;
    public float treeClearTime = 5f;

    [Header("???????????")]
    public string selectedItemId = "";
    private string lastSelectedItemId = "";
    private ToolType currentToolType = ToolType.None;

    [Header("Cinemachine")]
    public CinemachineFreeLook freeLookCamera;

    [Header("????????????")]
    public bool enablePaintAnimation = true;
    public float continuousDelay = 0.2f;

    [Header("???????")]
    public GameObject axeModel;
    public GameObject pickaxeModel;
    public GameObject hoeModel;

    private Tile currentTarget;
    private Tile previousTarget;
    private float clearProgress = 0;
    private bool isClearing = false;
    private List<Tile> allTiles = new List<Tile>();

    private PlayerMoveControl2 playerMoveControl;
    private bool isUIOpen = false;
    private bool isDialogOpen = false;
    private GamePanel gamePanel;
    private InventoryManager inventory;
    private Animator playerAnimator;

    private float holdTime = 0f;
    private bool isHoldingPaint = false;
    private bool isLooping = false;
    private int paintLayerIndex = -1;

    private static readonly int IsPaintingHash = Animator.StringToHash("IsPainting");
    private static readonly int PaintNowHash = Animator.StringToHash("PaintNow");
    private static readonly int PaintLoopStateHash = Animator.StringToHash("PaintLoop");

    void Start()
    {
        allTiles.AddRange(FindObjectsOfType<Tile>());
        playerMoveControl = GetComponent<PlayerMoveControl2>();
        inventory = InventoryManager.Instance;
        playerAnimator = GetComponent<Animator>();

        if (playerAnimator != null)
        {
            playerAnimator.SetBool(IsPaintingHash, false);
            paintLayerIndex = playerAnimator.GetLayerIndex("PaintLayer");
        }

        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        }

        gamePanel = FindObjectOfType<GamePanel>();

        if (axeModel != null) axeModel.SetActive(false);
        if (pickaxeModel != null) pickaxeModel.SetActive(false);
        if (hoeModel != null) hoeModel.SetActive(false);

        lastSelectedItemId = "";
    }

    void Update()
    {
        ApplyCursorState(isUIOpen || isDialogOpen);

        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTaskPanel();
            return;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBackpack();
            return;
        }

        // ?? ESC ?????????
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenSetPanel();
            return;
        }

        // ???????????????????UI???????????
        if (!isUIOpen && !isDialogOpen)
        {
            HandlePaintAnimation();
        }

        if (isUIOpen) return;

        UpdateSelectedItem();

        FindNearestTile();
        HandleTargetChange();
        CheckMatureCrops();

        if (currentTarget == null)
        {
            if (isClearing) ResetClear();
            return;
        }

        if (Input.GetKey(interactKey))
        {
            HandleInteract();
        }
        else if (Input.GetKeyUp(interactKey))
        {
            ResetClear();
        }

        if (Input.GetKeyDown(plantKey))
        {
            HandleLeftClick();
        }

        if (Input.GetKeyDown(plowKey))
        {
            HandleRightClick();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            FarmManager.Instance.ManualSave();
        }

        if (Input.GetKeyDown(interactKey))
        {
            bool interactedWithNPC = TryInteractWithNPC();
            if (!interactedWithNPC && currentTarget != null)
            {
                HandleInteract();
            }
        }
    }

    // ========== ????????? ==========

    private void OpenSetPanel()
    {
        if (UISystem.Instance != null)
        {
            UISystem.Instance.OpenPanel<SetPanel>();
            OnSetPanelOpened();
        }
    }

    private void ToggleTaskPanel()
    {
        if (UISystem.Instance == null)
            return;

        if (MainShopPanel.Instance != null && MainShopPanel.Instance.gameObject.activeSelf)
        {
            UISystem.Instance.ClosePanel(typeof(MainShopPanel).ToString());
            isUIOpen = false;
            ApplyCursorState(false);
            return;
        }

        UISystem.Instance.OpenPanel<MainShopPanel>();
        isUIOpen = true;
        ApplyCursorState(true);

        if (MainShopPanel.Instance != null)
            MainShopPanel.Instance.OpenTaskViewDirectly();
    }

    // ========== ??????????? ==========

    private void UpdateHeldTool()
    {
        if (axeModel != null) axeModel.SetActive(false);
        if (pickaxeModel != null) pickaxeModel.SetActive(false);
        if (hoeModel != null) hoeModel.SetActive(false);

        if (string.IsNullOrEmpty(selectedItemId)) return;

        string item = selectedItemId.ToLower();

        if (item == "axe" || item == "???")
        {
            if (axeModel != null) axeModel.SetActive(true);
        }
        else if (item == "pickaxe" || item == "????")
        {
            if (pickaxeModel != null) pickaxeModel.SetActive(true);
        }
        else if (item == "hoe" || item == "???")
        {
            if (hoeModel != null) hoeModel.SetActive(true);
        }
    }

    // ========== ???????????????? ==========

    private void HandlePaintAnimation()
    {
        if (!enablePaintAnimation || playerAnimator == null) return;

        bool paintButtonDown = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            isHoldingPaint = true;
            isLooping = false;
            holdTime = 0f;

            playerAnimator.SetBool(IsPaintingHash, false);
            playerAnimator.ResetTrigger(PaintNowHash);
        }

        if (isHoldingPaint && paintButtonDown && !isLooping)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= continuousDelay)
            {
                BeginPaintLoop();
            }
        }

        if (isHoldingPaint && !paintButtonDown)
        {
            if (!isLooping)
            {
                playerAnimator.SetTrigger(PaintNowHash);
            }
            else
            {
                EndPaintLoop();
            }

            isHoldingPaint = false;
            holdTime = 0f;
        }
    }

    private void BeginPaintLoop()
    {
        isLooping = true;
        playerAnimator.SetBool(IsPaintingHash, true);

        if (paintLayerIndex >= 0)
        {
            playerAnimator.Play(PaintLoopStateHash, paintLayerIndex, 0f);
        }
    }

    private void EndPaintLoop()
    {
        if (!isLooping || playerAnimator == null) return;

        playerAnimator.SetBool(IsPaintingHash, false);
        isLooping = false;
    }

    private void EndPaintAnimation()
    {
        if (isLooping)
        {
            EndPaintLoop();
        }

        isHoldingPaint = false;
        holdTime = 0f;
    }

    // ========== ????????? ==========

    public void SetDialogOpen(bool isOpen)
    {
        isDialogOpen = isOpen;

        if (!isOpen)
        {
            EndPaintAnimation();
        }
    }

    bool TryInteractWithNPC()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            NPCInteractable npc = hit.GetComponent<NPCInteractable>();
            if (npc != null)
            {
                npc.Interact();
                return true;
            }
        }
        return false;
    }

    void UpdateSelectedItem()
    {
        if (!string.IsNullOrEmpty(selectedItemId) && inventory != null)
        {
            ItemConfig config = inventory.GetItemConfig(selectedItemId);
            if (config != null && config.itemType == ItemType.Tool)
            {
                currentToolType = config.toolType;
            }
            else
            {
                currentToolType = ToolType.None;
            }
        }
        else
        {
            currentToolType = ToolType.None;
        }

        if (lastSelectedItemId != selectedItemId)
        {
            lastSelectedItemId = selectedItemId;
            UpdateHeldTool();
        }
    }

    void HandleLeftClick()
    {
        if (currentTarget == null) return;

        if (currentTarget.currentState == TileState.Plowed)
        {
            TryPlant();
        }
        else if (currentTarget.currentState == TileState.Obstacle)
        {
            TryQuickClear();
        }
    }

    void TryPlant()
    {
        if (string.IsNullOrEmpty(selectedItemId)) return;

        ItemConfig itemConfig = inventory?.GetItemConfig(selectedItemId);
        if (itemConfig == null || itemConfig.itemType != ItemType.Seed) return;

        CropConfig cropConfig = FarmManager.Instance.GetCropConfig(itemConfig.cropId);
        if (cropConfig == null) return;

        if (!inventory.HasItem(selectedItemId, 1)) return;

        inventory.RemoveItem(selectedItemId, 1);
        currentTarget.Plant(itemConfig.cropId);
    }

    void TryQuickClear()
    {
        if (!currentTarget.CanClearWithTool(currentToolType)) return;

        float baseTime = GetBaseClearTime(currentTarget.obstacleType);
        float actualTime = currentTarget.GetClearTimeWithTool(currentTarget.obstacleType, currentToolType, baseTime);
        StartCoroutine(QuickClearCoroutine(actualTime));
    }

    IEnumerator QuickClearCoroutine(float duration)
    {
        if (currentTarget == null) yield break;

        currentTarget.ShowProgressBar();
        float elapsed = 0;

        while (elapsed < duration && currentTarget != null && currentTarget.currentState == TileState.Obstacle)
        {
            elapsed += Time.deltaTime;
            currentTarget.SetProgress(elapsed / duration);
            yield return null;
        }

        if (currentTarget != null && currentTarget.currentState == TileState.Obstacle)
        {
            currentTarget.ClearObstacle();

            if (QuestManager.Instance != null)
            {
                string targetId = GetObstacleTargetId(currentTarget.obstacleType);
                QuestManager.Instance.UpdateProgress(null, QuestGoalType.ClearObstacle, targetId, 1);
            }
        }

        currentTarget.HideCanvas();
        ResetClear();
    }

    private string GetObstacleTargetId(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Weed: return "weed";
            case ObstacleType.Rock: return "rock";
            case ObstacleType.Tree: return "tree";
            default: return "";
        }
    }

    void HandleRightClick()
    {
        if (currentTarget == null) return;

        if (currentToolType != ToolType.Hoe)
        {
            Debug.Log("??????????????");
            return;
        }

        if (currentTarget.currentState == TileState.Empty)
        {
            currentTarget.Plow();
        }
        else if (currentTarget.currentState == TileState.Plowed)
        {
            Debug.Log("???????????");
        }
        else if (currentTarget.currentState == TileState.Obstacle)
        {
            Debug.Log("??????????????????");
        }
    }

    void ToggleBackpack()
    {
        if (isUIOpen) CloseBackpack();
        else OpenBackpack();
    }

    void OpenBackpack()
    {
        if (UISystem.Instance != null)
        {
            UISystem.Instance.OpenPanel<MainShopPanel>();
            PausePlayer(true);
            ApplyCursorState(true);
        }
    }

    void CloseBackpack()
    {
        if (UISystem.Instance != null)
        {
            UISystem.Instance.ClosePanel(typeof(MainShopPanel).ToString());
            PausePlayer(false);
            ApplyCursorState(false);
        }
    }

    void PausePlayer(bool pause)
    {
        isUIOpen = pause;

        if (playerMoveControl != null)
            playerMoveControl.enabled = !pause;

        if (freeLookCamera != null)
            freeLookCamera.enabled = !pause;
    }

    private void ApplyCursorState(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void OnBackpackClosed()
    {
        isUIOpen = false;
        ApplyCursorState(false);
        if (playerMoveControl != null)
            playerMoveControl.enabled = true;
        if (freeLookCamera != null)
            freeLookCamera.enabled = true;
    }

    public void OnSetPanelOpened()
    {
        PausePlayer(true);
        ApplyCursorState(true);
    }

    public void OnSetPanelClosed()
    {
        PausePlayer(false);
        ApplyCursorState(false);
    }

    void CheckMatureCrops()
    {
        foreach (var tile in allTiles)
        {
            if (tile.ShouldForceShow())
            {
                tile.ShowStageText();
            }
        }
    }

    void HandleTargetChange()
    {
        if (currentTarget != previousTarget)
        {
            if (previousTarget != null && !previousTarget.ShouldForceShow())
            {
                previousTarget.HideUI();
            }

            if (currentTarget != null)
            {
                currentTarget.ShowUI();
            }

            previousTarget = currentTarget;
        }
    }

    void FindNearestTile()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);

        float nearestDistance = Mathf.Infinity;
        Tile nearestTile = null;

        foreach (var hitCollider in hitColliders)
        {
            Tile tile = hitCollider.GetComponent<Tile>();
            if (tile != null)
            {
                float distance = Vector3.Distance(transform.position, tile.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTile = tile;
                }
            }
        }

        currentTarget = nearestTile;
    }

    void HandleInteract()
    {
        if (currentTarget == null) return;

        switch (currentTarget.currentState)
        {
            case TileState.Obstacle:
                if (!isClearing)
                {
                    isClearing = true;
                    clearProgress = 0;
                }

                float requiredTime = GetBaseClearTime(currentTarget.obstacleType);
                clearProgress += Time.deltaTime / requiredTime;
                currentTarget.SetProgress(clearProgress);

                if (clearProgress >= 1f)
                {
                    currentTarget.ClearObstacle();
                    ResetClear();
                }
                break;

            case TileState.Planted:
                if (currentTarget.IsMature())
                {
                    currentTarget.Harvest();

                    if (QuestManager.Instance != null && currentTarget.cropData != null)
                    {
                        QuestManager.Instance.UpdateProgress(null, QuestGoalType.HarvestCrop, currentTarget.cropData.cropId, 1);
                    }
                }
                break;
        }
    }

    float GetBaseClearTime(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Weed: return weedClearTime;
            case ObstacleType.Rock: return rockClearTime;
            case ObstacleType.Tree: return treeClearTime;
            default: return 1f;
        }
    }

    void ResetClear()
    {
        if (isClearing)
        {
            if (currentTarget != null)
                currentTarget.ResetProgress();
            isClearing = false;
            clearProgress = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}