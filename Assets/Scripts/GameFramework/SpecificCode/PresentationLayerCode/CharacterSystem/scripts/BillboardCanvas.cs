using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Transform mainCamera;
    private Canvas canvas;
    private Vector3 originalScale;

    void Start()
    {
        mainCamera = Camera.main?.transform;
        canvas = GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }

        // 保存原始缩放，不要修改它
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 让Canvas始终面向摄像机
            transform.LookAt(mainCamera.position);
            // 反向旋转，让文字正面朝向摄像机
            transform.Rotate(0, 180, 0);

            // 恢复原始缩放，确保不被修改
            transform.localScale = originalScale;
        }
    }
}