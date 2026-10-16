using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//功能说明：
public class GameEventSystem : IEventSystem
{
    private Dictionary<Type, IEventRegistraion> eventRegistrationsDict = new Dictionary<Type, IEventRegistraion>();

    public void Regist<T>(Action<object> onEvent)     //T你要监听的事件的类型//T 是你要监听的具体事件类，比如 PlayerHurtEvent, GameStartEvent 等
    {
        var type = typeof(T);
        IEventRegistraion eventRegistraion;
        if (eventRegistrationsDict.TryGetValue(type,out eventRegistraion))    //TryGetValue这个方法需要out参数
        {
            (eventRegistraion as EventRegistration).OnEvent += onEvent;    //eventRegistraion这个是接口类型要转为实际类型
        }                                                                   //光是接口没有这个方法，所以要转换一下类型
        else
        {
            eventRegistraion = new EventRegistration()   //等价于：eventRegistraion = new EventRegistration(); // 1. 创建对象
            {                                                   //eventRegistraion.OnEvent = onEvent;         // 2. 设置属性
                OnEvent = onEvent
            };
            eventRegistrationsDict.Add(type,eventRegistraion);
        }
    }

    public void Send<T>(object obj) where T : new()
    {
        var type = typeof(T);
        IEventRegistraion eventRegistraion;   //创建具体的消息内容
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistraion))
        {
            (eventRegistraion as EventRegistration).OnEvent.Invoke(obj);
        }
    }

    public void UnRegist<T>(Action<object> onEvent)
    {
        var type = typeof(T);
        IEventRegistraion eventRegistraion;
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistraion))
        {
            (eventRegistraion as EventRegistration).OnEvent -= onEvent;
        }
    }
}
