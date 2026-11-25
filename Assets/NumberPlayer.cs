using System.Collections;
using UnityEngine;
using TMPro;

public class NumberPlayer : MonoBehaviour
{
    public int value;
    public int wrongCount = 0;
    public int maxWrong = 2;
    public float reviveInvulnerabilitySeconds = 1.75f;

    public TextMeshPro text;

    public bool IsInvulnerable { get; private set; }

    private GateSpawner spawner;
    private Renderer[] blinkRenderers;
    private Coroutine invulnerabilityRoutine;

    void Awake()
    {
        EnsureSpawner();
        blinkRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void OnDisable()
    {
        StopInvulnerability(true);
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLivesUI(wrongCount, maxWrong);
        }
    }

    public int GetValue()
    {
        return value;
    }

    public void SetTargetValue(int newValue)
    {
        value = newValue;
        if (text != null)
        {
            text.text = value.ToString();
        }
    }

    public void CorrectChoice()
    {
        EnsureSpawner();
        if (spawner != null)
        {
            spawner.SpawnGates();
        }
    }

    public void WrongChoice()
    {
        wrongCount++;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLivesUI(wrongCount, maxWrong);
        }

        if (wrongCount < maxWrong)
        {
            EnsureSpawner();
            if (spawner != null)
            {
                spawner.SpawnGates();
            }
        }
        else
        {
            int currentScore = ScoreManager.instance != null ? ScoreManager.instance.GetCurrentScore() : 0;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TriggerGameOverWithSlowMotion(currentScore);
            }
        }
    }

    public void Revive()
    {
        wrongCount = Mathf.Max(0, maxWrong - 1);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLivesUI(wrongCount, maxWrong);
        }

        EnsureSpawner();
        if (spawner != null)
        {
            spawner.SpawnGates();
        }

        BeginInvulnerability(reviveInvulnerabilitySeconds);
    }

    public void ContinueWithoutPenalty()
    {
        EnsureSpawner();
        if (spawner != null)
        {
            spawner.SpawnGates();
        }
    }

    public void BeginInvulnerability(float seconds)
    {
        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
        }

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine(Mathf.Max(0.1f, seconds)));
    }

    private IEnumerator InvulnerabilityRoutine(float seconds)
    {
        IsInvulnerable = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < seconds)
        {
            visible = !visible;
            SetBlinkVisible(visible);
            yield return new WaitForSecondsRealtime(0.12f);
            elapsed += 0.12f;
        }

        SetBlinkVisible(true);
        IsInvulnerable = false;
        invulnerabilityRoutine = null;
    }

    private void StopInvulnerability(bool showMesh)
    {
        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
            invulnerabilityRoutine = null;
        }

        IsInvulnerable = false;
        if (showMesh)
        {
            SetBlinkVisible(true);
        }
    }

    private void SetBlinkVisible(bool visible)
    {
        if (blinkRenderers == null)
        {
            return;
        }

        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            if (blinkRenderers[i] != null)
            {
                blinkRenderers[i].enabled = visible;
            }
        }
    }

    private void EnsureSpawner()
    {
        if (spawner == null)
        {
            spawner = SceneQuery.FindFirst<GateSpawner>();
        }
    }
}
