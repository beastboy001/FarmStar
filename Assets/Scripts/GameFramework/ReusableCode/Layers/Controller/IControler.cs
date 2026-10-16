using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：表现层接口
//***************************************** 
public interface IController : ICanGetSystem, ICanSendEvent, ICanRegistAndUnRegistEvent, ICanSendCommand
{

}
