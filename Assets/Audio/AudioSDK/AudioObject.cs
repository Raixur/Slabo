using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace AudioSDK
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("ZG/Audio/AudioObject")]
    public class AudioObject : RegisteredComponent
    {
        public delegate void AudioEventDelegate(AudioObject audioObject);

        private const float VolumeTransformPower = 1.6f;
        internal float VolumeExcludingCategory = 1f;
        private float volumeFromPrimaryFade = 1f;
        private float volumeFromSecondaryFade = 1f;
        internal float VolumeFromScriptCall = 1f;
        private double playTime = -1.0;
        private double playStartTimeLocal = -1.0;
        private double playStartTimeSystem = -1.0;
        private double playScheduledTimeDsp = -1.0;
        private bool isInactive = true;
        internal float AudioSourceMinDistanceSaved = 1f;
        internal float AudioSourceMaxDistanceSaved = 500f;
        internal int LastChosenSubItemIndex = -1;

        [NonSerialized] private AudioCategory category;

        private AudioSubItem subItemPrimary;
        private AudioSubItem subItemSecondary;
        private int pauseCoroutineCounter;
        private bool areSources1And2Swapped;
        private bool paused;
        private bool applicationPaused;
        private AudioFader primaryFader;
        private AudioFader secondaryFader;
        private bool stopRequested;
        private bool finishSequence;
        private int loopSequenceCount;
        private bool pauseWithFadeOutRequested;
        private double dspTimeRemainingAtPause;
        private AudioController audioController;
        internal bool IsCurrentPlaylistTrack;
        internal float AudioSourceSpatialBlendSaved;
        private AudioMixerGroup audioMixerGroup;
        private AudioSource audioSource1;
        private AudioSource audioSource2;
        private bool primaryAudioSourcePaused;
        private bool secondaryAudioSourcePaused;

        public string AudioID { get; internal set; }

        public AudioCategory Category
        {
            get { return category; }
            internal set { category = value; }
        }

        public AudioSubItem SubItem
        {
            get { return subItemPrimary; }
            internal set { subItemPrimary = value; }
        }

        public bool IsPlayedAsMusicOrAmbienceSound { get; internal set; }

        public AudioItem AudioItem
        {
            get
            {
                if(SubItem != null)
                    return SubItem.Item;
                return null;
            }
        }

        public AudioEventDelegate CompletelyPlayedDelegate { set; get; }

        public float Volume
        {
            get { return VolumeWithCategory; }
            set
            {
                var volumeFromCategory = VolumeFromCategory;
                VolumeExcludingCategory = (double)volumeFromCategory <= 0.0 ? value : value / volumeFromCategory;
                _ApplyVolumeBoth();
            }
        }

        public float VolumeItem
        {
            get
            {
                if(VolumeFromScriptCall > 0.0)
                    return VolumeExcludingCategory / VolumeFromScriptCall;
                return VolumeExcludingCategory;
            }
            set
            {
                VolumeExcludingCategory = value * VolumeFromScriptCall;
                _ApplyVolumeBoth();
            }
        }

        public float VolumeTotal
        {
            get { return VolumeTotalWithoutFade * volumeFromPrimaryFade; }
        }

        public float VolumeTotalWithoutFade
        {
            get
            {
                var num = VolumeWithCategory;
                var controller = Category == null ? audioController : Category.AudioController;
                if(controller != null)
                {
                    num *= controller.Volume;
                    if(controller.SoundMuted && !IsPlayedAsMusicOrAmbienceSound)
                        num = 0.0f;
                }
                return num;
            }
        }

        public double PlayCalledAtTime
        {
            get { return playTime; }
        }

        public double StartedPlayingAtTime
        {
            get { return playStartTimeSystem; }
        }

        public float TimeUntilEnd
        {
            get { return ClipLength - AudioTime; }
        }

        public double ScheduledPlayingAtDspTime
        {
            get { return playScheduledTimeDsp; }
            set
            {
                playScheduledTimeDsp = value;
                PrimaryAudioSource.SetScheduledStartTime(playScheduledTimeDsp);
            }
        }

        public float ClipLength
        {
            get
            {
                if(StopClipAtTime > 0.0)
                    return StopClipAtTime - StartClipAtTime;
                if(PrimaryAudioSource.clip != null)
                    return PrimaryAudioSource.clip.length - StartClipAtTime;
                return 0.0f;
            }
        }

        public float AudioTime
        {
            get { return PrimaryAudioSource.time - StartClipAtTime; }
            set { PrimaryAudioSource.time = value + StartClipAtTime; }
        }

        public bool IsFadingOut
        {
            get { return primaryFader.IsFadingOut; }
        }

        public bool IsFadeOutComplete
        {
            get { return primaryFader.IsFadingOutComplete; }
        }

        public bool IsFadingOutOrScheduled
        {
            get { return primaryFader.IsFadingOutOrScheduled; }
        }

        public bool IsFadingIn
        {
            get { return primaryFader.IsFadingIn; }
        }

        public float Pitch
        {
            get { return PrimaryAudioSource.pitch; }
            set { PrimaryAudioSource.pitch = value; }
        }

        public float Pan
        {
            get { return PrimaryAudioSource.panStereo; }
            set { PrimaryAudioSource.panStereo = value; }
        }

        public double AudioObjectTime { get; private set; }

        public bool StopAfterFadeOut { get; set; }

        public AudioSource PrimaryAudioSource
        {
            get { return audioSource1; }
        }

        public AudioSource SecondaryAudioSource
        {
            get { return audioSource2; }
        }

        internal float VolumeFromCategory
        {
            get
            {
                return Category != null ? Category.VolumeTotal : 1f;
            }
        }

        internal float VolumeWithCategory
        {
            get { return VolumeFromCategory * VolumeExcludingCategory; }
        }

        private float StopClipAtTime
        {
            get
            {
                return SubItem == null ? 0.0f : SubItem.ClipStopTime;
            }
        }

        private float StartClipAtTime
        {
            get
            {
                return SubItem == null ? 0.0f : SubItem.ClipStartTime;
            }
        }

        private bool ShouldStopIfPrimaryFadedOut
        {
            get
            {
                if(StopAfterFadeOut)
                    return !pauseWithFadeOutRequested;
                return false;
            }
        }

        public void FadeIn(float fadeInTime)
        {
            if(playStartTimeLocal > 0.0 && playStartTimeLocal - AudioObjectTime > 0.0)
            {
                primaryFader.FadeIn(fadeInTime, playStartTimeLocal);
                _UpdateFadeVolume();
            }
            else
            {
                primaryFader.FadeIn(fadeInTime, AudioObjectTime, !ShouldStopIfPrimaryFadedOut);
                _UpdateFadeVolume();
            }
        }

        public void PlayScheduled(double dspTime) { _PlayScheduled(dspTime); }

        public void PlayAfter(string audioID, double deltaDspTime = 0.0, float volume = 1f, float startTime = 0.0f)
        {
            AudioController.PlayAfter(audioID, this, deltaDspTime, volume, startTime);
        }

        public void PlayNow(string audioID, float delay = 0.0f, float volume = 1f, float startTime = 0.0f)
        {
            var audioItem = AudioController.GetAudioItem(audioID);
            if(audioItem == null)
                Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
            else
                audioController.PlayAudioItem(audioItem, volume, transform.position, transform.parent, delay, startTime, false, this);
        }

        public void Play(float delay = 0.0f) { _PlayDelayed(delay); }

        public void Stop() { Stop(-1f); }

        public void Stop(float fadeOutLength) { Stop(fadeOutLength, 0.0f); }

        public void Stop(float fadeOutLength, float startToFadeTime)
        {
            if(IsPaused(false))
            {
                fadeOutLength = 0.0f;
                startToFadeTime = 0.0f;
            }
            if(startToFadeTime > 0.0)
            {
                StartCoroutine(_WaitForSecondsThenStop(startToFadeTime, fadeOutLength));
            }
            else
            {
                stopRequested = true;
                if(fadeOutLength < 0.0)
                    fadeOutLength = SubItem == null ? 0.0f : SubItem.FadeOut;
                if(fadeOutLength == 0.0 && startToFadeTime == 0.0)
                {
                    _Stop();
                }
                else
                {
                    FadeOut(fadeOutLength, startToFadeTime);
                    if(!IsSecondaryPlaying())
                        return;
                    SwitchAudioSources();
                    FadeOut(fadeOutLength, startToFadeTime);
                    SwitchAudioSources();
                }
            }
        }

        public void FinishSequence()
        {
            if(finishSequence)
                return;
            if(AudioItem == null)
                return;
            switch(AudioItem.Loop)
            {
                case AudioItem.LoopMode.LoopSequence:
                case AudioItem.LoopMode.LoopSubitem | AudioItem.LoopMode.LoopSequence:
                    finishSequence = true;
                    break;
                case AudioItem.LoopMode.PlaySequenceAndLoopLast:
                case AudioItem.LoopMode.IntroLoopOutroSequence:
                    PrimaryAudioSource.loop = false;
                    finishSequence = true;
                    break;
            }
        }

        private IEnumerator _WaitForSecondsThenStop(float startToFadeTime, float fadeOutLength)
        {
            yield return new WaitForSeconds(startToFadeTime);
            if(!isInactive)
                Stop(fadeOutLength);
        }

        public void FadeOut(float fadeOutLength) { FadeOut(fadeOutLength, 0.0f); }

        public void FadeOut(float fadeOutLength, float startToFadeTime)
        {
            if(fadeOutLength < 0.0)
                fadeOutLength = SubItem == null ? 0.0f : SubItem.FadeOut;
            if(fadeOutLength > 0.0 || startToFadeTime > 0.0)
            {
                primaryFader.FadeOut(fadeOutLength, startToFadeTime);
            }
            else
            {
                if(fadeOutLength != 0.0)
                    return;
                if(ShouldStopIfPrimaryFadedOut)
                    _Stop();
                else
                    primaryFader.FadeOut(0.0f, startToFadeTime);
            }
        }

        public void Pause() { Pause(0.0f); }

        public void Pause(float fadeOutTime)
        {
            if(paused)
                return;
            paused = true;
            if(fadeOutTime > 0.0)
            {
                pauseWithFadeOutRequested = true;
                FadeOut(fadeOutTime);
                double num1 = fadeOutTime;
                var num2 = pauseCoroutineCounter + 1;
                pauseCoroutineCounter = num2;
                var counter = num2;
                StartCoroutine(_WaitThenPause((float)num1, counter));
            }
            else
            {
                _PauseNow();
            }
        }

        private void _PauseNow()
        {
            if(playScheduledTimeDsp > 0.0)
            {
                dspTimeRemainingAtPause = playScheduledTimeDsp - AudioSettings.dspTime;
                ScheduledPlayingAtDspTime = 9000000000.0;
            }
            _PauseAudioSources();
            if(!pauseWithFadeOutRequested)
                return;
            pauseWithFadeOutRequested = false;
            primaryFader.Set0();
        }

        public void Unpause() { Unpause(0.0f); }

        public void Unpause(float fadeInTime)
        {
            if(!paused)
                return;
            _UnpauseNow();
            if(fadeInTime > 0.0)
                FadeIn(fadeInTime);
            pauseWithFadeOutRequested = false;
        }

        private void _UnpauseNow()
        {
            paused = false;
            if((bool)SecondaryAudioSource && secondaryAudioSourcePaused)
                SecondaryAudioSource.Play();
            if(dspTimeRemainingAtPause > 0.0 && primaryAudioSourcePaused)
            {
                var time = AudioSettings.dspTime + dspTimeRemainingAtPause;
                playStartTimeSystem = AudioController.SystemTime + dspTimeRemainingAtPause;
                PrimaryAudioSource.PlayScheduled(time);
                ScheduledPlayingAtDspTime = time;
                dspTimeRemainingAtPause = -1.0;
            }
            else
            {
                if(!primaryAudioSourcePaused)
                    return;
                PrimaryAudioSource.Play();
            }
        }

        private IEnumerator _WaitThenPause(float waitTime, int counter)
        {
            yield return new WaitForSeconds(waitTime);
            if(pauseWithFadeOutRequested && counter == pauseCoroutineCounter)
                _PauseNow();
        }

        private void _PauseAudioSources()
        {
            if(PrimaryAudioSource.isPlaying)
            {
                primaryAudioSourcePaused = true;
                PrimaryAudioSource.Pause();
            }
            else
            {
                primaryAudioSourcePaused = false;
            }
            if((bool)SecondaryAudioSource && SecondaryAudioSource.isPlaying)
            {
                secondaryAudioSourcePaused = true;
                SecondaryAudioSource.Pause();
            }
            else
            {
                secondaryAudioSourcePaused = false;
            }
        }

        public bool IsPaused(bool returnTrueIfStillFadingOut = true)
        {
            if(!returnTrueIfStillFadingOut && pauseWithFadeOutRequested)
                return false;
            return paused;
        }

        public bool IsPlaying()
        {
            return IsPrimaryPlaying() || IsSecondaryPlaying();
        }

        public bool IsPrimaryPlaying() { return PrimaryAudioSource.isPlaying; }

        public bool IsSecondaryPlaying()
        {
            return SecondaryAudioSource != null && SecondaryAudioSource.isPlaying;
        }

        public void SwitchAudioSources()
        {
            if(audioSource2 == null)
                _CreateSecondAudioSource();
            _SwitchValues(ref audioSource1, ref audioSource2);
            _SwitchValues(ref primaryFader, ref secondaryFader);
            _SwitchValues(ref subItemPrimary, ref subItemSecondary);
            _SwitchValues(ref volumeFromPrimaryFade, ref volumeFromSecondaryFade);
            areSources1And2Swapped = !areSources1And2Swapped;
        }

        private void _SwitchValues<T>(ref T v1, ref T v2)
        {
            var obj = v1;
            v1 = v2;
            v2 = obj;
        }

        protected override void Awake()
        {
            base.Awake();
            if(primaryFader == null)
                primaryFader = new AudioFader();
            else
                primaryFader.Set0();
            if(secondaryFader == null)
                secondaryFader = new AudioFader();
            else
                secondaryFader.Set0();
            if(audioSource1 == null)
            {
                var components = GetComponents<AudioSource>();
                if(components.Length == 0)
                {
                    Debug.LogError("AudioObject does not have an AudioSource component!");
                }
                else
                {
                    audioSource1 = components[0];
                    if(components.Length >= 2)
                        audioSource2 = components[1];
                }
            }
            else if((bool)audioSource2 && areSources1And2Swapped)
            {
                SwitchAudioSources();
            }
            audioMixerGroup = PrimaryAudioSource.outputAudioMixerGroup;
            _Set0();
            audioController = AudioController.Instance;
        }

        private void _CreateSecondAudioSource()
        {
            audioSource2 = gameObject.AddComponent<AudioSource>();
            audioSource2.rolloffMode = audioSource1.rolloffMode;
            audioSource2.minDistance = audioSource1.minDistance;
            audioSource2.maxDistance = audioSource1.maxDistance;
            audioSource2.dopplerLevel = audioSource1.dopplerLevel;
            audioSource2.spread = audioSource1.spread;
            audioSource2.spatialBlend = audioSource1.spatialBlend;
            audioSource2.outputAudioMixerGroup = audioSource1.outputAudioMixerGroup;
            audioSource2.velocityUpdateMode = audioSource1.velocityUpdateMode;
            audioSource2.ignoreListenerVolume = audioSource1.ignoreListenerVolume;
            audioSource2.playOnAwake = false;
            audioSource2.priority = audioSource1.priority;
            audioSource2.bypassEffects = audioSource1.bypassEffects;
            audioSource2.ignoreListenerPause = audioSource1.ignoreListenerPause;
        }

        private void _Set0()
        {
            _SetReferences0();
            AudioObjectTime = 0.0;
            PrimaryAudioSource.playOnAwake = false;
            if((bool)SecondaryAudioSource)
                SecondaryAudioSource.playOnAwake = false;
            LastChosenSubItemIndex = -1;
            primaryFader.Set0();
            secondaryFader.Set0();
            playTime = -1.0;
            playStartTimeLocal = -1.0;
            playStartTimeSystem = -1.0;
            playScheduledTimeDsp = -1.0;
            volumeFromPrimaryFade = 1f;
            volumeFromSecondaryFade = 1f;
            VolumeFromScriptCall = 1f;
            isInactive = true;
            stopRequested = false;
            finishSequence = false;
            VolumeExcludingCategory = 1f;
            paused = false;
            applicationPaused = false;
            IsCurrentPlaylistTrack = false;
            loopSequenceCount = 0;
            StopAfterFadeOut = true;
            pauseWithFadeOutRequested = false;
            dspTimeRemainingAtPause = -1.0;
            primaryAudioSourcePaused = false;
            secondaryAudioSourcePaused = false;
        }

        private void _SetReferences0()
        {
            audioController = null;
            PrimaryAudioSource.clip = null;
            if(SecondaryAudioSource != null)
            {
                SecondaryAudioSource.playOnAwake = false;
                SecondaryAudioSource.clip = null;
            }
            SubItem = null;
            Category = null;
            CompletelyPlayedDelegate = null;
        }

        private void _PlayScheduled(double dspTime)
        {
            if(!(bool)PrimaryAudioSource.clip)
            {
                Debug.LogError("audio.clip == null in " + gameObject.name);
            }
            else
            {
                playScheduledTimeDsp = dspTime;
                var num = dspTime - AudioSettings.dspTime;
                playStartTimeLocal = num + AudioObjectTime;
                playStartTimeSystem = num + AudioController.SystemTime;
                PrimaryAudioSource.PlayScheduled(dspTime);
                _OnPlay();
            }
        }

        private void _PlayDelayed(float delay)
        {
            if(!(bool)PrimaryAudioSource.clip)
            {
                Debug.LogError("audio.clip == null in " + gameObject.name);
            }
            else
            {
                PrimaryAudioSource.PlayDelayed(delay);
                playScheduledTimeDsp = -1.0;
                playStartTimeLocal = AudioObjectTime + delay;
                playStartTimeSystem = AudioController.SystemTime + delay;
                _OnPlay();
            }
        }

        private void _OnPlay()
        {
            isInactive = false;
            playTime = AudioObjectTime;
            paused = false;
            primaryAudioSourcePaused = false;
            secondaryAudioSourcePaused = false;
            primaryFader.Set0();
        }

        private void _Stop()
        {
            primaryFader.Set0();
            secondaryFader.Set0();
            PrimaryAudioSource.Stop();
            if((bool)SecondaryAudioSource)
                SecondaryAudioSource.Stop();
            paused = false;
            primaryAudioSourcePaused = false;
            secondaryAudioSourcePaused = false;
        }

        [UsedImplicitly]
        private void Update()
        {
            if(isInactive)
                return;
            if(!IsPaused(false))
            {
                AudioObjectTime = AudioObjectTime + AudioController.SystemDeltaTime;
                primaryFader.Time = AudioObjectTime;
                secondaryFader.Time = AudioObjectTime;
            }
            if(playScheduledTimeDsp > 0.0 && AudioObjectTime > playStartTimeLocal)
                playScheduledTimeDsp = -1.0;
            if(!paused && !applicationPaused)
            {
                var num = IsPrimaryPlaying() ? 1 : 0;
                if(num == 0 && !IsSecondaryPlaying())
                {
                    var flag2 = true;
                    if(!stopRequested && CompletelyPlayedDelegate != null)
                    {
                        CompletelyPlayedDelegate(this);
                        flag2 = !IsPlaying();
                    }
                    if(IsCurrentPlaylistTrack && AudioController.DoesInstanceExist())
                        AudioController.Instance.NotifyPlaylistTrackCompleteleyPlayed(this);
                    if(flag2)
                    {
                        DestroyAudioObject();
                        return;
                    }
                }
                else
                {
                    if(!stopRequested && _IsAudioLoopSequenceMode() && !IsSecondaryPlaying() && TimeUntilEnd < 1.0 + Mathf.Max(0.0f, AudioItem.LoopSequenceOverlap) &&
                       playScheduledTimeDsp < 0.0)
                        _ScheduleNextInLoopSequence();
                    if(!PrimaryAudioSource.loop)
                        if(IsCurrentPlaylistTrack && (bool)audioController && audioController.CrossfadePlaylist &&
                           AudioTime > ClipLength - (double)audioController.MusicCrossFadeTimeOut)
                        {
                            if(AudioController.DoesInstanceExist())
                                AudioController.Instance.NotifyPlaylistTrackCompleteleyPlayed(this);
                        }
                        else
                        {
                            _StartFadeOutIfNecessary();
                            if(IsSecondaryPlaying())
                            {
                                SwitchAudioSources();
                                _StartFadeOutIfNecessary();
                                SwitchAudioSources();
                            }
                        }
                }
            }
            _UpdateFadeVolume();
        }

        private void _StartFadeOutIfNecessary()
        {
            if(SubItem == null)
            {
                Debug.LogWarning("subItem == null");
            }
            else
            {
                var audioTime = AudioTime;
                var num = 0.0f;
                if(SubItem.FadeOut > 0.0)
                    num = SubItem.FadeOut;
                else if(StopClipAtTime > 0.0)
                    num = 0.1f;
                if(IsFadingOutOrScheduled || num <= 0.0 || audioTime <= ClipLength - (double)num)
                    return;
                FadeOut(SubItem.FadeOut);
            }
        }

        private bool _IsAudioLoopSequenceMode()
        {
            var audioItem = AudioItem;
            if(audioItem != null)
                switch(audioItem.Loop)
                {
                    case AudioItem.LoopMode.LoopSequence:
                    case AudioItem.LoopMode.LoopSubitem | AudioItem.LoopMode.LoopSequence:
                        return true;
                    case AudioItem.LoopMode.PlaySequenceAndLoopLast:
                    case AudioItem.LoopMode.IntroLoopOutroSequence:
                        return !PrimaryAudioSource.loop;
                }
            return false;
        }

        private bool _ScheduleNextInLoopSequence()
        {
            var num = AudioItem.LoopSequenceCount <= 0 ? AudioItem.SubItems.Length : AudioItem.LoopSequenceCount;
            if(finishSequence && (AudioItem.Loop != AudioItem.LoopMode.IntroLoopOutroSequence || loopSequenceCount <= num - 3 || loopSequenceCount >= num - 1) ||
               AudioItem.LoopSequenceCount > 0 && AudioItem.LoopSequenceCount <= loopSequenceCount + 1)
                return false;
            var dspTime = AudioSettings.dspTime + TimeUntilEnd + _GetRandomLoopSequenceDelay(AudioItem);
            var audioItem = AudioItem;
            SwitchAudioSources();
            audioController.PlayAudioItem(audioItem, VolumeFromScriptCall, Vector3.zero, null, 0.0f, 0.0f, false, this, dspTime);
            loopSequenceCount = loopSequenceCount + 1;
            if(AudioItem.Loop == AudioItem.LoopMode.PlaySequenceAndLoopLast || AudioItem.Loop == AudioItem.LoopMode.IntroLoopOutroSequence)
                if(AudioItem.Loop == AudioItem.LoopMode.IntroLoopOutroSequence)
                {
                    if(!finishSequence && num <= loopSequenceCount + 2)
                        PrimaryAudioSource.loop = true;
                }
                else if(num <= loopSequenceCount + 1)
                {
                    PrimaryAudioSource.loop = true;
                }
            return true;
        }

        private void _UpdateFadeVolume()
        {
            bool finishedFadeOut;
            var num1 = _EqualizePowerForCrossfading(primaryFader.Get(out finishedFadeOut));
            if(finishedFadeOut)
            {
                if(stopRequested)
                {
                    _Stop();
                    return;
                }
                if(!_IsAudioLoopSequenceMode())
                {
                    if(!ShouldStopIfPrimaryFadedOut)
                        return;
                    _Stop();
                    return;
                }
            }
            if(num1 != (double)volumeFromPrimaryFade)
                volumeFromPrimaryFade = num1;
            _ApplyVolumePrimary();
            if(!(audioSource2 != null))
                return;
            var num2 = _EqualizePowerForCrossfading(secondaryFader.Get(out finishedFadeOut));
            if(finishedFadeOut)
            {
                audioSource2.Stop();
            }
            else
            {
                if(num2 == (double)volumeFromSecondaryFade)
                    return;
                volumeFromSecondaryFade = num2;
                _ApplyVolumeSecondary();
            }
        }

        private float _EqualizePowerForCrossfading(float v)
        {
            return !audioController.EqualPowerCrossfade ? v : InverseTransformVolume(Mathf.Sin((float)(v * 3.14159274101257 * 0.5)));
        }
        
        [UsedImplicitly]
        private void OnApplicationPause(bool b) { SetApplicationPaused(b); }

        private void SetApplicationPaused(bool isPaused) { applicationPaused = isPaused; }

        public void DestroyAudioObject()
        {
            if(IsPlaying())
                _Stop();
            Destroy(gameObject);
            isInactive = true;
        }

        public static float TransformVolume(float volume) { return Mathf.Pow(volume, 1.6f); }

        public static float InverseTransformVolume(float volume) { return Mathf.Pow(volume, 0.625f); }

        public static float TransformPitch(float pitchSemiTones) { return Mathf.Pow(2f, pitchSemiTones / 12f); }

        public static float InverseTransformPitch(float pitch) { return (float)(Mathf.Log(pitch) / (double)Mathf.Log(2f) * 12.0); }

        internal void _ApplyVolumeBoth()
        {
            var totalWithoutFade = VolumeTotalWithoutFade;
            PrimaryAudioSource.volume = TransformVolume(totalWithoutFade * volumeFromPrimaryFade);
            if(!(bool)SecondaryAudioSource)
                return;
            SecondaryAudioSource.volume = TransformVolume(totalWithoutFade * volumeFromSecondaryFade);
        }

        internal void _ApplyVolumePrimary(float volumeMultiplier = 1f)
        {
            var num = TransformVolume(VolumeTotalWithoutFade * volumeFromPrimaryFade * volumeMultiplier);
            if(PrimaryAudioSource.volume == (double)num)
                return;
            PrimaryAudioSource.volume = num;
        }

        internal void _ApplyVolumeSecondary(float volumeMultiplier = 1f)
        {
            if(!(bool)SecondaryAudioSource)
                return;
            var num = TransformVolume(VolumeTotalWithoutFade * volumeFromSecondaryFade * volumeMultiplier);
            if(SecondaryAudioSource.volume == (double)num)
                return;
            SecondaryAudioSource.volume = num;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            var audioItem = AudioItem;
            if(audioItem != null && audioItem.OverrideAudioSourceSettings)
                _RestoreOverrideAudioSourceSettings();
            _SetReferences0();
            PrimaryAudioSource.outputAudioMixerGroup = audioMixerGroup;
        }

        private void _RestoreOverrideAudioSourceSettings()
        {
            PrimaryAudioSource.minDistance = AudioSourceMinDistanceSaved;
            PrimaryAudioSource.maxDistance = AudioSourceMaxDistanceSaved;
            PrimaryAudioSource.spatialBlend = AudioSourceSpatialBlendSaved;
            if(!(SecondaryAudioSource != null))
                return;
            SecondaryAudioSource.minDistance = AudioSourceMinDistanceSaved;
            SecondaryAudioSource.maxDistance = AudioSourceMaxDistanceSaved;
            SecondaryAudioSource.spatialBlend = AudioSourceSpatialBlendSaved;
        }

        public bool DoesBelongToCategory(string categoryName)
        {
            for(var audioCategory = Category; audioCategory != null; audioCategory = audioCategory.ParentCategory)
                if(audioCategory.Name == categoryName)
                    return true;
            return false;
        }

        private float _GetRandomLoopSequenceDelay(AudioItem audioItem)
        {
            var num = -audioItem.LoopSequenceOverlap;
            if(audioItem.LoopSequenceRandomDelay > 0.0)
                num += Random.Range(0.0f, audioItem.LoopSequenceRandomDelay);
            return num;
        }
    }
}
