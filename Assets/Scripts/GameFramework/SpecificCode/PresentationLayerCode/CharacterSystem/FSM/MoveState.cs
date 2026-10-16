using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//功能说明：移动状态
public class MoveState : BaseState
{
    public MoveState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs) : base(characterFSM, animator, cs)
    {
    }

    public override void EnterState()
    {
        animator.SetBool("MoveState",true);
    }

    public override void ExitState()
    {
        animator.SetBool("MoveState", false);
    }

    public override void InitState()
    {
        
    }

    public override void UpdateState()
    {
        float h = cfsm.GetFloatInputValue(InputCode.HorizontalMoveValue);
        float v = cfsm.GetFloatInputValue(InputCode.VerticalMoveValue);
        int moveValue = 0; // 默认值
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveValue = 1;
        }
        else
        {
            moveValue = 0;
        }
        animator.SetFloat("MoveValue", moveValue);
        //if (v!=0)
        //{
        //    animator.SetFloat("MoveValue",v);
        //}
        //else
        //{
        //    if (h!=0)
        //    {
        //        animator.SetFloat("MoveValue", 1);
        //    }
        //}
    }
}
