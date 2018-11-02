using System;
using UnityEngine;

namespace AudioSDK
{
    public abstract class RegisteredComponent : MonoBehaviour, IRegisteredComponent
    {
        private bool isRegistered;
        private bool isUnregistered;

        public Type GetRegisteredComponentBaseClassType() { return typeof(RegisteredComponent); }

        protected virtual void Awake()
        {
            if(!isRegistered)
            {
                RegisteredComponentController._Register(this);
                isRegistered = true;
                isUnregistered = false;
            }
            else
            {
                Debug.LogWarning("RegisteredComponent: Awake() / OnDestroy() not correctly called. Object: " + name);
            }
        }

        protected virtual void OnDestroy()
        {
            if(isRegistered && !isUnregistered)
            {
                RegisteredComponentController._Unregister(this);
                isRegistered = false;
                isUnregistered = true;
            }
            else
            {
                if((isRegistered ? 0 : (isUnregistered ? 1 : 0)) != 0)
                    return;
                Debug.LogWarning("RegisteredComponent: Awake() / OnDestroy() not correctly called. Object: " + name + " isRegistered:" + isRegistered + " isUnregistered:" +
                                 isUnregistered);
            }
        }
    }
}
