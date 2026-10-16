using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：UI系统接口
//***************************************** 
public interface IUISystem : ISystem
{
    /// <summary>
    /// 打开该类型的面板
    /// </summary>
    /// <param name="panelType"></param>
    public void OpenPanel<T>(params object[] objs) where T : BasePanel;
    /// <summary>
    /// 关闭该类型的面板
    /// </summary>
    /// <param name="panelType"></param>
    public void ClosePanel(string panelType);
    /// <summary>
    /// 加载游戏面板
    /// </summary>
    public void UnLoadLoginPanelsAndLoadGamePanels();
    /// <summary>
    /// 当前选择的人物数据
    /// </summary>
}
