using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMoveControl : MonoBehaviour
{
    //旋转
    public float rotateSpeed = 0.5f;
    //移动
    public float moveSpeed = 15f;
    //跑步速度
    public float runSpeed = 20f;
    //跳跃
    public float gravity = 9.8f;
    public float verticalVelocity = 0;
    public float maxJumpHeight = 10f;
    public float checkSphereRadius = 0.1f;

    public Transform checkpoint;

    private bool isGround;
    private bool isJumping;  //是否在跳跃过程中
    public LayerMask groundLayer;
    private Animator ani;


    protected InputController ic;
    private CharacterFSM characterFSM;
    private CharacterController characterController;

    void Start()
    {
        // 隐藏鼠标
        Cursor.visible = false;
        // 获取 CharacterController 组件
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController component not found!");
        }
        ani = GetComponent<Animator>();
        ic = gameObject.AddComponent<InputController>();        //动态添加组件
        characterFSM = gameObject.AddComponent<CharacterFSM>();  //动态添加组件
        characterFSM.InitFSM(ani, ic);
    }


    void Update()
    {
        //PlayerRotateViewControl();
        PlayerMoveAndJumpControl();
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    Cursor.visible = false;

        //}
    }

    //人物旋转
    private void PlayerRotateViewControl()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * rotateSpeed);
    }
    //人物跑跳
    private void PlayerMoveAndJumpControl()
    {
        Vector3 motionVector = Vector3.zero;   //每次调用时都归零一次，否则移动会出错
        float h = ic.GetFloatInputValue(InputCode.HorizontalMoveValue);
        float v = ic.GetFloatInputValue(InputCode.VerticalMoveValue);

        if (characterFSM.GetCurrentState() != CHARACTERSTATE.JUMP)
        {
            JudgeAndChangeStateIdleOrMove(v, h);
        }

        //控制人物前后左右移动
        motionVector += transform.forward * moveSpeed * v * Time.deltaTime;
        motionVector += transform.right * moveSpeed * h * Time.deltaTime;

        //控制人物起跳和落地
        isGround = Physics.CheckSphere(checkpoint.position, checkSphereRadius, groundLayer);  //检测是否落地 
                                                                                              //在脚部位置创建一个球形检测区域，判断是否与地面层碰撞
        if (!isGround || isJumping)  //!isGround = 不在地面（自然下落） //isJumping = 正在跳跃中 //判断这里的isJumping是否为true
        {   //只要满足其中一个，就应用重力
            verticalVelocity -= gravity * Time.deltaTime;

            // ... 落地检测逻辑
            if (verticalVelocity <= 0 && isJumping && isGround)   //按下跳跃键的时isJumping变为true//isGround这里是指与地面有碰撞
            {
                isJumping = false;
                JudgeAndChangeStateIdleOrMove(v, h);  //落地判断是跑步还是闲置   
            }
        }
        motionVector += Vector3.up * verticalVelocity * Time.deltaTime;

        //如果按下跳跃键就 isJumping = true
        if (ic.GetBoolInputValue(InputCode.JumpState))
        {
            if (isGround && isJumping == false)  //如果在地面
            {
                isJumping = true;
                verticalVelocity = Mathf.Sqrt(2 * gravity * maxJumpHeight);
                characterFSM.ChangeState(CHARACTERSTATE.JUMP);
            }
        }

        characterController.Move(motionVector);

        //如果在地面并且没有跳跃
        if (isGround && !isJumping)
        {
            verticalVelocity = 0;
            JudgeAndChangeStateIdleOrMove(v, h);
            transform.position = new Vector3(transform.position.x, 0.825f, transform.position.z);
        }
    }
    /// <summary>
    /// 判断状态切为idle还是Move
    /// </summary>
    private void JudgeAndChangeStateIdleOrMove(float v, float h)
    {
        if (v != 0 || h != 0)      //  || (或)
        {
            characterFSM.ChangeState(CHARACTERSTATE.WALK);
        }
        else
        {
            characterFSM.ChangeState(CHARACTERSTATE.IDLE);
        }
    }

}