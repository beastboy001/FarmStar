using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridGenerator : EditorWindow
{
    // 网格设置
    private int gridWidth = 10;          // 宽度方向格子数
    private int gridHeight = 10;         // 高度方向格子数
    private float spacing = 1.1f;         // 格子间距
    private Vector3 startPosition = Vector3.zero;  // 起始位置

    // 方块设置
    private GameObject cubePrefab;        // 方块预制体
    private Material cubeMaterial;         // 方块材质
    private Vector3 cubeSize = Vector3.one; // 方块大小

    // 生成选项
    private bool autoGroup = true;         // 自动分组
    private string groupName = "GeneratedGrid"; // 组名

    // 颜色设置
    private bool useRandomColors = false;   // 使用随机颜色
    private Color[] tileColors;              // 格子颜色数组

    [MenuItem("Tools/网格生成器")]
    public static void ShowWindow()
    {
        GetWindow<GridGenerator>("网格生成器");
    }

    void OnEnable()
    {
        // 初始化颜色数组
        tileColors = new Color[]
        {
            Color.green,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            Color.gray
        };
    }

    void OnGUI()
    {
        GUILayout.Label("网格生成设置", EditorStyles.boldLabel);

        // 网格参数
        EditorGUILayout.Space(10);
        GUILayout.Label("网格尺寸", EditorStyles.label);
        gridWidth = EditorGUILayout.IntField("宽度 (X轴)", gridWidth);
        gridHeight = EditorGUILayout.IntField("高度 (Z轴)", gridHeight);

        EditorGUILayout.Space(5);
        spacing = EditorGUILayout.FloatField("格子间距", spacing);
        startPosition = EditorGUILayout.Vector3Field("起始位置", startPosition);

        EditorGUILayout.Space(10);
        GUILayout.Label("方块设置", EditorStyles.label);
        cubePrefab = (GameObject)EditorGUILayout.ObjectField("方块预制体", cubePrefab, typeof(GameObject), false);
        cubeMaterial = (Material)EditorGUILayout.ObjectField("方块材质", cubeMaterial, typeof(Material), false);
        cubeSize = EditorGUILayout.Vector3Field("方块大小", cubeSize);

        EditorGUILayout.Space(10);
        GUILayout.Label("生成选项", EditorStyles.label);
        autoGroup = EditorGUILayout.Toggle("自动分组", autoGroup);
        if (autoGroup)
        {
            groupName = EditorGUILayout.TextField("组名", groupName);
        }

        useRandomColors = EditorGUILayout.Toggle("随机颜色", useRandomColors);

        EditorGUILayout.Space(20);

        // 按钮区域
        EditorGUILayout.BeginHorizontal();

        GUI.color = Color.green;
        if (GUILayout.Button("生成网格", GUILayout.Height(40)))
        {
            GenerateGrid();
        }

        GUI.color = Color.red;
        if (GUILayout.Button("清除所有", GUILayout.Height(40)))
        {
            ClearAllGrids();
        }

        GUI.color = Color.yellow;
        if (GUILayout.Button("选中组", GUILayout.Height(40)))
        {
            SelectGroup();
        }

        EditorGUILayout.EndHorizontal();

        GUI.color = Color.white;

        // 显示信息
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox($"将生成 {gridWidth * gridHeight} 个方块", MessageType.Info);
    }

    void GenerateGrid()
    {
        // 创建父对象
        GameObject parent = null;
        if (autoGroup)
        {
            parent = GameObject.Find(groupName);
            if (parent == null)
            {
                parent = new GameObject(groupName);
                Undo.RegisterCreatedObjectUndo(parent, "创建网格组");
            }
        }

        // 记录生成的物体用于撤销
        List<GameObject> generatedObjects = new List<GameObject>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // 计算位置
                Vector3 position = startPosition + new Vector3(x * spacing, 0, z * spacing);

                // 创建方块
                GameObject cube;
                if (cubePrefab != null)
                {
                    cube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab);
                    cube.transform.position = position;
                }
                else
                {
                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = position;
                }

                // 设置大小
                cube.transform.localScale = cubeSize;

                // 设置材质
                if (cubeMaterial != null)
                {
                    cube.GetComponent<Renderer>().sharedMaterial = cubeMaterial;
                }

                // 设置随机颜色
                if (useRandomColors)
                {
                    Color randomColor = tileColors[Random.Range(0, tileColors.Length)];
                    cube.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                    cube.GetComponent<Renderer>().sharedMaterial.color = randomColor;
                }

                // 设置名字
                cube.name = $"Tile_{x}_{z}";

                // 设置父对象
                if (autoGroup && parent != null)
                {
                    cube.transform.SetParent(parent.transform);
                }

                generatedObjects.Add(cube);
            }
        }

        // 注册撤销操作
        foreach (var obj in generatedObjects)
        {
            Undo.RegisterCreatedObjectUndo(obj, "生成网格");
        }

        Debug.Log($"✅ 成功生成 {generatedObjects.Count} 个方块");
    }

    void ClearAllGrids()
    {
        if (!EditorUtility.DisplayDialog("确认清除",
            $"确定要删除所有名为 {groupName} 的物体吗？",
            "确认", "取消"))
        {
            return;
        }

        // 查找所有要删除的物体
        GameObject parent = GameObject.Find(groupName);
        if (parent != null)
        {
            Undo.DestroyObjectImmediate(parent);
            Debug.Log($"✅ 已清除 {groupName}");
        }
        else
        {
            Debug.Log("没有找到要清除的物体");
        }
    }

    void SelectGroup()
    {
        GameObject parent = GameObject.Find(groupName);
        if (parent != null)
        {
            Selection.activeGameObject = parent;
            EditorGUIUtility.PingObject(parent);
            Debug.Log($"已选中 {groupName}");
        }
        else
        {
            Debug.Log("没有找到要选中的物体");
        }
    }
}