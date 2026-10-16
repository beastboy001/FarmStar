using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartArchitecture : Singleton<StartArchitecture>, ISingleton
{
    private IArchitecture gameArchitecture;

    private StartArchitecture()
    {
        Debug.Log("StartArchitecture 构造函数被调用");
        Init();
    }

    public void Init()
    {
        Debug.Log("StartArchitecture.Init() 被调用");
    }

    /// <summary>
    /// 实例化并设置不同项目的架构
    /// </summary>
    public void SetGameArchitecture(IArchitecture architecture)
    {
        Debug.Log($"SetGameArchitecture 被调用，传入架构: {architecture != null}");
        gameArchitecture = architecture;
    }

    /// <summary>
    /// 获取架构实例
    /// </summary>
    public IArchitecture GetArchitecture()
    {
        if (gameArchitecture == null)
        {
            Debug.LogWarning("警告：当前没有架构实例，请先调用 SetGameArchitecture");
        }
        else
        {
            Debug.Log("获取架构成功");
        }
        return gameArchitecture;
    }

    /// <summary>
    /// 初始化框架中的所有模块
    /// </summary>
    public void InitAllModulesInArchitecture()
    {
        Debug.Log("InitAllModulesInArchitecture 被调用");
        if (gameArchitecture != null)
        {
            gameArchitecture.InitAllModules();
            Debug.Log("所有模块初始化完成");
        }
        else
        {
            Debug.LogError("架构为空，无法初始化模块");
        }
    }
}