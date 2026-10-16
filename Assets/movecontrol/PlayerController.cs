using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    private Animator anim;
    private Vector3 moveDir;

    void Start() => anim = GetComponent<Animator>();

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveDir = new Vector3(h, 0, v).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
        anim.SetFloat("MoveSpeed", moveDir.magnitude);
    }

    void FixedUpdate()
    {
        GetComponent<Rigidbody>().MovePosition(transform.position + moveDir * moveSpeed * Time.fixedDeltaTime);
    }

    internal void Initialize(PlayerData data)
    {
        throw new NotImplementedException();
    }
}