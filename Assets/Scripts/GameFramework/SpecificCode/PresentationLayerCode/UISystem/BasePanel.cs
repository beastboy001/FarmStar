using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：面板基类
//***************************************** 
public class BasePanel : MonoBehaviour,IBasePanel
{
    protected IUISystem uiSystem;

    public virtual void OnClose()
    {
        gameObject.SetActive(false);

        if (this is MainShopPanel)
        {
            PlayerFarmAction playerAction = FindObjectOfType<PlayerFarmAction>();
            if (playerAction != null)
            {
                playerAction.OnBackpackClosed();
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (this is SetPanel)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public virtual void OnInit()    //继承Base panel的子类要在初始化的时候调用一下这个方法，这样就可以获取uisystem，也可以隐藏子类面板
    {
        uiSystem = this.GetSystem<IUISystem>();
        gameObject.SetActive(false);
    }

    public virtual void OnShow(params object[] objs)
    {
        gameObject.SetActive(true);
    }
}
