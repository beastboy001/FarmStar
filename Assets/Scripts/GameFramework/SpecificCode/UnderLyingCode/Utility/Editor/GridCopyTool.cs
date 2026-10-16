using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridCopyTool : EditorWindow
{
    private GameObject sourcePrefab;      // 要复制的预制体
    private int width = 5;                 // 宽度方向数量
    private int height = 5;                // 高度方向数量
    private float spacing = 1.0f;          // 间距
    private Vector3 startPosition = Vector3.zero;  // 起始位置
    private string groupName = "GridGroup";        // 组名

    // 缩放参数
    private float scaleMultiplier = 1.0f;           // 整体缩放倍数
    private bool randomScale = false;                // 是否随机缩放
    private float minRandomScale = 0.8f;             // 最小随机缩放
    private float maxRandomScale = 1.2f;             // 最大随机缩放

    // 旋转参数
    private bool randomRotation = false;              // 是否随机旋转
    private bool rotateOnYAxis = true;                // 是否绕Y轴旋转
    private bool rotateOnXAxis = false;               // 是否绕X轴旋转
    private bool rotateOnZAxis = false;               // 是否绕Z轴旋转
    private float minRandomRotation = 0f;              // 最小旋转角度
    private float maxRandomRotation = 360f;            // 最大旋转角度

    // ===== 【新增】父物体中心参数 =====
    private bool setParentToCenter = true;             // 是否将父物体设为中心
    private string centerParentName = "CenterGroup";   // 中心父物体名称

    [MenuItem("Tools/网格复制工具")]
    public static void ShowWindow()
    {
        GetWindow<GridCopyTool>("网格复制工具");
    }

    void OnGUI()
    {
        GUILayout.Label("网格复制设置", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // 预制体选择
        sourcePrefab = (GameObject)EditorGUILayout.ObjectField(
            "源预制体",
            sourcePrefab,
            typeof(GameObject),
            false
        );

        EditorGUILayout.Space(5);

        // 网格参数
        width = EditorGUILayout.IntField("宽度数量 (X轴)", width);
        height = EditorGUILayout.IntField("高度数量 (Z轴)", height);
        spacing = EditorGUILayout.FloatField("间距", spacing);
        startPosition = EditorGUILayout.Vector3Field("起始位置", startPosition);

        EditorGUILayout.Space(10);

        // ===== 缩放设置区域 =====
        GUILayout.Label("缩放设置", EditorStyles.boldLabel);

        scaleMultiplier = EditorGUILayout.FloatField("整体缩放倍数", scaleMultiplier);

        EditorGUILayout.Space(5);

        randomScale = EditorGUILayout.Toggle("随机缩放", randomScale);

        if (randomScale)
        {
            EditorGUI.indentLevel++;
            minRandomScale = EditorGUILayout.FloatField("最小随机倍数", minRandomScale);
            maxRandomScale = EditorGUILayout.FloatField("最大随机倍数", maxRandomScale);
            EditorGUI.indentLevel--;

            if (minRandomScale > maxRandomScale)
            {
                minRandomScale = maxRandomScale;
            }
        }

        EditorGUILayout.Space(10);

        // ===== 旋转设置区域 =====
        GUILayout.Label("旋转设置", EditorStyles.boldLabel);

        randomRotation = EditorGUILayout.Toggle("随机旋转", randomRotation);

        if (randomRotation)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("旋转轴", EditorStyles.boldLabel);
            rotateOnYAxis = EditorGUILayout.Toggle("绕Y轴旋转 (水平)", rotateOnYAxis);
            rotateOnXAxis = EditorGUILayout.Toggle("绕X轴旋转 (俯仰)", rotateOnXAxis);
            rotateOnZAxis = EditorGUILayout.Toggle("绕Z轴旋转 (倾斜)", rotateOnZAxis);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("旋转角度范围", EditorStyles.boldLabel);
            minRandomRotation = EditorGUILayout.FloatField("最小角度", minRandomRotation);
            maxRandomRotation = EditorGUILayout.FloatField("最大角度", maxRandomRotation);

            if (minRandomRotation > maxRandomRotation)
            {
                minRandomRotation = maxRandomRotation;
            }

            if (!rotateOnYAxis && !rotateOnXAxis && !rotateOnZAxis)
            {
                EditorGUILayout.HelpBox("请至少选择一个旋转轴", MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ===== 【新增】父物体中心设置区域 =====
        GUILayout.Label("父物体中心设置", EditorStyles.boldLabel);

        setParentToCenter = EditorGUILayout.Toggle("将父物体设为中心", setParentToCenter);

        if (setParentToCenter)
        {
            EditorGUI.indentLevel++;
            centerParentName = EditorGUILayout.TextField("中心父物体名称", centerParentName);
            EditorGUILayout.HelpBox(
                "功能说明：\n" +
                "1. 生成的所有物体会先放入一个组\n" +
                "2. 再创建一个中心父物体，位置设为所有物体的中心\n" +
                "3. 方便整体移动、旋转整个网格",
                MessageType.Info);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // 组设置
        groupName = EditorGUILayout.TextField("网格组名", groupName);

        EditorGUILayout.Space(10);

        // 显示统计
        int totalCount = width * height;
        string infoText = $"将生成 {totalCount} 个物体\n";
        infoText += $"整体缩放: {scaleMultiplier}x";
        if (randomScale) infoText += $"\n随机缩放: {minRandomScale}x - {maxRandomScale}x";
        if (randomRotation) infoText += $"\n随机旋转: {minRandomRotation}° - {maxRandomRotation}° (轴: {(rotateOnYAxis ? "Y" : "")}{(rotateOnXAxis ? "X" : "")}{(rotateOnZAxis ? "Z" : "")})";
        if (setParentToCenter) infoText += $"\n父物体中心: {centerParentName} (位置为所有物体的中心)";

        EditorGUILayout.HelpBox(infoText, MessageType.Info);

        EditorGUILayout.Space(10);

        // 生成按钮
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("生成网格", GUILayout.Height(40)))
        {
            GenerateGrid();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("清除所有", GUILayout.Height(30)))
        {
            ClearAll();
        }

        GUI.backgroundColor = Color.white;
    }

    void GenerateGrid()
    {
        // 检查预制体
        if (sourcePrefab == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择要复制的预制体！", "确定");
            return;
        }

        // 检查数量
        if (width <= 0 || height <= 0)
        {
            EditorUtility.DisplayDialog("错误", "宽度和高度必须大于0！", "确定");
            return;
        }

        // 检查随机缩放范围
        if (randomScale && minRandomScale > maxRandomScale)
        {
            EditorUtility.DisplayDialog("错误", "最小随机倍数不能大于最大随机倍数！", "确定");
            return;
        }

        // 检查旋转轴
        if (randomRotation && !rotateOnYAxis && !rotateOnXAxis && !rotateOnZAxis)
        {
            EditorUtility.DisplayDialog("错误", "请至少选择一个旋转轴！", "确定");
            return;
        }

        // 查找或创建网格组
        GameObject gridGroup = GameObject.Find(groupName);
        if (gridGroup == null)
        {
            gridGroup = new GameObject(groupName);
            Undo.RegisterCreatedObjectUndo(gridGroup, "创建网格组");
        }

        // 记录生成的物体
        List<GameObject> generatedObjects = new List<GameObject>();

        // 计算起始偏移
        float startX = startPosition.x;
        float startZ = startPosition.z;

        // 生成网格
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // 计算位置
                float posX = startX + x * spacing * scaleMultiplier;
                float posZ = startZ + z * spacing * scaleMultiplier;
                Vector3 position = new Vector3(posX, startPosition.y, posZ);

                // 实例化物体
                GameObject newObj;

                if (PrefabUtility.IsPartOfAnyPrefab(sourcePrefab))
                {
                    newObj = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
                    newObj.transform.position = position;
                }
                else
                {
                    newObj = Instantiate(sourcePrefab, position, Quaternion.identity);
                }

                // 设置缩放
                float finalScale = scaleMultiplier;
                if (randomScale)
                {
                    finalScale = Random.Range(minRandomScale, maxRandomScale);
                }
                newObj.transform.localScale = Vector3.one * finalScale;

                // 设置随机旋转
                if (randomRotation)
                {
                    Vector3 rotationAngles = Vector3.zero;

                    if (rotateOnYAxis)
                    {
                        rotationAngles.y = Random.Range(minRandomRotation, maxRandomRotation);
                    }
                    if (rotateOnXAxis)
                    {
                        rotationAngles.x = Random.Range(minRandomRotation, maxRandomRotation);
                    }
                    if (rotateOnZAxis)
                    {
                        rotationAngles.z = Random.Range(minRandomRotation, maxRandomRotation);
                    }

                    newObj.transform.rotation = Quaternion.Euler(rotationAngles);
                }

                // 设置名称
                string name = $"{sourcePrefab.name}_{x}_{z}";
                if (randomScale)
                {
                    name += $"_{finalScale:F2}x";
                }
                if (randomRotation)
                {
                    name += "_rot";
                }
                newObj.name = name;

                // 先放入网格组
                newObj.transform.SetParent(gridGroup.transform);
                generatedObjects.Add(newObj);
            }
        }

        // ===== 【新增】设置父物体为中心 =====
        if (setParentToCenter)
        {
            // 计算所有物体的中心点
            Vector3 center = GetObjectsCenter(generatedObjects);

            // 创建中心父物体
            GameObject centerParent = GameObject.Find(centerParentName);
            if (centerParent == null)
            {
                centerParent = new GameObject(centerParentName);
                Undo.RegisterCreatedObjectUndo(centerParent, "创建中心父物体");
            }

            // 设置中心父物体的位置
            centerParent.transform.position = center;

            // 记录网格组的当前位置
            Vector3 gridGroupPos = gridGroup.transform.position;

            // 将网格组设为中心父物体的子物体
            Undo.SetTransformParent(gridGroup.transform, centerParent.transform, "设置父物体");

            // 保持网格组的世界位置不变
            gridGroup.transform.position = gridGroupPos;

            Debug.Log($"✅ 中心父物体 '{centerParentName}' 位置设为：{center}");

            // 选中中心父物体
            Selection.activeGameObject = centerParent;
        }
        else
        {
            // 选中生成的组
            Selection.activeGameObject = gridGroup;
        }

        // 注册撤销操作
        foreach (var obj in generatedObjects)
        {
            Undo.RegisterCreatedObjectUndo(obj, "生成网格物体");
        }

        // 输出信息
        string scaleInfo = randomScale ? $"随机缩放 {minRandomScale}x-{maxRandomScale}x" : $"整体缩放 {scaleMultiplier}x";
        string rotationInfo = randomRotation ? $"随机旋转 {minRandomRotation}°-{maxRandomRotation}°" : "";
        string centerInfo = setParentToCenter ? $"，已创建中心父物体 '{centerParentName}' 位置为物体中心" : "";

        Debug.Log($"✅ 成功生成 {generatedObjects.Count} 个物体，{scaleInfo}，{rotationInfo}{centerInfo}，已放入组 '{groupName}'");
    }

    // ===== 【新增】计算物体中心点 =====
    Vector3 GetObjectsCenter(List<GameObject> objects)
    {
        if (objects.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var obj in objects)
        {
            sum += obj.transform.position;
        }

        return sum / objects.Count;
    }

    void ClearAll()
    {
        // 先清除中心父物体（如果存在）
        if (setParentToCenter)
        {
            GameObject centerParent = GameObject.Find(centerParentName);
            if (centerParent != null)
            {
                Undo.DestroyObjectImmediate(centerParent);
                Debug.Log($"✅ 已清除中心父物体 '{centerParentName}'");
            }
        }

        // 再清除网格组
        GameObject groupObject = GameObject.Find(groupName);
        if (groupObject != null)
        {
            Undo.DestroyObjectImmediate(groupObject);
            Debug.Log($"✅ 已清除网格组 '{groupName}' 及其所有子物体");
        }
        else
        {
            Debug.Log("没有找到要清除的组");
        }
    }
}