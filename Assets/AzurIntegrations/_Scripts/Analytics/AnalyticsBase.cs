using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class AnalyticsBase : StaticBehaviour, IAnalytics
    {
        public override StaticType StaticType => StaticType.Analytics;

        public virtual bool Validate(IInAppItem item, System.Action<IInAppItem, bool> callback)
        {
            return true;
        }

        public virtual void SendPurchase(IInAppItem item)
        {
        }

        public virtual void SendEvent(string eventName, Dictionary<string, object> data)
        {
        }

        public virtual void SendADS(string eventName, Dictionary<string, object> data)
        {
        }

        public virtual void SendBuffer()
        {

        }

        public virtual void RemoveUserData()
        {

        }

        public virtual Dictionary<string, string> GetDataForRemove()
        {
            return new Dictionary<string, string>();
        }
    }

    public interface IAnalytics : IStatic
    {
        bool Validate(IInAppItem item, System.Action<IInAppItem, bool> callback);
        void SendPurchase(IInAppItem item);
        void SendEvent(string eventName, Dictionary<string, object> data);
        void SendADS(string eventName, Dictionary<string, object> data);

        void SendBuffer();
        void RemoveUserData();

        Dictionary<string, string> GetDataForRemove();
    }
}