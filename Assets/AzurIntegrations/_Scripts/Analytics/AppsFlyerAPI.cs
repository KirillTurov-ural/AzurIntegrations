using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if APPSFLYER_INT
using AppsFlyerSDK;
#endif
namespace BoGD
{
    /// <summary>
    /// Appsflyer singleton
    /// </summary>
    public class AppsFlyerAPI : AnalyticsBase
    {
        [SerializeField]
        private string devKey = "r9vNC83N8nYpCzYGigyjUh";
        [SerializeField]
        private string iOSAppId = "";
        [SerializeField]
        private bool isDebug = false;

        private bool inited = false;

        public override StaticType StaticType
        {
            get
            {
                return StaticType.AnalyticsAppsFlyer;
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void Start()
        {
            StartCoroutine(Initialization());
        }

        private IEnumerator Initialization()
        {
#if APPSFLYER_INT
            AppsFlyer.setIsDebug(isDebug);
            //Debug.LogError("APPSFLYER START INIT");
#endif
            yield return new WaitForSeconds(0.5f);
#if APPSFLYER_INT
            AppsFlyer.initSDK(devKey, iOSAppId);
#endif

            //Debug.LogError("APPSFLYER END INIT");
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.waitForATTUserAuthorizationWithTimeoutInterval(30);
#endif

#if APPSFLYER_INT
            //Debug.LogError("APPSFLYER TIMEOUT SET");
            AppsFlyer.startSDK();
            //Debug.LogError("APPSFLYER START SDK");
            inited = true;
#endif
        }

        public void FinishValidateReceipt(string validateResult)
        {
            Debug.Log(name + ":: got didFinishValidateReceipt  = " + validateResult);
        }

        public void FinishValidateReceiptWithError(string error)
        {
            Debug.Log(name + ":: got idFinishValidateReceiptWithError error = " + error);
        }

        public override bool Validate(IInAppItem item, System.Action<IInAppItem, bool> callback)
        {
            //AppsFlyer.validateReceipt(item.ID, item.LocalizedPrice.ToString(), item.ISO, item.TransactionID, null);            
            return true;
        }

        public override void SendPurchase(IInAppItem item)
        {
            base.SendPurchase(item);

#if APPSFLYER_INT
            if (!inited)
            {
                return;
            }
            Dictionary<string, string> data = new Dictionary<string, string> ();
            data.Add(AFInAppEvents.CURRENCY, item.ISO);
            data.Add(AFInAppEvents.REVENUE, item.LocalizedPrice.ToString());
            data.Add(AFInAppEvents.QUANTITY, "1");
            data.Add(AFInAppEvents.CONTENT_ID, item.ID);
            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, data);
#endif
        }

        public override void SendADS(string eventName, Dictionary<string, object> data)
        {
#if APPSFLYER_INT
            if (!inited)
            {
                return;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach(var item in data)
            {
                dictionary[item.Key] = item.Value.ToString();
            }

            AppsFlyer.sendEvent(eventName, dictionary);
#endif
        }
    }

}
