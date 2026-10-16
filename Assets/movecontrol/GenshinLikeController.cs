using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenshinLikeController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float rotateSpeed = 15f;

    [Header("相机设置")]
    public Camera mainCamera; // 拖入场景主相机
    private Vector3 moveDir;
    private Animator anim;
    private Rigidbody rb;
    private bool isGrounded; // 是否在地面
    private float currentSpeed;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标
    }

    void Update()
    {
        // 1. 鼠标控制相机旋转（需给相机添加Follow脚本，这里简化为角色旋转）
        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // 2. 移动方向与相机朝向对齐
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveDir = (cameraForward * v + cameraRight * h).normalized;

        // 3. 冲刺/跳跃判断
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            anim.SetTrigger("Jump");
            isGrounded = false;
        }

        // 4. 动画参数传递
        anim.SetFloat("MoveSpeed", moveDir.magnitude);
        anim.SetBool("IsSprinting", Input.GetKey(KeyCode.LeftShift) && moveDir.magnitude > 0.1f);
    }

    void FixedUpdate()
    {
        // 5. 移动（角色朝自身前方，移动方向由相机决定）
        if (moveDir.magnitude > 0.1f)
        {
            rb.MovePosition(transform.position + moveDir * currentSpeed * Time.fixedDeltaTime);
        }
    }

    // 6. 落地检测
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("IsGrounded", true);
        }
    }
}