using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.SceneManagement;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [Header("AdMob IDs")]
    [SerializeField] private string bannerId = "ca-app-pub-8887980314186674/6224864705";
    [SerializeField] private string interstitialId = "ca-app-pub-8887980314186674/4906137500";
    [SerializeField] private string rewardedId = "ca-app-pub-8887980314186674/6832929864";

    [Header("Config")]
    [Tooltip("Cada cuántos retries mostrar un interstitial")]
    public int retriesPerInterstitial = 3;

    [Tooltip("Cada cuántos niveles mostrar un interstitial")]
    public int levelsPerInterstitial = 0;

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private int retryCount = 0;
    private int levelsPlayed = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => { });

        RequestBanner();
        RequestInterstitial();
        RequestRewarded();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Escena cargada: " + scene.name + " → Forzando banner abajo");
        RequestBanner();

        // 🔹 Contar nivel jugado y mostrar interstitial si corresponde
        OnLevelStarted();
    }

    private void RequestBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);

        Debug.Log("Banner solicitado y forzado en parte inferior.");
    }

    private void RequestInterstitial()
    {
        AdRequest request = new AdRequest();

        InterstitialAd.Load(interstitialId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Error al cargar interstitial: " + error);
                Invoke(nameof(RequestInterstitial), 10f);
                return;
            }

            interstitialAd = ad;

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial cerrado, recargando...");
                RequestInterstitial();
            };
        });
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
    }

    private void RequestRewarded()
    {
        AdRequest request = new AdRequest();

        RewardedAd.Load(rewardedId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Error al cargar rewarded: " + error);
                Invoke(nameof(RequestRewarded), 10f);
                return;
            }

            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded cerrado, recargando...");
                RequestRewarded();
            };
        });
    }

    public void ShowRewarded(Action onRewardEarned)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Jugador ganó recompensa: " + reward.Amount);
                onRewardEarned?.Invoke();
            });
        }
    }

    // 🔹 Control de retries para interstitial
    public void OnRetry()
    {
        retryCount++;

        if (retriesPerInterstitial > 0 && retryCount % retriesPerInterstitial == 0)
        {
            ShowInterstitial();
        }
    }

    // 🔹 Control de niveles para interstitial
    public void OnLevelStarted()
    {
        levelsPlayed++;

        if (levelsPerInterstitial > 0 && levelsPlayed % levelsPerInterstitial == 0)
        {
            ShowInterstitial();
        }
    }
}