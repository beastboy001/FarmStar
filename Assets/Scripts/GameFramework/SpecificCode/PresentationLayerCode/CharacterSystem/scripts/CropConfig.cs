using UnityEngine;

[CreateAssetMenu(fileName = "新作物", menuName = "游戏数据/作物配置")]
public class CropConfig : ScriptableObject
{
    public string cropId;
    public string cropName;

    [Header("生长模型")]
    public GameObject seedPrefab;
    public GameObject growingPrefab;
    public GameObject maturePrefab;

    [Header("生长时间（秒）")]
    [Tooltip("从种植到成熟需要的现实秒数")]
    public float growSeconds = 60f;

    [Header("收获物品")]
    public string harvestItemId;
    public int minHarvest = 1;
    public int maxHarvest = 3;
}