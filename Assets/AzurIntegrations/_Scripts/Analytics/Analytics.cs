using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class RemoveDataId
    {
        public string Type
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>() { { "type", Type }, { "value", Value } };
        }
    }


    public class Analytics : AnalyticsBase
    {
        [SerializeField]
        private List<StaticType>    purchasesAnalitycs = new List<StaticType>() { StaticType.AnalyticsAppsFlyer };
        [SerializeField]
        private List<StaticType>    eventsAnalitycs = new List<StaticType>() { StaticType.AnalyticsAppMetrica };
        [SerializeField]
        private List<StaticType>    purchasesValidators = new List<StaticType>() { StaticType.AnalyticsAppsFlyer };
        [SerializeField]
        private List<StaticType>    removeDataAnalytics = new List<StaticType>() { StaticType.AnalyticsFirebase };
        [SerializeField]
        private bool                debug = false;
        [SerializeField]
        private string              appId = "IOS_APP_ID";
        [SerializeField]
        private string              urlRemoveData = "https://r5pxow34k2.execute-api.us-east-1.amazonaws.com/prod/streams/GDPR/record";

        private List<IAnalytics>    purchasesAnalyticsInstances = new List<IAnalytics>();
        private List<IAnalytics>    eventsAnalyticsInstances = new List<IAnalytics>();
        private List<IAnalytics>    purchasesValidatorsInstances = new List<IAnalytics>();
        private List<IAnalytics>    removeDataAnalyticsInstances = new List<IAnalytics>();

        private Dictionary<StaticType, IAnalytics> dicts = new Dictionary<StaticType, IAnalytics>();

        public override void SendBuffer()
        {
            base.SendBuffer();
            foreach (var analytics in eventsAnalyticsInstances)
            {
                if (analytics == null)
                {
                    Debug.LogErrorFormat("Analytics {0} is null", analytics);
                    continue;
                }

                analytics.SendBuffer();
            }
        }

        private IAnalytics GetAnalytics(StaticType staticType)
        {
            IAnalytics res = null;
            if (!dicts.TryGetValue(staticType, out res))
            {
                res = staticType.Instance<IAnalytics>();
                if (res == null)
                {
                    Debug.LogErrorFormat("StaticType: {0} is NULL!", staticType);
                }
                dicts[staticType] = res;
            }

            return res;
        }

        private void Start()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer)
            {
                foreach (var staticType in purchasesAnalitycs)
                {
                    purchasesAnalyticsInstances.Add(GetAnalytics(staticType));
                }

                foreach (var staticType in eventsAnalitycs)
                {
                    eventsAnalyticsInstances.Add(GetAnalytics(staticType));
                }

                foreach (var staticType in purchasesValidators)
                {
                    purchasesValidatorsInstances.Add(GetAnalytics(staticType));
                }

                foreach (var staticType in removeDataAnalytics)
                {
                    removeDataAnalyticsInstances.Add(GetAnalytics(staticType));
                }
            }
        }

        public override bool Validate(IInAppItem item, System.Action<IInAppItem, bool> callback)
        {
            if (purchasesValidatorsInstances.Count == 0)
            {
                return true;
            }

            foreach (var analytics in purchasesValidatorsInstances)
            {
                if (!analytics.Validate(item, callback))
                {
                    return false;
                }
            }

            return true;
        }

        public override void SendPurchase(IInAppItem item)
        {
            base.SendPurchase(item);

            foreach (var analytics in purchasesAnalyticsInstances)
            {
                analytics.SendPurchase(item);
            }
            if (debug)
            {
                Debug.LogWarningFormat("Send Purchase: {0}", item.ID);
            }
        }

        public override void SendEvent(string eventName, Dictionary<string, object> data)
        {
            var global = new Dictionary<string, object>();
            //var global = GameManager.GetGlobalParameters();
            foreach (var pair in global)
            {
                data[pair.Key] = pair.Value;
            }

            base.SendEvent(eventName, data);

            foreach (var analytics in eventsAnalyticsInstances)
            {
                if (analytics == null)
                {
                    Debug.LogErrorFormat("Analytics {0} is null", analytics);
                    continue;
                }
                analytics.SendEvent(eventName, data);
            }

            if (debug)
            {
                Debug.LogWarningFormat("Sent Event: {0}; Data: {1}", eventName, data.ToJSON());
            }
        }

        public override void SendADS(string eventName, Dictionary<string, object> data)
        {
            base.SendADS(eventName, data);

            foreach (var analytics in eventsAnalyticsInstances)
            {
                analytics.SendADS(eventName, data);
            }
        }

        /// <summary>
        /// Central solution
        /// </summary>
        public override void RemoveUserData()
        {
            var data = new Dictionary<string, object>();
            data["event_datetime"] = ExtensionsCommon.CurrentTime().ToString();
            data["bundle_id"] = Application.identifier;
            data["platform"] = "ios";

            List<Dictionary<string, string>> ids = new List<Dictionary<string, string>>();


#if UNITY_IOS
            ids.Add(new Dictionary<string, object>() { { "type", "IDFA" }, { "value", UnityEngine.iOS.Device.advertisingIdentifier } });
            ids.Add(new Dictionary<string, object>() { { "type", "IDFV" }, { "value", UnityEngine.iOS.Device.vendorIdentifier } });
#else
            ids.Add(new Dictionary<string, string>() { { "type", "IDFA" }, { "value", "TEST IDFA" } });
            ids.Add(new Dictionary<string, string>() { { "type", "IDFV" }, { "value", "TEST IDFV" } });
#endif
            ids.Add(new Dictionary<string, string>() { { "type", "app_id" }, { "value", appId } });


            foreach (var analytics in removeDataAnalyticsInstances)
            {
                ids.Add(analytics.GetDataForRemove());
            }

            data["ids"] = ids;

            //data["firebase_id"] = "";
            StartCoroutine(Upload(data));
        }

        private IEnumerator Upload(Dictionary<string, object> data)
        {
            var payload = new Dictionary<string, object>();
            payload["Data"] = data;
            payload["PartitionKey"] = "gdpr";
            string payloadStr = payload.ToJSON();

            var www = UnityEngine.Networking.UnityWebRequest.Put(urlRemoveData, payloadStr);

            www.SetRequestHeader("Authorization", "Bearer 2b961016-6776-43e7-a0c8-1b1d8f5ccc5d");
            www.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log("Try to remove! " + payloadStr);
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!" + www.result.ToString());
            }
        }
    }
}