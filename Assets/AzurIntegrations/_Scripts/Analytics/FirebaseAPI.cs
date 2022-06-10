using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class FirebaseAPI : AnalyticsBase
    {
        [SerializeField]
        private DataString         tokenData = new DataString("strings.firebase.pushtoken");
        public override StaticType StaticType => StaticType.AnalyticsFirebase;

#if FIREBASE_INT
        private Firebase.FirebaseApp    app;
#endif
        private string                  token;

        [SerializeField]
        private bool                    sendMessagingToken = true;

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


#if FIREBASE_MESSAGING_INT
                    if (sendMessagingToken)
                    {
                        StartCoroutine(WaitForAppsflyerInit());
                        StartCoroutine(WaitForToken());
                    }
#endif
                    //Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    //Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
#endif


        }

        private IEnumerator WaitForAppsflyerInit()
        {
            while (!AppsFlyerAPI.Inited)
            {
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log("TRY TO GET FIREBASE TOKEN");
#if FIREBASE_MESSAGING_INT
            Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWith(task =>
            {
                token = task.Result;
            });
#endif
        }

        private IEnumerator WaitForToken()
        {
            while (token.IsNullOrEmpty())
            {
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log("GOT FIREBASE TOKEN: " + token);
            if (token != tokenData.Value)
            {
                //Debug.Log("TRY TO SEND FIREBASE TOKEN: " + token);
                AppsFlyerAPI.SendEvent("push_token_received", new Dictionary<string, object>() { { "token", token } });
                Debug.Log("SENT FIREBASE TOKEN: " + token);
                tokenData.Value = token;
            }
        }

        //#if FIREBASE_MESSAGING_INT
        //        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        //        {
        //            Debug.Log("GOT FIREBASE TOKEN: " + token.Token);
        //            if (token.Token != tokenData.GetData<DataString>().Value)
        //            {
        //                AppsFlyerAPI.SendEvent("push_token_received", new Dictionary<string, object>() { { "token", token.Token } });
        //                Debug.Log("SENT FIREBASE TOKEN: " + token.Token);
        //                tokenData.GetData<DataString>().Set(token.Token);
        //            }
        //        }
        //
        //        public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        //        {
        //        }
        //#endif

        public override void SendEvent(string eventName, Dictionary<string, object> data)
        {
            base.SendEvent(eventName, data);

#if FIREBASE_INT
            List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();
            foreach (var pair in data)
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

            foreach (var pair in data)
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

            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventPurchase, parameters.ToArray());
#endif
        }
    }
}