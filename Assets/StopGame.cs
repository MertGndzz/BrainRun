using UnityEngine;

public class StopGame : MonoBehaviour
{
    bool isPaused = false;

    public void TogglePause()
    {
        if (isPaused)
        {
            Time.timeScale = 1f; // oyun devam
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0f; // oyun durur
            isPaused = true;
        }
    }
}