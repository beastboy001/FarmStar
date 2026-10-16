using UnityEngine;
using System;

public enum ItemType
{
    Crop, Seed, Tool, Material, Food
}

public enum ToolType
{
    None, Hoe, Axe, Pickaxe, Scythe, WateringCan
}

[CreateAssetMenu(fileName = "新物品", menuName = "游戏数据/物品配置")]
public class ItemConfig : ScriptableObject
{
    [Header("基础信息")]
    public string itemId;
    public string itemName;
    [TextArea] public string description;
    [TextArea] public string acquisitionMethod;
    public Sprite icon;

    [Header("类型")]
    public ItemType itemType;
    public ToolType toolType = ToolType.None;

    [Header("堆叠")]
    public int maxStack = 99;

    [Header("价格")]
    public int buyPrice;
    public int sellPrice;

    [Header("工具属性（仅工具）")]
    [Tooltip("砍树效率倍数（越大越快）")]
    public float treeEfficiency = 1f;
    [Tooltip("挖石效率倍数（越大越快）")]
    public float rockEfficiency = 1f;
    [Tooltip("除草效率倍数（越大越快）")]
    public float weedEfficiency = 1f;

    [Header("作物属性（仅种子）")]
    public string cropId;
    public int growTime;
}