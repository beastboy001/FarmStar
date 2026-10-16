using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Singleton<T> where T:class ,ISingleton
{
    private static T mInstance;
    public static T Instance     //提供外部访问入口
    {
        get
        {
            if (mInstance==null)
            {
                var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);//GetParameters这个方法是用来获取参数的
                if (ctor == null)
                {
                    throw new Exception("没有找到非公共构造函数" + typeof(T));
                }
                mInstance = ctor.Invoke(null) as T;
            }
            return mInstance;

        }
    }



}
