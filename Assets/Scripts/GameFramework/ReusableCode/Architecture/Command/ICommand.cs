using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：命令接口
//***************************************** 
public interface ICommand
{
    public void Execute(object dataObj);
}
