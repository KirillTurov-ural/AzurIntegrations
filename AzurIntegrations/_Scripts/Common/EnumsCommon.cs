using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public enum Message
    {
        None                = 0,        
        PrivatePolicyAgreed = 1,
        RequestPage         = 2,
        ResetADSTimer       = 3,
        RequestADS          = 4,
        ProfileLocalLoaded  = 5,
        StaticTypeInited    = 6,
        ShowDebugger        = 7,
    }

    public enum StaticType
    {
        None                = 0,

        UI                  = 1,
        Coroutines          = 2,

        Analytics           = 1001,
        AnalyticsAppsFlyer  = 1002,
        AnalyticsAppMetrica = 1003,
        AnalyticsFirebase   = 1004,

        AdsManager          = 2001,
        AdsAppLovin         = 2002,
        AnalyticsFacebook   = 2003,
    }

    public enum AdType
    {
        rewarded        = 0,
        interstitial    = 1,
        banner          = 2,
    }
}