using UnityEngine;
using UnityEngine.UI;

public class InputPanel : BasePanel
{
    private Button ExitButton;
    private Button StartButton;
    private Button ConfirmMaleButton;
    private Button ConfirmFemaleButton;
    private Button ConfirmSkipButton;
    private GameObject ConfirmMale;
    private GameObject ConfirmFemale;
    private GameObject ConfirmSkip;
    private InputField NameInput;
    private InputField FarmNameInput;
    private bool isMale = true;

    private Button rightArrow;
    private Button leftArrow;

    private int currentSkinIndex = 0;
    private int maxSkins = 4;
    private GameObject playerModel;
    private SkinnedMeshRenderer targetRenderer;

    private string[] skinNames = { "Yellow", "Blue", "Green", "Red" };

    public override void OnInit()
    {
        base.OnInit();

        ExitButton = DeepFindTransform.DeepFindChild(transform, "ExitButton").GetComponent<Button>();
        StartButton = DeepFindTransform.DeepFindChild(transform, "StartButton").GetComponent<Button>();
        ConfirmMaleButton = DeepFindTransform.DeepFindChild(transform, "ConfirmMaleButton").GetComponent<Button>();
        ConfirmFemaleButton = DeepFindTransform.DeepFindChild(transform, "ConfirmFemaleButton").GetComponent<Button>();
        ConfirmFemale = DeepFindTransform.DeepFindChild(transform, "ConfirmFemale").gameObject;
        ConfirmMale = DeepFindTransform.DeepFindChild(transform, "ConfirmMale").gameObject;
        ConfirmSkipButton = DeepFindTransform.DeepFindChild(transform, "ConfirmskipButton").GetComponent<Button>();
        ConfirmSkip = DeepFindTransform.DeepFindChild(transform, "Confirmskip").gameObject;
        NameInput = DeepFindTransform.DeepFindChild(transform, "NameInput").GetComponent<InputField>();
        FarmNameInput = DeepFindTransform.DeepFindChild(transform, "FarmNameInput").GetComponent<InputField>();

        rightArrow = DeepFindTransform.DeepFindChild(transform, "rightarrow").GetComponent<Button>();
        leftArrow = DeepFindTransform.DeepFindChild(transform, "leftarrow").GetComponent<Button>();

        UpdateGenderDisplay(true);
        UpdateSkipAnimationDisplay(false);

        LoadSkinFromSave();
        FindPlayerModel();
        ApplySkinToModel();

        ExitButton.onClick.AddListener(OnExitButton);
        StartButton.onClick.AddListener(OnStartButton);
        ConfirmMaleButton.onClick.AddListener(OnConfirmMaleButton);
        ConfirmFemaleButton.onClick.AddListener(OnConfirmFemaleButton);
        ConfirmSkipButton.onClick.AddListener(OnConfirmSkipButton);
        rightArrow.onClick.AddListener(OnNextSkin);
        leftArrow.onClick.AddListener(OnPrevSkin);
    }

    void FindPlayerModel()
    {
        playerModel = GameObject.Find("playermodel");

        if (playerModel == null)
        {
            Transform modelTrans = DeepFindTransform.DeepFindChild(transform.root, "playermodel");
            if (modelTrans != null)
                playerModel = modelTrans.gameObject;
        }

        if (playerModel == null)
        {
            Debug.LogError("找不到 playermodel，请检查场景中的模型名称");
            return;
        }

        Transform peasant = playerModel.transform.Find("Peasant Nolant");
        if (peasant == null)
        {
            Debug.LogError("找不到 Peasant Nolant");
            return;
        }

        targetRenderer = peasant.GetComponent<SkinnedMeshRenderer>();
        if (targetRenderer == null)
            Debug.LogError("Peasant Nolant 没有 SkinnedMeshRenderer 组件");
        else
            Debug.Log("成功找到 playermodel 和 SkinnedMeshRenderer");
    }

    void ApplySkinToModel()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning("targetRenderer 为空，无法应用皮肤");
            return;
        }

        string materialPath = $"Materials/character/Peasant Nolant {skinNames[currentSkinIndex]}";
        Material material = Resources.Load<Material>(materialPath);

        if (material == null)
        {
            Debug.LogError($"找不到材质: {materialPath}");
            return;
        }

        targetRenderer.material = material;
        Debug.Log($"应用皮肤 {currentSkinIndex}: {skinNames[currentSkinIndex]}");
    }

    public void OnNextSkin()
    {
        currentSkinIndex++;
        if (currentSkinIndex >= maxSkins)
            currentSkinIndex = 0;

        ApplySkinToModel();
        SaveSkinToSave();
        Debug.Log($"切换到皮肤 {currentSkinIndex}: {skinNames[currentSkinIndex]}");
    }

    public void OnPrevSkin()
    {
        currentSkinIndex--;
        if (currentSkinIndex < 0)
            currentSkinIndex = maxSkins - 1;

        ApplySkinToModel();
        SaveSkinToSave();
        Debug.Log($"切换到皮肤 {currentSkinIndex}: {skinNames[currentSkinIndex]}");
    }

    void SaveSkinToSave()
    {
        PlayerPrefs.SetInt("SelectedSkin", currentSkinIndex);
        PlayerPrefs.Save();
    }

    void LoadSkinFromSave()
    {
        if (PlayerPrefs.HasKey("SelectedSkin"))
        {
            currentSkinIndex = PlayerPrefs.GetInt("SelectedSkin");
            Debug.Log($"从 PlayerPrefs 加载皮肤索引: {currentSkinIndex}");
        }
        else
        {
            currentSkinIndex = 0;
        }
    }

    private void UpdateGenderDisplay(bool male)
    {
        if (male)
        {
            if (ConfirmMale != null) ConfirmMale.SetActive(true);
            if (ConfirmFemale != null) ConfirmFemale.SetActive(false);
        }
        else
        {
            if (ConfirmMale != null) ConfirmMale.SetActive(false);
            if (ConfirmFemale != null) ConfirmFemale.SetActive(true);
        }

        isMale = male;
        Debug.Log("当前性别：" + (male ? "男" : "女"));
    }

    private void UpdateSkipAnimationDisplay(bool skip)
    {
        if (ConfirmSkip != null)
            ConfirmSkip.SetActive(skip);
    }

    private void ClearInputFields()
    {
        if (NameInput != null) NameInput.text = "";
        if (FarmNameInput != null) FarmNameInput.text = "";
    }

    public void OnExitButton()
    {
        Debug.Log("关闭面板");
        OnClose();
    }

    public void OnStartButton()
    {
        string playerName = NameInput != null ? NameInput.text : "";
        string farmName = FarmNameInput != null ? FarmNameInput.text : "";

        Debug.Log("准备保存存档");
        Debug.Log("玩家昵称：" + playerName);
        Debug.Log("农场名称：" + farmName);
        Debug.Log("性别：" + (isMale ? "男" : "女"));
        Debug.Log("皮肤索引：" + currentSkinIndex);

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("请输入昵称");
            return;
        }

        if (string.IsNullOrEmpty(farmName))
        {
            Debug.LogWarning("请输入农场名");
            return;
        }

        int slotIndex = ChoicePanel.currentSelectedSlot;
        Debug.Log("保存到存档槽位：" + (slotIndex + 1));

        GameData newData = SaveManager.CreateNewGameData();
        newData.playerData.playerName = playerName;
        newData.playerData.farmName = farmName;
        newData.playerData.isMale = isMale;
        newData.playerData.skinIndex = currentSkinIndex;

        SaveManager.SaveGame(slotIndex, newData);
        ChoicePanel.RefreshSlotDisplay(slotIndex);
        ChoicePanel.currentGameData = newData;

        bool skipAnimation = ConfirmSkip != null && ConfirmSkip.activeSelf;

        ClearInputFields();
        UpdateGenderDisplay(true);

        if (skipAnimation)
        {
            CloseInputAndChoicePanels();
            Debug.Log("跳过开头动画，打开加载面板");
            uiSystem.OpenPanel<LoadPanel>(1);
        }
        else
        {
            Debug.Log("播放开头动画，先打开遮罩面板");
            uiSystem.OpenPanel<MaskPanel>();
        }
    }

    private void CloseInputAndChoicePanels()
    {
        OnClose();
        ChoicePanel choicePanel = FindObjectOfType<ChoicePanel>();
        if (choicePanel != null)
            choicePanel.OnClose();
    }

    public static void ClosePanelsAfterMask()
    {
        if (UISystem.Instance == null) return;
        UISystem.Instance.ClosePanel(typeof(InputPanel).ToString());
        UISystem.Instance.ClosePanel(typeof(ChoicePanel).ToString());
    }

    public void OnConfirmMaleButton()
    {
        UpdateGenderDisplay(true);
    }

    public void OnConfirmFemaleButton()
    {
        UpdateGenderDisplay(false);
    }

    public void OnConfirmSkipButton()
    {
        bool skip = ConfirmSkip != null && !ConfirmSkip.activeSelf;
        UpdateSkipAnimationDisplay(skip);
        Debug.Log(skip ? "已勾选：跳过开头动画" : "已取消：播放开头动画");
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        ClearInputFields();
        UpdateGenderDisplay(true);
        UpdateSkipAnimationDisplay(false);

        FindPlayerModel();
        ApplySkinToModel();

        Debug.Log("InputPanel 显示 - 输入框已清空");
    }
}
