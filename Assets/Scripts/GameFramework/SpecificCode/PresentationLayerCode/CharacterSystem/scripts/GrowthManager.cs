using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log(" GrowthManager 启动 - 作物生长基于真实时间");
    }

    // 不需要 Update，因为 Tile 自己会更新时间
}