using UnityEngine;

public class FollowParentPositionOnly : MonoBehaviour
{
    private Transform parentTransform;
    private Vector3 localOffset;      // 相对于父物体的原始位置偏移
    private Quaternion fixedRotation; // 固定的旋转

    void Start()
    {
        parentTransform = transform.parent;

        if (parentTransform != null)
        {
            // 记录相对于父物体的位置偏移
            localOffset = transform.localPosition;
        }

        // 记录固定的旋转角度
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (parentTransform != null)
        {
            // 位置 = 父物体位置 + 偏移量
            transform.position = parentTransform.position + localOffset;

            // 旋转保持固定，不跟随父物体
            transform.rotation = fixedRotation;
        }
    }
}