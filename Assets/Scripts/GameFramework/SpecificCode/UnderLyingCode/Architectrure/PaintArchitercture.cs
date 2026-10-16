using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintArchitercture : Architecture<PaintArchitercture>
{
    protected override void Init()
    {
        //在init方法里面注册系统
        //注册UI系统
        this.RegistSystem<IUISystem>(new UISystem());
    }
}
