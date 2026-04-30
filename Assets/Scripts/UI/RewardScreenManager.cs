using UnityEngine;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI rewardButtonText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rewardText == null)
        {
            TextMeshProUGUI[] texts = rewardUI.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.text == "Wave Time")
                {
                    rewardText = text;
                    break;
                }
            }
        }

        if (rewardButtonText == null)
        {
            TextMeshProUGUI[] texts = rewardUI.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.text == "Next Wave")
                {
                    rewardButtonText = text;
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            rewardUI.SetActive(true);
            if (rewardText != null)
            {
                rewardText.text = "Wave " + GameManager.Instance.currentWave + " Complete!\nTime: " + GameManager.Instance.lastWaveTime.ToString("0.0") + "s";
            }
            if (rewardButtonText != null)
            {
                rewardButtonText.text = "Next Wave";
            }
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            rewardUI.SetActive(true);
            if (rewardText != null)
            {
                rewardText.text = GameManager.Instance.gameOverMessage + "\nTime: " + GameManager.Instance.lastWaveTime.ToString("0.0") + "s";
            }
            if (rewardButtonText != null)
            {
                rewardButtonText.text = "Return to Start";
            }
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }
}
