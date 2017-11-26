using System;
using System.Linq;
using JetBrains.Annotations;

namespace AudioSDK
{
    [Serializable]
    public class AudioItem
    {
        [Serializable]
        [Flags]
        public enum LoopMode
        {
            DoNotLoop = 0,
            LoopSubitem = 1,
            LoopSequence = 2,
            PlaySequenceAndLoopLast = 4,
            IntroLoopOutroSequence = 5
        }

        public bool DestroyOnLoad = true;
        public float Volume = 1f;
        public AudioPickSubItemMode SubItemPickMode = AudioPickSubItemMode.RandomNotSameTwice;
        public float MinTimeBetweenPlayCalls = 0.1f;
        public float AudioSourceMinDistance = 1f;
        public float AudioSourceMaxDistance = 500f;
        internal int LastChosen = -1;
        internal double LastPlayedTime = -1.0;
        public string Name;
        public LoopMode Loop;
        public int LoopSequenceCount;
        public float LoopSequenceOverlap;
        public float LoopSequenceRandomDelay;
        public float LoopSequenceRandomPitch;
        public float LoopSequenceRandomVolume;
        public int MaxInstanceCount;
        public float Delay;
        public float RandomVolume;
        public float RandomPitch;
        public float RandomDelay;
        public bool OverrideAudioSourceSettings;
        public float SpatialBlend;
        public AudioSubItem[] SubItems;

        [NonSerialized] private AudioCategory category;

        public AudioItem() { }

        public AudioItem(AudioItem orig)
        {
            Name = orig.Name;
            Loop = orig.Loop;
            LoopSequenceCount = orig.LoopSequenceCount;
            LoopSequenceOverlap = orig.LoopSequenceOverlap;
            LoopSequenceRandomDelay = orig.LoopSequenceRandomDelay;
            LoopSequenceRandomPitch = orig.LoopSequenceRandomPitch;
            LoopSequenceRandomVolume = orig.LoopSequenceRandomVolume;
            DestroyOnLoad = orig.DestroyOnLoad;
            Volume = orig.Volume;
            SubItemPickMode = orig.SubItemPickMode;
            MinTimeBetweenPlayCalls = orig.MinTimeBetweenPlayCalls;
            MaxInstanceCount = orig.MaxInstanceCount;
            Delay = orig.Delay;
            RandomVolume = orig.RandomVolume;
            RandomPitch = orig.RandomPitch;
            RandomDelay = orig.RandomDelay;
            OverrideAudioSourceSettings = orig.OverrideAudioSourceSettings;
            AudioSourceMinDistance = orig.AudioSourceMinDistance;
            AudioSourceMaxDistance = orig.AudioSourceMaxDistance;
            SpatialBlend = orig.SpatialBlend;
            for(var index = 0; index < orig.SubItems.Length; ++index)
                ArrayHelper.AddArrayElement(ref SubItems, new AudioSubItem(orig.SubItems[index], this));
        }

        public AudioCategory Category
        {
            private set { category = value; }
            get { return category; }
        }

        [UsedImplicitly]
        private void Awake()
        {
            if(Loop == (LoopMode.LoopSubitem | LoopMode.LoopSequence))
                Loop = LoopMode.LoopSequence;
            LastChosen = -1;
        }

        public void ResetSequence() { LastChosen = -1; }

        internal void _Initialize(AudioCategory categ)
        {
            Category = categ;
            _NormalizeSubItems();
        }

        private void _NormalizeSubItems()
        {
            var num1 = 0.0f;
            var num2 = 0;
            var flag = SubItems.Any(subItem => _IsValidSubItem(subItem) && subItem.DisableOtherSubitems);
            foreach(var subItem in SubItems)
            {
                subItem.Item = this;
                if(_IsValidSubItem(subItem) && (subItem.DisableOtherSubitems || !flag))
                    num1 += subItem.Probability;
                subItem.SubItemId = num2;
                ++num2;
            }
            if(num1 <= 0.0)
                return;
            var num3 = 0.0f;
            foreach(var subItem in SubItems)
                if(_IsValidSubItem(subItem))
                {
                    if(subItem.DisableOtherSubitems || !flag)
                        num3 += subItem.Probability / num1;
                    subItem.SummedProbability = num3;
                }
        }

        private static bool _IsValidSubItem(AudioSubItem item)
        {
            switch(item.SubItemType)
            {
                case AudioSubItemType.Clip:
                    return item.Clip != null;
                case AudioSubItemType.Item:
                    if(item.ItemModeAudioID != null)
                        return item.ItemModeAudioID.Length > 0;
                    return false;
                default:
                    return false;
            }
        }

        public void UnloadAudioClip()
        {
            foreach(var subItem in SubItems)
                if((bool)subItem.Clip && !subItem.Clip.preloadAudioData)
                    subItem.Clip.UnloadAudioData();
        }
    }
}
