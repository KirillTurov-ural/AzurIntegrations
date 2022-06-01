using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public class StaticBehaviour : MonoBehaviourBase, IStatic
    {
        [SerializeField]
        private bool        dontDestroy =true;

        public bool IsEmpty => this == null;

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

        public virtual void Event(Message message, params object[] parameters)
        {
            for(int i = 0; i < Subscribers.Count; ++i)
            {
                Subscribers[i].Reaction(message, parameters);
            }
        }
        #endregion

        #region ISubscriber
        public virtual string Description
        {
            get
            {
                return "[" + StaticType + "] " + name;
            }
            set
            {
                name = value;
            }
        }

        public virtual void Reaction(Message message, params object[] parameters)
        {
            switch (message)
            {
                default:
                    break;
            }
        }
        #endregion

        #region IStatic
        public virtual StaticType StaticType
        {
            get;
        }

        public void SaveInstance()
        {
            StaticContainer.Set(StaticType, this);
        }

        public void DeleteInstance()
        {
            if (StaticContainer.Get(StaticType) == (IStatic)this)
            {
                StaticContainer.Set(StaticType, null);
            }
        }
        #endregion

        #region SpecialForMono
        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                if (dontDestroy)
                {
                    DontDestroyOnLoad(transform.root);
                }

                SaveInstance();
            }
        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
            {
                DeleteInstance();
            }
        }
        #endregion
    }
}