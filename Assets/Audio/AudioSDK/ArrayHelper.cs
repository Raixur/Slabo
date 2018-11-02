using System;
using UnityEngine;

namespace AudioSDK
{
    public static class ArrayHelper
    {
        public static T AddArrayElement<T>(ref T[] array) where T : new() { return AddArrayElement(ref array, Activator.CreateInstance<T>()); }

        public static T AddArrayElement<T>(ref T[] array, T elToAdd)
        {
            if(array == null)
            {
                array = new T[1];
                array[0] = elToAdd;
                return elToAdd;
            }
            var objArray = new T[array.Length + 1];
            array.CopyTo(objArray, 0);
            objArray[array.Length] = elToAdd;
            array = objArray;
            return elToAdd;
        }

        public static void DeleteArrayElement<T>(ref T[] array, int index)
        {
            if(index >= array.Length || index < 0)
            {
                Debug.LogWarning("invalid index in DeleteArrayElement: " + index);
            }
            else
            {
                var objArray = new T[array.Length - 1];
                for(var index1 = 0; index1 < index; ++index1)
                    objArray[index1] = array[index1];
                for(var index1 = index + 1; index1 < array.Length; ++index1)
                    objArray[index1 - 1] = array[index1];
                array = objArray;
            }
        }
    }
}
