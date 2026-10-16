using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class QuestConfigEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private List<QuestConfig> questConfigs = new List<QuestConfig>();
    private string questFolderPath = "Assets/Resources/QuestConfigs";
    private bool autoRefresh = true;

    // 任务模板数据（用于快速创建）
    private string newQuestId = "";
    private string newQuestName = "";
    private string newQuestCategory = "主线";

    [MenuItem("Tools/任务配置编辑器")]
    public static void ShowWindow()
    {
        GetWindow<QuestConfigEditor>("任务配置编辑器");
    }

    private void OnEnable()
    {
        RefreshQuestList();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // ========== 标题 ==========
        EditorGUILayout.LabelField("任务配置编辑器", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // ========== 工具栏 ==========
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新列表", GUILayout.Height(30)))
        {
            RefreshQuestList();
        }
        if (GUILayout.Button("打开文件夹", GUILayout.Height(30)))
        {
            EditorUtility.RevealInFinder(questFolderPath);
        }
        if (GUILayout.Button("创建新任务", GUILayout.Height(30)))
        {
            CreateNewQuestConfig();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ========== 快速创建区域 ==========
        EditorGUILayout.LabelField("快速创建任务", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        newQuestId = EditorGUILayout.TextField("任务ID", newQuestId);
        newQuestName = EditorGUILayout.TextField("任务名称", newQuestName);
        newQuestCategory = EditorGUILayout.TextField("任务分类", newQuestCategory);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("创建默认任务模板"))
        {
            CreateQuestFromTemplate();
        }
        if (GUILayout.Button("从JSON导入（已有的两个任务）"))
        {
            ImportFromJson();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // ========== 任务列表 ==========
        EditorGUILayout.LabelField($"任务列表 ({questConfigs.Count})", EditorStyles.boldLabel);

        if (questConfigs.Count == 0)
        {
            EditorGUILayout.HelpBox("没有找到任务配置，请点击「创建新任务」或「从JSON导入」", MessageType.Info);
        }
        else
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

            for (int i = 0; i < questConfigs.Count; i++)
            {
                var quest = questConfigs[i];
                if (quest == null) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                // 任务名称和ID
                EditorGUILayout.LabelField($"[{quest.category}] {quest.questName}", EditorStyles.boldLabel, GUILayout.Width(200));
                EditorGUILayout.LabelField($"ID: {quest.questId}", EditorStyles.miniLabel);

                GUILayout.FlexibleSpace();

                // 按钮
                if (GUILayout.Button("选中", GUILayout.Width(50)))
                {
                    Selection.activeObject = quest;
                    EditorGUIUtility.PingObject(quest);
                }
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", $"确定要删除任务「{quest.questName}」吗？", "删除", "取消"))
                    {
                        DeleteQuestConfig(quest);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // 简要信息
                EditorGUILayout.LabelField($"描述: {quest.description}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"目标数量: {quest.goals?.Count ?? 0}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"奖励金币: {quest.reward?.gold ?? 0}", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        // ========== 帮助信息 ==========
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 「创建新任务」→ 创建一个空的任务配置\n" +
            "2. 「从JSON导入」→ 自动创建「拜访村长爷爷」和「尝试种植」两个任务\n" +
            "3. 创建后可以在 Inspector 中编辑详细数据\n" +
            "4. 记得将任务拖到 QuestManager 的 allQuestConfigs 列表中",
            MessageType.Info);
    }

    private void RefreshQuestList()
    {
        questConfigs.Clear();

        // 确保文件夹存在
        if (!Directory.Exists(questFolderPath))
        {
            Directory.CreateDirectory(questFolderPath);
            AssetDatabase.Refresh();
        }

        // 查找所有 QuestConfig
        string[] guids = AssetDatabase.FindAssets("t:QuestConfig");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var config = AssetDatabase.LoadAssetAtPath<QuestConfig>(path);
            if (config != null)
            {
                questConfigs.Add(config);
            }
        }

        Debug.Log($"刷新任务列表，找到 {questConfigs.Count} 个任务配置");
    }

    private void CreateNewQuestConfig()
    {
        // 确保文件夹存在
        if (!Directory.Exists(questFolderPath))
        {
            Directory.CreateDirectory(questFolderPath);
            AssetDatabase.Refresh();
        }

        // 创建默认ID
        string questId = "new_quest_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        string assetPath = $"{questFolderPath}/Quest_{questId}.asset";

        // 创建配置
        QuestConfig config = ScriptableObject.CreateInstance<QuestConfig>();
        config.questId = questId;
        config.questName = "新任务";
        config.description = "任务描述";
        config.category = "主线";
        config.prerequisites = new List<string>();
        config.goals = new List<QuestGoalData>();
        config.reward = new QuestRewardData
        {
            gold = 100,
            items = new List<ItemRewardData>()
        };

        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RefreshQuestList();

        // 选中新创建的任务
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);

        Debug.Log($"创建新任务配置: {assetPath}");
    }

    private void CreateQuestFromTemplate()
    {
        if (string.IsNullOrEmpty(newQuestId))
        {
            EditorUtility.DisplayDialog("错误", "请填写任务ID", "确定");
            return;
        }

        if (string.IsNullOrEmpty(newQuestName))
        {
            EditorUtility.DisplayDialog("错误", "请填写任务名称", "确定");
            return;
        }

        // 确保文件夹存在
        if (!Directory.Exists(questFolderPath))
        {
            Directory.CreateDirectory(questFolderPath);
            AssetDatabase.Refresh();
        }

        string assetPath = $"{questFolderPath}/Quest_{newQuestId}.asset";

        // 检查是否已存在
        if (File.Exists(assetPath))
        {
            if (!EditorUtility.DisplayDialog("警告", $"任务 {newQuestId} 已存在，是否覆盖？", "覆盖", "取消"))
            {
                return;
            }
        }

        // 创建配置
        QuestConfig config = ScriptableObject.CreateInstance<QuestConfig>();
        config.questId = newQuestId;
        config.questName = newQuestName;
        config.description = "任务描述";
        config.category = newQuestCategory;
        config.prerequisites = new List<string>();
        config.goals = new List<QuestGoalData>();
        config.reward = new QuestRewardData
        {
            gold = 100,
            items = new List<ItemRewardData>()
        };

        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RefreshQuestList();

        // 选中新创建的任务
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);

        Debug.Log($"创建任务配置: {assetPath}");

        // 清空输入框
        newQuestId = "";
        newQuestName = "";
    }

    private void ImportFromJson()
    {
        // 模拟 JSON 中的两个任务数据
        // 任务1：拜访村长爷爷
        CreateQuestFromJsonData(
            "visit_village_head",
            "拜访村长爷爷",
            "去村庄广场找到村长爷爷，向他询问农场的事情",
            "主线",
            new List<string>(), // 无前置任务
            new List<QuestGoalData>
            {
                new QuestGoalData { type = QuestGoalType.TalkToNPC, targetId = "村长", requiredAmount = 1 }
            },
            100,
            new List<ItemRewardData>
            {
                new ItemRewardData { itemId = "hoe", amount = 1 },
                new ItemRewardData { itemId = "axe", amount = 1 },
                new ItemRewardData { itemId = "pickaxe", amount = 1 },
                new ItemRewardData { itemId = "tomato_seed", amount = 5 },
                new ItemRewardData { itemId = "wheat_seed", amount = 5 }
            }
        );

        // 任务2：尝试种植
        CreateQuestFromJsonData(
            "try_planting",
            "尝试种植",
            "在你的农场种植并收获5个番茄",
            "主线",
            new List<string> { "visit_village_head" },
            new List<QuestGoalData>
            {
                new QuestGoalData { type = QuestGoalType.HarvestCrop, targetId = "tomato", requiredAmount = 5 }
            },
            200,
            new List<ItemRewardData>()
        );

        RefreshQuestList();

        EditorUtility.DisplayDialog("导入完成",
            "已成功创建两个任务配置：\n" +
            "- 拜访村长爷爷 (visit_village_head)\n" +
            "- 尝试种植 (try_planting)\n\n" +
            "请将它们拖入 QuestManager 的 allQuestConfigs 列表中",
            "确定");
    }

    private void CreateQuestFromJsonData(
        string questId,
        string questName,
        string description,
        string category,
        List<string> prerequisites,
        List<QuestGoalData> goals,
        int goldReward,
        List<ItemRewardData> itemRewards)
    {
        // 确保文件夹存在
        if (!Directory.Exists(questFolderPath))
        {
            Directory.CreateDirectory(questFolderPath);
            AssetDatabase.Refresh();
        }

        string assetPath = $"{questFolderPath}/Quest_{questId}.asset";

        // 如果已存在，先询问是否覆盖
        if (File.Exists(assetPath))
        {
            if (!EditorUtility.DisplayDialog("确认", $"任务 {questName} 已存在，是否覆盖？", "覆盖", "跳过"))
            {
                return;
            }
        }

        // 创建配置
        QuestConfig config = ScriptableObject.CreateInstance<QuestConfig>();
        config.questId = questId;
        config.questName = questName;
        config.description = description;
        config.category = category;
        config.prerequisites = prerequisites ?? new List<string>();
        config.goals = goals ?? new List<QuestGoalData>();
        config.reward = new QuestRewardData
        {
            gold = goldReward,
            items = itemRewards ?? new List<ItemRewardData>()
        };

        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"创建任务配置: {questName} -> {assetPath}");
    }

    private void DeleteQuestConfig(QuestConfig config)
    {
        string path = AssetDatabase.GetAssetPath(config);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
        RefreshQuestList();
        Debug.Log($"删除任务配置: {path}");
    }
}