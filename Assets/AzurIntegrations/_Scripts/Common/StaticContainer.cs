using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class StaticContainer : ISender
    {
        private static StaticContainer      instance = null;
        public static StaticContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StaticContainer();
                }

                return instance;
            }
        }

        #region ISender
        private List<ISubscriber>       subscribers = new List<ISubscriber>();

        public List<ISubscriber> Subscribers => subscribers;

        public void AddSubscriber(ISubscriber subscriber)
        {
            if (subscriber == null)
            {
                Debug.LogErrorFormat("Subscriber is null! {0}", Description);
                return;
            }

            if (Subscribers.Contains(subscriber))
            {
                return;
            }

            Subscribers.Add(subscriber);
        }

        public void RemoveSubscriber(ISubscriber subscriber)
        {
            Subscribers.Remove(subscriber);
        }

        public void Event(Message message, params object[] parameters)
        {
            for (int i = 0; i < Subscribers.Count; ++i)
            {
                Subscribers[i].Reaction(message, parameters);
            }
        }

        public virtual string Description
        {
            get;
            set;

        }
        #endregion


        private Dictionary<int, IStatic> staticContainer = new Dictionary<int, IStatic>();

        public static IStatic Get(StaticType staticType)
        {
            IStatic result = null;
            int type = (int)staticType;
            if (!Instance.staticContainer.TryGetValue(type, out result))
            {
                return null;
            }
            return result;
        }

        private static Dictionary<StaticType, int> counts = new Dictionary<StaticType, int>();

        public static T Get<T>(StaticType staticType) where T : IStatic
        {
            T result = default(T);
            try
            {
                result = (T)Get(staticType);
            }
            catch
            {
                Debug.Log("Static with type " + staticType + " not initialized! Fix IT!", null);
                return result;
            }

            if (!counts.ContainsKey(staticType))
            {
                counts.Add(staticType, 0);
            }
            counts[staticType]++;
            if (counts[staticType] > 1)
            {
                Debug.LogWarning("Too more calls: " + staticType + "=>" + counts[staticType]);
            }
            else if (counts[staticType] == 1)
            {
                //Debug.LogWarning("First call: " + staticType + "=>" + counts[staticType]);
            }

            return result;
        }

        public static void Set(StaticType staticType, IStatic staticObject)
        {
            int type = (int)staticType;

            IStatic staticInContainer = null;
            bool contains = Instance.staticContainer.TryGetValue(type, out staticInContainer);
            if (contains)
            {
                staticInContainer = staticObject;
            }

            if (staticObject == null)
            {
                if (contains)
                {
                    Instance.staticContainer.Remove(type);
                }
            }
            else
            {
                if (contains)
                {
                    staticInContainer = staticObject;
                    Instance.staticContainer[type] = staticObject;
                }
                else
                {
                    Instance.staticContainer.Add(type, staticObject);
                }
            }
        }
    }
}