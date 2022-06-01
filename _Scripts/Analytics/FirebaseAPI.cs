using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class FirebaseAPI : AnalyticsBase
    {
        public override StaticType      StaticType => StaticType.AnalyticsFirebase;

#if FIREBASE_INT
        private Firebase.FirebaseApp    app;
#endif

        private string                  token;

        private void Start()
        {
#if FIREBASE_INT
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;                
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }

                Firebase.Analytics.FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWith(task =>
                {
                    token = task.Result;
                });
            });
#endif
        }

        public override void SendEvent(string eventName, Dictionary<string, object> data)
        {
            base.SendEvent(eventName, data);

#if FIREBASE_INT
            List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();
            foreach(var pair in data)
            {
                Firebase.Analytics.Parameter parameter;
                if (pair.Value.GetType() == typeof(long))
                {
                    parameter = new Firebase.Analytics.Parameter(pair.Key, (long)pair.Value);
                }
                else if (pair.Value.GetType() == typeof(double))
                {
                    parameter = new Firebase.Analytics.Parameter(pair.Key, (double)pair.Value);
                }
                else
                {
                    parameter = new Firebase.Analytics.Parameter(pair.Key, pair.Value.ToString());
                }
                parameters.Add(parameter);
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
#endif
        }

        public override void SendPurchase(IInAppItem item)
        {
            base.SendPurchase(item);
        }

        public override void SendADS(string eventName, Dictionary<string, object> data)
        {
            base.SendADS(eventName, data);


#if FIREBASE_INT
            List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();
            parameters.Add(new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterTransactionId, ""));
            parameters.Add(new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterAffiliation, data.Extract<string>("network")));
            parameters.Add(new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"));
            parameters.Add(new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, data.Extract<double>("value")));

            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventPurchase, parameters.ToArray());
#endif
        }

        public override Dictionary<string, string> GetDataForRemove()
        {
            var result = base.GetDataForRemove();
#if FIREBASE_INT
            if (!token.IsNullOrEmpty())
            {
                result["firebase_id"] = token;
            }
#endif
            return result;
        }
    }
}