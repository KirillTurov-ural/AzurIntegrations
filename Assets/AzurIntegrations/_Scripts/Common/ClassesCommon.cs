using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if NEWTONSOFT_JSON_INT
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
#endif

namespace BoGD
{
    public class DataPurchases
    {
        private List<DataPurchase> purchases = null;

        public bool AnyPurchased
        {
            get
            {
                foreach (var purchase in purchases)
                {
                    if (purchase.Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public class DataPurchase
    {
        public long Count
        {
            get;
            set;
        }
    }

    [System.Serializable]
    public class DataInt
    {
        [SerializeField]
        private string  key = "";
        [SerializeField]
        private int     defaultValue = 0;

        public int Value
        {
            get => PlayerPrefs.GetInt(key, defaultValue);
            set => PlayerPrefs.SetInt(key, value);
        }


        public void Increment(int delta)
        {
            Value += delta;
        }

        public DataInt()
        {

        }

        public DataInt(string key)
        {
            this.key = key;
        }
    }

    [System.Serializable]
    public class DataFloat
    {
        [SerializeField]
        private string      key = "";
        [SerializeField]
        private float       defaultValue = 0;

        public string Key
        {
            get => key;
            set => key = value;
        }

        public float Value
        {
            get => PlayerPrefs.GetFloat(key, defaultValue);
            set => PlayerPrefs.SetFloat(key, value);
        }


        public void Increment(int delta)
        {
            Value += delta;
        }

        public DataFloat()
        {

        }

        public DataFloat(string key)
        {
            this.key = key;
        }
    }

    [System.Serializable]
    public class RequirementTimeResource
    {
        [SerializeField]
        private DataFloat   timerData = null;
        [SerializeField]
        private float       delay;

        public float Delay
        {
            get => delay;
            set => delay = value;
        }

        public RequirementTimeResource(string key, float delay)
        {
            timerData = new DataFloat(key);
            this.delay = delay;
        }

        public bool Check()
        {
            return ExtensionsCommon.CurrentTime() + delay > timerData.Value;
        }

        public void Increment(double delta)
        {
            timerData.Value = (float)(ExtensionsCommon.CurrentTime() + delta);
        }

        public void Update()
        {
            Increment(0);
        }
    }

    public class DataString
    {
        [SerializeField]
        private string      key = "";
        [SerializeField]
        private string      defaultValue;

        public string Key
        {
            get => key;
            set => key = value;
        }

        public string Value
        {
            get => PlayerPrefs.GetString(key, defaultValue);
            set => PlayerPrefs.SetString(key, value);
        }


        public void Increment(object delta)
        {
            Value += delta;
        }

        public DataString()
        {

        }

        public DataString(string key)
        {
            this.key = key;
        }
    }

    [System.Serializable]
    public class ReferencePriceAds 
    {
        [SerializeField]
        private string placementId;

        public bool Check()
        {
            return MonoBehaviourBase.AdsManager.AdAvailable(placementId);
        }

        public void Do(System.Action<bool> callback = null)
        {
            MonoBehaviourBase.AdsManager.ShowAds(placementId, callback);
        }

        public void Pay(System.Action<bool> callback = null)
        {
            Do(callback);
        }

        public ReferencePriceAds()
        {

        }

        public ReferencePriceAds(string placementId)
        {
            this.placementId = placementId;
        }
    }

    public static class JsonHelper
    {
        public static string ToJSON<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            if (dictionary == null)
            {
                return "";
            }

#if NEWTONSOFT_JSON_INT
            return JsonConvert.SerializeObject(dictionary, Formatting.Indented);
#endif
            return "";
        }

        public static string ToJSON(this Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                return "";
            }

#if NEWTONSOFT_JSON_INT
            return JsonConvert.SerializeObject(dictionary, Formatting.Indented);
#endif
            return "";
        }

        public static Dictionary<string, object> GetByKeys(this Dictionary<string, object> dictionary, params object[] keys)
        {
            Dictionary<string, object> result = dictionary;
            string key = "";
            for (int i = 0; i < keys.Length; i++)
            {
                key += keys[i] + " -> ";
                object tmp = null;
                if (!result.TryGetValue(keys[i].ToString(), out tmp))
                {
                    Debug.LogErrorFormat("Key not found: '{0}'", key);
                    return result;
                }

                result = (Dictionary<string, object>)tmp;
            }
            return result;
        }

        public static Dictionary<string, object> FromJSON(this string json)
        {
            return (Dictionary<string, object>)json.Deserealize();
        }

        public static object Deserealize(this string json)
        {
#if NEWTONSOFT_JSON_INT
            return ToObject(JToken.Parse(json));
#endif
            return null;
        }

#if NEWTONSOFT_JSON_INT
        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }
#endif

        public static string GetString(this Dictionary<string, object> dictionary)
        {
            string result = "";
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                string value = "";
                if (pair.Value == null)
                {
                    value = "NULL";
                }
                else
                {
                    if (pair.Value.GetType() == typeof(Dictionary<string, object>))
                    {
                        value = ((Dictionary<string, object>)pair.Value).GetString();
                    }
                    else
                    {
                        value = pair.Value.ToString();
                    }
                }
                result += pair.Key + ": " + value + "; ";
            }
            return result;
        }
    }

    // Declaration of the Receipt structure for getting information about the IAP.
    [System.Serializable]
    public struct Receipt
    {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    // Additional information about the IAP for Android.
    [System.Serializable]
    public struct PayloadAndroid
    {
        public string Json;
        public string Signature;
    }
}