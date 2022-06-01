using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public enum RequestADSType
    {
        Interstitial    = 0,
        Banner          = 1,
    }    


    [System.Serializable]
    public class InterstitialCheckerBase : IInterstitialChecker
    {
        public virtual bool Check()
        {
            return true;
        }
    }

    public class AdsContainer : StaticBehaviour, IADS
    {
        [SerializeField]
        private AdsMediationSettings        advertisements = new AdsMediationSettings();
        [SerializeField]
        private bool                        adsEditorAvailable = true;
        [SerializeField]
        private InterstitialAdsHelper       interstitialHelper = new InterstitialAdsHelper();
        [SerializeField]
        private InterstitialCheckerBase     interstitialChecker = null;
        [SerializeField]
        private bool                        cheatBuild = false;

        private IInterstitialChecker InterstitialChecker => interstitialChecker;

        public override StaticType StaticType => StaticType.AdsManager;        

        public override void Reaction(Message message, params object[] parameters)
        {
            base.Reaction(message, parameters);

            advertisements.Instance.Reaction(message, parameters);

            switch (message)
            {
                case Message.ProfileLocalLoaded:                    
                    Init();
                    break;

                case Message.RequestADS:
                    if (!InterstitialChecker.Check())
                    {
                        return;
                    }

                    string placement = parameters.Get<string>();
                    switch (parameters.Get<RequestADSType>())
                    {
                        case RequestADSType.Interstitial:
                            Debug.LogWarningFormat("RequestADS! {0}", placement);
                            interstitialHelper.ShowADS(placement);
                            break;

                        case RequestADSType.Banner:
                            if (parameters.Get<bool>())
                            {
                                if (interstitialHelper.CheckAvailable() && interstitialHelper.BannerEnabled)
                                {
                                    if (AdsManager.AdAvailable(placement))
                                    {
                                        AdsManager.ShowAds(placement);
                                    }
                                }
                                else
                                {
                                    AdsManager.StopAds(placement);
                                }
                            }
                            else
                            {
                                AdsManager.StopAds(placement);
                            }
                            break;
                    }
                    break;

                case Message.ResetADSTimer:
                    interstitialHelper.UpdateTimer(parameters.Get<float>());
                    break;
            }
        }

        private void Start()
        {
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void Init()
        {
            advertisements.Init();
            interstitialHelper.UpdateTimer(30);
        }

        public void ShowAds(string placementId, System.Action<bool> action = null)
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || cheatBuild)
            {
                if (action != null)
                {
                    action(adsEditorAvailable);
                }
                return;
            }           

            advertisements.ShowAds(placementId, action);            
        }

        public bool AdAvailable(string placementId)
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || cheatBuild)
            {
                return adsEditorAvailable;
            }         
            return advertisements.AdAvailable(placementId);
        }

        public void StopAds(string placementId)
        {
            advertisements.StopAds(placementId);
        }
    }

    [System.Serializable]
    public class AdsMediationSettings
    {
        [SerializeField]
        private string      name = "AdsAppLovin";
        [SerializeField]
        private StaticType  staticType = StaticType.AdsAppLovin;
        [SerializeField]
        private long        count = 1;

        private IADS        instance = null;

        public IADS Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = staticType.Instance<IADS>();
                }
                return instance;
            }
        }

        public System.Action OnChangeOrder
        {
            get;
            set;
        }


        public long Current
        {
            get;
            set;
        }

        public long Max
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
            }
        }

        public void StopAds(string placementId)
        {
            Instance.StopAds(placementId);
        }

        public void Init()
        {
            Instance.Init();
        }

        public void ShowAds(string placementId, System.Action<bool> action = null)
        {
            Instance.ShowAds(placementId, action);
        }

        public bool AdAvailable(string placementId)
        {
            return Instance.AdAvailable(placementId);
        }

        public AdsMediationSettings()
        {

        }
    }
}