using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSDK
{
    public static class RegisteredComponentController
    {
        private static readonly Dictionary<Type, InstanceContainer> InstanceContainers = new Dictionary<Type, InstanceContainer>();

        public static T[] GetAllOfType<T>() where T : IRegisteredComponent
        {
            InstanceContainer instanceContainer;
            if(!InstanceContainers.TryGetValue(typeof(T), out instanceContainer))
                return new T[0];
            var objArray = new T[instanceContainer.Count];
            var num = 0;
            foreach(var registeredComponent in instanceContainer)
                objArray[num++] = (T)registeredComponent;
            return objArray;
        }

        public static object[] GetAllOfType(Type type)
        {
            InstanceContainer instanceContainer;
            if(!InstanceContainers.TryGetValue(type, out instanceContainer))
                return new object[0];
            var objArray = new object[instanceContainer.Count];
            var num = 0;
            foreach(var registeredComponent in instanceContainer)
                objArray[num++] = registeredComponent;
            return objArray;
        }

        public static int InstanceCountOfType<T>() where T : IRegisteredComponent
        {
            InstanceContainer instanceContainer;
            if(!InstanceContainers.TryGetValue(typeof(T), out instanceContainer))
                return 0;
            return instanceContainer.Count;
        }

        private static InstanceContainer _GetInstanceContainer(Type type)
        {
            InstanceContainer instanceContainer1;
            if(InstanceContainers.TryGetValue(type, out instanceContainer1))
                return instanceContainer1;
            var instanceContainer2 = new InstanceContainer();
            InstanceContainers.Add(type, instanceContainer2);
            return instanceContainer2;
        }

        private static void _RegisterType(IRegisteredComponent component, Type type)
        {
            if(_GetInstanceContainer(type).Add(component))
                return;
            Debug.LogError("RegisteredComponentController error: Tried to register same instance twice");
        }

        internal static void _Register(IRegisteredComponent component)
        {
            var type = component.GetType();
            do
            {
                _RegisterType(component, type);
                type = type.BaseType;
            } while(type != component.GetRegisteredComponentBaseClassType());
        }

        internal static void _UnregisterType(IRegisteredComponent component, Type type)
        {
            if(_GetInstanceContainer(type).Remove(component))
                return;
            Debug.LogError("RegisteredComponentController error: Tried to unregister unknown instance");
        }

        internal static void _Unregister(IRegisteredComponent component)
        {
            var type = component.GetType();
            do
            {
                _UnregisterType(component, type);
                type = type.BaseType;
            } while(type != component.GetRegisteredComponentBaseClassType());
        }

        public class InstanceContainer : HashSet<IRegisteredComponent>
        { }
    }
}
