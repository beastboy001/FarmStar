using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : BasePanel
{
    private Button ExitButton;
    private Button ConfirmButton;

    // 记录要删除的存档槽位
    private int targetSlotIndex = -1;

    public override void OnInit()
    {
        base.OnInit();

        //寻找组件
        ExitButton = DeepFindTransform.DeepFindChild(transform, "ExitButton").GetComponent<Button>();
        ConfirmButton = DeepFindTransform.DeepFindChild(transform, "ConfirmButton").GetComponent<Button>();

        //添加监听
        ExitButton.onClick.AddListener(OnExitButton);
        ConfirmButton.onClick.AddListener(OnConfirmButton);
    }

    /// <summary>
    /// 重写OnShow方法，接收要删除的存档槽位
    /// </summary>
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);

        // 从参数中获取要删除的存档槽位
        if (objs.Length > 0 && objs[0] is int)
        {
            targetSlotIndex = (int)objs[0];
            Debug.Log("确认面板 - 准备删除存档：" + (targetSlotIndex + 1));
        }
        else
        {
            Debug.LogError("ConfirmPanel: 没有接收到要删除的存档槽位");
        }
    }

    public void OnExitButton()
    {
        Debug.Log("取消删除");
        OnClose();
    }

    public void OnConfirmButton()
    {
        if (targetSlotIndex >= 0)
        {
            Debug.Log("确认删除存档：" + (targetSlotIndex + 1));

            // 执行删除操作
            SaveManager.DeleteSave(targetSlotIndex);

            // 通知ChoicePanel刷新显示（通过静态方法或事件）
            // 这里我们通过调用ChoicePanel的静态方法来刷新
            ChoicePanel.RefreshSlotDisplay(targetSlotIndex);
        }

        OnClose();
    }
}