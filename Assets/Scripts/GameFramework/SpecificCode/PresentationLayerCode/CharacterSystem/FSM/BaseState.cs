using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：状态基类
//***************************************** 
public abstract class BaseState
{
    protected CharacterFSM cfsm;
    protected Animator animator;
    public CHARACTERSTATE stateType;

    //这个构造函数在创建状态对象时被调用，用于初始化状态的基本属性。
    public BaseState(CharacterFSM characterFSM,Animator animator,CHARACTERSTATE cs)  
    {
        cfsm = characterFSM;
        this.animator = animator;
        stateType = cs;
    }
    /// <summary>
    /// 初始化状态
    /// </summary>
    public abstract void InitState();
    /// <summary>
    /// 进入状态
    /// </summary>
    public abstract void EnterState();
    /// <summary>
    /// 退出状态
    /// </summary>
    public abstract void ExitState();
    /// <summary>
    /// 更新状态
    /// </summary>
    public abstract void UpdateState();
}

public enum CHARACTERSTATE
{ 
    //NONE,
    IDLE,
    WALK,
    JUMP,
    ATTACK,
    HIT,
    DEAD
}
