using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BoGD
{

    public static class ExtensionsCommon
    {
        public static DateTime              epochStart = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        public static float                 lastTimeGet;
        public static float                 lastTimeGetTimeStamp;
        public static double                lastTime;

        public static double GetTimestamp(this DateTime date)
        {
            float time = Time.time;
            if (lastTimeGetTimeStamp == time)
            {
                return lastTime;
            }
            lastTimeGetTimeStamp = time;
            lastTime = (date - epochStart).TotalSeconds;

            return lastTime;
        }

        public static double CurrentTime()
        {
            float time = Time.time;
            if (lastTimeGet == time)
            {
                return lastTime;
            }
            lastTimeGet = time;
            lastTime = (DateTime.UtcNow - epochStart).TotalSeconds;
            return lastTime;
        }


        public static int ControlInternetConnect()
        {
            return Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        }

        public static void Event(this StaticType staticType, Message message, params object[] parameters)
        {
            staticType.Instance().Event(message, parameters);
        }

        public static void Reaction(this StaticType staticType, Message message, params object[] parameters)
        {
            if (!staticType.Exists())
            {
                return;
            }

            staticType.Instance().Reaction(message, parameters);
        }

        public static void AddSubscriber(this StaticType staticType, ISubscriber subscriber)
        {
            if (!staticType.Exists())
            {
                return;
            }

            staticType.Instance().AddSubscriber(subscriber);
        }


        public static void RemoveSubscriber(this StaticType staticType, ISubscriber subscriber)
        {
            if (!staticType.Exists())
            {
                return;
            }

            staticType.Instance().RemoveSubscriber(subscriber);
        }

        public static bool Exists(this StaticType staticType)
        {
            IStatic iStatic = StaticContainer.Get(staticType);
            return iStatic != null && !iStatic.IsEmpty;
        }

        public static T Instance<T>(this StaticType staticType) where T : IStatic
        {
            return StaticContainer.Get<T>(staticType);
        }

        public static IStatic Instance(this StaticType staticType)
        {
            return StaticContainer.Get(staticType);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static T Get<T>(this object[] parameters, int index = 0)
        {
            T result = default(T);
            int currentIndex = -1;
            System.Type targetType = typeof(T);
#if NETFX_CORE
            TypeInfo targetTypeInfo = targetType.GetTypeInfo();
#else
            System.Type targetTypeInfo = targetType;
#endif
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    continue;
                }
#if NETFX_CORE
                TypeInfo type = parameters[i].GetType().GetTypeInfo();
#else
                System.Type type = parameters[i].GetType();
#endif
                if (targetTypeInfo.IsAssignableFrom(type) ||
                    type.IsAssignableFrom(targetTypeInfo) ||
                    type.IsSubclassOf(targetType))
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        result = (T)parameters[i];
                        break;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Извлеччение данных из словаря
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Extract<T>(this Dictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
            {
                Debug.LogWarningFormat("Dictionary is null! '{0}'", key);
                return default(T);
            }

            object obj = null;
            if (!dictionary.TryGetValue(key, out obj))
            {
                Debug.LogWarningFormat("Key was not found '{0}'", key);
                return default(T);
            }

            T result = default(T);
            if (typeof(T).IsEnum)
            {
                result = (T)System.Enum.Parse(typeof(T), (string)obj, true);
            }
            else
            {
                try
                {
                    result = (T)obj;
                }
                catch
                {
                    Debug.LogErrorFormat("Can't convert '{0}' to format '{1}', it '{2}'", key, typeof(T), obj.GetType());
                    return result;
                }

                //if (typeof(T).IsArray)
                //{ 
                //    Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)obj;
                //    result = array.ToObject<T>();
                //}
                //else
                //{
                //    result = (T)obj;
                //}
            }
            return result;
        }
    }
}