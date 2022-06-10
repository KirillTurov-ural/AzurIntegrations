using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    /// <summary>
    /// Appmetrica singleton
    /// </summary>
    public class AppMetricaAPI : AnalyticsBase
    {
        public override StaticType StaticType => StaticType.AnalyticsAppMetrica;

        public override void SendEvent(string eventName, Dictionary<string, object> data)
        {
            base.SendEvent(eventName, data);
#if APPMETRICA_INT
            AppMetrica.Instance.ReportEvent(eventName, data);
#endif
        }

        public override void SendBuffer()
        {
            base.SendBuffer();

#if APPMETRICA_INT
            AppMetrica.Instance.SendEventsBuffer();
#endif
        }

        public override void SendPurchase(IInAppItem item)
        {
            base.SendPurchase(item);

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary["inapp_id"] = item.ID;
            dictionary["currency"] = item.ISO;
            dictionary["price"] = (float)item.LocalizedPrice;
            dictionary["transaction_id"] = item.TransactionID;
            dictionary["inapp_type"] = item.Type;
            SendEvent("payment_succeed", dictionary);
            Debug.Log("SendPurchase to AppMetrica: " + item.ID);
        }

        public override bool Validate(IInAppItem item, System.Action<IInAppItem, bool> callback)
        {
#if APPMETRICA_INT
            string currency = item.ISO;
            var price = item.LocalizedPrice;

            // Creating the instance of the YandexAppMetricaRevenue class.
            YandexAppMetricaRevenue revenue = new YandexAppMetricaRevenue(price, currency);
            if (item.Receipt != null)
            {
                // Creating the instance of the YandexAppMetricaReceipt class.
                YandexAppMetricaReceipt yaReceipt = new YandexAppMetricaReceipt();
                Receipt receipt = JsonUtility.FromJson<Receipt>(item.Receipt);
#if UNITY_ANDROID
                PayloadAndroid payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(receipt.Payload);
                yaReceipt.Signature = payloadAndroid.Signature;
                yaReceipt.Data = payloadAndroid.Json;
#elif UNITY_IPHONE
                    yaReceipt.TransactionID = receipt.TransactionID;
                    yaReceipt.Data = receipt.Payload;
#endif
                revenue.Receipt = yaReceipt;

                AppMetrica.Instance.ReportRevenue(revenue);
            }
#endif
            return true;
        }
    }
}