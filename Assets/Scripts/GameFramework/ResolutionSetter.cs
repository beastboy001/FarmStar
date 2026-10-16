using UnityEngine;

public class ResolutionSetter : MonoBehaviour
{
    [Header("固定分辨率")]
    public int targetWidth = 1920;
    public int targetHeight = 1080;

    [Header("是否全屏")]
    public bool fullscreen = false;

    private void Awake()
    {
        Screen.SetResolution(targetWidth, targetHeight, fullscreen);
    }
}