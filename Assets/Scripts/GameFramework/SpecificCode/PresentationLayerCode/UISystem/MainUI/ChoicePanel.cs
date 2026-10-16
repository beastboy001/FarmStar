using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoicePanel : BasePanel
{
    private Button ReturnButton;
    private Button StartButton;

    // 添加一个公共属性，让其他面板可以访问当前选中的存档槽位
    public static int currentSelectedSlot { get; private set; } = 0;  // 0-3

    // 新增：存储当前要进入游戏的存档数据
    public static GameData currentGameData { get; set; }

    //存档一
    private Button Information1;
    private Button DeleteButton1;
    private GameObject Arrow1;
    private GameObject NewGame1;
    private GameObject IDName1;
    private Text IDNameText1;

    //存档二
    private Button Information2;
    private Button DeleteButton2;
    private GameObject Arrow2;
    private GameObject NewGame2;
    private GameObject IDName2;
    private Text IDNameText2;

    //存档三
    private Button Information3;
    private Button DeleteButton3;
    private GameObject Arrow3;
    private GameObject NewGame3;
    private GameObject IDName3;
    private Text IDNameText3;

    //存档四
    private Button Information4;
    private Button DeleteButton4;
    private GameObject Arrow4;
    private GameObject NewGame4;
    private GameObject IDName4;
    private Text IDNameText4;

    public override void OnInit()
    {
        base.OnInit();

        //寻找组件
        ReturnButton = transform.Find("ReturnButton").GetComponent<Button>();
        StartButton = transform.Find("StartButton").GetComponent<Button>();

        //存档一
        DeleteButton1 = DeepFindTransform.DeepFindChild(transform, "DeleteButton1").GetComponent<Button>();
        Information1 = DeepFindTransform.DeepFindChild(transform, "Information1").GetComponent<Button>();
        Arrow1 = DeepFindTransform.DeepFindChild(transform, "Arrow1").gameObject;
        NewGame1 = DeepFindTransform.DeepFindChild(transform, "NewGame1").gameObject;
        IDName1 = DeepFindTransform.DeepFindChild(transform, "IDName1").gameObject;
        IDNameText1 = IDName1?.GetComponent<Text>();

        //存档二
        DeleteButton2 = DeepFindTransform.DeepFindChild(transform, "DeleteButton2").GetComponent<Button>();
        Information2 = DeepFindTransform.DeepFindChild(transform, "Information2").GetComponent<Button>();
        Arrow2 = DeepFindTransform.DeepFindChild(transform, "Arrow2").gameObject;
        NewGame2 = DeepFindTransform.DeepFindChild(transform, "NewGame2").gameObject;
        IDName2 = DeepFindTransform.DeepFindChild(transform, "IDName2").gameObject;
        IDNameText2 = IDName2?.GetComponent<Text>();

        //存档三
        DeleteButton3 = DeepFindTransform.DeepFindChild(transform, "DeleteButton3").GetComponent<Button>();
        Information3 = DeepFindTransform.DeepFindChild(transform, "Information3").GetComponent<Button>();
        Arrow3 = DeepFindTransform.DeepFindChild(transform, "Arrow3").gameObject;
        NewGame3 = DeepFindTransform.DeepFindChild(transform, "NewGame3").gameObject;
        IDName3 = DeepFindTransform.DeepFindChild(transform, "IDName3").gameObject;
        IDNameText3 = IDName3?.GetComponent<Text>();

        //存档四
        DeleteButton4 = DeepFindTransform.DeepFindChild(transform, "DeleteButton4").GetComponent<Button>();
        Information4 = DeepFindTransform.DeepFindChild(transform, "Information4").GetComponent<Button>();
        Arrow4 = DeepFindTransform.DeepFindChild(transform, "Arrow4").gameObject;
        NewGame4 = DeepFindTransform.DeepFindChild(transform, "NewGame4").gameObject;
        IDName4 = DeepFindTransform.DeepFindChild(transform, "IDName4").gameObject;
        IDNameText4 = IDName4?.GetComponent<Text>();

        // 初始化所有存档槽位的显示状态
        RefreshAllSaveSlots();

        CloseArrow();
        Arrow1.SetActive(true);
        currentSelectedSlot = 0;

        //添加监听
        ReturnButton.onClick.AddListener(OnReturnButton);
        StartButton.onClick.AddListener(OnStartButton);

        //存档一 
        DeleteButton1.onClick.AddListener(OnDeleteButton1);
        Information1.onClick.AddListener(OnInformation1);

        //存档二
        DeleteButton2.onClick.AddListener(OnDeleteButton2);
        Information2.onClick.AddListener(OnInformation2);

        //存档三
        DeleteButton3.onClick.AddListener(OnDeleteButton3);
        Information3.onClick.AddListener(OnInformation3);

        //存档四 
        DeleteButton4.onClick.AddListener(OnDeleteButton4);
        Information4.onClick.AddListener(OnInformation4);
    }

    /// <summary>
    /// 刷新所有存档槽位的显示状态
    /// </summary>
    private void RefreshAllSaveSlots()
    {
        UpdateSlotUI(0, NewGame1, IDName1, IDNameText1);
        UpdateSlotUI(1, NewGame2, IDName2, IDNameText2);
        UpdateSlotUI(2, NewGame3, IDName3, IDNameText3);
        UpdateSlotUI(3, NewGame4, IDName4, IDNameText4);
    }

    /// <summary>
    /// 更新单个存档槽位的UI显示
    /// </summary>
    private void UpdateSlotUI(int slotIndex, GameObject newGameObj, GameObject idNameObj, Text idNameText)
    {
        if (SaveManager.IsSaveExists(slotIndex))
        {
            GameData data = SaveManager.LoadGame(slotIndex);
            if (data != null)
            {
                if (newGameObj != null) newGameObj.SetActive(false);
                if (idNameObj != null)
                {
                    idNameObj.SetActive(true);
                    if (idNameText != null)
                    {
                        idNameText.text = "ID:" + data.playerData.playerName;
                    }
                }
            }
        }
        else
        {
            if (newGameObj != null) newGameObj.SetActive(true);
            if (idNameObj != null) idNameObj.SetActive(false);
        }
    }

    //开始游戏
    public void OnStartButton()
    {
        Debug.Log("开始游戏 - 当前选中的存档：" + (currentSelectedSlot + 1));

        // 检查当前选中的存档是否存在
        if (SaveManager.IsSaveExists(currentSelectedSlot))
        {
            // 有存档记录，加载数据
            GameData data = SaveManager.LoadGame(currentSelectedSlot);
            Debug.Log("加载现有存档：" + data.playerData.playerName + "的" + data.playerData.farmName);

            // 保存数据到静态变量
            currentGameData = data;

            // 关闭当前面板
            OnClose();

            // 打开LoadPanel，传入游戏场景的索引（假设GameScene是索引2）
            Debug.Log("打开加载面板，准备进入游戏场景");
            uiSystem.OpenPanel<LoadPanel>(2);  // 传入场景索引2
        }
        else
        {
            // 空存档，打开InputPanel填写信息
            Debug.Log("新存档，打开输入面板");
            uiSystem.OpenPanel<InputPanel>();
        }
    }
    //返回按钮
    public void OnReturnButton()
    {
        OnClose();
        uiSystem.OpenPanel<LoginPanel>();
    }

    //存档一
    public void OnDeleteButton1()
    {
        CloseArrow();
        Arrow1.SetActive(true);
        currentSelectedSlot = 0;
        Debug.Log("打开确认面板 - 存档1");
        uiSystem.OpenPanel<ConfirmPanel>(0);
    }

    public void OnInformation1()
    {
        CloseArrow();
        Arrow1.SetActive(true);
        currentSelectedSlot = 0;
        Debug.Log("选中存档1");
    }

    //存档二
    public void OnDeleteButton2()
    {
        CloseArrow();
        Arrow2.SetActive(true);
        currentSelectedSlot = 1;
        Debug.Log("打开确认面板 - 存档2");
        uiSystem.OpenPanel<ConfirmPanel>(1);
    }

    public void OnInformation2()
    {
        CloseArrow();
        Arrow2.SetActive(true);
        currentSelectedSlot = 1;
        Debug.Log("选中存档2");
    }

    //存档三
    public void OnDeleteButton3()
    {
        CloseArrow();
        Arrow3.SetActive(true);
        currentSelectedSlot = 2;
        Debug.Log("打开确认面板 - 存档3");
        uiSystem.OpenPanel<ConfirmPanel>(2);
    }

    public void OnInformation3()
    {
        CloseArrow();
        Arrow3.SetActive(true);
        currentSelectedSlot = 2;
        Debug.Log("选中存档3");
    }

    //存档四
    public void OnDeleteButton4()
    {
        CloseArrow();
        Arrow4.SetActive(true);
        currentSelectedSlot = 3;
        Debug.Log("打开确认面板 - 存档4");
        uiSystem.OpenPanel<ConfirmPanel>(3);
    }

    public void OnInformation4()
    {
        CloseArrow();
        Arrow4.SetActive(true);
        currentSelectedSlot = 3;
        Debug.Log("选中存档4");
    }

    //关闭所有箭头
    public void CloseArrow()
    {
        if (Arrow1 != null) Arrow1.SetActive(false);
        if (Arrow2 != null) Arrow2.SetActive(false);
        if (Arrow3 != null) Arrow3.SetActive(false);
        if (Arrow4 != null) Arrow4.SetActive(false);
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        RefreshAllSaveSlots();
        CloseArrow();
        switch (currentSelectedSlot)
        {
            case 0: if (Arrow1 != null) Arrow1.SetActive(true); break;
            case 1: if (Arrow2 != null) Arrow2.SetActive(true); break;
            case 2: if (Arrow3 != null) Arrow3.SetActive(true); break;
            case 3: if (Arrow4 != null) Arrow4.SetActive(true); break;
        }
        Debug.Log("ChoicePanel显示 - 已自动读取所有存档");
    }

    public static void RefreshSlotDisplay(int slotIndex)
    {
        ChoicePanel instance = FindObjectOfType<ChoicePanel>();
        if (instance != null)
        {
            instance.UpdateSlotUIAfterDelete(slotIndex);
        }
    }

    private void UpdateSlotUIAfterDelete(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                UpdateSlotUI(0, NewGame1, IDName1, IDNameText1);
                break;
            case 1:
                UpdateSlotUI(1, NewGame2, IDName2, IDNameText2);
                break;
            case 2:
                UpdateSlotUI(2, NewGame3, IDName3, IDNameText3);
                break;
            case 3:
                UpdateSlotUI(3, NewGame4, IDName4, IDNameText4);
                break;
        }
        Debug.Log("存档" + (slotIndex + 1) + "UI已更新为空存档状态");
    }
}