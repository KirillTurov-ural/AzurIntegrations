using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoGD
{
    public class WindowSample : MonoBehaviourBase
    {
        [SerializeField]
        private Button                          buttonBanner = null;
        [SerializeField]
        private Text                            textBannerState = null;
        [SerializeField]
        private Button                          buttonInterstitial = null;
        [SerializeField]
        private Button                          buttonRewarded = null;
        [SerializeField]
        private Button                          buttonShowMediationDebugger = null;

        [SerializeField]
        private Button                          buttonSendStart = null;
        [SerializeField]
        private Button                          buttonSendFinish = null;
        [SerializeField]
        private Button                          buttonRemoveUserData = null;

        [SerializeField]
        protected DataInt                       battlesAnalytics = new DataInt("longs.battles");

        protected Dictionary<string, object>    analyticsFixedData = new Dictionary<string, object>();
        protected float                         timeStartLevel = 0;
        private bool                            bannerEnabled = false;

        [SerializeField]
        private ReferencePriceAds               rewardedSample = null;        
        [SerializeField]
        private int                             rewardedBonus = 5;
        [SerializeField]
        private DataInt                         soft = new DataInt("resources.soft");
        [SerializeField]
        private Text                            textMoneySample = null;

        private void Start()
        {
            buttonBanner.onClick.AddListener(SwitchBanner);
            buttonInterstitial.onClick.AddListener(ShowInter);
            buttonRewarded.onClick.AddListener(ShowRewarded);
            buttonSendStart.onClick.AddListener(SendStart);
            buttonSendFinish.onClick.AddListener(SendFinish);
            buttonShowMediationDebugger.onClick.AddListener(ShowDebugger);
            buttonRemoveUserData.onClick.AddListener(RemoveUserData);

            buttonSendFinish.interactable = false;

            //AFTER LOADING YOUR PROFILE DATA 
            AdsManager.Reaction(Message.ProfileLocalLoaded);
            textMoneySample.text = soft.Value.ToString();
        }

        private void RemoveUserData()
        {
            Analytics.RemoveUserData();
        }

        /// <summary>
        /// Sample Code for Mediation Debugger
        /// </summary>
        private void ShowDebugger()
        {
            AdsManager.Reaction(Message.ShowDebugger);
        }

        /// <summary>
        /// Sample Code for Rewarded, Callback - after close rewarded ads
        /// </summary>
        private void ShowRewarded()
        {
            rewardedSample.Pay(AfterAdsShow);
        }

        /// <summary>
        /// Rewarded callback sample
        /// </summary>
        /// <param name="result"></param>
        private void AfterAdsShow(bool result)
        {
            if (result)
            {
                Debug.LogError("GRANT REWARD!!!");
                soft.Increment(rewardedBonus);
                textMoneySample.text = soft.Value.ToString();
            }
        }

        /// <summary>
        /// Show interstitial sample
        /// </summary>
        private void ShowInter()
        {
            AdsManager.Reaction(Message.RequestADS, RequestADSType.Interstitial, "common");
        }

        /// <summary>
        /// Show/Hide banner sample
        /// </summary>
        private void SwitchBanner()
        {
            bannerEnabled = !bannerEnabled;
            AdsManager.Reaction(Message.RequestADS, RequestADSType.Banner, "banner", bannerEnabled);
            textBannerState.text = "BANNER (" + (bannerEnabled ? "ON" : "OFF") + ")";
        }

        /// <summary>
        /// Send level_start event sample
        /// </summary>
        private void SendStart()
        {
            var data = GetParametersForSendStart("level_winter");
            Analytics.SendEvent("level_start", data);
            Analytics.SendBuffer();

            buttonSendFinish.interactable = true;
            buttonSendStart.interactable = false;            
        }


        /// <summary>
        /// Send level_finish event sample
        /// </summary>
        private void SendFinish()
        {
            var data = GetParametersForSendFinish();
            Analytics.SendEvent("level_finish", data);
            Analytics.SendBuffer();

            buttonSendFinish.interactable = false;
            buttonSendStart.interactable = true;
        }

        /// <summary>
        /// Sample parameters for level_start
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetParametersForSendStart(params object[] parameters)
        {
            battlesAnalytics.Increment(1);
            analyticsFixedData.Clear();
            analyticsFixedData["level_number"] = 1;//number of level
            analyticsFixedData["level_name"] = parameters.Get<string>();//name of level
            analyticsFixedData["level_loop"] = 1;//number of loop if nesessary

            analyticsFixedData["level_count"] = battlesAnalytics.Value;//number of game
            analyticsFixedData["level_diff"] = "normal";//difficult
            analyticsFixedData["level_random"] = 0;//random or not?
            analyticsFixedData["level_type"] = "normal";//type of level
            analyticsFixedData["game_mode"] = "classic";//game mode
            timeStartLevel = Time.time;//start time
            return analyticsFixedData;
        }

        /// <summary>
        /// Sample parameters for level_finish
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetParametersForSendFinish(params object[] parameters)
        {
            var result = parameters.Get<string>();
            var analyticsData = new Dictionary<string, object>();

            foreach (var data in analyticsFixedData)
            {
                analyticsData[data.Key] = data.Value;
            }

            var progress = Random.Range(0, 101);

            analyticsData["progress"] = progress;//level progress
            analyticsData["result"] = progress == 100? "win" : "lose";//result - win, lose, leave
            analyticsData["time"] = (long)(Time.time - timeStartLevel);//duration game
            analyticsData["continue"] = 1;//ads continues

            return analyticsData;
        }
    }
}