using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Referans?")]
    public TextMeshProUGUI skorText;

    [Header("Animasyon Ayarlar?")]
    public float punchScale = 1.4f;
    public float animDuration = 0.25f;
    public Color punchColor = new Color(1f, 0.85f, 0.2f);

    private int skor = 0;
    private int cachedHighScore;
    private Vector3 originalScale;
    private Color originalColor;
    private bool hasCachedVisuals;
    private Coroutine punchCoroutine;

    public const string HIGH_SCORE_KEY = "HighScore";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        cachedHighScore = SafePrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void Start()
    {
        if (instance != this)
        {
            return;
        }

        if (skorText != null)
        {
            originalScale = skorText.transform.localScale;
            originalColor = skorText.color;
            hasCachedVisuals = true;
        }

        SetScoreUIVisible(false);
    }

    private void OnDisable()
    {
        StopPunch();
    }

    private void OnDestroy()
    {
        StopPunch();

        if (instance == this)
        {
            instance = null;
        }
    }

    public void SetScoreUIVisible(bool visible)
    {
        if (skorText == null)
        {
            return;
        }

        skorText.gameObject.SetActive(visible);
        if (visible)
        {
            skorText.text = skor.ToString();
            skorText.transform.localScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
        }
    }

    public void AddScore(int amount)
    {
        skor += amount;

        if (skorText != null)
        {
            skorText.text = skor.ToString();

            if (punchCoroutine != null)
            {
                StopCoroutine(punchCoroutine);
            }

            punchCoroutine = StartCoroutine(ScorePunchAnimation());
        }

        if (skor > cachedHighScore)
        {
            cachedHighScore = skor;
            SafePrefs.SetInt(HIGH_SCORE_KEY, skor);
            SafePrefs.Save();
        }
    }

    private IEnumerator ScorePunchAnimation()
    {
        if (skorText == null)
        {
            yield break;
        }

        Transform scoreTransform = skorText.transform;
        Vector3 baseScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
        float elapsed = 0f;

        scoreTransform.localScale = baseScale * punchScale;
        skorText.color = punchColor;

        while (elapsed < animDuration)
        {
            if (skorText == null)
            {
                punchCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / animDuration;

            scoreTransform.localScale = Vector3.Lerp(baseScale * punchScale, baseScale, progress);
            skorText.color = Color.Lerp(punchColor, originalColor, progress);

            yield return null;
        }

        if (skorText != null)
        {
            scoreTransform.localScale = baseScale;
            skorText.color = originalColor;
        }

        punchCoroutine = null;
    }

    public int GetCurrentScore()
    {
        return skor;
    }

    public int GetHighScore()
    {
        return cachedHighScore;
    }

    private void StopPunch()
    {
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }

        if (!hasCachedVisuals || skorText == null)
        {
            return;
        }

        skorText.transform.localScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
        skorText.color = originalColor;
    }
}
