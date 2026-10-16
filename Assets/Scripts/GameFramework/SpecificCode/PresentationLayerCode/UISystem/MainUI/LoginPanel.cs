using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel 
{
    private Button StartButton;
    //private Button NewButton;
    private Button ExitButton;
    private Button ShowButton;
    private Button SetButton;

    public override void OnInit()
    {
        base.OnInit();
        //寻找组件
        StartButton = transform.Find("StartButton").GetComponent<Button>();
        //NewButton = transform.Find("NewButton").GetComponent<Button>();
        ExitButton = transform.Find("ExitButton").GetComponent<Button>();
        ShowButton = transform.Find("ShowButton").GetComponent<Button>();
        SetButton = transform.Find("SetButton").GetComponent<Button>();

        //添加监听
        StartButton.onClick.AddListener(OnStartButton);
        //NewButton.onClick.AddListener(OnNewButton);
        ExitButton.onClick.AddListener(OnExitButton);
        ShowButton.onClick.AddListener(OnShowButton);
        SetButton.onClick.AddListener(OnSetButton);

    }

    public void OnStartButton()
    {
        OnClose();
        uiSystem.OpenPanel<ChoicePanel>();

    }
    //public void OnNewButton()
    //{
    //    Debug.Log("新游戏");
    //}
    public void OnExitButton()
    {
        Debug.Log("退出游戏");
    }
    public void OnSetButton()
    {
        // 从登录面板进入设置，关闭设置后保持鼠标可见
        uiSystem.OpenPanel<SetPanel>(true);
    }

    public void OnShowButton()
    {
        uiSystem.OpenPanel<ShowPanel>();
    }

}
