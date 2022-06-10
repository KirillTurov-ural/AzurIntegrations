using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BoGD
{
    /// <summary>
    /// Coroutines - singleton for independence launching of unity coroutines
    /// </summary>
    public class Coroutines : StaticBehaviour, ICoroutines
    {
        public override StaticType StaticType => StaticType.Coroutines;

        private Dictionary<object, Coroutine>   coroutines = null;

        public void StopStaticCoroutine(object sender)
        {
            if(coroutines == null)
            {
                return;
            }

            Coroutine result = null;            
            if (coroutines.TryGetValue(sender, out result))
            {
                if (this != null && gameObject != null && result != null)
                {
                    StopCoroutine(result);
                }
            }
        }

        public Coroutine StartStaticCoroutine(object sender, IEnumerator method)
        {
            if(this == null)
            {
                return null;
            }

            if (coroutines == null)
            {
                coroutines = new Dictionary<object, Coroutine>();
            }

            Coroutine result = null;
            if (coroutines.TryGetValue(sender, out result) && result != null)
            {
                StopCoroutine(result);
            }

            result = StartCoroutine(method);
            coroutines[sender] = result;
            return result;
        }
    }
}
