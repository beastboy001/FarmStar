using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//功能说明：跳跃状态
public class JumpState : BaseState
{

    public JumpState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs) : base(characterFSM, animator, cs)
    {
    }

    public override void EnterState()
    {
        animator.SetBool("IsGround", false);
        animator.CrossFade("Jump", 0.1f);

    }

    public override void ExitState()
    {
        animator.SetBool("IsGround", true);
    }

    public override void InitState()
    {

    }

    public override void UpdateState()
    {

    }
}