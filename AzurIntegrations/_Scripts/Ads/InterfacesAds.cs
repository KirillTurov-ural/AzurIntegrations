using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoGD
{

    /// <summary>
    /// Реклама
    /// </summary>
    public interface IADS : IStatic
    {
        void Init();
        void ShowAds(string placementId, System.Action<bool> action = null);
        bool AdAvailable(string placementId);
        void StopAds(string placementId);
    }
}
