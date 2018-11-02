using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSDK
{
    [Serializable]
    public class AudioCategory
    {
        [SerializeField] private float volume = 1f;
        [SerializeField] private string parentCategoryName;
        public string Name;
        private AudioCategory parentCategory;
        private AudioFader audioFader;
        public GameObject AudioObjectPrefab;
        public AudioItem[] AudioItems;
        public AudioMixerGroup AudioMixerGroup;

        public AudioCategory(AudioController audioController) { AudioController = audioController; }

        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                ApplyVolumeChange();
            }
        }

        public float VolumeTotal
        {
            get
            {
                UpdateFadeTime();
                var num = AudioFader.Get();
                if(ParentCategory != null)
                    return ParentCategory.VolumeTotal * volume * num;
                return volume * num;
            }
        }

        public AudioCategory ParentCategory
        {
            set
            {
                parentCategory = value;
                parentCategoryName = value != null ? parentCategory.Name : null;
            }
            get
            {
                if(string.IsNullOrEmpty(parentCategoryName))
                    return null;
                if(parentCategory == null)
                    if(AudioController != null)
                        parentCategory = AudioController._GetCategory(parentCategoryName);
                    else
                        Debug.LogWarning("_audioController == null");
                return parentCategory;
            }
        }

        private AudioFader AudioFader
        {
            get { return audioFader ?? (audioFader = new AudioFader()); }
        }

        public AudioController AudioController { get; set; }

        public bool IsFadingIn
        {
            get { return AudioFader.IsFadingIn; }
        }

        public bool IsFadingOut
        {
            get { return AudioFader.IsFadingOut; }
        }

        public bool IsFadeOutComplete
        {
            get { return AudioFader.IsFadingOutComplete; }
        }

        public GameObject GetAudioObjectPrefab()
        {
            if(AudioObjectPrefab != null)
                return AudioObjectPrefab;
            return ParentCategory != null ? ParentCategory.GetAudioObjectPrefab() : AudioController.AudioObjectPrefab;
        }

        public AudioMixerGroup GetAudioMixerGroup()
        {
            if(AudioMixerGroup != null)
                return AudioMixerGroup;
            return ParentCategory != null ? ParentCategory.GetAudioMixerGroup() : null;
        }

        internal void AnalyseAudioItems(Dictionary<string, AudioItem> audioItemsDict)
        {
            if(AudioItems == null)
                return;
            foreach(var audioItem in AudioItems)
                if(audioItem != null)
                {
                    audioItem._Initialize(this);
                    if(audioItemsDict != null)
                        try
                        {
                            audioItemsDict.Add(audioItem.Name, audioItem);
                        }
                        catch(ArgumentException)
                        {
                            Debug.LogWarning("Multiple audio items with name '" + audioItem.Name + "'", AudioController);
                        }
                }
        }

        internal int GetIndexOf(AudioItem audioItem)
        {
            if(AudioItems == null)
                return -1;
            for(var index = 0; index < AudioItems.Length; ++index)
                if(audioItem == AudioItems[index])
                    return index;
            return -1;
        }

        private void ApplyVolumeChange()
        {
            var playingAudioObjects = AudioController.GetPlayingAudioObjects();
            foreach(var audioObject in playingAudioObjects)
                if(IsCategoryParentOf(audioObject.Category, this))
                    audioObject._ApplyVolumeBoth();
        }

        private static bool IsCategoryParentOf(AudioCategory toTest, AudioCategory parent)
        {
            for(var audioCategory = toTest; audioCategory != null; audioCategory = audioCategory.ParentCategory)
                if(audioCategory == parent)
                    return true;
            return false;
        }

        public void UnloadAllAudioClips()
        {
            foreach(var item in AudioItems)
                item.UnloadAudioClip();
        }

        public void FadeIn(float fadeInTime, bool stopCurrentFadeOut = true)
        {
            UpdateFadeTime();
            AudioFader.FadeIn(fadeInTime, stopCurrentFadeOut);
        }

        public void FadeOut(float fadeOutLength, float startToFadeTime = 0.0f)
        {
            UpdateFadeTime();
            AudioFader.FadeOut(fadeOutLength, startToFadeTime);
        }

        private void UpdateFadeTime() { AudioFader.Time = AudioController.SystemTime; }
    }
}
