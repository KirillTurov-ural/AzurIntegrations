using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System;

namespace BoGD
{
    [System.Serializable]
    public class InterstitialFrequenceSettings
    {
        [SerializeField]
        private string          id= "common";

        [SerializeField]
        private DataInt minCount = new DataInt("longs.ads.minrequestnumber");
        [SerializeField]
        private DataInt freqAds = new DataInt("longs.ads.freq");
        [SerializeField]
        private DataInt endsCount = new DataInt("longs.ads.currentrequestnumber");

        public string Id => id;

        public bool Check()
        {
            endsCount.Increment(1);
            if (minCount.Value > endsCount.Value)
            {
                //Debug.LogWarning("minCount.GetData<DataLong>().Value > endsCount.GetData<DataLong>().Value");
                return false;
            }

            if (endsCount.Value % Mathf.Max(freqAds.Value, 1) != 0)
            {
                //Debug.LogWarning("endsCount.GetData<DataLong>().Value % Mathf.Max(freqAds.GetData<DataLong>().Value, 1) != 0");
                return false;
            }

            return true;
        }

        public InterstitialFrequenceSettings()
        {

        }
    }

    [Serializable]
    public class InterstitialFrequenceSystem
    {
        [SerializeField]
        private InterstitialFrequenceSettings[] settings = new InterstitialFrequenceSettings[] { new InterstitialFrequenceSettings() };

        private Dictionary<string, InterstitialFrequenceSettings> settingsDict = null;
        private Dictionary<string, InterstitialFrequenceSettings> Settings
        {
            get
            {
                if(settingsDict == null)
                {
                    settingsDict = new Dictionary<string, InterstitialFrequenceSettings>();
                    foreach(var set in settings)
                    {
                        settingsDict[set.Id] = set;
                    }
                }

                return settingsDict;
            }
        }

        public bool Check(string placement)
        {
            if (placement.IsNullOrEmpty())
            {
                placement = "common";
            }

            InterstitialFrequenceSettings freq;
            if (!Settings.TryGetValue(placement, out freq))
            {
                Debug.LogWarningFormat("Can't find placement: {0}", placement);
                if (!Settings.TryGetValue("common", out freq))
                {
                    return false;
                }
            }

            return freq.Check();
        }

        public InterstitialFrequenceSystem()
        {

        }
    }


    [System.Serializable]
    public class InterstitialAdsHelper
    {
        [SerializeField]
        [Header("REMOVE ADS PURCHASES")]
        private DataPurchase[]                  purchasesAds = null;
        
        [SerializeField]
        [Header("REMOVE ADS ANY PURCHASE")]
        private bool                            removeAdsByAnyPurchase = false;
        
        [SerializeField]
        [Header("CHEAT BUILD (SKIP ANY ADS)")]
        private bool                            cheatBuild = false;

        [SerializeField]
        [Header("HOW OFTEN SHOW ADS")]
        private InterstitialFrequenceSystem     frequence = new InterstitialFrequenceSystem();
        
        [SerializeField]
        [Header("SEND EVENT 'SHOW OFFER'?")]
        private bool                            showRemoveAdsDialogue = true;
        
        [SerializeField]
        [Header("LAST OFFER SHOW DATE")]
        private DataInt                         lastADSShowDate = new DataInt("longs.ads.lastday");

        [SerializeField]
        [Header("SHOW BANNER")]
        private bool                            bannerEnabled = false;

        [SerializeField]
        [Header("DELAY BETWEEN INTERS")]
        private RequirementTimeResource         timeDelay = new RequirementTimeResource("resources.time.intestitial", 35);
        
        [SerializeField]
        [Header("INTER PLACEMENT ID")]
        private ReferencePriceAds               priceBattleCompleteAds = new ReferencePriceAds("interstitial");
        
        [SerializeField]
        [Header("PAGE ID (FOR UI)")]
        private string                          removeAdsPage = "page.remove_ads";
        
        private DataPurchases DataPurchases
        {
            get;
            set;
        }
        
        public void IncrementTimer(double delta)
        {
            timeDelay.Increment(delta);
        }

        public void UpdateTimer(double delta = 0)
        {
            timeDelay.Update();
            timeDelay.Increment(delta);
        }

        public bool BannerEnabled
        {
            get => bannerEnabled;
        }


        protected IStatic ui = null;
        protected IStatic UI
        {
            get
            {
                if (ui == null)
                {
                    ui = StaticType.UI.Instance<IStatic>();
                }
                return ui;
            }
        }

        public virtual bool CheckAvailable()
        {
            if (removeAdsByAnyPurchase)
            {
                if (DataPurchases.AnyPurchased)
                {
                    return false;
                }
            }
            else
            {
                if (purchasesAds != null)
                {
                    foreach (var purchaseAds in purchasesAds)
                    {
                        if (purchaseAds == null)
                        {
                            continue;
                        }

                        if (purchaseAds.Count > 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public virtual bool Check(string placement = "common")
        {
            if (!frequence.Check(placement))
            {
                return false;
            }

            if (!timeDelay.Check())
            {
                return false;
            }
            timeDelay.Update();

            return true;
        }


        public virtual bool ShowADS(string placement = "common")
        {
            if (cheatBuild)
            {
                return false;
            }

            if (!CheckAvailable())
            {
                //Debug.LogWarning("!CheckAvailable()");
                return false;
            }

            if (!priceBattleCompleteAds.Check())
            {
                return false;
            }

            if (!Check(placement))
            {
                //Debug.LogWarning("!Check()");
                return false;
            }

            priceBattleCompleteAds.Pay(OnPayAds);          

            Debug.LogWarning("Show Ads all checks!");
            return true;
        }

        private void OnPayAds(bool result)
        {
            if (!showRemoveAdsDialogue)
            {
                return;
            }
                
            if (DateTime.Now.DayOfYear == lastADSShowDate.Value)
            {
                return;
            }
            else
            {
                lastADSShowDate.Value = DateTime.Now.DayOfYear;
            }
            MonoBehaviourBase.AdsManager.Event(Message.RequestPage, removeAdsPage);
        }
    }

    public class AdsBase : StaticBehaviour, IADS
    {
        [SerializeField]
        protected string                iOSId = "";
        [SerializeField]
        protected string                androidId = "";
        [SerializeField]
        protected bool                  debug = false;
        [SerializeField]
        private List<AdsPlacementBase>  placementsList = null;

        private void Reset()
        {
            placementsList = new List<AdsPlacementBase>();
            placementsList.Add(new AdsPlacementBase("interstitial", AdsLogicType.AppLovin, AdType.interstitial));
            placementsList.Add(new AdsPlacementBase("banner", AdsLogicType.AppLovin, AdType.banner));
            placementsList.Add(new AdsPlacementBase("rewarded.extra", AdsLogicType.AppLovin, AdType.rewarded));
            placementsList.Add(new AdsPlacementBase("rewarded.getsoft", AdsLogicType.AppLovin, AdType.rewarded));
            placementsList.Add(new AdsPlacementBase("rewarded.revive", AdsLogicType.AppLovin, AdType.rewarded));
        }

        protected virtual string LogicType => "AppLovin";

        public System.Action<string, bool> OnAdsComplete
        {
            get;
            set;
        }

        private Dictionary<string, AdsPlacementBase> placements = null;

        protected Dictionary<string, AdsPlacementBase> Placements
        {
            get
            {
                if (placements == null)
                {
                    placements = new Dictionary<string, AdsPlacementBase>();

                    foreach (var placement in placementsList)
                    {
                        placements[placement.PlacementId] = placement;
                    }
                }

                return placements;
            }
        }

        public virtual void Init()
        {
            foreach (var placement in placementsList)
            {
                placement.Load();
            }
        }

        public virtual bool AdAvailable(string placementId)
        {
            var placement = GetPlacement(placementId);

            if (placement != null && !placement.Ready)
            {
                SenAnalytics("video_ads_available", placement, placement.Ready ? "success" : "not_available");
            }

            return placement != null && placement.Ready;
        }

        public virtual AdsPlacementBase GetPlacement(string placementId)
        {
            AdsPlacementBase placement = null;
            if (!Placements.TryGetValue(placementId, out placement))
            {
                return null;
            }

            return placement;
        }

        private void SenAnalytics(string eventName, AdsPlacementBase placement, string result)
        {
            if(placement.AdType == AdType.banner)
            {
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary["placement"] = placement.PlacementId;
            dictionary["result"] = result;
            dictionary["ad_type"] = placement.AdType.ToString();
            dictionary["type"] = placement.LogicType.ToString();
            //dictionary["connection"] = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
            dictionary["connection"] = ExtensionsCommon.ControlInternetConnect();

            Analytics.SendEvent(eventName, dictionary);
        }

        public virtual void ShowAds(string placementId, Action<bool> action = null)
        {
            var placement = GetPlacement(placementId);
            //SenAnalytics("video_ads_available", placement,  placement.Ready? "");

            SenAnalytics("video_ads_available", placement, placement.Ready ? "success" : "not_available");

            placement.OnAdsComplete = (string placeId, bool result) =>
            {
                if (OnAdsComplete != null)
                {
                    OnAdsComplete(placeId, result);
                }

                if (action != null)
                {
                    action(result);
                    action = null;
                    AdsManager.Reaction(Message.ResetADSTimer, this, placement.AdType == AdType.rewarded ? 40 : 0f);                 
                    SenAnalytics("video_ads_watch", placement, result ? "watched" : "canceled");
                }
            };            

            if (!Application.isEditor)
            {
                if (placement.Ready)
                {
                    SenAnalytics("video_ads_started", placement, "started");
                }

                placement.Show(gameObject);
            }
        }

        public void StopAds(string placementId)
        {
            if(!placementId.IsNullOrEmpty())
            {
                AdsPlacementBase placemant = GetPlacement(placementId);
                if(placemant != null)
                {
                    placemant.Stop();
                }
            }
        }
    }

    [System.Serializable]
    public class AdsPlacementBase
    {
        public Action<string, bool> OnAdsComplete
        {
            get;
            set;
        }

        public Action OnAdsStarted
        {
            get;
            set;
        }

        [SerializeField]
        private string          placementId = "";
        [SerializeField]
        private string          androidId = "";
        [SerializeField]
        private string          iosId = "";

        [SerializeField]
        private AdType          type = AdType.rewarded;
        [SerializeField]
        private bool            available = true;

        [SerializeField]
        private AdsLogicType    logicType = AdsLogicType.AppLovin;

        private AdsLogicBase    logic = null;

        private IAnalytics      analitycs = null;

        public AdType AdType => type;

        public AdsLogicType LogicType => logicType;

        public bool Available => available;

        public AdsPlacementBase()
        {

        }

        public AdsPlacementBase(string placementId, AdsLogicType logicType, AdType type)
        {
            this.placementId = placementId;
            this.logicType = logicType;
            this.type = type;
        }

        public AdsLogicBase Logic
        {
            get
            {
                if (logic == null)
                {
                    switch (logicType)
                    {
                        case AdsLogicType.AppLovin:
                            switch (type)
                            {
                                case AdType.interstitial:
                                    logic = new AdsLogicAppLovinInterstitial(placementId, Application.platform == RuntimePlatform.Android ? androidId : iosId, HandleShowResult);
                                    break;

                                case AdType.rewarded:
                                    logic = new AdsLogicAppLovinRewarded(placementId, Application.platform == RuntimePlatform.Android ? androidId : iosId, HandleShowResult);
                                    break;

                                case AdType.banner:
                                    logic = new AdsLogicAppLovinBanner(placementId, Application.platform == RuntimePlatform.Android ? androidId : iosId, HandleShowResult);
                                    break;
                            }
                            break;
                    }
                }

                return logic;
            }
        }

        public string PlacementId => placementId;

        public virtual bool Ready => available && Logic.Ready;

        public virtual void Load(params object[] parameters)
        {
            if (analitycs == null)
            {
                analitycs = MonoBehaviourBase.Analytics;
            }

            if (available)
            {
                Logic.Load(parameters);
            }
        }

        public virtual void Show(params object[] parameters)
        {
            if (available)
            {
                Logic.Show(parameters);
            }
        }

        public virtual void Stop()
        {
            if (available)
            {
                Logic.Stop();
            }
        }

        private void HandleShowResult(bool result)
        {
            if (OnAdsComplete != null)
            {
                OnAdsComplete(placementId, result);
            }
        }
    }

    public class AdsLogicId : AdsLogicBase
    {
        protected string        placementId = "";
        protected string        platformId = "";
        protected Action<bool>  callback;

        public AdsLogicId(string placementId, string platformId, Action<bool> callback)
        {
            this.placementId = placementId;
            this.platformId = platformId;
            this.callback = callback;
        }
    }

    public class AdsLogicBase
    {
        public System.Action<string, bool> OnAdsComplete
        {
            get;
            set;
        }

        public virtual bool Ready => false;

        public virtual void Show(params object[] parameters)
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void Load(params object[] parameters)
        {

        }
    }

    public enum AdsLogicType
    {
        None        = 0,
        AppLovin    = 1,
    }
}