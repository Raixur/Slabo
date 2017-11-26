using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace UnityEditor
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public static class AudioUtility
    {
        public static void PlayClip(AudioClip clip)
        {
            typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new[]
            {
                typeof (AudioClip)
            }, null).Invoke(null, new object[]
            {
                clip
            });
        }

        public static void StopClip(AudioClip clip)
        {
            typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("StopClip", BindingFlags.Static | BindingFlags.Public, null, new[]
            {
                typeof (AudioClip)
            }, null).Invoke(null, new object[]
            {
                clip
            });
        }

        public static void PauseClip(AudioClip clip)
        {
            typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("PauseClip", BindingFlags.Static | BindingFlags.Public, null, new[]
            {
                typeof (AudioClip)
            }, null).Invoke(null, new object[]
            {
                clip
            });
        }

        public static void ResumeClip(AudioClip clip)
        {
            typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("ResumeClip", BindingFlags.Static | BindingFlags.Public, null, new[]
            {
                typeof (AudioClip)
            }, null).Invoke(null, new object[]
            {
                clip
            });
        }
    }
}
