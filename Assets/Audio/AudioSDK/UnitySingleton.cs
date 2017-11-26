using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioSDK
{
    public class UnitySingleton<T> where T : MonoBehaviour
    {
        internal static Type MyType = typeof(T);
        private static int globalInstanceCount;
        private static bool _awakeSingletonCalled;
        private static T _instance;
        internal static GameObject AutoCreatePrefab;

        private UnitySingleton() { }

        public static T GetSingleton(bool throwErrorIfNotFound, bool autoCreate)
        {
            if(!(bool)_instance)
            {
                var obj = Object.FindObjectsOfType(MyType).Cast<ISingletonMonoBehaviour>().Where(singletonMonoBehaviour => singletonMonoBehaviour.IsSingletonObject).Cast<Object>().FirstOrDefault();
                if(!(bool)obj)
                    if(autoCreate && AutoCreatePrefab != null)
                    {
                        Object.Instantiate(AutoCreatePrefab).name = AutoCreatePrefab.name;
                        if(!(bool)Object.FindObjectOfType(MyType))
                        {
                            Debug.LogError("Auto created object does not have component " + MyType.Name);
                            return default(T);
                        }
                    }
                    else
                    {
                        if(throwErrorIfNotFound)
                            Debug.LogError("No singleton component " + MyType.Name + " found in the scene.");
                        return default(T);
                    }
                else
                    _AwakeSingleton(obj as T);
                _instance = (T)obj;
            }
            return _instance;
        }

        internal static void _Awake(T instance)
        {
            ++globalInstanceCount;
            if(globalInstanceCount > 1)
                Debug.LogError("More than one instance of SingletonMonoBehaviour " + typeof(T).Name);
            else
                _instance = instance;
            _AwakeSingleton(instance);
        }

        internal static void _Destroy()
        {
            if(globalInstanceCount <= 0)
                return;
            --globalInstanceCount;
            if(globalInstanceCount != 0)
                return;
            _awakeSingletonCalled = false;
            _instance = default(T);
        }

        private static void _AwakeSingleton(T instance)
        {
            if(_awakeSingletonCalled)
                return;
            _awakeSingletonCalled = true;
            instance.SendMessage("AwakeSingleton", SendMessageOptions.DontRequireReceiver);
        }
    }
}
