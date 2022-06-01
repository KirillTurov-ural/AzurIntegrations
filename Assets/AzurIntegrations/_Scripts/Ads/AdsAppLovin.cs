using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class AdsAppLovin : AdsBase
    {
        [SerializeField]
        private string          sdkKey = "6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR";
        [SerializeField]
        private string          reportKey = "_kcJV4v8Td-r-cg789xwkL6rvU_QWICV0pqWtieKLcAJEsQN_MHiiOTctX1NqDur0hLuP_ClcN3rDUDCsVwTuS";        
        [SerializeField]
        private DataInt         privacy = new DataInt("int.privacy");
        [SerializeField]
        private bool            enableDebugger = true;

        private int             countClicks = 0;

        public override StaticType StaticType => StaticType.AdsAppLovin;

       

        public override void Reaction(Message message, params object[] parameters)
        {
            base.Reaction(message, parameters);

            switch (message)
            {
                case Message.PrivatePolicyAgreed:
#if APPLOVIN_INT
                    MaxSdk.SetHasUserConsent(true);
#endif
                    break;

                case Message.ShowDebugger:
#if APPLOVIN_INT
                    MaxSdk.ShowMediationDebugger();
#endif
                    break;
            }
        }

        public override void Init()
        {
#if APPLOVIN_INT
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                base.Init();

                switch (sdkConfiguration.ConsentDialogState)
                {
                    case MaxSdkBase.ConsentDialogState.Applies:
                        break;

                    case MaxSdkBase.ConsentDialogState.DoesNotApply:
                        privacy.Value = 1;
                        break;
                }
                Event(Message.StaticTypeInited, StaticType);
            };

            MaxSdk.SetSdkKey(sdkKey);
            MaxSdk.InitializeSdk();
#else

            Event(Message.StaticTypeInited, StaticType);
#endif
        }
#if APPLOVIN_INT
#endif
    }

    public class AdsLogicAppLovinInterstitial : AdsLogicId
    {
        public AdsLogicAppLovinInterstitial(string placementId, string platformId, System.Action<bool> callback) : base(placementId, platformId, callback)
        {
            Subscribe();
        }

        public override bool Ready
        {
            get
            {
#if APPLOVIN_INT
                return MaxSdk.IsInterstitialReady(platformId);
#else
                return base.Ready;
#endif
            }
        }

        public override void Load(params object[] parameters)
        {
            base.Load(parameters);
            if (Application.isEditor)
            {
                return;
            }
            
#if APPLOVIN_INT
            MaxSdk.LoadInterstitial(platformId);
#endif
        }

        public override void Show(params object[] parameters)
        {
            base.Show(parameters);

#if APPLOVIN_INT
            if (Ready)
            {
                MaxSdk.ShowInterstitial(platformId);
            }
            else
            {
                Load();
            }
#endif
        }

        private void Subscribe()
        {
            //Debug.LogError(placementId + " Subscribe");
#if APPLOVIN_INT
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClicked;
            // Load the first interstitial
            //Load();
#endif
        }

#if APPLOVIN_INT
        private void OnInterstitialClicked(string adUnitId, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarningFormat("OnInterstitialClicked: {0}", adUnitId);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarningFormat("OnInterstitialDisplayedEvent: {0}", adUnitId);
            if (callback != null)
            {
                callback(true);
            }
            TryToLoad();
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarningFormat("OnInterstitialLoadedEvent: {0}", adUnitId);
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
        }

        private void TryToLoad()
        {
            if (!Ready)
            {
                MonoBehaviourBase.Coroutines.StartStaticCoroutine(this, LoadCoroutine());
            }
        }

        private IEnumerator LoadCoroutine()
        {
            yield return new WaitForSeconds(3);
            Load();
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend re-trying in 3 seconds.
            //Debug.LogWarningFormat("OnInterstitialFailedEvent: {0}=>{1}", adUnitId, errorInfo.Code);
            TryToLoad();
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            //Debug.LogWarningFormat("InterstitialFailedToDisplayEvent: {0}=>{1}", adUnitId, errorInfo.Code);
            TryToLoad();
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            //Debug.LogWarningFormat("OnInterstitialHiddenEvent: {0}", adUnitId);
            TryToLoad();
        }
#endif
    }

    public class AdsLogicAppLovinBanner : AdsLogicId
    {
        private bool show = false;

        public AdsLogicAppLovinBanner(string placementId, string platformId, System.Action<bool> callback) : base(placementId, platformId, callback)
        {
            Subscribe();
        }

        public override bool Ready
        {
            get
            {
                return !show;
            }
        }

        private void Subscribe()
        {
            //Debug.LogError(placementId + " Subscribe");

#if APPLOVIN_INT
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadFailedEvent;

            //Debug.LogWarning("BANNER CREATE");
            MaxSdk.CreateBanner(platformId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerPlacement(platformId, placementId);
            MaxSdk.SetBannerBackgroundColor(platformId, Color.white);
#endif
        }

#if APPLOVIN_INT
        private void OnBannerAdLoadedEvent(string obj, MaxSdkBase.AdInfo info)
        {

        }
#endif

        public override void Show(params object[] parameters)
        {
            base.Show(parameters);

#if APPLOVIN_INT
            if (!show)
            {
                MonoBehaviourBase.Coroutines.StartStaticCoroutine(this, ShowCoroutine());
                show = true;
            }
#endif
        }

        private IEnumerator ShowCoroutine()
        {
            yield return new WaitForSeconds(1f);
#if APPLOVIN_INT
            MaxSdk.ShowBanner(platformId);
#endif
            //Debug.LogWarning("BANNER ON");
        }

        public override void Stop()
        {
            if (show)
            {
#if APPLOVIN_INT
                MaxSdk.HideBanner(platformId);
                //Debug.LogWarning("BANNER OFF");

                show = false;
#endif
            }
        }

#if APPLOVIN_INT
        private void OnBannerAdLoadFailedEvent(string arg1, MaxSdkBase.AdInfo info)
        {
        }

        private void OnBannerAdExpandedEvent(string obj, MaxSdkBase.AdInfo info)
        {
        }

        private void OnBannerAdCollapsedEvent(string obj, MaxSdkBase.AdInfo info)
        {
        }

        private void OnBannerAdClickedEvent(string obj, MaxSdkBase.AdInfo info)
        {
        }
#endif
    }

    public class AdsLogicAppLovinRewarded : AdsLogicId
    {
        public AdsLogicAppLovinRewarded(string placementId, string platformId, System.Action<bool> callback) : base(placementId, platformId, callback
            )
        {
            Subscribe();
        }

        private void Subscribe()
        {
            //Debug.LogError(placementId + " Subscribe");

#if APPLOVIN_INT
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
#endif
        }

#if APPLOVIN_INT
        private void OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarning("OnRewardedAdReceivedRewardEvent: " + arg2.Label + "=>" + arg2.Amount);

            if (callback != null)
            {
                callback(true);
            }

            TryToLoad();
        }


        private void OnRewardedAdLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo errorInfo)
        {
            //Debug.LogWarning("OnRewardedAdLoadFailedEvent! " + arg1);
            TryToLoad();
        }

        private void OnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo errorInfo)
        {
            //Debug.LogWarning("OnAdRevenuePaidEvent! " + arg1);
            TryToLoad();
        }
#endif

        private void TryToLoad()
        {
            if (!Ready)
            {
                MonoBehaviourBase.Coroutines.StartStaticCoroutine(this, LoadCoroutine());
            }
        }

        private IEnumerator LoadCoroutine()
        {
            yield return new WaitForSeconds(3);
            Load();
        }

#if APPLOVIN_INT
        private void OnRewardedAdLoadedEvent(string obj, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarning("OnRewardedAdLoadedEvent!");
        }

        private void OnRewardedAdHiddenEvent(string obj, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarning("OnRewardedAdHiddenEvent!");
            if (callback != null)
            {
                callback(false);
            }
            TryToLoad();
        }

        private void OnRewardedAdFailedToDisplayEvent(string arg1, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            //Debug.LogWarning("OnRewardedAdFailedToDisplayEvent!");
            if (callback != null)
            {
                callback(false);
            }
            TryToLoad();
        }

        private void OnRewardedAdDisplayedEvent(string obj, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarning("OnRewardedAdDisplayedEvent!");
        }

        private void OnRewardedAdClickedEvent(string obj, MaxSdkBase.AdInfo info)
        {
            //Debug.LogWarning("OnRewardedAdClickedEvent!");
        }
#endif

        public override bool Ready
        {
            get
            {
#if APPLOVIN_INT
                return MaxSdk.IsRewardedAdReady(platformId);
#else
                return base.Ready;
#endif
            }
        }

        public override void Load(params object[] parameters)
        {
            base.Load(parameters);
            if (Application.isEditor)
            {
                return;
            }

#if APPLOVIN_INT
            //Debug.LogError("LoadRewardedAd:" + platformId);
            MaxSdk.LoadRewardedAd(platformId);
#endif
        }


        public override void Show(params object[] parameters)
        {
            base.Show(parameters);
#if APPLOVIN_INT
            if (Ready)
            {
                MaxSdk.ShowRewardedAd(platformId);
            }
            else
            {
                if (callback != null)
                {
                    callback(false);
                }

                Load();
            }
#endif
        }
    }
}