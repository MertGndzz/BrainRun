using UnityEngine;
using TMPro;

public class HighScore : MonoBehaviour
{
    [Header("UI Referans?")]
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        int savedRecord = SafePrefs.GetInt(ScoreManager.HIGH_SCORE_KEY, 0);

        if (highScoreText != null)
        {
            highScoreText.text = "Rekor: " + savedRecord.ToString();
        }
    }
}
