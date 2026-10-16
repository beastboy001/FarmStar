#if UNITY_EDITOR
using System.IO;
using AHD2TimeOfDay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 一键把 AHD2 TOD 资源复制到 Assets，并配置 GameScene 的 EnvironmentManager。
/// </summary>
public static class FarmStarTODSetupEditor
{
    const string TodSystemFolder = "Assets/TODSystem";
    const string PackageTodFolder = "Packages/com.ahd2.tod-system/TODSystem";
    const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("FarmStar/昼夜系统/初始化 TOD 资源")]
    public static void CopyTodAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/TODSystem"))
            AssetDatabase.CreateFolder("Assets", "TODSystem");

        TimeOfDaysEditorUtility.CopyDirectory(PackageTodFolder, TodSystemFolder);
        EditorUtility.DisplayDialog("昼夜系统", "已将 TOD 配置复制到 Assets/TODSystem。", "确定");
    }

    [MenuItem("FarmStar/昼夜系统/配置 GameScene 昼夜（含体积雾 Feature）")]
    public static void SetupGameScene()
    {
        CopyTodAssets();
        AddVolumetricFogToUrpRenderer();
        WireEnvironmentManagerInOpenScene();
        EditorUtility.DisplayDialog(
            "昼夜系统",
            "已完成：\n" +
            "1. TOD 资源 → Assets/TODSystem\n" +
            "2. URP 体积雾 Render Feature\n" +
            "3. 当前场景 EnvironmentManager 引用\n\n" +
            "请保存场景。主光请使用 Color 模式；URP 渲染路径建议 Forward+。",
            "确定");
    }

    static void WireEnvironmentManagerInOpenScene()
    {
        var manager = Object.FindObjectOfType<EnvironmentManager>();
        if (manager == null)
        {
            Debug.LogWarning("当前场景未找到 EnvironmentManager，请打开 GameScene 后重试。");
            return;
        }

        var globalParams = AssetDatabase.LoadAssetAtPath<TODGlobalParameters>(
            $"{TodSystemFolder}/TODGlobalParameters.asset");
        var skyMat = AssetDatabase.LoadAssetAtPath<Material>(
            $"{TodSystemFolder}/Material/HDRSkyBox_BaseSky.mat");

        var controller = manager.GetComponent<TODController>();
        if (controller == null)
            controller = manager.gameObject.AddComponent<TODController>();

        manager.todGlobalParameters = globalParams;
        controller.todGlobalParameters = globalParams;
        controller.isTimeFlow = false;

        if (manager.sunLight == null)
        {
            foreach (var light in Object.FindObjectsOfType<Light>())
            {
                if (light.type == LightType.Directional)
                {
                    manager.sunLight = light;
                    break;
                }
            }
        }

        controller.MainLight = manager.sunLight;
        if (skyMat != null)
            RenderSettings.skybox = skyMat;

        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(controller);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    static void AddVolumetricFogToUrpRenderer()
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("未找到 URP 资源，请在 Graphics 设置中指定 Universal Render Pipeline Asset。");
            return;
        }

        var field = typeof(UniversalRenderPipelineAsset).GetField(
            "m_RendererDataList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(urpAsset) is not ScriptableRendererData[] rendererList || rendererList.Length == 0)
            return;

        var rendererData = rendererList[0] as UniversalRendererData;
        if (rendererData == null)
            return;

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is VolumetricFogRendererFeature)
            {
                Debug.Log("URP 已存在 VolumetricFogRendererFeature。");
                return;
            }
        }

        var fogFeature = ScriptableObject.CreateInstance<VolumetricFogRendererFeature>();
        fogFeature.name = "VolumetricFogRendererFeature";

        var compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(
            "Packages/com.ahd2.tod-system/Shaders/VolumetricFog/DensityAndLighting.compute");
        if (compute != null)
        {
            var settingsField = typeof(VolumetricFogRendererFeature).GetField(
                "volumetricFogSettings",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (settingsField?.GetValue(fogFeature) != null)
            {
                var settings = settingsField.GetValue(fogFeature);
                var csField = settings.GetType().GetField(
                    "densityAndLightingComputeShader",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                csField?.SetValue(settings, compute);
            }
        }

        AddRenderFeature(rendererData, fogFeature);
        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();
    }

    static void AddRenderFeature(ScriptableRendererData data, ScriptableRendererFeature feature)
    {
        var serializedObject = new SerializedObject(data);
        var renderFeaturesProp = serializedObject.FindProperty("m_RendererFeatures");
        var renderFeaturesMapProp = serializedObject.FindProperty("m_RendererFeatureMap");
        serializedObject.Update();

        if (EditorUtility.IsPersistent(data))
            AssetDatabase.AddObjectToAsset(feature, data);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);

        renderFeaturesProp.arraySize++;
        renderFeaturesProp.GetArrayElementAtIndex(renderFeaturesProp.arraySize - 1).objectReferenceValue = feature;

        renderFeaturesMapProp.arraySize++;
        renderFeaturesMapProp.GetArrayElementAtIndex(renderFeaturesMapProp.arraySize - 1).longValue = localId;

        if (EditorUtility.IsPersistent(data))
            AssetDatabase.SaveAssetIfDirty(data);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
