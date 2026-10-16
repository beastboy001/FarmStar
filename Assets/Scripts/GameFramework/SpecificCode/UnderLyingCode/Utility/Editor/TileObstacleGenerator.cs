using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TileObstacleSetter : EditorWindow
{
    [Header("杂物比例设置")]
    private float treeRatio = 0.30f;      // 树的比例 30%
    private float rockRatio = 0.20f;      // 石头比例 20%
    private float weedRatio = 0.25f;      // 杂草比例 25%

    [Header("设置范围")]
    private bool affectAll = true;         // 设置所有Tile
    private bool affectSelected = false;   // 只设置选中的Tile

    [Header("预览")]
    private bool showPreview = true;

    [Header("父物体中心")]
    private bool setParentCenter = false;
    private string parentGroupName = "Farm_Group";

    private Vector2 scrollPosition;
    private List<TileInfo> tileInfos = new List<TileInfo>();

    private class TileInfo
    {
        public Tile tile;
        public string name;
        public ObstacleType type;
        public TileState state;
    }

    [MenuItem("Tools/Tile杂物设置器")]
    public static void ShowWindow()
    {
        GetWindow<TileObstacleSetter>("Tile杂物设置器");
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("杂物比例设置", EditorStyles.boldLabel);
        GUILayout.Space(5);

        treeRatio = EditorGUILayout.Slider("树 (%)", treeRatio * 100, 0, 100) / 100;
        rockRatio = EditorGUILayout.Slider("石头 (%)", rockRatio * 100, 0, 100) / 100;
        weedRatio = EditorGUILayout.Slider("杂草 (%)", weedRatio * 100, 0, 100) / 100;

        float emptyRatio = 1f - treeRatio - rockRatio - weedRatio;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("空地比例", (emptyRatio * 100).ToString("F1") + "%", EditorStyles.helpBox);

        float total = treeRatio + rockRatio + weedRatio;
        if (total > 1.01f)
        {
            EditorGUILayout.HelpBox("杂物比例总和超过100%，请调整！", MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox($"分布：树 {treeRatio * 100:F0}%  石头 {rockRatio * 100:F0}%  杂草 {weedRatio * 100:F0}%  空地 {emptyRatio * 100:F0}%", MessageType.Info);
        }

        GUILayout.Space(10);
        GUILayout.Label("设置范围", EditorStyles.boldLabel);

        affectAll = EditorGUILayout.Toggle("设置所有Tile", affectAll);
        affectSelected = EditorGUILayout.Toggle("只设置选中的Tile", affectSelected);

        if (affectSelected && affectAll)
        {
            EditorGUILayout.HelpBox("不能同时勾选两者，请只选一个", MessageType.Warning);
        }

        GUILayout.Space(10);
        GUILayout.Label("父物体中心设置", EditorStyles.boldLabel);

        setParentCenter = EditorGUILayout.Toggle("创建父物体组（将中心设为所有Tile的中心）", setParentCenter);

        if (setParentCenter)
        {
            EditorGUI.indentLevel++;
            parentGroupName = EditorGUILayout.TextField("父物体组名", parentGroupName);
            EditorGUILayout.HelpBox(
                "功能说明：\n" +
                "1. 创建一个父物体，位置设为所有Tile的中心点\n" +
                "2. 将所有Tile设为该父物体的子物体\n" +
                "3. 方便整体移动、旋转农场",
                MessageType.Info);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(10);

        List<Tile> allTiles = GetTargetTiles();
        EditorGUILayout.LabelField($"当前目标Tile数量：{allTiles.Count}");

        GUILayout.Space(10);

        if (GUILayout.Button("预览随机分布", GUILayout.Height(30)))
        {
            PreviewDistribution();
        }

        if (showPreview && tileInfos.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("预览（前30个）", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(120));

            for (int i = 0; i < Mathf.Min(30, tileInfos.Count); i++)
            {
                var info = tileInfos[i];
                GUI.color = GetColorForType(info.type);
                EditorGUILayout.LabelField($"{info.name} : {GetTypeName(info.type)} -> 状态: {info.state}");
            }

            GUI.color = Color.white;
            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        GUI.enabled = (total <= 1.01f);
        if (GUILayout.Button("按比例设置杂物类型", GUILayout.Height(40)))
        {
            SetObstaclesByRatio();
        }
        GUI.enabled = true;

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("全部设为空地", GUILayout.Height(30)))
        {
            SetAllToEmpty();
        }
        if (GUILayout.Button("全部设为树", GUILayout.Height(30)))
        {
            SetAllToType(ObstacleType.Tree);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("全部设为石头", GUILayout.Height(30)))
        {
            SetAllToType(ObstacleType.Rock);
        }
        if (GUILayout.Button("全部设为杂草", GUILayout.Height(30)))
        {
            SetAllToType(ObstacleType.Weed);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (setParentCenter)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("创建/更新父物体中心", GUILayout.Height(35)))
            {
                SetParentToCenter();
            }
            if (GUILayout.Button("解除父物体关系", GUILayout.Height(35)))
            {
                UnparentTiles();
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "状态设置规则：\n" +
            "- obstacleType = None → currentState = Empty（空地）\n" +
            "- obstacleType = Tree/Rock/Weed → currentState = Obstacle（有杂物）\n\n" +
            "设置后运行游戏，Tile会根据新的数据自动生成对应模型。",
            MessageType.Info);
    }

    void PreviewDistribution()
    {
        List<Tile> tiles = GetTargetTiles();
        if (tiles.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到Tile", "确定");
            return;
        }

        tileInfos.Clear();
        System.Random rand = new System.Random();

        for (int i = 0; i < Mathf.Min(100, tiles.Count); i++)
        {
            var tile = tiles[i];
            ObstacleType type = GetRandomType(rand);
            // 关键：根据obstacleType设置currentState
            TileState state = (type == ObstacleType.None) ? TileState.Empty : TileState.Obstacle;

            tileInfos.Add(new TileInfo
            {
                tile = tile,
                name = tile.gameObject.name,
                type = type,
                state = state
            });
        }

        showPreview = true;

        int treeCount = 0, rockCount = 0, weedCount = 0, emptyCount = 0;
        foreach (var info in tileInfos)
        {
            if (info.type == ObstacleType.Tree) treeCount++;
            else if (info.type == ObstacleType.Rock) rockCount++;
            else if (info.type == ObstacleType.Weed) weedCount++;
            else emptyCount++;
        }

        Debug.Log($"预览统计（{tileInfos.Count}个样本）：树{treeCount} 石头{rockCount} 杂草{weedCount} 空地{emptyCount}");
    }

    void SetObstaclesByRatio()
    {
        List<Tile> tiles = GetTargetTiles();
        if (tiles.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到Tile", "确定");
            return;
        }

        float total = treeRatio + rockRatio + weedRatio;
        if (total > 1.01f)
        {
            EditorUtility.DisplayDialog("错误", "杂物比例总和超过100%，请调整", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认设置",
            $"将按比例设置 {tiles.Count} 个Tile的杂物类型，确定继续吗？\n\n" +
            $"树: {treeRatio * 100:F0}%  石头: {rockRatio * 100:F0}%  杂草: {weedRatio * 100:F0}%  空地: {(1 - total) * 100:F0}%",
            "确定", "取消"))
        {
            return;
        }

        Undo.RecordObjects(tiles.ToArray(), "批量设置杂物类型");

        System.Random rand = new System.Random();
        int treeCount = 0, rockCount = 0, weedCount = 0, emptyCount = 0;

        foreach (var tile in tiles)
        {
            ObstacleType type = GetRandomType(rand);

            // 设置杂物类型
            tile.obstacleType = type;

            // 关键：根据obstacleType设置currentState
            if (type == ObstacleType.None)
            {
                tile.currentState = TileState.Empty;
                emptyCount++;
            }
            else
            {
                tile.currentState = TileState.Obstacle;
                if (type == ObstacleType.Tree) treeCount++;
                else if (type == ObstacleType.Rock) rockCount++;
                else if (type == ObstacleType.Weed) weedCount++;
            }

            // 清除作物数据
            tile.cropData = null;

            EditorUtility.SetDirty(tile);
        }

        Debug.Log($"杂物类型设置完成！共 {tiles.Count} 个Tile\n" +
                  $"树: {treeCount} ({treeCount * 100f / tiles.Count:F1}%)\n" +
                  $"石头: {rockCount} ({rockCount * 100f / tiles.Count:F1}%)\n" +
                  $"杂草: {weedCount} ({weedCount * 100f / tiles.Count:F1}%)\n" +
                  $"空地: {emptyCount} ({emptyCount * 100f / tiles.Count:F1}%)");

        EditorUtility.DisplayDialog("完成",
            $"已设置 {tiles.Count} 个Tile的杂物类型\n\n" +
            $"树: {treeCount}\n石头: {rockCount}\n杂草: {weedCount}\n空地: {emptyCount}",
            "确定");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    /// <summary>
    /// 全部设为空地
    /// obstacleType = None, currentState = Empty
    /// </summary>
    void SetAllToEmpty()
    {
        List<Tile> tiles = GetTargetTiles();
        if (tiles.Count == 0) return;

        if (!EditorUtility.DisplayDialog("确认清空", $"将 {tiles.Count} 个Tile设为空地", "确定", "取消"))
            return;

        Undo.RecordObjects(tiles.ToArray(), "全部设为空地");

        foreach (var tile in tiles)
        {
            tile.obstacleType = ObstacleType.None;
            tile.currentState = TileState.Empty;  // 空地状态
            tile.cropData = null;
            EditorUtility.SetDirty(tile);
        }

        Debug.Log($"已将 {tiles.Count} 个Tile设为空地");
        EditorUtility.DisplayDialog("完成", $"已将 {tiles.Count} 个Tile设为空地", "确定");
    }

    /// <summary>
    /// 全部设为指定杂物类型
    /// obstacleType = 指定类型, currentState = Obstacle
    /// </summary>
    void SetAllToType(ObstacleType type)
    {
        string typeName = GetTypeName(type);
        List<Tile> tiles = GetTargetTiles();
        if (tiles.Count == 0) return;

        if (!EditorUtility.DisplayDialog("确认设置", $"将所有Tile设为{typeName}，确定继续吗？", "确定", "取消"))
            return;

        Undo.RecordObjects(tiles.ToArray(), $"全部设为{typeName}");

        foreach (var tile in tiles)
        {
            tile.obstacleType = type;
            tile.currentState = TileState.Obstacle;  // 有杂物状态
            tile.cropData = null;
            EditorUtility.SetDirty(tile);
        }

        Debug.Log($"已将 {tiles.Count} 个Tile设为{typeName}");
        EditorUtility.DisplayDialog("完成", $"已将 {tiles.Count} 个Tile设为{typeName}", "确定");
    }

    void SetParentToCenter()
    {
        List<Tile> tiles = GetTargetTiles();
        if (tiles.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到Tile", "确定");
            return;
        }

        GameObject parent = GameObject.Find(parentGroupName);
        if (parent == null)
        {
            parent = new GameObject(parentGroupName);
            Undo.RegisterCreatedObjectUndo(parent, "创建父物体组");
            Debug.Log($"创建父物体：{parentGroupName}");
        }

        Vector3 center = GetTilesCenter(tiles);
        parent.transform.position = center;

        foreach (var tile in tiles)
        {
            Vector3 worldPos = tile.transform.position;
            Undo.SetTransformParent(tile.transform, parent.transform, "设置父物体");
            tile.transform.position = worldPos;
        }

        Debug.Log($"✅ 已将 {tiles.Count} 个Tile放入父物体 '{parentGroupName}'");
        Debug.Log($"父物体位置（所有Tile的中心）：{center}");

        EditorUtility.DisplayDialog("完成",
            $"已将 {tiles.Count} 个Tile放入父物体 '{parentGroupName}'\n\n" +
            $"父物体位置（中心点）：{center}\n\n" +
            "现在可以整体移动、旋转这个父物体来调整整个农场的位置。",
            "确定");
    }

    void UnparentTiles()
    {
        GameObject parent = GameObject.Find(parentGroupName);
        if (parent == null)
        {
            EditorUtility.DisplayDialog("提示", $"没有找到父物体 '{parentGroupName}'", "确定");
            return;
        }

        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent.transform)
        {
            children.Add(child);
        }

        if (children.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "父物体下没有子物体", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认解除",
            $"将解除 {children.Count} 个Tile的父物体关系，确定继续吗？\n\n注意：Tile将保持当前世界位置。",
            "确定", "取消"))
        {
            return;
        }

        foreach (var child in children)
        {
            Vector3 worldPos = child.position;
            Undo.SetTransformParent(child, null, "解除父物体关系");
            child.position = worldPos;
        }

        Undo.DestroyObjectImmediate(parent);

        Debug.Log($"✅ 已解除 {children.Count} 个Tile的父物体关系");
        EditorUtility.DisplayDialog("完成", $"已解除 {children.Count} 个Tile的父物体关系", "确定");
    }

    Vector3 GetTilesCenter(List<Tile> tiles)
    {
        if (tiles.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var tile in tiles)
        {
            sum += tile.transform.position;
        }

        return sum / tiles.Count;
    }

    List<Tile> GetTargetTiles()
    {
        List<Tile> tiles = new List<Tile>();

        if (affectSelected)
        {
            foreach (var obj in Selection.gameObjects)
            {
                Tile tile = obj.GetComponent<Tile>();
                if (tile != null) tiles.Add(tile);
            }

            if (tiles.Count == 0)
            {
                Debug.LogWarning("没有选中的Tile，请先在Hierarchy中选择Tile物体");
            }
        }
        else
        {
            tiles.AddRange(FindObjectsOfType<Tile>());
        }

        return tiles;
    }

    ObstacleType GetRandomType(System.Random rand)
    {
        double value = rand.NextDouble();

        if (value < treeRatio)
            return ObstacleType.Tree;
        else if (value < treeRatio + rockRatio)
            return ObstacleType.Rock;
        else if (value < treeRatio + rockRatio + weedRatio)
            return ObstacleType.Weed;
        else
            return ObstacleType.None;
    }

    string GetTypeName(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Tree: return "🌲 树";
            case ObstacleType.Rock: return "🪨 石头";
            case ObstacleType.Weed: return "🌿 杂草";
            default: return "⬜ 空地";
        }
    }

    Color GetColorForType(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Tree: return new Color(0.3f, 0.7f, 0.3f);
            case ObstacleType.Rock: return new Color(0.6f, 0.6f, 0.6f);
            case ObstacleType.Weed: return new Color(0.4f, 0.8f, 0.4f);
            default: return Color.gray;
        }
    }
}