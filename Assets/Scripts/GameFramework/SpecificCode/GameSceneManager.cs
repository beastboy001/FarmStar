using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI显示")]
    public Text playerNameText;
    public Text farmNameText;
    public Text timeText;

    [Header("玩家设置")]
    public GameObject playerPrefab;  // 玩家预制体（如果有）
    public Transform spawnPoint;

    private GameData gameData;
    private GameObject currentPlayer;
    private SkinnedMeshRenderer targetRenderer;

    void Start()
    {
        GetSaveData();

        if (gameData != null)
        {
            InitializeGame();
        }
        else
        {
            Debug.LogError("无法获取存档数据！");
        }
    }

    void GetSaveData()
    {
        gameData = ChoicePanel.currentGameData;

        if (gameData != null)
        {
            PlayerData playerData = gameData.playerData;
            Debug.Log($"获取到存档数据：玩家 {playerData.playerName}，农场 {playerData.farmName}，皮肤 {playerData.skinIndex}");
        }
        else
        {
            Debug.LogWarning("静态变量为空，尝试从文件加载...");
            if (SaveManager.IsSaveExists(0))
            {
                gameData = SaveManager.LoadGame(0);
            }
        }
    }

    void InitializeGame()
    {
        PlayerData playerData = gameData.playerData;

        ShowPlayerInfo(playerData);
        FindAndApplySkin(playerData);
        InitializeGameState(gameData);
    }

    void ShowPlayerInfo(PlayerData data)
    {
        if (playerNameText != null)
            playerNameText.text = data.playerName;

        if (farmNameText != null)
            farmNameText.text = data.farmName;

        if (timeText != null)
            timeText.text = $"第{gameData.timeData.year}年 {GetSeasonName(gameData.timeData.season)} 第{gameData.timeData.day}天";
    }

    string GetSeasonName(int season)
    {
        string[] seasons = { "春天", "夏天", "秋天", "冬天" };
        return seasons[Mathf.Clamp(season, 0, 3)];
    }

    /// <summary>
    /// 找到游戏场景中的玩家模型并应用皮肤
    /// </summary>
    void FindAndApplySkin(PlayerData data)
    {
        // 查找游戏场景中的玩家模型
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindObjectOfType<PlayerMoveControl2>()?.gameObject;
        }

        if (player == null && playerPrefab != null && spawnPoint != null)
        {
            player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        if (player == null)
        {
            Debug.LogWarning("找不到玩家对象，无法应用皮肤");
            return;
        }

        currentPlayer = player;

        // 查找 playermodel
        Transform playerModel = FindPlayerModel(currentPlayer.transform);
        if (playerModel == null)
        {
            Debug.LogWarning("找不到 playermodel");
            return;
        }

        // 查找 Peasant Nolant
        Transform peasant = playerModel.Find("Peasant Nolant");
        if (peasant == null)
        {
            peasant = DeepFindTransform.DeepFindChild(playerModel, "Peasant Nolant");
        }

        if (peasant == null)
        {
            Debug.LogWarning("找不到 Peasant Nolant");
            return;
        }

        targetRenderer = peasant.GetComponent<SkinnedMeshRenderer>();
        if (targetRenderer == null)
        {
            Debug.LogWarning("Peasant Nolant 没有 SkinnedMeshRenderer");
            return;
        }

        // 应用皮肤
        ApplySkinByIndex(data.skinIndex);
    }

    /// <summary>
    /// 查找玩家模型
    /// </summary>
    Transform FindPlayerModel(Transform root)
    {
        Transform model = root.Find("playermodel");
        if (model != null) return model;

        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "playermodel")
            {
                return child;
            }
        }

        return root;
    }

    /// <summary>
    /// 根据皮肤索引应用材质
    /// </summary>
    void ApplySkinByIndex(int skinIndex)
    {
        if (targetRenderer == null) return;

        string[] skinNames = { "Yellow", "Blue", "Green", "Red" };
        string materialPath = $"Materials/character/Peasant Nolant {skinNames[skinIndex]}";
        Material material = Resources.Load<Material>(materialPath);

        if (material != null)
        {
            targetRenderer.material = material;
            Debug.Log($"游戏场景应用皮肤 {skinIndex}: {skinNames[skinIndex]}");
        }
        else
        {
            Debug.LogWarning($"找不到材质: {materialPath}");
        }
    }

    void InitializeGameState(GameData data)
    {
        Debug.Log("游戏初始化完成！");
    }

    public void SaveGame()
    {
        if (gameData != null)
        {
            UpdateGameData();
            SaveManager.SaveGame(ChoicePanel.currentSelectedSlot, gameData);
            Debug.Log("游戏已保存");
        }
    }

    void UpdateGameData()
    {
        if (currentPlayer != null)
        {
            Vector3 pos = currentPlayer.transform.position;
            gameData.playerData.posX = pos.x;
            gameData.playerData.posY = pos.y;
            gameData.playerData.posZ = pos.z;
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}