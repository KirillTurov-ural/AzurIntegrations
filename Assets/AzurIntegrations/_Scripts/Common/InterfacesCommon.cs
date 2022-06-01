using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{
    public interface ICoroutines : IStatic
    {
        void StopStaticCoroutine(object sender);
        Coroutine StartStaticCoroutine(object sender, IEnumerator method);
    }

    public interface IStatic : ISender, ISubscriber
    {
        bool IsEmpty
        {
            get;
        }

        StaticType StaticType
        {
            get;
        }

        void SaveInstance();
        void DeleteInstance();
    }

    /// <summary>
    /// Отправитель
    /// </summary>
    public interface ISender
    {
        string Description
        {
            get;
            set;
        }

        List<ISubscriber> Subscribers
        {
            get;
        }

        void AddSubscriber(ISubscriber subscriber);
        void RemoveSubscriber(ISubscriber subscriber);
        void Event(Message type, params object[] parameters);
    }

    /// <summary>
    /// Подписчик
    /// </summary>
    public interface ISubscriber
    {
        string Description
        {
            get;
            set;
        }

        void Reaction(Message message, params object[] parameters);
    }

    /// <summary>
    /// Внутриигровая покупка
    /// </summary>
    public interface IInAppItem
    {
        string ID
        {
            get;
        }

        string Type
        {
            get;
            set;
        }

        string Title
        {
            get;
        }

        decimal LocalizedPrice
        {
            get;
        }

        string Price
        {
            get;
        }

        string ISO
        {
            get;
        }

        string TransactionID
        {
            get;
            set;
        }

        string Receipt
        {
            get;
            set;
        }
    }

    public interface IInterstitialChecker
    {
        bool Check();
    }
}
