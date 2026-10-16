using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：闲置状态
//***************************************** 
public class IdleState : BaseState
{
    public IdleState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs) : base(characterFSM, animator, cs)
    {
        
    }

    public override void EnterState()
    {
        animator.SetBool("MoveState", false);   
    }

    public override void ExitState()
    {
    }

    public override void InitState()
    {
        
    }

    public override void UpdateState()
    {
    }
}
