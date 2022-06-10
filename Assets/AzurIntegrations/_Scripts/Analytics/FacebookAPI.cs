using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if FACEBOOK_INT
using Facebook;
using Facebook.Unity;
#endif

#if FACEBOOK_INT && UNITY_IOS
namespace AudienceNetwork
{
    public static class AdSettings
    {
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
            Debug.LogWarningFormat("Set ATE as {0}", advertiserTrackingEnabled);
        }
    }
}
#endif

namespace BoGD
{
    /// <summary>
    /// Facebook singleton
    /// </summary>
    public class FacebookAPI : AnalyticsBase
    {
        public override StaticType StaticType => StaticType.AnalyticsFacebook;
        private bool    tryToInit =false;

        protected override void Awake()
        {
            base.Awake();
            Init();            
        }

        public void Init()
        {
#if FACEBOOK_INT            
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                if (!FB.IsInitialized)
                {
                    tryToInit = true;
                    //Handle FB.Init
                    FB.Init(() =>
                    {
                        FB.ActivateApp();
                        tryToInit = false;
#if UNITY_IOS
                        AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
#endif
                    });
                }
            }
#endif
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                Init();
            }
        }

        public override void SendEvent(string eventName, Dictionary<string, object> data)
        {
#if FACEBOOK_INT
            Debug.LogFormat("{0} try to send event: {1}", name, eventName);
            if (FB.IsInitialized)
            {
                FB.LogAppEvent(eventName, null, data);
            }
            else
            {
                Init();
            }
#endif
        }

        public override void SendPurchase(IInAppItem item)
        {
#if FACEBOOK_INT
            if (FB.IsInitialized)
            {
                FB.LogPurchase((float)item.LocalizedPrice, item.ISO);
            }
            else
            {
                Init();
            }
#endif
        }
    }
}
