using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//功能说明：事件
public class EventRegistration:IEventRegistraion
{
    public Action<object> OnEvent = obj => { }; 
}
//public Action<object> OnEvent;


//    public EventRegistration()
//{
//    OnEvent = EmptyMethod; // 在构造函数中赋值
//}

//private void EmptyMethod(object obj)
//{
//    // 什么都不做
//}