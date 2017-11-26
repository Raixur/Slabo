using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSDK
{
    [Serializable]
    public class AudioSubItem
    {
        public float Probability = 1f;
        public float Volume = 1f;
        public List<string> IndividualSettings = new List<string>();
        private float summedProbability = -1f;
        public AudioSubItemType SubItemType;
        public bool DisableOtherSubitems;
        public string ItemModeAudioID;
        public AudioClip Clip;
        public float PitchShift;
        public float Pan2D;
        public float Delay;
        public float RandomPitch;
        public float RandomVolume;
        public float RandomDelay;
        public float ClipStopTime;
        public float ClipStartTime;
        public float FadeIn;
        public float FadeOut;
        public bool RandomStartPosition;
        internal int SubItemId;
        [NonSerialized] private AudioItem item;

        public AudioSubItem() { }

        public AudioSubItem(AudioSubItem orig, AudioItem item)
        {
            SubItemType = orig.SubItemType;
            if(SubItemType == AudioSubItemType.Clip)
                Clip = orig.Clip;
            else if(SubItemType == AudioSubItemType.Item)
                ItemModeAudioID = orig.ItemModeAudioID;
            Probability = orig.Probability;
            DisableOtherSubitems = orig.DisableOtherSubitems;
            Clip = orig.Clip;
            Volume = orig.Volume;
            PitchShift = orig.PitchShift;
            Pan2D = orig.Pan2D;
            Delay = orig.Delay;
            RandomPitch = orig.RandomPitch;
            RandomVolume = orig.RandomVolume;
            RandomDelay = orig.RandomDelay;
            ClipStopTime = orig.ClipStopTime;
            ClipStartTime = orig.ClipStartTime;
            FadeIn = orig.FadeIn;
            FadeOut = orig.FadeOut;
            RandomStartPosition = orig.RandomStartPosition;
            for(var index = 0; index < orig.IndividualSettings.Count; ++index)
                IndividualSettings.Add(orig.IndividualSettings[index]);
            Item = item;
        }

        internal float SummedProbability
        {
            get { return summedProbability; }
            set { summedProbability = value; }
        }

        public AudioItem Item
        {
            internal set { item = value; }
            get { return item; }
        }

        public override string ToString()
        {
            if(SubItemType == AudioSubItemType.Clip)
                return "CLIP: " + Clip.name;
            return "ITEM: " + ItemModeAudioID;
        }
    }

    public enum AudioSubItemType
    {
        Clip,
        Item
    }

    public enum AudioPickSubItemMode
    {
        Disabled,
        Random,
        RandomNotSameTwice,
        Sequence,
        SequenceWithRandomStart,
        AllSimultaneously,
        TwoSimultaneously,
        StartLoopSequenceWithFirst,
        RandomNotSameTwiceOddsEvens
    }
}
