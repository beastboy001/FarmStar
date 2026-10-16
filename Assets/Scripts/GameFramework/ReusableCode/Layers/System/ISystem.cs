using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：系统层接口
//***************************************** 
public interface ISystem : INeedInit, ICanSendCommand, ICanSendEvent, ICanRegistAndUnRegistEvent
{

}
