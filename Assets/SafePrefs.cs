using UnityEngine;

public static class SafePrefs
{
    public static int GetInt(string key, int defaultValue = 0)
    {
        try
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        catch (System.Exception exception)
        {
            Report(key, exception.Message);
            return defaultValue;
        }
    }

    public static void SetInt(string key, int value)
    {
        try
        {
            PlayerPrefs.SetInt(key, value);
        }
        catch (System.Exception exception)
        {
            Report(key, exception.Message);
        }
    }

    public static void Save()
    {
        try
        {
            PlayerPrefs.Save();
        }
        catch (System.Exception exception)
        {
            Report("Save", exception.Message);
        }
    }

    private static void Report(string key, string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning("PlayerPrefs failed (" + key + "): " + message);
#endif
    }
}
