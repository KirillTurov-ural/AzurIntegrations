using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class MonoBehaviourBase : MonoBehaviour
    {
        private static IADS adsManager = null;
        public static IADS AdsManager
        {
            get
            {
                if (adsManager == null)
                {
                    adsManager = StaticType.AdsManager.Instance<IADS>();
                }
                return adsManager;
            }
        }

        private static IAnalytics analytics = null;
        public static IAnalytics Analytics
        {
            get
            {
                if (analytics == null)
                {
                    analytics = StaticType.Analytics.Instance<IAnalytics>();
                }

                return analytics;
            }
        }


        private static ICoroutines coroutines = null;
        public static ICoroutines Coroutines
        {
            get
            {
                if (coroutines == null)
                {
                    coroutines = StaticType.Coroutines.Instance<ICoroutines>();
                }
                return coroutines;
            }
        }

        private static IAnalytics appsFlyerAPI = null;
        public static IAnalytics AppsFlyerAPI
        {
            get
            {
                if (appsFlyerAPI == null)
                {
                    appsFlyerAPI = StaticType.AnalyticsAppsFlyer.Instance<IAnalytics>();
                }
                return appsFlyerAPI;
            }
        }

        public static string GetUniqueID()
        {            
            return System.Guid.NewGuid().ToString();
        }
    }
}