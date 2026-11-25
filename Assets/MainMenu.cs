using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";

    public void PlayGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(GameSceneName))
        {
            ReportSceneFailure(GameSceneName);
            return;
        }

        try
        {
            SceneManager.LoadScene(GameSceneName);
        }
        catch (System.Exception exception)
        {
            ReportSceneFailure(exception.Message);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private static void ReportSceneFailure(string detail)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("Game scene could not be loaded: " + detail);
#endif
    }
}
