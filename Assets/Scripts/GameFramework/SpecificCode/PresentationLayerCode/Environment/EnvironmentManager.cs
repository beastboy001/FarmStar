using AHD2TimeOfDay;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 场景昼夜环境：用 AHD2 TOD 驱动天空与主光源，时间由 GamePanel 游戏时钟同步。
/// </summary>
[DefaultExecutionOrder(50)]
public class EnvironmentManager : MonoBehaviour
{
    private const string TodSystemAssetPath = "Assets/TODSystem/TODGlobalParameters.asset";
    private const string TodSystemPackagePath =
        "Packages/com.ahd2.tod-system/TODSystem/TODGlobalParameters.asset";
    private const string TodSkyMaterialPackagePath =
        "Packages/com.ahd2.tod-system/TODSystem/Material/HDRSkyBox_BaseSky.mat";

    [Header("太阳（场景主方向光）")]
    public Light sunLight;

    [Header("昼夜系统")]
    public TODController todController;

    [Tooltip("留空则自动从 Assets/TODSystem 或 Package 默认配置加载")]
    public TODGlobalParameters todGlobalParameters;

    [Header("调试")]
    [Tooltip("未进入游戏 / GamePanel 未就绪时使用")]
    public float debugTimeHours = 6f;

    [Tooltip("勾选后使用 GamePanel 的游戏内时间（推荐）")]
    public bool useGamePanelTime = true;

    [Header("亮度微调")]
    [Tooltip("白天主光倍率")]
    [Range(0.4f, 1.5f)]
    public float dayLightIntensityMultiplier = 0.65f;

    [Tooltip("白天主光强度上限，防止过曝")]
    [Range(1f, 3f)]
    public float dayMaxMainLightIntensity = 3f;

    [Tooltip("夜间主光最低亮度，避免全黑")]
    [Range(0f, 2f)]
    public float nightMinMainLightIntensity = 0.9f;

    [Tooltip("夜间主光额外倍率")]
    [Range(1f, 2f)]
    public float nightLightIntensityMultiplier = 1.2f;

    [Header("交界柔和（缓解一半亮一半黑）")]
    [Tooltip("日出日落时额外环境光强度")]
    [Range(0f, 1.5f)]
    public float twilightAmbientBoost = 0.65f;

    [Tooltip("太阳贴地平线时阴影减弱")]
    [Range(0.1f, 1f)]
    public float twilightMinShadowStrength = 0.35f;

    [Header("已弃用（保留场景引用兼容）")]
    public Material daySkybox;
    public Material nightSkybox;

    bool _initialized;

    void Awake()
    {
        EnsureTodSystem();
    }

    void OnEnable()
    {
        if (!_initialized)
            EnsureTodSystem();
        else
            ApplySkyboxIfNeeded();
    }

    void Update()
    {
        if (!_initialized || todController == null || todController.todGlobalParameters == null)
            return;

        var parameters = todController.todGlobalParameters;
        parameters.isTimeFlow = false;
        todController.isTimeFlow = false;
        parameters.CurrentTime = ResolveTimeOfDayHours();
    }

    void LateUpdate()
    {
        ApplyLightIntensityFloor();
        ApplySoftAmbientAndShadows();
    }

    void ApplyLightIntensityFloor()
    {
        if (sunLight == null || todController?.todGlobalParameters == null)
            return;

        var parameters = todController.todGlobalParameters;
        float intensity = parameters.MainlightIntensity;

        if (parameters._dayOrNight == 1)
        {
            intensity *= nightLightIntensityMultiplier;
            intensity = Mathf.Max(intensity, nightMinMainLightIntensity);
        }
        else
        {
            intensity *= dayLightIntensityMultiplier;
            intensity = Mathf.Min(intensity, dayMaxMainLightIntensity);
        }

        sunLight.intensity = intensity;
    }

    void ApplySoftAmbientAndShadows()
    {
        if (sunLight == null || todController?.todGlobalParameters == null)
            return;

        var parameters = todController.todGlobalParameters;
        float nightBlend = parameters.dayNightBlend;
        float sunElevation = Mathf.Clamp01(Vector3.Dot(-sunLight.transform.forward, Vector3.up));
        float twilightFactor = 1f - Mathf.SmoothStep(0.12f, 0.5f, sunElevation);

        RenderSettings.ambientMode = AmbientMode.Trilight;

        Color daySky = new Color(0.42f, 0.5f, 0.65f);
        Color dayEquator = new Color(0.34f, 0.37f, 0.4f);
        Color dayGround = new Color(0.22f, 0.24f, 0.27f);
        Color nightSky = new Color(0.14f, 0.18f, 0.32f);
        Color nightEquator = new Color(0.12f, 0.14f, 0.22f);
        Color nightGround = new Color(0.1f, 0.11f, 0.16f);

        RenderSettings.ambientSkyColor = Color.Lerp(daySky, nightSky, nightBlend);
        RenderSettings.ambientEquatorColor = Color.Lerp(dayEquator, nightEquator, nightBlend);
        RenderSettings.ambientGroundColor = Color.Lerp(dayGround, nightGround, nightBlend);
        RenderSettings.ambientIntensity = 0.82f + nightBlend * 0.35f + twilightFactor * twilightAmbientBoost;

        float shadowStrength = Mathf.Lerp(twilightMinShadowStrength, 0.82f, sunElevation);
        sunLight.shadowStrength = shadowStrength;
    }

    float ResolveTimeOfDayHours()
    {
        if (useGamePanelTime && GamePanel.Instance != null)
            return GamePanel.Instance.GetTimeOfDayHoursSmooth();
        return debugTimeHours;
    }

    void EnsureTodSystem()
    {
        if (todController == null)
            todController = GetComponent<TODController>();
        if (todController == null)
            todController = gameObject.AddComponent<TODController>();

        if (todGlobalParameters == null)
            todGlobalParameters = LoadTodGlobalParameters();

        if (todGlobalParameters == null)
        {
            Debug.LogError("EnvironmentManager: 未找到 TODGlobalParameters。请执行菜单 FarmStar/昼夜系统/初始化 TOD 资源。");
            return;
        }

        todController.todGlobalParameters = todGlobalParameters;
        todController.isTimeFlow = false;
        todGlobalParameters.isTimeFlow = false;

        if (sunLight == null)
            sunLight = FindMainDirectionalLight();
        todController.MainLight = sunLight;

        if (sunLight != null)
            sunLight.useColorTemperature = false;

        todGlobalParameters.Initailize();
        if (todGlobalParameters.timeOfDays != null)
        {
            foreach (var tod in todGlobalParameters.timeOfDays)
            {
                if (tod != null)
                    tod.Initialize();
            }
        }

        todGlobalParameters.CurrentTime = ResolveTimeOfDayHours();
        todGlobalParameters.FixTimeOfDay();

        ApplySkyboxIfNeeded();

        _initialized = true;
    }

    void ApplySkyboxIfNeeded()
    {
        var skybox = ResolveSkyboxMaterial();
        if (skybox != null && RenderSettings.skybox != skybox)
            RenderSettings.skybox = skybox;
    }

    static TODGlobalParameters LoadTodGlobalParameters()
    {
#if UNITY_EDITOR
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TODGlobalParameters>(TodSystemAssetPath);
        if (asset != null)
            return asset;
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TODGlobalParameters>(TodSystemPackagePath);
#else
        return Resources.Load<TODGlobalParameters>("TODGlobalParameters");
#endif
    }

    Material ResolveSkyboxMaterial()
    {
        if (todGlobalParameters?.materials != null && todGlobalParameters.materials.Length > 0 &&
            todGlobalParameters.materials[0] != null)
            return todGlobalParameters.materials[0];

#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(TodSkyMaterialPackagePath);
#else
        return null;
#endif
    }

    static Light FindMainDirectionalLight()
    {
        var lights = Object.FindObjectsOfType<Light>();
        Light fallback = null;
        foreach (var light in lights)
        {
            if (light.type != LightType.Directional)
                continue;
            if (light.name.Contains("Sun") || light.name.Contains("Directional Light (1)"))
                return light;
            if (fallback == null)
                fallback = light;
        }
        return fallback;
    }
}
