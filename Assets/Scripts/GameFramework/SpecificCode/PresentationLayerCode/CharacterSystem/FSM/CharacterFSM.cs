using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//功能说明：人物有限状态机     //用来控制角色状态//而basestate则是用来播放对应的动画
public class CharacterFSM : MonoBehaviour
{
    private Dictionary<CHARACTERSTATE, BaseState> statesDict;
    private BaseState currentState;
    private BaseState lastState;
    private InputController ic;
    public CHARACTERSTATE c;
    /// <summary>
    /// 初始化状态机
    /// </summary>
    public void InitFSM(Animator currentAnimator,InputController inputController)
    {
        statesDict = new Dictionary<CHARACTERSTATE, BaseState>()
        {
            { CHARACTERSTATE.IDLE,new IdleState(this,currentAnimator,CHARACTERSTATE.IDLE)},
            { CHARACTERSTATE.WALK ,new MoveState(this,currentAnimator,CHARACTERSTATE.WALK)},
            { CHARACTERSTATE.JUMP,new JumpState(this,currentAnimator,CHARACTERSTATE.JUMP)}
        };
        ic = inputController;
        SetDefaultState();
    }
    /// <summary>
    /// 设置默认状态并初始化其他状态
    /// </summary>
    private void SetDefaultState()
    {
        foreach (var item in statesDict)
        {
            item.Value.InitState();
        }
        currentState = statesDict[CHARACTERSTATE.IDLE];
        currentState.EnterState();
    }
    /// <summary>
    /// 改变状态
    /// </summary>
    public void ChangeState(CHARACTERSTATE newStateType)
    {
        if (statesDict.ContainsKey(newStateType))
        {
            BaseState changeState = statesDict[newStateType];
            if (changeState!=currentState)
            {
                currentState.ExitState();
                lastState = currentState;
                currentState = changeState;
                currentState.EnterState();   //相当于运行动画
            }
        }
    }
    /// <summary>
    /// 获取当前状态
    /// </summary>
    /// <returns></returns>
    public CHARACTERSTATE GetCurrentState()
    {
        return currentState.stateType;
    }
    /// <summary>
    /// 获取上一个状态
    /// </summary>
    /// <returns></returns>
    public CHARACTERSTATE GetLastState()
    {
        return lastState.stateType;
    }

    void Update()
    {
       
        if (currentState!=null)
        {
            c = currentState.stateType;
            currentState.UpdateState();
        }
    }

    #region 输入值的设置和获取    //这样是为了只能通过FSM来访问输入系统
    /// <summary>
    /// 设置当前某个输入状态的值(bool)
    /// </summary>
    /// <param name="inputCode"></param>
    /// <param name="inputValue"></param>
    public void SetInputValue(string inputCode, bool inputValue)      
    {
        ic.SetInputValue(inputCode,inputValue);
    }
    /// <summary>
    /// 获取当前某个输入的状态(bool)
    /// </summary>
    /// <param name="inputCode"></param>
    /// <returns></returns>
    public bool GetBoolInputValue(string inputCode)
    {
        return ic.GetBoolInputValue(inputCode);
    }
    /// <summary>
    /// 设置当前某个输入状态的值(float)
    /// </summary>
    /// <param name="inputCode"></param>
    /// <param name="inputValue"></param>
    public void SetInputValue(string inputCode, float inputValue)
    {
        ic.SetInputValue(inputCode, inputValue);
    }
    /// <summary>
    /// 获取当前某个输入的状态(float)
    /// </summary>
    /// <param name="inputCode"></param>
    /// <returns></returns>
    public float GetFloatInputValue(string inputCode)
    {
        return ic.GetFloatInputValue(inputCode);
    }
    #endregion

}
