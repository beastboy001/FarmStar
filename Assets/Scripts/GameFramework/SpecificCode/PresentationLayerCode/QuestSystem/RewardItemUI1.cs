using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    public Image prizeImage;
    public Text prizeNum;

    public void Init(Sprite icon, int amount)
    {
        if (prizeImage != null)
        {
            if (icon != null)
            {
                prizeImage.sprite = icon;
                prizeImage.gameObject.SetActive(true);
            }
            else
                prizeImage.gameObject.SetActive(false);
        }
        if (prizeNum != null)
        {
            prizeNum.text = amount.ToString();
            prizeNum.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        if (prizeImage != null) prizeImage.gameObject.SetActive(false);
        if (prizeNum != null) prizeNum.gameObject.SetActive(false);
    }
}