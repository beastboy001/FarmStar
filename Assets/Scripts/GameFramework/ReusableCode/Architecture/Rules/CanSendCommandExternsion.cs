using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：可以发送命令的拓展方法
//***************************************** 
public static class CanSendCommandExternsion
{
    public static void SendCommand<T>(this ICanSendCommand self,object dataObj=null) where T:ICommand,new ()
    {
        StartArchitecture.Instance.GetArchitecture().SendCommand<T>(dataObj);
    }
}
