using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//功能说明：架构抽象基类
public abstract class Architecture<T> : IArchitecture where T : new()
{
    private IOCContainer iocContainer = new IOCContainer();
    private GameEventSystem gameEventSystem = new GameEventSystem();

    public Architecture() { Init(); }

    public void InitAllModules()
    {
        iocContainer.InitAllModules();
    }

    public void RegistEvent<U>(Action<object> onEvent) where U : new()
    {
        gameEventSystem.Regist<U>(onEvent);
    }

    public void RegistModel<U>(U instance) where U : IModel     // 例：this.RegistSystem<INetSystem>(new NetSystem());

    {
        iocContainer.Register<U>(instance);
    }

    public void RegistSystem<U>(U instance) where U : ISystem
    {
        iocContainer.Register<U>(instance);
    }

    public void RegistUtility<U>(U instance) where U : IUtility
    {
        iocContainer.Register<U>(instance);
    }

    public void SendCommand<U>(object dataObj) where U : ICommand, new()
    {
        var command = new U();
        command.Execute(dataObj);
    }

    public void SendEvent<U>(object dataObj) where U : new()
    {
        gameEventSystem.Send<U>(dataObj);
    }

    public void UnRegistEvent<U>(Action<object> onEvent) where U : new()
    {
        gameEventSystem.UnRegist<U>(onEvent);
    }

    protected abstract void Init();
    public U GetSystem<U>() where U : class, ISystem
    {
        return iocContainer.Get<U>();
    }
    public U GetModel<U>() where U : class, IModel
    {
        return iocContainer.Get<U>();
    }
    public U GetUtility<U>() where U : class, IUtility
    {
        return iocContainer.Get<U>();
    }
}
