using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance;

    [Header("Unity Ads")]
    public string androidGameId = "";
    public string iosGameId = "";
    public string androidRewardedPlacement = "Rewarded_Android";
    public string iosRewardedPlacement = "Rewarded_iOS";
    public bool testMode = true;

    [Header("Test")]
    [Tooltip("Aù?kken reklam yerine 1 saniyelik sahte ba?ar? ak??? ùal???r.")]
    public bool forceMockAds = false;

    private string gameId = "";
    private string rewardedPlacement = "";
    private Action<bool> pendingCallback;
    private bool rewardedLoaded;
    private bool loadInFlight;
    private bool showing;
    private bool initializing;
    private Coroutine mockRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolvePlatformIds();
    }

    private void Start()
    {
        if (Instance != this || ShouldUseMock() || !Advertisement.isSupported)
        {
            return;
        }

        BeginInitialize();
    }

    private void OnDestroy()
    {
        if (mockRoutine != null)
        {
            StopCoroutine(mockRoutine);
            mockRoutine = null;
        }

        pendingCallback = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowAd(Action<bool> callback)
    {
        ShowRewardedAd(callback);
    }

    public void ShowRewardedAd(Action<bool> callback)
    {
        if (callback == null)
        {
            return;
        }

        if (ShouldUseMock() || !Advertisement.isSupported)
        {
            if (mockRoutine != null)
            {
                callback.Invoke(false);
                return;
            }

            mockRoutine = StartCoroutine(MockRewardedAd(callback));
            return;
        }

        if (pendingCallback != null || showing)
        {
            callback.Invoke(false);
            return;
        }

        pendingCallback = callback;

        if (!Advertisement.isInitialized)
        {
            BeginInitialize();
            return;
        }

        if (rewardedLoaded)
        {
            ShowLoadedAd();
            return;
        }

        RequestLoad();
    }

    public void OnInitializationComplete()
    {
        initializing = false;
        RequestLoad();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        initializing = false;
        ReportFailure("Unity Ads init failed: " + message);
        CompletePending(false);
    }

    private void BeginInitialize()
    {
        if (Advertisement.isInitialized)
        {
            RequestLoad();
            return;
        }

        if (initializing)
        {
            return;
        }

        initializing = true;
        Advertisement.Initialize(gameId, testMode, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != rewardedPlacement)
        {
            return;
        }

        loadInFlight = false;
        rewardedLoaded = true;

        if (pendingCallback != null && !showing)
        {
            ShowLoadedAd();
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (placementId != rewardedPlacement)
        {
            return;
        }

        loadInFlight = false;
        rewardedLoaded = false;
        ReportFailure("Unity Ads load failed: " + message);
        CompletePending(false);
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        showing = false;
        rewardedLoaded = false;
        ReportFailure("Unity Ads show failed: " + message);
        CompletePending(false);
        RequestLoad();
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        showing = false;
        rewardedLoaded = false;
        bool earned = showCompletionState == UnityAdsShowCompletionState.COMPLETED;
        CompletePending(earned);
        RequestLoad();
    }

    private void ResolvePlatformIds()
    {
#if UNITY_IOS
        gameId = iosGameId;
        rewardedPlacement = string.IsNullOrEmpty(iosRewardedPlacement) ? "Rewarded_iOS" : iosRewardedPlacement;
#else
        gameId = androidGameId;
        rewardedPlacement = string.IsNullOrEmpty(androidRewardedPlacement) ? "Rewarded_Android" : androidRewardedPlacement;
#endif
    }

    private bool ShouldUseMock()
    {
        return forceMockAds || string.IsNullOrEmpty(gameId);
    }

    private void RequestLoad()
    {
        if (ShouldUseMock() || !Advertisement.isInitialized || loadInFlight || rewardedLoaded || showing)
        {
            return;
        }

        loadInFlight = true;
        Advertisement.Load(rewardedPlacement, this);
    }

    private void ShowLoadedAd()
    {
        if (!rewardedLoaded || showing)
        {
            return;
        }

        rewardedLoaded = false;
        showing = true;
        Advertisement.Show(rewardedPlacement, this);
    }

    private IEnumerator MockRewardedAd(Action<bool> callback)
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        mockRoutine = null;
        callback?.Invoke(true);
    }

    private void CompletePending(bool earned)
    {
        Action<bool> callback = pendingCallback;
        pendingCallback = null;
        showing = false;
        callback?.Invoke(earned);
    }

    private static void ReportFailure(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning(message);
#endif
    }
}
