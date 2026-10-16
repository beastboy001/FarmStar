using UnityEngine;
using System.Collections.Generic;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance { get; private set; }

    [Header("杂物预制体")]
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject weedPrefab;

    [Header("作物配置")]
    public CropConfig[] cropConfigs;

    [Header("农场设置")]
    public int gridWidth = 10;
    public int gridHeight = 10;

    private Dictionary<string, CropConfig> cropDict;
    private List<Tile> allTiles = new List<Tile>();
    private int currentSlotIndex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        cropDict = new Dictionary<string, CropConfig>();
        if (cropConfigs != null)
        {
            foreach (var config in cropConfigs)
            {
                if (config != null && !cropDict.ContainsKey(config.cropId))
                {
                    cropDict[config.cropId] = config;
                }
            }
        }
    }

    void Start()
    {
        RefreshTileList();

        currentSlotIndex = ChoicePanel.currentSelectedSlot;
        Debug.Log($"当前存档槽位: {currentSlotIndex + 1}");

        GameData gameData = null;
        if (currentSlotIndex >= 0 && currentSlotIndex < SaveManager.gameDatas.Length)
        {
            gameData = SaveManager.gameDatas[currentSlotIndex];
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Init(currentSlotIndex, gameData?.inventoryData);
        }

        LoadFarmData();

        Debug.Log("FarmManager 初始化完成");
    }

    void RefreshTileList()
    {
        allTiles.Clear();
        allTiles.AddRange(FindObjectsOfType<Tile>());

        for (int i = 0; i < allTiles.Count; i++)
        {
            if (allTiles[i] != null)
            {
                allTiles[i].index = i;
            }
        }

        Debug.Log($"找到 {allTiles.Count} 个Tile");
    }

    public CropConfig GetCropConfig(string cropId)
    {
        if (cropDict != null && cropDict.ContainsKey(cropId))
            return cropDict[cropId];

        Debug.LogWarning("找不到作物配置：" + cropId);
        return null;
    }

    public void SaveFarmData()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= SaveManager.gameDatas.Length)
        {
            Debug.LogError("无效的存档槽位");
            return;
        }

        RefreshTileList();

        SaveFarmData saveData = new SaveFarmData
        {
            gridWidth = gridWidth,
            gridHeight = gridHeight,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            tiles = new List<SaveTileData>()
        };

        foreach (var tile in allTiles)
        {
            if (tile == null) continue;

            SaveTileData tileData = new SaveTileData
            {
                gridX = tile.gridX,
                gridY = tile.gridY,
                state = tile.currentState,
                obstacleType = tile.obstacleType,
                cropId = tile.cropData != null ? tile.cropData.cropId : "",
                remainingTime = tile.cropData != null ? tile.cropData.remainingTime : 0,
                totalGrowTime = tile.cropData != null ? tile.cropData.totalGrowTime : 0,
                isMature = tile.cropData != null ? tile.cropData.isMature : false
            };

            saveData.tiles.Add(tileData);
        }

        string json = JsonUtility.ToJson(saveData, true);

        GameData gameData = SaveManager.gameDatas[currentSlotIndex];
        if (gameData == null)
        {
            gameData = new GameData();
            SaveManager.gameDatas[currentSlotIndex] = gameData;
        }

        gameData.farmDataJson = json;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SaveToGameData();
        }

        SaveManager.SaveGame(currentSlotIndex, gameData);
        Debug.Log("农场数据已保存");
    }

    public void LoadFarmData()
    {
        GameData gameData = SaveManager.gameDatas[currentSlotIndex];

        if (gameData == null || string.IsNullOrEmpty(gameData.farmDataJson))
        {
            Debug.Log("没有找到农场存档，使用默认设置");
            return;
        }

        SaveFarmData saveData = JsonUtility.FromJson<SaveFarmData>(gameData.farmDataJson);

        if (saveData == null || saveData.tiles == null)
        {
            Debug.LogError("农场存档数据解析失败");
            return;
        }

        Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();
        foreach (var tile in allTiles)
        {
            Vector2Int pos = new Vector2Int(tile.gridX, tile.gridY);
            if (!tileMap.ContainsKey(pos))
            {
                tileMap[pos] = tile;
            }
        }

        int restoredCount = 0;
        foreach (var saveTile in saveData.tiles)
        {
            Vector2Int pos = new Vector2Int(saveTile.gridX, saveTile.gridY);

            if (tileMap.TryGetValue(pos, out Tile tile))
            {
                // 关键：先恢复基础状态
                tile.currentState = saveTile.state;
                tile.obstacleType = saveTile.obstacleType;

                // 根据是否有作物数据恢复作物
                if (!string.IsNullOrEmpty(saveTile.cropId))
                {
                    if (tile.cropData == null)
                    {
                        tile.cropData = new Tile.CropData(saveTile.cropId, saveTile.remainingTime, saveTile.totalGrowTime);
                    }
                    tile.cropData.cropId = saveTile.cropId;
                    tile.cropData.remainingTime = saveTile.remainingTime;
                    tile.cropData.totalGrowTime = saveTile.totalGrowTime;
                    tile.cropData.isMature = saveTile.isMature;

                    // 如果已经成熟，确保显示正确
                    if (tile.cropData.isMature)
                    {
                        tile.currentState = TileState.Planted;
                    }

                    Debug.Log($"加载作物: {saveTile.cropId}, 剩余: {saveTile.remainingTime:F1}秒, 成熟: {saveTile.isMature}");
                }
                else
                {
                    tile.cropData = null;
                }

                tile.UpdateVisual();
                restoredCount++;
            }
        }

        Debug.Log($"农场数据加载完成，共恢复 {restoredCount} 个地块");
    }

    public void ManualSave()
    {
        SaveFarmData();
        Debug.Log("游戏手动保存完成");
    }

    void OnApplicationQuit()
    {
        SaveFarmData();
    }
}