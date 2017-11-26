using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace AudioSDK
{
    [AddComponentMenu("ZG/Audio/AudioController")]
    public class AudioController : MonoBehaviour, ISingletonMonoBehaviour
    {
        private static bool isPlaylistPlaying;
        private static double lastSystemTime = -1.0;

        public bool UsePooledAudioObjects = true;
        public Playlist[] MusicPlaylists = new Playlist[1];
        public float DelayBetweenPlaylistTracks = 1f;

        private bool musicEnabled = true;
        private bool ambienceSoundEnabled = true;

        [SerializeField] private float volume = 1f;

        public AudioController_CurrentInspectorSelection CurrentInspectorSelection = new AudioController_CurrentInspectorSelection();
        public const string AudioToolkitVersion = "8.2";
        public GameObject AudioObjectPrefab;
        public bool Persistent;
        public bool UnloadAudioClipsOnDestroy;
        public bool PlayWithZeroVolume;
        public bool EqualPowerCrossfade;
        public float MusicCrossFadeTime;
        public float AmbienceSoundCrossFadeTime;
        public bool SpecifyCrossFadeInAndOutSeperately;

        [SerializeField] private float musicCrossFadeTimeIn;
        [SerializeField] private float musicCrossFadeTimeOut;
        [SerializeField] private float ambienceSoundCrossFadeTimeIn;
        [SerializeField] private float ambienceSoundCrossFadeTimeOut;

        public AudioCategory[] AudioCategories;

        public bool LoopPlaylist;
        public bool ShufflePlaylist;
        public bool CrossfadePlaylist;
        protected static AudioObject CurrentMusic;
        protected static AudioObject CurrentAmbienceSound;
        private string currentPlaylistName;
        protected AudioListener CurrentAudioListener;
        private bool soundMuted;
        private bool categoriesValidated;

        [SerializeField] private bool isAdditionalTestAudioController;
        [SerializeField] private bool audioDisabled;

        private Dictionary<string, AudioItem> audioItems;
        private static List<int> playlistPlayed;
        private List<AudioController> additionalTestAudioControllers;

        public static AudioController Instance
        {
            get { return UnitySingleton<AudioController>.GetSingleton(true, false); }
        }

        public static bool DoesInstanceExist() { return !Equals(UnitySingleton<AudioController>.GetSingleton(false, false), null); }

        public static void SetSingletonType(Type type) { UnitySingleton<AudioController>.MyType = type; }

        public bool DisableAudio
        {
            set { audioDisabled = value; }
            get { return audioDisabled; }
        }

        public bool IsAdditionalAudioController
        {
            get { return isAdditionalTestAudioController; }
            set { isAdditionalTestAudioController = value; }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                if(value == (double)volume)
                    return;
                volume = value;
                _ApplyVolumeChange();
            }
        }

        public bool MusicEnabled
        {
            get { return musicEnabled; }
            set
            {
                if(musicEnabled == value)
                    return;
                musicEnabled = value;
                if(!(bool)CurrentMusic)
                    return;
                if(value)
                {
                    if(!CurrentMusic.IsPaused())
                        return;
                    CurrentMusic.Play();
                }
                else
                    CurrentMusic.Pause();
            }
        }

        public bool AmbienceSoundEnabled
        {
            get { return ambienceSoundEnabled; }
            set
            {
                if(ambienceSoundEnabled == value)
                    return;
                ambienceSoundEnabled = value;
                if(!(bool)CurrentAmbienceSound)
                    return;
                if(value)
                {
                    if(!CurrentAmbienceSound.IsPaused())
                        return;
                    CurrentAmbienceSound.Play();
                }
                else
                    CurrentAmbienceSound.Pause();
            }
        }

        public bool SoundMuted
        {
            get { return soundMuted; }
            set
            {
                soundMuted = value;
                _ApplyVolumeChange();
            }
        }

        public float MusicCrossFadeTimeIn
        {
            get { return SpecifyCrossFadeInAndOutSeperately ? musicCrossFadeTimeIn : MusicCrossFadeTime; }
            set { musicCrossFadeTimeIn = value; }
        }

        public float MusicCrossFadeTimeOut
        {
            get { return SpecifyCrossFadeInAndOutSeperately ? musicCrossFadeTimeOut : MusicCrossFadeTime; }
            set { musicCrossFadeTimeOut = value; }
        }

        public float AmbienceSoundCrossFadeTimeIn
        {
            get { return SpecifyCrossFadeInAndOutSeperately ? ambienceSoundCrossFadeTimeIn : AmbienceSoundCrossFadeTime; }
            set { ambienceSoundCrossFadeTimeIn = value; }
        }

        public float AmbienceSoundCrossFadeTimeOut
        {
            get { return SpecifyCrossFadeInAndOutSeperately ? ambienceSoundCrossFadeTimeOut : AmbienceSoundCrossFadeTime; }
            set { ambienceSoundCrossFadeTimeOut = value; }
        }

        public static double SystemTime { get; private set; }

        public static double SystemDeltaTime { get; private set; }

        public static AudioObject PlayMusic(string audioID, float volume = 1f, float delay = 0.0f, float startTime = 0.0f)
        {
            isPlaylistPlaying = false;
            return Instance._PlayMusic(audioID, volume, delay, startTime);
        }

        public static AudioObject PlayMusic(string audioID, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float delay = 0.0f, float startTime = 0.0f)
        {
            isPlaylistPlaying = false;
            return Instance._PlayMusic(audioID, worldPosition, parentObj, volume, delay, startTime);
        }

        public static AudioObject PlayMusic(string audioID, Transform parentObj, float volume = 1f, float delay = 0.0f, float startTime = 0.0f)
        {
            isPlaylistPlaying = false;
            return Instance._PlayMusic(audioID, parentObj.position, parentObj, volume, delay, startTime);
        }

        public static bool StopMusic() { return Instance._StopMusic(0.0f); }

        public static bool StopMusic(float fadeOut) { return Instance._StopMusic(fadeOut); }

        public static bool PauseMusic(float fadeOut = 0.0f) { return Instance._PauseMusic(fadeOut); }

        public static bool IsMusicPaused() { return CurrentMusic != null && CurrentMusic.IsPaused(); }

        public static bool UnpauseMusic(float fadeIn = 0.0f)
        {
            if(!Instance.musicEnabled || !(CurrentMusic != null) || !CurrentMusic.IsPaused())
                return false;
            CurrentMusic.Unpause(fadeIn);
            return true;
        }

        public static AudioObject PlayAmbienceSound(string audioID, float volume = 1f, float delay = 0.0f, float startTime = 0.0f)
        {
            return Instance._PlayAmbienceSound(audioID, volume, delay, startTime);
        }

        public static AudioObject PlayAmbienceSound(string audioID, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float delay = 0.0f,
                                                    float startTime = 0.0f)
        {
            return Instance._PlayAmbienceSound(audioID, worldPosition, parentObj, volume, delay, startTime);
        }

        public static AudioObject PlayAmbienceSound(string audioID, Transform parentObj, float volume = 1f, float delay = 0.0f, float startTime = 0.0f)
        {
            return Instance._PlayAmbienceSound(audioID, parentObj.position, parentObj, volume, delay, startTime);
        }

        public static bool StopAmbienceSound() { return Instance._StopAmbienceSound(0.0f); }

        public static bool StopAmbienceSound(float fadeOut) { return Instance._StopAmbienceSound(fadeOut); }

        public static bool PauseAmbienceSound(float fadeOut = 0.0f) { return Instance._PauseAmbienceSound(fadeOut); }

        public static bool IsAmbienceSoundPaused()
        {
            return CurrentAmbienceSound != null && CurrentAmbienceSound.IsPaused();
        }

        public static bool UnpauseAmbienceSound(float fadeIn = 0.0f)
        {
            if(!Instance.ambienceSoundEnabled || !(CurrentAmbienceSound != null) || !CurrentAmbienceSound.IsPaused())
                return false;
            CurrentAmbienceSound.Unpause(fadeIn);
            return true;
        }

        public static int EnqueueMusic(string audioID) { return Instance._EnqueueMusic(audioID); }

        private Playlist GetCurrentPlaylist()
        {
            return string.IsNullOrEmpty(currentPlaylistName) ? null : GetPlaylistByName(currentPlaylistName);
        }

        public Playlist GetPlaylistByName(string playlistName)
        {
            foreach(var playlist in MusicPlaylists)
            {
                if(playlistName == playlist.Name)
                    return playlist;
            }
            if(additionalTestAudioControllers != null)
            {
                return additionalTestAudioControllers.SelectMany(additionalTestAudioController => additionalTestAudioController.MusicPlaylists)
                                                      .FirstOrDefault(playlist => playlistName == playlist.Name);
            }
            return null;
        }

        public static string[] GetMusicPlaylist(string playlistName = null)
        {
            var playlist = !string.IsNullOrEmpty(playlistName) ? Instance.GetPlaylistByName(playlistName) : Instance.GetCurrentPlaylist();
            if(playlist == null)
                return null;
            var strArray = new string[playlist.PlaylistItems != null ? playlist.PlaylistItems.Length : 0];
            if(strArray.Length != 0)
                Array.Copy(playlist.PlaylistItems, strArray, strArray.Length);
            return strArray;
        }

        public static bool SetCurrentMusicPlaylist(string playlistName)
        {
            if(Instance.GetPlaylistByName(playlistName) == null)
            {
                Debug.LogError("Playlist with name " + playlistName + " not found");
                return false;
            }
            Instance.currentPlaylistName = playlistName;
            return true;
        }

        public static AudioObject PlayMusicPlaylist(string playlistName = null)
        {
            if(!string.IsNullOrEmpty(playlistName) && !SetCurrentMusicPlaylist(playlistName))
                return null;
            return Instance._PlayMusicPlaylist();
        }

        public static AudioObject PlayNextMusicOnPlaylist() { return IsPlaylistPlaying() ? Instance._PlayNextMusicOnPlaylist(0.0f) : null; }

        public static AudioObject PlayPreviousMusicOnPlaylist() { return IsPlaylistPlaying() ? Instance._PlayPreviousMusicOnPlaylist(0.0f) : null; }

        public static bool IsPlaylistPlaying()
        {
            if(!isPlaylistPlaying)
                return false;
            if((bool)CurrentMusic)
                return true;
            isPlaylistPlaying = false;
            return false;
        }

        public static void ClearPlaylists() { Instance.MusicPlaylists = null; }

        public static void AddPlaylist(string playlistName, string[] audioItemIDs)
        {
            ArrayHelper.AddArrayElement(ref Instance.MusicPlaylists, new Playlist(playlistName, audioItemIDs));
        }

        public static AudioObject Play(string audioID)
        {
            var currentAudioListener = GetCurrentAudioListener();
            if(!(currentAudioListener == null))
                return Play(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, 1f);
            Debug.LogWarning("No AudioListener found in the scene");
            return null;
        }

        public static AudioObject Play(string audioID, float volume, float delay = 0.0f, float startTime = 0.0f)
        {
            var currentAudioListener = GetCurrentAudioListener();
            if(!(currentAudioListener == null))
                return Play(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
            Debug.LogWarning("No AudioListener found in the scene");
            return null;
        }

        public static AudioObject Play(string audioID, Transform parentObj) { return Play(audioID, parentObj.position, parentObj, 1f); }

        public static AudioObject Play(string audioID, Transform parentObj, float volume, float delay = 0.0f, float startTime = 0.0f)
        {
            return Play(audioID, parentObj.position, parentObj, volume, delay, startTime);
        }

        public static AudioObject Play(string audioID, Vector3 worldPosition, Transform parentObj = null)
        {
            return Instance._PlayAsSound(audioID, 1f, worldPosition, parentObj, 0.0f, 0.0f, false);
        }

        public static AudioObject Play(string audioID, Vector3 worldPosition, Transform parentObj, float volume, float delay = 0.0f, float startTime = 0.0f)
        {
            return Instance._PlayAsSound(audioID, volume, worldPosition, parentObj, delay, startTime, false);
        }

        public static AudioObject PlayScheduled(string audioID, double dspTime, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float startTime = 0.0f)
        {
            return Instance._PlayAsSound(audioID, volume, worldPosition, parentObj, 0.0f, startTime, false, dspTime);
        }

        public static AudioObject PlayAfter(string audioID, AudioObject playingAudio, double deltaDspTime = 0.0, float volume = 1f, float startTime = 0.0f)
        {
            var dspTime1 = AudioSettings.dspTime;
            if(playingAudio.IsPlaying())
                dspTime1 += playingAudio.TimeUntilEnd;
            var dspTime2 = dspTime1 + deltaDspTime;
            return PlayScheduled(audioID, dspTime2, playingAudio.transform.position, playingAudio.transform.parent, volume, startTime);
        }

        public static bool Stop(string audioID, float fadeOutLength)
        {
            if(Instance._GetAudioItem(audioID) == null)
            {
                Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
                return false;
            }
            var playingAudioObjects = GetPlayingAudioObjects(audioID);
            foreach(var audioObject in playingAudioObjects)
            {
                if(fadeOutLength < 0.0)
                    audioObject.Stop();
                else
                    audioObject.Stop(fadeOutLength);
            }
            return playingAudioObjects.Count > 0;
        }

        public static bool Stop(string audioID) { return Stop(audioID, -1f); }

        public static void StopAll(float fadeOutLength)
        {
            Instance._StopMusic(fadeOutLength);
            Instance._StopAmbienceSound(fadeOutLength);
            var playingAudioObjects = GetPlayingAudioObjects();
            foreach(var audioObject in playingAudioObjects)
            {
                if(audioObject != null)
                    audioObject.Stop(fadeOutLength);
            }
        }

        public static void StopAll() { StopAll(-1f); }

        public static void PauseAll(float fadeOutLength = 0.0f)
        {
            Instance._PauseMusic(fadeOutLength);
            Instance._PauseAmbienceSound(fadeOutLength);

            var playingAudioObjects = GetPlayingAudioObjects();
            foreach(var audioObject in playingAudioObjects)
            {
                if(audioObject != null)
                    audioObject.Pause(fadeOutLength);
            }
        }

        public static void UnpauseAll(float fadeInLength = 0.0f)
        {
            UnpauseMusic(fadeInLength);
            UnpauseAmbienceSound(fadeInLength);
            var playingAudioObjects = GetPlayingAudioObjects(true);
            var instance = Instance;
            foreach(var audioObject in playingAudioObjects)
            {
                if(audioObject != null && audioObject.IsPaused()
                   && (instance.MusicEnabled || !(CurrentMusic == audioObject))
                   && (instance.AmbienceSoundEnabled || !(CurrentAmbienceSound == audioObject)))
                    audioObject.Unpause(fadeInLength);
            }
        }

        public static void PauseCategory(string categoryName, float fadeOutLength = 0.0f)
        {
            if(CurrentMusic != null && CurrentMusic.Category.Name == categoryName)
                PauseMusic(fadeOutLength);
            if(CurrentAmbienceSound != null && CurrentAmbienceSound.Category.Name == categoryName)
                PauseAmbienceSound(fadeOutLength);

            var objectsInCategory = GetPlayingAudioObjectsInCategory(categoryName);
            foreach(var audioObject in objectsInCategory)
                audioObject.Pause(fadeOutLength);
        }

        public static void UnpauseCategory(string categoryName, float fadeInLength = 0.0f)
        {
            if(CurrentMusic != null && CurrentMusic.Category.Name == categoryName)
                UnpauseMusic(fadeInLength);
            if(CurrentAmbienceSound != null && CurrentAmbienceSound.Category.Name == categoryName)
                UnpauseAmbienceSound(fadeInLength);

            var objectsInCategory = GetPlayingAudioObjectsInCategory(categoryName, true);
            foreach(var audioObject in objectsInCategory)
            {
                if(audioObject.IsPaused())
                    audioObject.Unpause(fadeInLength);
            }
        }

        public static void StopCategory(string categoryName, float fadeOutLength = 0.0f)
        {
            if(CurrentMusic != null && CurrentMusic.Category.Name == categoryName)
                StopMusic(fadeOutLength);
            if(CurrentAmbienceSound != null && CurrentAmbienceSound.Category.Name == categoryName)
                StopAmbienceSound(fadeOutLength);

            var objectsInCategory = GetPlayingAudioObjectsInCategory(categoryName);
            foreach(var audioObject in objectsInCategory)
                audioObject.Stop(fadeOutLength);
        }

        public static bool IsPlaying(string audioID) { return GetPlayingAudioObjects(audioID).Count > 0; }

        public static List<AudioObject> GetPlayingAudioObjects(string audioID, bool includePausedAudio = false)
        {
            var playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
            var audioObjectList = new List<AudioObject>(playingAudioObjects.Count);
            audioObjectList.AddRange(playingAudioObjects.Where(audioObject => audioObject != null && audioObject.AudioID == audioID));
            return audioObjectList;
        }

        public static List<AudioObject> GetPlayingAudioObjectsInCategory(string categoryName, bool includePausedAudio = false)
        {
            var playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
            var audioObjectList = new List<AudioObject>(playingAudioObjects.Count);
            audioObjectList.AddRange(playingAudioObjects.Where(audioObject => audioObject != null && audioObject.DoesBelongToCategory(categoryName)));
            return audioObjectList;
        }

        public static List<AudioObject> GetPlayingAudioObjects(bool includePausedAudio = false)
        {
            var allOfType = RegisteredComponentController.GetAllOfType(typeof(AudioObject));
            var audioObjectList = new List<AudioObject>(allOfType.Length);
            audioObjectList.AddRange(allOfType.Cast<AudioObject>()
                                              .Where(audioObject => audioObject.IsPlaying() || includePausedAudio && audioObject.IsPaused()));
            return audioObjectList;
        }

        public static int GetPlayingAudioObjectsCount(string audioID, bool includePausedAudio = false)
        {
            var playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
            return playingAudioObjects.Count(audioObject => audioObject != null && audioObject.AudioID == audioID);
        }

        public static void EnableMusic(bool b) { Instance.MusicEnabled = b; }

        public static void EnableAmbienceSound(bool b) { Instance.AmbienceSoundEnabled = b; }

        public static void MuteSound(bool b) { Instance.SoundMuted = b; }

        public static bool IsMusicEnabled() { return Instance.MusicEnabled; }

        public static bool IsAmbienceSoundEnabled() { return Instance.AmbienceSoundEnabled; }

        public static bool IsSoundMuted() { return Instance.SoundMuted; }

        public static AudioListener GetCurrentAudioListener()
        {
            var instance = Instance;
            if(instance.CurrentAudioListener != null && instance.CurrentAudioListener.gameObject == null)
                instance.CurrentAudioListener = null;
            return instance.CurrentAudioListener ?? (instance.CurrentAudioListener = (AudioListener)FindObjectOfType(typeof(AudioListener)));
        }

        public static AudioObject GetCurrentMusic() { return CurrentMusic; }

        public static AudioObject GetCurrentAmbienceSound() { return CurrentAmbienceSound; }

        public static AudioCategory GetCategory(string name)
        {
            var instance = Instance;
            var category = instance._GetCategory(name);
            if(category != null)
                return category;
            return instance.additionalTestAudioControllers != null
                       ? instance.additionalTestAudioControllers.Select(t => t._GetCategory(name)).FirstOrDefault(category2 => category2 != null)
                       : null;
        }

        public static void SetCategoryVolume(string name, float volume)
        {
            var allCategories = GetAllCategories(name);
            if(allCategories.Count == 0)
            {
                Debug.LogWarning("No audio category with name " + name);
            }
            else
            {
                foreach(var category in allCategories)
                    category.Volume = volume;
            }
        }

        public static float GetCategoryVolume(string name)
        {
            var category = GetCategory(name);
            if(category != null)
                return category.Volume;
            Debug.LogWarning("No audio category with name " + name);
            return 0.0f;
        }

        public static void FadeOutCategory(string name, float fadeOutLength, float startToFadeTime = 0.0f)
        {
            var allCategories = GetAllCategories(name);
            if(allCategories.Count == 0)
            {
                Debug.LogWarning("No audio category with name " + name);
            }
            else
            {
                foreach(var category in allCategories)
                    category.FadeOut(fadeOutLength, startToFadeTime);
            }
        }

        public static void FadeInCategory(string name, float fadeInTime, bool stopCurrentFadeOut = true)
        {
            var allCategories = GetAllCategories(name);
            if(allCategories.Count == 0)
            {
                Debug.LogWarning("No audio category with name " + name);
            }
            else
            {
                foreach(var category in allCategories)
                    category.FadeIn(fadeInTime, stopCurrentFadeOut);
            }
        }

        public static void SetGlobalVolume(float volume)
        {
            var instance = Instance;
            instance.Volume = volume;
            if(instance.additionalTestAudioControllers == null)
                return;
            foreach(var controller in instance.additionalTestAudioControllers)
                controller.Volume = volume;
        }

        public static float GetGlobalVolume() { return Instance.Volume; }

        public static AudioCategory NewCategory(string categoryName)
        {
            var index = Instance.AudioCategories != null ? Instance.AudioCategories.Length : 0;
            var audioCategories = Instance.AudioCategories;
            Instance.AudioCategories = new AudioCategory[index + 1];
            if(index > 0)
                audioCategories.CopyTo(Instance.AudioCategories, 0);
            var audioCategory = new AudioCategory(Instance) { Name = categoryName };
            Instance.AudioCategories[index] = audioCategory;
            Instance.InvalidateCategories();
            return audioCategory;
        }

        public static void RemoveCategory(string categoryName)
        {
            var num1 = -1;
            var num2 = Instance.AudioCategories == null ? 0 : Instance.AudioCategories.Length;
            for(var index = 0; index < num2; ++index)
            {
                if(Instance.AudioCategories[index].Name == categoryName)
                {
                    num1 = index;
                    break;
                }
            }
            if(num1 == -1)
            {
                Debug.LogError("AudioCategory does not exist: " + categoryName);
            }
            else
            {
                var audioCategoryArray = new AudioCategory[Instance.AudioCategories.Length - 1];
                for(var index = 0; index < num1; ++index)
                    audioCategoryArray[index] = Instance.AudioCategories[index];
                for(var index = num1 + 1; index < Instance.AudioCategories.Length; ++index)
                    audioCategoryArray[index - 1] = Instance.AudioCategories[index];
                Instance.AudioCategories = audioCategoryArray;
                Instance.InvalidateCategories();
            }
        }

        public static void AddToCategory(AudioCategory category, AudioItem audioItem)
        {
            var index = category.AudioItems != null ? category.AudioItems.Length : 0;
            var audioItems = category.AudioItems;
            category.AudioItems = new AudioItem[index + 1];
            if(index > 0)
                audioItems.CopyTo(category.AudioItems, 0);
            category.AudioItems[index] = audioItem;
            Instance.InvalidateCategories();
        }

        public static AudioItem AddToCategory(AudioCategory category, AudioClip audioClip, string audioID)
        {
            var audioItem = new AudioItem
            {
                Name = audioID,
                SubItems = new AudioSubItem[1]
            };
            audioItem.SubItems[0] = new AudioSubItem
            {
                Clip = audioClip
            };
            AddToCategory(category, audioItem);
            return audioItem;
        }

        public static bool RemoveAudioItem(string audioID)
        {
            var audioItem = Instance._GetAudioItem(audioID);
            if(audioItem == null)
                return false;
            var indexOf = audioItem.Category.GetIndexOf(audioItem);
            if(indexOf < 0)
                return false;
            var audioItems = audioItem.Category.AudioItems;
            var audioItemArray = new AudioItem[audioItems.Length - 1];
            for(var index = 0; index < indexOf; ++index)
                audioItemArray[index] = audioItems[index];
            for(var index = indexOf + 1; index < audioItems.Length; ++index)
                audioItemArray[index - 1] = audioItems[index];
            audioItem.Category.AudioItems = audioItemArray;
            if(Instance.categoriesValidated)
                Instance.audioItems.Remove(audioID);
            return true;
        }

        public static bool IsValidAudioID(string audioID) { return Instance._GetAudioItem(audioID) != null; }

        public static AudioItem GetAudioItem(string audioID) { return Instance._GetAudioItem(audioID); }

        public static void DetachAllAudios(GameObject gameObjectWithAudios)
        {
            foreach(Component componentsInChild in gameObjectWithAudios.GetComponentsInChildren<AudioObject>(true))
                componentsInChild.transform.parent = null;
        }

        public static float GetAudioItemMaxDistance(string audioID)
        {
            var audioItem = GetAudioItem(audioID);
            return audioItem.OverrideAudioSourceSettings ? audioItem.AudioSourceMaxDistance : audioItem.Category.GetAudioObjectPrefab().GetComponent<AudioSource>().maxDistance;
        }

        public void UnloadAllAudioClips()
        {
            foreach(var audioCategory in AudioCategories)
                audioCategory.UnloadAllAudioClips();
        }

        private void _ApplyVolumeChange()
        {
            var playingAudioObjects = GetPlayingAudioObjects(true);
            foreach(var audioObject in playingAudioObjects)
            {
                if(audioObject != null)
                    audioObject._ApplyVolumeBoth();
            }
        }

        internal AudioItem _GetAudioItem(string audioID)
        {
            ValidateCategories();
            AudioItem audioItem;
            return audioItems.TryGetValue(audioID, out audioItem) ? audioItem : null;
        }

        protected AudioObject _PlayMusic(string audioID, float vol, float delay, float startTime)
        {
            var currentAudioListener = GetCurrentAudioListener();
            if(!(currentAudioListener == null))
                return _PlayMusic(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
            Debug.LogWarning("No AudioListener found in the scene");
            return null;
        }

        protected AudioObject _PlayAmbienceSound(string audioID, float vol, float delay, float startTime)
        {
            var currentAudioListener = GetCurrentAudioListener();
            if(!(currentAudioListener == null))
                return _PlayAmbienceSound(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
            Debug.LogWarning("No AudioListener found in the scene");
            return null;
        }

        protected bool _StopMusic(float fadeOutLength)
        {
            if(!(CurrentMusic != null))
                return false;
            CurrentMusic.Stop(fadeOutLength);
            CurrentMusic = null;
            return true;
        }

        protected bool _PauseMusic(float fadeOut)
        {
            if(!(CurrentMusic != null))
                return false;
            CurrentMusic.Pause(fadeOut);
            return true;
        }

        protected bool _StopAmbienceSound(float fadeOutLength)
        {
            if(!(CurrentAmbienceSound != null))
                return false;
            CurrentAmbienceSound.Stop(fadeOutLength);
            CurrentAmbienceSound = null;
            return true;
        }

        protected bool _PauseAmbienceSound(float fadeOut)
        {
            if(!(CurrentAmbienceSound != null))
                return false;
            CurrentAmbienceSound.Pause(fadeOut);
            return true;
        }

        protected AudioObject _PlayMusic(string audioID, Vector3 position, Transform parentObj, float vol, float delay, float startTime)
        {
            if(!IsMusicEnabled())
                return null;
            bool flag;
            if(CurrentMusic != null && CurrentMusic.IsPlaying())
            {
                flag = true;
                CurrentMusic.Stop(MusicCrossFadeTimeOut);
            }
            else
                flag = false;
            if(MusicCrossFadeTimeIn <= 0.0)
                flag = false;
            CurrentMusic = _PlayAsMusicOrAmbienceSound(audioID, volume, position, parentObj, delay, startTime, false, 0.0, null, flag ? 0.0f : 1f);
            if(flag && (bool)CurrentMusic)
                CurrentMusic.FadeIn(MusicCrossFadeTimeIn);
            return CurrentMusic;
        }

        protected AudioObject _PlayAmbienceSound(string audioID, Vector3 position, Transform parentObj, float volume, float delay, float startTime)
        {
            if(!IsAmbienceSoundEnabled())
                return null;
            bool flag;
            if(CurrentAmbienceSound != null && CurrentAmbienceSound.IsPlaying())
            {
                flag = true;
                CurrentAmbienceSound.Stop(AmbienceSoundCrossFadeTimeOut);
            }
            else
                flag = false;
            if(AmbienceSoundCrossFadeTimeIn <= 0.0)
                flag = false;
            CurrentAmbienceSound = _PlayAsMusicOrAmbienceSound(audioID, volume, position, parentObj, delay, startTime, false, 0.0, null, flag ? 0.0f : 1f);
            if(flag && (bool)CurrentAmbienceSound)
                CurrentAmbienceSound.FadeIn(AmbienceSoundCrossFadeTimeIn);
            return CurrentAmbienceSound;
        }

        protected int _EnqueueMusic(string audioID)
        {
            var currentPlaylist = GetCurrentPlaylist();
            var length = currentPlaylist != null ? MusicPlaylists.Length + 1 : 1;
            var strArray = new string[length];
            if(currentPlaylist != null)
                currentPlaylist.PlaylistItems.CopyTo(strArray, 0);
            strArray[length - 1] = audioID;
            currentPlaylist.PlaylistItems = strArray;
            return length;
        }

        protected AudioObject _PlayMusicPlaylist()
        {
            _ResetLastPlayedList();
            return _PlayNextMusicOnPlaylist(0.0f);
        }

        private AudioObject _PlayMusicTrackWithID(int nextTrack, float delay, bool addToPlayedList)
        {
            if(nextTrack < 0)
                return null;
            playlistPlayed.Add(nextTrack);
            isPlaylistPlaying = true;
            var audioObject = _PlayMusic(GetCurrentPlaylist().PlaylistItems[nextTrack], 1f, delay, 0.0f);
            if(audioObject != null)
            {
                audioObject.IsCurrentPlaylistTrack = true;
                audioObject.PrimaryAudioSource.loop = false;
            }
            return audioObject;
        }

        internal AudioObject _PlayNextMusicOnPlaylist(float delay) { return _PlayMusicTrackWithID(GetNextMusicTrack(), delay, true); }

        internal AudioObject _PlayPreviousMusicOnPlaylist(float delay) { return _PlayMusicTrackWithID(GetPreviousMusicTrack(), delay, false); }

        private static void _ResetLastPlayedList() { playlistPlayed.Clear(); }

        protected int GetNextMusicTrack()
        {
            var currentPlaylist = GetCurrentPlaylist();
            if(currentPlaylist == null || currentPlaylist.PlaylistItems == null)
            {
                Debug.LogWarning("There is no current playlist set");
                return -1;
            }
            if(currentPlaylist.PlaylistItems.Length == 1)
                return 0;
            return ShufflePlaylist ? GetNextMusicTrackShuffled() : GetNextMusicTrackInOrder();
        }

        protected int GetPreviousMusicTrack()
        {
            if(GetCurrentPlaylist().PlaylistItems.Length == 1)
                return 0;
            return ShufflePlaylist ? GetPreviousMusicTrackShuffled() : GetPreviousMusicTrackInOrder();
        }

        private int GetPreviousMusicTrackShuffled()
        {
            if(playlistPlayed.Count < 2)
                return -1;
            var num = playlistPlayed[playlistPlayed.Count - 2];
            RemoveLastPlayedOnList();
            RemoveLastPlayedOnList();
            return num;
        }

        private void RemoveLastPlayedOnList() { playlistPlayed.RemoveAt(playlistPlayed.Count - 1); }

        private int GetNextMusicTrackShuffled()
        {
            var hashSetFlash = new HashSet<int>();
            var num1 = playlistPlayed.Count;
            var currentPlaylist = GetCurrentPlaylist();
            if(LoopPlaylist)
            {
                var num2 = Mathf.Clamp(currentPlaylist.PlaylistItems.Length / 4, 2, 10);
                if(num1 > currentPlaylist.PlaylistItems.Length - num2)
                {
                    num1 = currentPlaylist.PlaylistItems.Length - num2;
                    if(num1 < 1)
                        num1 = 1;
                }
            }
            else if(num1 >= currentPlaylist.PlaylistItems.Length)
                return -1;
            for(var index = 0; index < num1; ++index)
                hashSetFlash.Add(playlistPlayed[playlistPlayed.Count - 1 - index]);
            var intList = new List<int>();
            for(var index = 0; index < currentPlaylist.PlaylistItems.Length; ++index)
            {
                if(!hashSetFlash.Contains(index))
                    intList.Add(index);
            }
            return intList[UnityEngine.Random.Range(0, intList.Count)];
        }

        private int GetNextMusicTrackInOrder()
        {
            if(playlistPlayed.Count == 0)
                return 0;
            var num = playlistPlayed[playlistPlayed.Count - 1] + 1;
            var currentPlaylist = GetCurrentPlaylist();
            if(num >= currentPlaylist.PlaylistItems.Length)
            {
                if(!LoopPlaylist)
                    return -1;
                num = 0;
            }
            return num;
        }

        private int GetPreviousMusicTrackInOrder()
        {
            var currentPlaylist = GetCurrentPlaylist();
            if(playlistPlayed.Count < 2)
            {
                if(LoopPlaylist)
                    return currentPlaylist.PlaylistItems.Length - 1;
                return -1;
            }
            var num = playlistPlayed[playlistPlayed.Count - 1] - 1;
            RemoveLastPlayedOnList();
            RemoveLastPlayedOnList();
            if(num < 0)
            {
                if(!LoopPlaylist)
                    return -1;
                num = currentPlaylist.PlaylistItems.Length - 1;
            }
            return num;
        }

        protected AudioObject _PlayAsSound(string audioID, float vol, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject,
                                           double dspTime = 0.0, AudioObject useExistingAudioObject = null)
        {
            return _PlayEx(audioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObject);
        }

        protected AudioObject _PlayAsMusicOrAmbienceSound(string audioID, float vol, Vector3 worldPosition, Transform parentObj, float delay, float startTime,
                                                          bool playWithoutAudioObject, double dspTime = 0.0, AudioObject useExistingAudioObject = null,
                                                          float startVolumeMultiplier = 1f)
        {
            return _PlayEx(audioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObject, true, startVolumeMultiplier);
        }

        protected AudioObject _PlayEx(string audioID, float vol, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject,
                                      double dspTime = 0.0, AudioObject useExistingAudioObject = null, bool playAsMusicOrAmbienceSound = false, float startVolumeMultiplier = 1f)
        {
            if(audioDisabled)
                return null;
            var audioItem = _GetAudioItem(audioID);
            if(audioItem == null)
            {
                Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
                return null;
            }
            if(audioItem.LastPlayedTime > 0.0 && dspTime == 0.0 && SystemTime - audioItem.LastPlayedTime < audioItem.MinTimeBetweenPlayCalls)
                return null;
            if(audioItem.MaxInstanceCount > 0)
            {
                var playingAudioObjects = GetPlayingAudioObjects(audioID);
                if(playingAudioObjects.Count >= audioItem.MaxInstanceCount)
                {
                    var flag = playingAudioObjects.Count > audioItem.MaxInstanceCount;
                    AudioObject audioObject = null;
                    foreach(var audioObj in playingAudioObjects)
                    {
                        if((flag || !audioObj.IsFadingOut) && (audioObject == null || audioObj.StartedPlayingAtTime < audioObject.StartedPlayingAtTime))
                            audioObject = audioObj;
                    }
                    if(audioObject != null)
                        audioObject.Stop(flag ? 0.0f : 0.2f);
                }
            }
            return PlayAudioItem(audioItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, useExistingAudioObject, dspTime, playAsMusicOrAmbienceSound,
                                 startVolumeMultiplier);
        }

        public AudioObject PlayAudioItem(AudioItem sndItem, float vol, Vector3 worldPosition, Transform parentObj = null, float delay = 0.0f, float startTime = 0.0f,
                                         bool playWithoutAudioObject = false, AudioObject useExistingAudioObj = null, double dspTime = 0.0, bool playAsMusicOrAmbienceSound = false,
                                         float startVolumeMultiplier = 1f)
        {
            AudioObject audioObject1 = null;
            sndItem.LastPlayedTime = SystemTime;
            var audioSubItemArray = AudioControllerHelper._ChooseSubItems(sndItem, useExistingAudioObj);
            if(audioSubItemArray == null || audioSubItemArray.Length == 0)
                return null;
            foreach(var subItem in audioSubItemArray)
            {
                if(subItem != null)
                {
                    var audioObject2 = PlayAudioSubItem(subItem, vol, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, useExistingAudioObj, dspTime,
                                                        playAsMusicOrAmbienceSound, startVolumeMultiplier);
                    if((bool)audioObject2)
                    {
                        audioObject1 = audioObject2;
                        audioObject1.AudioID = sndItem.Name;
                        if(sndItem.OverrideAudioSourceSettings)
                        {
                            audioObject2.AudioSourceMinDistanceSaved = audioObject2.PrimaryAudioSource.minDistance;
                            audioObject2.AudioSourceMaxDistanceSaved = audioObject2.PrimaryAudioSource.maxDistance;
                            audioObject2.AudioSourceSpatialBlendSaved = audioObject2.PrimaryAudioSource.spatialBlend;
                            audioObject2.PrimaryAudioSource.minDistance = sndItem.AudioSourceMinDistance;
                            audioObject2.PrimaryAudioSource.maxDistance = sndItem.AudioSourceMaxDistance;
                            audioObject2.PrimaryAudioSource.spatialBlend = sndItem.SpatialBlend;
                            if(audioObject2.SecondaryAudioSource != null)
                            {
                                audioObject2.SecondaryAudioSource.minDistance = sndItem.AudioSourceMinDistance;
                                audioObject2.SecondaryAudioSource.maxDistance = sndItem.AudioSourceMaxDistance;
                                audioObject2.SecondaryAudioSource.spatialBlend = sndItem.SpatialBlend;
                            }
                        }
                    }
                }
            }
            return audioObject1;
        }

        internal AudioCategory _GetCategory(string categoryName) { return AudioCategories.FirstOrDefault(audioCategory => audioCategory.Name == categoryName); }

        [UsedImplicitly]
        private void Update()
        {
            if(isAdditionalTestAudioController)
                return;
            UpdateSystemTime();
        }

        private static void UpdateSystemTime()
        {
            var timeSinceLaunch = AudioSDK.SystemTime.TimeSinceLaunch;
            if(lastSystemTime >= 0.0)
            {
                SystemDeltaTime = timeSinceLaunch - lastSystemTime;
                if(SystemDeltaTime > Time.maximumDeltaTime + 0.00999999977648258)
                    SystemDeltaTime = Time.deltaTime;
                SystemTime += SystemDeltaTime;
            }
            else
            {
                SystemDeltaTime = 0.0;
                SystemTime = 0.0;
            }
            lastSystemTime = timeSinceLaunch;
        }

        protected virtual void Awake()
        {
            if(!Persistent)
                return;
            DontDestroyOnLoad(gameObject);
        }

        [UsedImplicitly]
        private void OnEnable()
        {
            if(!IsAdditionalAudioController)
                return;
            Instance.RegisterAdditionalTestAudioController(this);
        }

        [UsedImplicitly]
        private void OnDisable()
        {
            if(!IsAdditionalAudioController || !DoesInstanceExist())
                return;
            Instance.UnregisterAdditionalTestAudioController(this);
        }

        public virtual bool IsSingletonObject
        {
            get { return !isAdditionalTestAudioController; }
        }

        protected virtual void OnDestroy()
        {
            if(!UnloadAudioClipsOnDestroy)
                return;
            UnloadAllAudioClips();
        }

        private void AwakeSingleton()
        {
            UpdateSystemTime();
            if(AudioObjectPrefab == null)
                Debug.LogError("No AudioObject prefab specified in AudioController. To make your own AudioObject prefab create an empty game object, add Unity's AudioSource, the AudioObject script, and the PoolableObject script (if pooling is wanted ). Then create a prefab and set it in the AudioController.");
            else
                ValidateAudioObjectPrefab(AudioObjectPrefab);
            ValidateCategories();
            if(playlistPlayed == null)
            {
                playlistPlayed = new List<int>();
                isPlaylistPlaying = false;
            }
            SetDefaultCurrentPlaylist();
        }

        protected void ValidateCategories()
        {
            if(categoriesValidated)
                return;
            InitializeAudioItems();
            categoriesValidated = true;
        }

        protected void InvalidateCategories() { categoriesValidated = false; }

        public void InitializeAudioItems()
        {
            if(IsAdditionalAudioController)
                return;
            audioItems = new Dictionary<string, AudioItem>();
            InitializeAudioItems(this);
            if(additionalTestAudioControllers == null)
                return;
            foreach(var additionalTestAudioController in additionalTestAudioControllers)
            {
                if(additionalTestAudioController != null)
                    InitializeAudioItems(additionalTestAudioController);
            }
        }

        private void InitializeAudioItems(AudioController audioController)
        {
            foreach(var audioCategory in audioController.AudioCategories)
            {
                audioCategory.AudioController = audioController;
                audioCategory.AnalyseAudioItems(audioItems);
                if((bool)audioCategory.AudioObjectPrefab)
                    ValidateAudioObjectPrefab(audioCategory.AudioObjectPrefab);
            }
        }

        private void RegisterAdditionalTestAudioController(AudioController ac)
        {
            if(additionalTestAudioControllers == null)
                additionalTestAudioControllers = new List<AudioController>();
            additionalTestAudioControllers.Add(ac);
            InvalidateCategories();
            SyncCategoryVolumes(ac, this);
        }

        private void SyncCategoryVolumes(AudioController toSync, AudioController syncWith)
        {
            foreach(var audioCategory in toSync.AudioCategories)
            {
                var category = syncWith._GetCategory(audioCategory.Name);
                if(category != null)
                    audioCategory.Volume = category.Volume;
            }
        }

        private void UnregisterAdditionalTestAudioController(AudioController ac)
        {
            if(additionalTestAudioControllers != null)
            {
                for(var index = 0; index < additionalTestAudioControllers.Count; ++index)
                {
                    if(additionalTestAudioControllers[index] == ac)
                    {
                        additionalTestAudioControllers.RemoveAt(index);
                        InvalidateCategories();
                        break;
                    }
                }
            }
            else
                Debug.LogWarning("UnregisterAdditionalTestAudioController: TestAudioController " + ac.name + " not found");
        }

        private static List<AudioCategory> GetAllCategories(string name)
        {
            var instance = Instance;
            var audioCategoryList = new List<AudioCategory>();
            var category1 = instance._GetCategory(name);
            if(category1 != null)
                audioCategoryList.Add(category1);
            if(instance.additionalTestAudioControllers != null)
            {
                audioCategoryList.AddRange(instance.additionalTestAudioControllers.Select(c => c._GetCategory(name)).Where(category2 => category2 != null));
            }
            return audioCategoryList;
        }

        public AudioObject PlayAudioSubItem(AudioSubItem subItem, float vol, Vector3 worldPosition, Transform parentObj, float delay, float startTime,
                                            bool playWithoutAudioObject, AudioObject useExistingAudioObj, double dspTime = 0.0, bool playAsMusicOrAmbienceSound = false,
                                            float startVolumeMultiplier = 1f)
        {
            ValidateCategories();
            var audioItem = subItem.Item;
            switch(subItem.SubItemType)
            {
                case AudioSubItemType.Item:
                    if(subItem.ItemModeAudioID.Length != 0)
                        return _PlayAsSound(subItem.ItemModeAudioID, vol, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObj);
                    Debug.LogWarning("No item specified in audio sub-item with ITEM mode (audio item: '" + audioItem.Name + "')");
                    return null;
                default:
                    if(subItem.Clip == null)
                        return null;
                    var category = audioItem.Category;
                    var num = subItem.Volume * audioItem.Volume * vol;
                    if(subItem.RandomVolume != 0.0 || audioItem.LoopSequenceRandomVolume != 0.0)
                    {
                        var max = subItem.RandomVolume + audioItem.LoopSequenceRandomVolume;
                        num = Mathf.Clamp01(num + UnityEngine.Random.Range(-max, max));
                    }
                    var volume1 = num * category.VolumeTotal;
                    var audioController = GetTestAudioController(subItem);
                    if(!audioController.PlayWithZeroVolume && (volume1 <= 0.0 || Volume <= 0.0))
                        return null;
                    var gameObject1 = category.GetAudioObjectPrefab() ?? (!(audioController.AudioObjectPrefab != null) ? AudioObjectPrefab : audioController.AudioObjectPrefab);
                    if(playWithoutAudioObject)
                    {
                        gameObject1.GetComponent<AudioSource>().PlayOneShot(subItem.Clip, AudioObject.TransformVolume(volume1));
                        return null;
                    }
                    GameObject gameObject2;
                    AudioObject audioObject;
                    if(useExistingAudioObj == null)
                    {
                        if(audioItem.DestroyOnLoad)
                        {
                            gameObject2 = (GameObject)Instantiate((UnityEngine.Object)gameObject1, worldPosition, Quaternion.identity);
                        }
                        else
                        {
                            gameObject2 = (GameObject)Instantiate((UnityEngine.Object)gameObject1, worldPosition, Quaternion.identity);
                            DontDestroyOnLoad(gameObject2);
                        }
                        if((bool)parentObj)
                            gameObject2.transform.parent = parentObj;
                        audioObject = gameObject2.gameObject.GetComponent<AudioObject>();
                    }
                    else
                    {
                        gameObject2 = useExistingAudioObj.gameObject;
                        audioObject = useExistingAudioObj;
                    }
                    audioObject.SubItem = subItem;
                    if(useExistingAudioObj == null)
                        audioObject.LastChosenSubItemIndex = audioItem.LastChosen;
                    audioObject.PrimaryAudioSource.clip = subItem.Clip;
                    gameObject2.name = "AudioObject:" + audioObject.PrimaryAudioSource.clip.name;
                    audioObject.PrimaryAudioSource.pitch = AudioObject.TransformPitch(subItem.PitchShift);
                    audioObject.PrimaryAudioSource.panStereo = subItem.Pan2D;
                    if(subItem.RandomStartPosition)
                        startTime = UnityEngine.Random.Range(0.0f, audioObject.ClipLength);
                    audioObject.PrimaryAudioSource.time = startTime + subItem.ClipStartTime;
                    audioObject.PrimaryAudioSource.loop = audioItem.Loop == AudioItem.LoopMode.LoopSubitem ||
                                                          audioItem.Loop == (AudioItem.LoopMode.LoopSubitem | AudioItem.LoopMode.LoopSequence);
                    audioObject.VolumeExcludingCategory = num;
                    audioObject.VolumeFromScriptCall = vol;
                    audioObject.Category = category;
                    audioObject.IsPlayedAsMusicOrAmbienceSound = playAsMusicOrAmbienceSound;
                    if(subItem.FadeIn > 0.0)
                        audioObject.FadeIn(subItem.FadeIn);
                    audioObject._ApplyVolumePrimary(startVolumeMultiplier);
                    if((bool)category.GetAudioMixerGroup())
                        audioObject.PrimaryAudioSource.outputAudioMixerGroup = category.AudioMixerGroup;
                    if(subItem.RandomPitch != 0.0 || audioItem.LoopSequenceRandomPitch != 0.0)
                    {
                        var max = subItem.RandomPitch + audioItem.LoopSequenceRandomPitch;
                        audioObject.PrimaryAudioSource.pitch *= AudioObject.TransformPitch(UnityEngine.Random.Range(-max, max));
                    }
                    if(subItem.RandomDelay > 0.0)
                        delay += UnityEngine.Random.Range(0.0f, subItem.RandomDelay);
                    if(dspTime > 0.0)
                        audioObject.PlayScheduled(dspTime + delay + subItem.Delay + audioItem.Delay);
                    else
                        audioObject.Play(delay + subItem.Delay + audioItem.Delay);
                    if(subItem.FadeIn > 0.0)
                        audioObject.FadeIn(subItem.FadeIn);
                    return audioObject;
            }
        }

        private AudioController GetTestAudioController(AudioSubItem subItem)
        {
            if(subItem.Item != null && subItem.Item.Category != null)
                return subItem.Item.Category.AudioController;
            return this;
        }

        internal void NotifyPlaylistTrackCompleteleyPlayed(AudioObject audioObject)
        {
            audioObject.IsCurrentPlaylistTrack = false;
            if(!IsPlaylistPlaying() || !(CurrentMusic == audioObject) || !(_PlayNextMusicOnPlaylist(DelayBetweenPlaylistTracks) == null))
                return;
            isPlaylistPlaying = false;
        }

        private void ValidateAudioObjectPrefab(GameObject audioPrefab)
        {
            if(UsePooledAudioObjects)
                Debug.LogWarning("Poolable Audio objects not supported by the Audio Toolkit Demo version");
            if(!(audioPrefab.GetComponent<AudioObject>() == null))
                return;
            Debug.LogError("AudioObject prefab must have the AudioObject script component!");
        }

        public AudioController() { SetSingletonType(typeof(AudioController)); }

        static AudioController() { SystemDeltaTime = -1.0; }

        public void OnBeforeSerialize() { }

        private void SetDefaultCurrentPlaylist()
        {
            if(MusicPlaylists == null || MusicPlaylists.Length < 1 || MusicPlaylists[0] == null)
                return;
            currentPlaylistName = MusicPlaylists[0].Name;
        }
    }
}
