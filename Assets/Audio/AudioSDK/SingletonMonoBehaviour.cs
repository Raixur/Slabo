using UnityEngine;

namespace AudioSDK
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour, ISingletonMonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                return UnitySingleton<T>.GetSingleton(true, true);
            }
        }

        public static T DoesInstanceExist()
        {
            return UnitySingleton<T>.GetSingleton(false, false);
        }

        public static void ActivateSingletonInstance()
        {
            UnitySingleton<T>.GetSingleton(true, true);
        }

        public static void SetSingletonAutoCreate(GameObject autoCreatePrefab)
        {
            UnitySingleton<T>.AutoCreatePrefab = autoCreatePrefab;
        }

        public static void SetSingletonType(System.Type type)
        {
            UnitySingleton<T>.MyType = type;
        }

        protected virtual void Awake()
        {
            if (!IsSingletonObject)
                return;
            UnitySingleton<T>._Awake(this as T);
        }

        protected virtual void OnDestroy()
        {
            if (!IsSingletonObject)
                return;
            UnitySingleton<T>._Destroy();
        }

        public virtual bool IsSingletonObject
        {
            get
            {
                return true;
            }
        }
    }
}
