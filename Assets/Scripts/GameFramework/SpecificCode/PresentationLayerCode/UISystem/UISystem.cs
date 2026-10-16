using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem : IUISystem
{
    public static UISystem Instance { get; private set; }
    private Dictionary<string, BasePanel> panelDict = new Dictionary<string, BasePanel>();
    private Transform canvasTrans;
    private bool isInitialized = false;

    public void Init()
    {
        if (isInitialized)
        {
            Debug.Log("UISystem 已经初始化过，跳过");
            return;
        }
        isInitialized = true;

        Debug.Log("UISystem.Init() 开始初始化");
        Instance = this;

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("找不到Canvas对象！");
            return;
        }
        canvasTrans = canvasObj.transform;

        AddPanelToDict<LoginPanel>();
        AddPanelToDict<ShowPanel>();
        AddPanelToDict<ChoicePanel>();
        AddPanelToDict<ConfirmPanel>();
        AddPanelToDict<InputPanel>();
        AddPanelToDict<LoadPanel>();
        AddPanelToDict<GamePanel>();
        AddPanelToDict<MainShopPanel>();
        AddPanelToDict<DialogPanel>();
        AddPanelToDict<AnimationPanel>();
        AddPanelToDict<SetPanel>();

        AddPanelToDict<MaskPanel>();


        OpenPanel<LoginPanel>();
        Debug.Log($"UISystem初始化完成，共注册 {panelDict.Count} 个面板");
    }

    private void AddPanelToDict<T>() where T : BasePanel
    {
        string panelType = typeof(T).ToString();
        if (panelDict.ContainsKey(panelType))
        {
            Debug.Log("当前字典已有：" + panelType);
            return;
        }

        GameObject prefab = GameResSystem.GetRes<GameObject>("Prefabs/UI/" + panelType);
        if (prefab == null)
        {
            Debug.LogError($"找不到面板预制体：Prefabs/UI/{panelType}");
            return;
        }

        GameObject panelGo = GameObject.Instantiate(prefab, canvasTrans);
        panelDict.Add(panelType, panelGo.AddComponent<T>());
        panelDict[panelType].OnInit();
        Debug.Log($"成功添加面板：{panelType}");
    }

    public void OpenPanel<T>(params object[] objs) where T : BasePanel
    {
        string panelType = typeof(T).ToString();
        if (!panelDict.ContainsKey(panelType))
        {
            Debug.LogError($"当前字典不包含：{panelType}");
            return;
        }
        Debug.Log($"打开面板：{panelType}");
        panelDict[panelType].OnShow(objs);
    }

    public void ClosePanel(string panelType)
    {
        if (!panelDict.ContainsKey(panelType)) return;
        panelDict[panelType].OnClose();
    }

    public void UnLoadLoginPanelsAndLoadGamePanels() { }
    private void RemovePanelInDict(string panelType) { }
}