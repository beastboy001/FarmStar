[System.Serializable]
public class CropData
{
    public string cropId;
    public int growthStage;
    public float plantGameTime;      // 种植时的游戏时间（秒）
    public float totalGrowTime;      // 需要的总生长时间（秒）
    public bool isMature;
    public float growthProgress;

    public CropData(string id, float growMinutes, float currentGameTime)
    {
        cropId = id;
        growthStage = 0;
        plantGameTime = currentGameTime;
        totalGrowTime = growMinutes * 60f;
        isMature = false;
        growthProgress = 0;
    }
}