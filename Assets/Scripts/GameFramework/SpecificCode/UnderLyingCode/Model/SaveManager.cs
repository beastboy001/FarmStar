using System;
using System.Collections.Generic;
using UnityEngine;

// ========== 存档管理器 ==========
public static class SaveManager
{
    private static string savePath = Application.persistentDataPath + "/saves/";
    public static GameData[] gameDatas = new GameData[4];

    static SaveManager()
    {
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }
    }

    public static GameData CreateNewGameData()
    {
        GameData newData = new GameData
        {
            playerData = new PlayerData
            {
                playerName = "新玩家",
                farmName = "新农场",
                isMale = true,
                level = 1,
                exp = 0,
                gold = 500,
                stamina = 100,
                maxStamina = 100,
                posX = 0,
                posY = 1,
                posZ = 0,
                currentScene = "Farm",
                skinIndex = 0
            },
            timeData = new TimeData
            {
                year = 1,
                season = 0,
                day = 1,
                hour = 6,
                minute = 0,
                totalDays = 1
            },
            farmData = new FarmData(),
            inventoryData = new InventoryData(),
            questSaveData = new QuestSaveData()
        };

        if (newData.inventoryData.slots == null)
            newData.inventoryData.slots = new List<ItemSlot>();

        return newData;
    }

    public static void DeleteSave(int slotIndex)
    {
        string filePath = savePath + "save_" + slotIndex + ".json";
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            gameDatas[slotIndex] = null;
            Debug.Log("删除存档" + (slotIndex + 1));
        }
    }

    public static void SaveGame(int slotIndex, GameData data)
    {
        try
        {
            data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            string filePath = savePath + "save_" + slotIndex + ".json";
            System.IO.File.WriteAllText(filePath, json);
            gameDatas[slotIndex] = data;
            Debug.Log("存档" + (slotIndex + 1) + "保存成功");
        }
        catch (Exception e)
        {
            Debug.LogError("保存失败：" + e.Message);
        }
    }

    public static GameData LoadGame(int slotIndex)
    {
        if (gameDatas[slotIndex] != null)
            return gameDatas[slotIndex];

        string filePath = savePath + "save_" + slotIndex + ".json";

        if (!System.IO.File.Exists(filePath))
        {
            Debug.Log("存档" + (slotIndex + 1) + "不存在，返回空数据");
            return null;
        }

        try
        {
            string json = System.IO.File.ReadAllText(filePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data.questSaveData == null)
                data.questSaveData = new QuestSaveData();
            if (data.inventoryData == null)
                data.inventoryData = new InventoryData();
            if (data.inventoryData.slots == null)
                data.inventoryData.slots = new List<ItemSlot>();

            gameDatas[slotIndex] = data;
            Debug.Log("加载存档" + (slotIndex + 1) + "成功：" + data.playerData.playerName);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("加载失败：" + e.Message);
            return null;
        }
    }

    public static bool IsSaveExists(int slotIndex)
    {
        string filePath = savePath + "save_" + slotIndex + ".json";
        return System.IO.File.Exists(filePath);
    }
}

// ========== 游戏数据主类 ==========
[Serializable]
public class GameData
{
    public string saveTime;
    public float totalPlayTime;
    public PlayerData playerData = new PlayerData();
    public TimeData timeData = new TimeData();
    public FarmData farmData = new FarmData();
    public InventoryData inventoryData = new InventoryData();
    public string farmDataJson = "";
    public QuestSaveData questSaveData;

    public GameData()
    {
        saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        totalPlayTime = 0;
        if (inventoryData == null) inventoryData = new InventoryData();
        if (inventoryData.slots == null) inventoryData.slots = new List<ItemSlot>();
        if (questSaveData == null) questSaveData = new QuestSaveData();
    }
}

// ========== 玩家数据 ==========
[Serializable]
public class PlayerData
{
    public string playerName;
    public string farmName;
    public bool isMale;
    public int level = 1;
    public int exp = 0;
    public int gold = 500;
    public int stamina = 100;
    public int maxStamina = 100;
    public float posX;
    public float posY;
    public float posZ;
    public string currentScene = "Farm";
    public int farmingLevel = 1;
    public int miningLevel = 1;
    public int fishingLevel = 1;
    public int foragingLevel = 1;
    public int skinIndex = 0;

    public PlayerData()
    {
        playerName = "";
        farmName = "";
        isMale = true;
        skinIndex = 0;
    }
}

// ========== 时间数据 ==========
[Serializable]
public class TimeData
{
    public int year = 1;
    public int season = 0;
    public int day = 1;
    public int hour = 6;
    public int minute = 0;
    public int totalDays = 1;
    public enum Weather { Sunny, Rainy, Storm, Snow }
    public Weather currentWeather = Weather.Sunny;
    public Weather tomorrowWeather = Weather.Sunny;
}

// ========== 农场数据 ==========
[Serializable]
public class FarmData
{
    public List<AnimalData> animals = new List<AnimalData>();
    public List<BuildingData> buildings = new List<BuildingData>();
}

[Serializable]
public class AnimalData
{
    public string animalId;
    public string animalName;
    public float posX;
    public float posY;
    public float posZ;
    public int friendship;
    public bool isFed;
    public bool isPet;
}

[Serializable]
public class BuildingData
{
    public string buildingId;
    public float posX;
    public float posY;
    public float posZ;
    public int level;
    public bool isConstructed;
}

// ========== 背包数据 ==========
[Serializable]
public class InventoryData
{
    public List<ItemSlot> slots = new List<ItemSlot>();
    public int maxSlots = 20;
    public int gold = 0;
    public InventoryData()
    {
        if (slots == null) slots = new List<ItemSlot>();
    }
}

[Serializable]
public class ItemSlot
{
    public string itemId;
    public int quantity;
    public int slotIndex;
    public ItemSlot(string id, int qty)
    {
        itemId = id;
        quantity = qty;
        slotIndex = -1;
    }
    public bool IsEmpty => quantity <= 0 || string.IsNullOrEmpty(itemId);
    public void Clear()
    {
        itemId = "";
        quantity = 0;
        slotIndex = -1;
    }
}

// ========== 地块存档 ==========
public enum TileState { Obstacle, Empty, Plowed, Planted }
public enum ObstacleType { None, Tree, Rock, Weed }

[Serializable]
public class SaveTileData
{
    public int gridX;
    public int gridY;
    public TileState state;
    public ObstacleType obstacleType;
    public string cropId;
    public float remainingTime;
    public float totalGrowTime;
    public bool isMature;
}

[Serializable]
public class SaveFarmData
{
    public int gridWidth;
    public int gridHeight;
    public string saveTime;
    public int gameDay;
    public int gameHour;
    public int gameMinute;
    public List<SaveTileData> tiles = new List<SaveTileData>();
} 