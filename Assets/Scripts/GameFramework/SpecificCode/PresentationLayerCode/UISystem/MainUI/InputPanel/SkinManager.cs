using UnityEngine;
using System.Collections.Generic;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("皮肤材质列表")]
    public Material[] skinMaterials;  // 在Inspector中拖入4个材质

    [Header("模型路径")]
    public string modelPath = "Prefabs/Character/playermodel";

    private int currentSkinIndex = 0;
    private GameObject currentPlayerModel;
    private SkinnedMeshRenderer targetRenderer;

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
        }
    }

    void Start()
    {
        // 加载存档中的皮肤索引
        LoadSkinFromSave();
    }

    /// <summary>
    /// 设置皮肤（0-3）
    /// </summary>
    public void SetSkin(int skinIndex)
    {
        if (skinMaterials == null || skinMaterials.Length == 0)
        {
            Debug.LogError("未设置皮肤材质列表");
            return;
        }

        currentSkinIndex = Mathf.Clamp(skinIndex, 0, skinMaterials.Length - 1);
        ApplySkinToCurrentModel();
        SaveSkinToSave();
    }

    /// <summary>
    /// 获取当前皮肤索引
    /// </summary>
    public int GetCurrentSkinIndex()
    {
        return currentSkinIndex;
    }

    /// <summary>
    /// 获取皮肤材质
    /// </summary>
    public Material GetCurrentSkinMaterial()
    {
        if (skinMaterials != null && currentSkinIndex < skinMaterials.Length)
            return skinMaterials[currentSkinIndex];
        return null;
    }

    /// <summary>
    /// 生成玩家模型到指定位置
    /// </summary>
    public GameObject SpawnPlayerModel(Transform parent, Vector3 position, Quaternion rotation)
    {
        // 加载模型
        GameObject modelPrefab = Resources.Load<GameObject>(modelPath);
        if (modelPrefab == null)
        {
            Debug.LogError($"找不到模型: {modelPath}");
            return null;
        }

        // 实例化模型
        currentPlayerModel = Instantiate(modelPrefab, position, rotation, parent);
        currentPlayerModel.name = "playermodel";

        // 查找 SkinnedMeshRenderer
        FindSkinnedMeshRenderer(currentPlayerModel.transform);

        // 应用皮肤材质
        ApplySkinToCurrentModel();

        return currentPlayerModel;
    }

    /// <summary>
    /// 更新已有模型的皮肤（用于游戏场景）
    /// </summary>
    public void UpdateSkinOnModel(GameObject model)
    {
        if (model == null) return;

        currentPlayerModel = model;
        FindSkinnedMeshRenderer(model.transform);
        ApplySkinToCurrentModel();
    }

    void FindSkinnedMeshRenderer(Transform root)
    {
        targetRenderer = null;

        // 查找 Peasant Nolant
        Transform peasant = DeepFindTransform.DeepFindChild(root, "Peasant Nolant");
        if (peasant != null)
        {
            targetRenderer = peasant.GetComponent<SkinnedMeshRenderer>();
            if (targetRenderer != null)
            {
                Debug.Log("找到 Peasant Nolant 的 SkinnedMeshRenderer");
                return;
            }
        }

        // 如果没找到，递归查找所有 SkinnedMeshRenderer
        SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length > 0)
        {
            targetRenderer = renderers[0];
            Debug.Log($"找到 SkinnedMeshRenderer: {targetRenderer.name}");
        }
        else
        {
            Debug.LogWarning("未找到 SkinnedMeshRenderer");
        }
    }

    void ApplySkinToCurrentModel()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning("targetRenderer 为空，无法应用皮肤");
            return;
        }

        if (skinMaterials == null || currentSkinIndex >= skinMaterials.Length)
        {
            Debug.LogWarning($"皮肤材质无效，索引: {currentSkinIndex}");
            return;
        }

        Material newMaterial = skinMaterials[currentSkinIndex];
        if (newMaterial != null)
        {
            targetRenderer.material = newMaterial;
            Debug.Log($"应用皮肤 {currentSkinIndex}: {newMaterial.name}");
        }
    }

    void SaveSkinToSave()
    {
        // 保存到 PlayerPrefs（临时方案）
        PlayerPrefs.SetInt("SelectedSkin", currentSkinIndex);
        PlayerPrefs.Save();

        // 同时保存到存档数据中（如果有当前存档）
        if (ChoicePanel.currentGameData != null && ChoicePanel.currentGameData.playerData != null)
        {
            ChoicePanel.currentGameData.playerData.skinIndex = currentSkinIndex;
        }
    }

    void LoadSkinFromSave()
    {
        // 从 PlayerPrefs 加载
        if (PlayerPrefs.HasKey("SelectedSkin"))
        {
            currentSkinIndex = PlayerPrefs.GetInt("SelectedSkin");
            Debug.Log($"从 PlayerPrefs 加载皮肤索引: {currentSkinIndex}");
        }
        else if (ChoicePanel.currentGameData != null && ChoicePanel.currentGameData.playerData != null)
        {
            currentSkinIndex = ChoicePanel.currentGameData.playerData.skinIndex;
            Debug.Log($"从存档加载皮肤索引: {currentSkinIndex}");
        }
        else
        {
            currentSkinIndex = 0;
        }
    }
}