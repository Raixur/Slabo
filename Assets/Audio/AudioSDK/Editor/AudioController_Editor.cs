using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioSDK.Editor
{
    [CustomEditor(typeof(AudioController))]
    public class AudioController_Editor : EditorEx
    {
        public static bool GlobalFoldout = true;
        public static bool PlaylistFoldout = true;
        public static bool MusicFoldout = true;
        public static bool CategoryFoldout = true;
        public static bool ItemFoldout = true;
        public static bool SubitemFoldout = true;

        private static AudioItem clipBoardItem;
        private int lastCategoryIndex = -1;
        private int lastItemIndex = -1;
        private int lastSubItemIndex = -1;
        private AudioController ac;
        private GUIStyle foldoutStyle;
        private GUIStyle popupStyleColored;
        private GUIStyle styleChooseItem;
        private GUIStyle textAttentionStyle;
        private GUIStyle boxStyle;

        private int CurrentCategoryIndex
        {
            get { return ac.CurrentInspectorSelection.CurrentCategoryIndex; }
            set { ac.CurrentInspectorSelection.CurrentCategoryIndex = value; }
        }

        private int CurrentItemIndex
        {
            get { return ac.CurrentInspectorSelection.CurrentItemIndex; }
            set { ac.CurrentInspectorSelection.CurrentItemIndex = value; }
        }

        private int CurrentSubitemIndex
        {
            get { return ac.CurrentInspectorSelection.CurrentSubitemIndex; }
            set { ac.CurrentInspectorSelection.CurrentSubitemIndex = value; }
        }

        private int CurrentPlaylistEntryIndex
        {
            get { return ac.CurrentInspectorSelection.CurrentPlaylistEntryIndex; }
            set { ac.CurrentInspectorSelection.CurrentPlaylistEntryIndex = value; }
        }

        private int CurrentPlaylistIndex
        {
            get { return ac.CurrentInspectorSelection.CurrentPlaylistIndex; }
            set { ac.CurrentInspectorSelection.CurrentPlaylistIndex = value; }
        }

        private AudioCategory CurrentCategory
        {
            get
            {
                if(CurrentCategoryIndex < 0 || ac.AudioCategories == null || CurrentCategoryIndex >= ac.AudioCategories.Length)
                    return null;
                return ac.AudioCategories[CurrentCategoryIndex];
            }
        }

        private AudioItem CurrentItem
        {
            get
            {
                var currentCategory = CurrentCategory;
                if(CurrentCategory == null)
                    return null;
                if(CurrentItemIndex < 0 || currentCategory.AudioItems == null || CurrentItemIndex >= currentCategory.AudioItems.Length)
                    return null;
                return CurrentCategory.AudioItems[CurrentItemIndex];
            }
        }

        private AudioSubItem CurrentSubItem
        {
            get
            {
                var currentItem = CurrentItem;
                if(currentItem == null)
                    return null;
                if(CurrentSubitemIndex < 0 || currentItem.SubItems == null || CurrentSubitemIndex >= currentItem.SubItems.Length)
                    return null;
                return currentItem.SubItems[CurrentSubitemIndex];
            }
        }

        public int CurrentCategoryCount
        {
            get
            {
                return ac.AudioCategories != null ? ac.AudioCategories.Length : 0;
            }
        }

        public int CurrentItemCount
        {
            get
            {
                if(CurrentCategory != null && CurrentCategory.AudioItems != null)
                    return CurrentCategory.AudioItems.Length;
                return 0;
            }
        }

        public int CurrentSubItemCount
        {
            get { return CurrentItem != null && CurrentItem.SubItems != null ? CurrentItem.SubItems.Length : 0; }
        }

        protected override void LogUndo(string label) { Undo.RecordObject(ac, "AudioToolkit: " + label); }

        public new void SetStyles()
        {
            base.SetStyles();
            foldoutStyle = new GUIStyle(EditorStyles.foldout);
            var color = new Color(0.0f, 0.0f, 0.2f);
            foldoutStyle.onNormal.background = EditorStyles.boldLabel.onNormal.background;
            foldoutStyle.onFocused.background = EditorStyles.boldLabel.onNormal.background;
            foldoutStyle.onActive.background = EditorStyles.boldLabel.onNormal.background;
            foldoutStyle.onHover.background = EditorStyles.boldLabel.onNormal.background;
            foldoutStyle.normal.textColor = color;
            foldoutStyle.focused.textColor = color;
            foldoutStyle.active.textColor = color;
            foldoutStyle.hover.textColor = color;
            foldoutStyle.fixedWidth = 1000f;

            popupStyleColored = new GUIStyle(StylePopup);
            styleChooseItem = new GUIStyle(StylePopup);
            StyleFloatIndi = new GUIStyle(StyleFloat);
            var num = popupStyleColored.normal.textColor.grayscale > 0.5 ? 1 : 0;
            boxStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            popupStyleColored.normal.textColor = num == 0 ? new Color(0.6f, 0.1f, 0.0f) : new Color(0.9f, 0.9f, 0.5f);
            if(num != 0)
            {
                StyleFloatIndi.normal.textColor = new Color(0.9f, 0.9f, 0.5f);
                StyleFloatIndi.focused.textColor = new Color(0.9f, 0.9f, 0.5f);
            }
            else
            {
                StyleFloatIndi.normal.textColor = new Color(0.6f, 0.1f, 0.0f);
                StyleFloatIndi.focused.textColor = new Color(0.6f, 0.1f, 0.0f);
            }
            textAttentionStyle = new GUIStyle(EditorStyles.textField) { normal = { textColor = num == 0 ? new Color(1f, 0.0f, 0.0f) : new Color(1f, 0.3f, 0.3f) } };
        }

        public override void OnInspectorGUI()
        {
            SetStyles();
            BeginInspectorGUI();
            var current = Event.current;
            ac = (AudioController)target;
            _ValidateCurrentCategoryIndex();
            _ValidateCurrentItemIndex();
            _ValidateCurrentSubItemIndex();
            if(lastCategoryIndex != CurrentCategoryIndex || lastItemIndex != CurrentItemIndex || lastSubItemIndex != CurrentSubitemIndex)
            {
                GUIUtility.keyboardControl = 0;
                lastCategoryIndex = CurrentCategoryIndex;
                lastItemIndex = CurrentItemIndex;
                lastSubItemIndex = CurrentSubitemIndex;
            }
            EditorGUILayout.Space();
            if(GlobalFoldout = EditorGUILayout.Foldout(GlobalFoldout, "Global Audio Settings", foldoutStyle))
            {
                var additionalAudioController = ac.IsAdditionalAudioController;
                if(EditBool(ref additionalAudioController, "Additional Audio Controller",
                            "A scene can contain multiple AudioControllers. All but the main AudioController must be marked as 'additional'."))
                    ac.IsAdditionalAudioController = additionalAudioController;
                EditBool(ref ac.Persistent, "Persist Scene Loading", "A non-persisting AudioController will get destroyed when loading the next scene.");
                EditBool(ref ac.UnloadAudioClipsOnDestroy, "Unload Audio On Destroy",
                         "This option will unload all AudioClips from memory which are referenced by this AudioController if the controller gets destroyed (e.g. when loading a new scene and the AudioController is not persistent). \n\nUse this option in combination with additional none-persistent AudioControllers to keep only those audios in memory that are used by the current scene. Use the primary persistent AudioController for all global audio that is used throughout all scenes. \n\nWarning: Due to a bug in Unity5 unloading currently only works for audio clips with the 'Preload Audio Data' option disabled");
                var disableAudio = ac.DisableAudio;
                if(EditBool(ref disableAudio, "Disable Audio", "Disables all audio"))
                {
                    ac.DisableAudio = disableAudio;
                    if(disableAudio && AudioController.DoesInstanceExist())
                        AudioController.StopAll();
                }
                var volume = ac.Volume;
                EditFloat01(ref volume, "Volume", "%");
                ac.Volume = volume;
                EditPrefab(ref ac.AudioObjectPrefab, "Audio Object Prefab",
                           "You must specify a prefab here that will get instantiated for each played audio. This prefab must contain the following components: AudioSource, AudioObject, PoolableObject.");
                EditBool(ref ac.UsePooledAudioObjects, "Use Pooled AudioObjects",
                         "Pooling increases performance when playing many audio files. Strongly recommended particularly on mobile platforms.");
                EditBool(ref ac.PlayWithZeroVolume, "Play With Zero Volume", "If disabled Play() calls with a volume of zero will not create an AudioObject.");
                EditBool(ref ac.EqualPowerCrossfade, "Equal-power crossfade", "Unfortunatly not 100% correct due to unknown volume formulas used by Unity");
            }
            VerticalSpace();
            if(MusicFoldout = EditorGUILayout.Foldout(MusicFoldout, "Music / Ambience Settings", foldoutStyle))
            {
                EditBool(ref ac.SpecifyCrossFadeInAndOutSeperately, "Separate crossfade in/out",
                         "Allows to specify a separate fade-in and out value for all music and ambience sounds");
                if(ac.SpecifyCrossFadeInAndOutSeperately)
                {
                    var f = ac.MusicCrossFadeTimeIn;
                    EditFloat(ref f, "   Music Crossfade-in Time", "sec");
                    ac.MusicCrossFadeTimeIn = f;
                    var crossFadeTimeOut1 = ac.MusicCrossFadeTimeOut;
                    EditFloat(ref crossFadeTimeOut1, "   Music Crossfade-out Time", "sec");
                    ac.MusicCrossFadeTimeOut = crossFadeTimeOut1;
                    f = ac.AmbienceSoundCrossFadeTimeIn;
                    EditFloat(ref f, "   Ambience Crossfade-in Time", "sec");
                    ac.AmbienceSoundCrossFadeTimeIn = f;
                    var crossFadeTimeOut2 = ac.AmbienceSoundCrossFadeTimeOut;
                    EditFloat(ref crossFadeTimeOut2, "   Ambience Crossfade-out Time", "sec");
                    ac.AmbienceSoundCrossFadeTimeOut = crossFadeTimeOut2;
                }
                else
                {
                    EditFloat(ref ac.MusicCrossFadeTime, "Music Crossfade Time", "sec");
                    EditFloat(ref ac.AmbienceSoundCrossFadeTime, "Ambience Crossfade Time", "sec");
                }
            }
            VerticalSpace();
            if(PlaylistFoldout = EditorGUILayout.Foldout(PlaylistFoldout, "Playlist Settings", foldoutStyle))
            {
                EditorGUILayout.BeginHorizontal();
                var playlistNames = GetPlaylistNames();
                CurrentPlaylistIndex = PopupWithStyle("Playlist", CurrentPlaylistIndex, playlistNames, popupStyleColored, "List of playlists, click on '+' to add a new playlist",
                                                      false);
                if(GUILayout.Button("+", GUILayout.Width(25f)) && ac.MusicPlaylists != null && ac.MusicPlaylists.Length != 0)
                {
                    ArrayHelper.AddArrayElement(ref ac.MusicPlaylists, new Playlist
                    {
                        Name = "!!! Enter Unique Playlist Name here !!!"
                    });
                    CurrentPlaylistIndex = ac.MusicPlaylists.Length - 1;
                    KeepChanges();
                }
                if(GUILayout.Button("-", GUILayout.Width(25f)) && ac.MusicPlaylists != null && ac.MusicPlaylists.Length != 0)
                    if(ac.MusicPlaylists.Length > 1)
                    {
                        ArrayHelper.DeleteArrayElement(ref ac.MusicPlaylists, CurrentPlaylistIndex);
                        CurrentPlaylistIndex = Mathf.Clamp(CurrentPlaylistIndex - 1, 0, ac.MusicPlaylists.Length - 1);
                        KeepChanges();
                    }
                    else
                    {
                        ac.MusicPlaylists[0] = new Playlist();
                    }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditString(ref ac.MusicPlaylists[CurrentPlaylistIndex].Name, "Playlist Name",
                           ac.MusicPlaylists[CurrentPlaylistIndex].Name == "!!! Enter Unique Playlist Name here !!!" ? textAttentionStyle : null);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                var playlistEntryNames = GetPlaylistEntryNames();
                CurrentPlaylistEntryIndex = Popup("Playlist Entry", CurrentPlaylistEntryIndex, playlistEntryNames,
                                                  "List of audioIDs, click on 'add to playlist' to add audio items", false);
                GUI.enabled = (uint)playlistEntryNames.Length > 0U;
                if(GUILayout.Button("Up", GUILayout.Width(35f)) && ac.MusicPlaylists != null && ac.MusicPlaylists.Length != 0 &&
                   SwapArrayElements(ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems, CurrentPlaylistEntryIndex, CurrentPlaylistEntryIndex - 1))
                {
                    CurrentPlaylistEntryIndex = CurrentPlaylistEntryIndex - 1;
                    KeepChanges();
                }
                if(GUILayout.Button("Dwn", GUILayout.Width(40f)) && ac.MusicPlaylists != null && ac.MusicPlaylists.Length != 0 &&
                   SwapArrayElements(ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems, CurrentPlaylistEntryIndex, CurrentPlaylistEntryIndex + 1))
                {
                    CurrentPlaylistEntryIndex = CurrentPlaylistEntryIndex + 1;
                    KeepChanges();
                }
                if(GUILayout.Button("-", GUILayout.Width(25f)) && ac.MusicPlaylists != null && ac.MusicPlaylists.Length != 0)
                {
                    ArrayHelper.DeleteArrayElement(ref ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems, CurrentPlaylistEntryIndex);
                    CurrentPlaylistEntryIndex = Mathf.Clamp(CurrentPlaylistEntryIndex - 1, 0, ac.MusicPlaylists.Length - 1);
                    KeepChanges();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                var entryName = _ChooseItem("Add to Playlist");
                if(!string.IsNullOrEmpty(entryName))
                    AddToPlayList(playlistNames[CurrentPlaylistIndex], entryName);
                EditBool(ref ac.LoopPlaylist, "Loop Playlist");
                EditBool(ref ac.ShufflePlaylist, "Shuffle Playlist",
                         "Enables random playback of music playlists. Takes care that the same audio will not get played again too early");
                EditBool(ref ac.CrossfadePlaylist, "Crossfade Playlist");
                EditFloat(ref ac.DelayBetweenPlaylistTracks, "Delay Betw. Playlist Tracks", "sec");
            }
            VerticalSpace();
            var num1 = ac.AudioCategories != null ? ac.AudioCategories.Length : 0;
            CurrentCategoryIndex = Mathf.Clamp(CurrentCategoryIndex, 0, num1 - 1);
            if(CategoryFoldout = EditorGUILayout.Foldout(CategoryFoldout, "Category Settings", foldoutStyle))
            {
                EditorGUILayout.BeginHorizontal();
                var flag1 = false;
                var index1 = PopupWithStyle("Category", CurrentCategoryIndex, GetCategoryNames(), popupStyleColored);
                if(GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    var flag2 = false;
                    if(num1 > 0)
                        flag2 = ac.AudioCategories[CurrentCategoryIndex].Name == "!!! Enter Unique Category Name Here !!!";
                    if(!flag2)
                    {
                        index1 = ac.AudioCategories != null ? ac.AudioCategories.Length : 0;
                        ArrayHelper.AddArrayElement(ref ac.AudioCategories, new AudioCategory(ac));
                        ac.AudioCategories[index1].Name = "!!! Enter Unique Category Name Here !!!";
                        flag1 = true;
                        KeepChanges();
                    }
                }
                if(GUILayout.Button("-", GUILayout.Width(30f)) && num1 > 0)
                {
                    index1 = CurrentCategoryIndex >= ac.AudioCategories.Length - 1 ? Mathf.Max(CurrentCategoryIndex - 1, 0) : CurrentCategoryIndex;
                    ArrayHelper.DeleteArrayElement(ref ac.AudioCategories, CurrentCategoryIndex);
                    KeepChanges();
                }
                EditorGUILayout.EndHorizontal();
                if(index1 != CurrentCategoryIndex)
                {
                    CurrentCategoryIndex = index1;
                    CurrentItemIndex = 0;
                    CurrentSubitemIndex = 0;
                    _ValidateCurrentItemIndex();
                    _ValidateCurrentSubItemIndex();
                }
                var currentCategory1 = CurrentCategory;
                if(currentCategory1 != null)
                {
                    if(currentCategory1.AudioController == null)
                        currentCategory1.AudioController = ac;
                    if(flag1)
                        SetFocusForNextEditableField();
                    EditString(ref currentCategory1.Name, "Name", currentCategory1.Name == "!!! Enter Unique Category Name Here !!!" ? textAttentionStyle : null);
                    var volume = currentCategory1.Volume;
                    EditFloat01(ref volume, "Volume", " %");
                    currentCategory1.Volume = volume;
                    EditPrefab(ref currentCategory1.AudioObjectPrefab, "Audio Object Prefab Override",
                               "Use different Audio Object prefabs if you want to specify different parameters such as the volume rolloff etc. per category");
                    EditPrefab(ref currentCategory1.AudioMixerGroup, "Audio Mixer Group", "You can specify a Unity 5 Audio Mixer Group here");
                    int selectedParentCategoryIndex;
                    var listIncludingNone = _GenerateCategoryListIncludingNone(out selectedParentCategoryIndex, currentCategory1.ParentCategory);
                    var index2 = Popup("Parent Category", selectedParentCategoryIndex, listIncludingNone,
                                       "The effective volume of a category is multiplied with the volume of the parent category.");
                    if(index2 != selectedParentCategoryIndex)
                    {
                        KeepChanges();
                        currentCategory1.ParentCategory = index2 > 0 ? _GetCategory(listIncludingNone[index2]) : null;
                    }
                    var currentItemCount = CurrentItemCount;
                    _ValidateCurrentItemIndex();
                    EditorGUILayout.BeginHorizontal();
                    if(GUILayout.Button("Copy current AudioItem"))
                        clipBoardItem = new AudioItem(CurrentItem);
                    if(GUILayout.Button("Paste AudioItem"))
                    {
                        if(clipBoardItem == null)
                            return;
                        var elToAdd = new AudioItem(clipBoardItem);
                        if(currentCategory1.AudioItems != null)
                        {
                            var flag2 = true;
                            while(flag2)
                            {
                                flag2 = false;
                                foreach(var audioItem in currentCategory1.AudioItems)
                                    if(audioItem.Name == elToAdd.Name)
                                    {
                                        elToAdd.Name = audioItem.Name + " Copy";
                                        flag2 = true;
                                    }
                            }
                        }
                        ArrayHelper.AddArrayElement(ref currentCategory1.AudioItems, elToAdd);
                        CurrentItemIndex = currentCategory1.AudioItems.Length - 1;
                    }
                    EditorGUILayout.EndHorizontal();
                    VerticalSpace();
                    if(ItemFoldout = EditorGUILayout.Foldout(ItemFoldout, "Audio Item Settings", foldoutStyle))
                    {
                        EditorGUILayout.BeginHorizontal();
                        if(GUILayout.Button("Add selected audio clips", EditorStyles.miniButton))
                        {
                            var selectedAudioclips = GetSelectedAudioclips();
                            if(selectedAudioclips.Length != 0)
                            {
                                var num2 = currentItemCount;
                                CurrentItemIndex = num2;
                                foreach(var audioClip in selectedAudioclips)
                                {
                                    ArrayHelper.AddArrayElement(ref currentCategory1.AudioItems);
                                    var audioItem = currentCategory1.AudioItems[CurrentItemIndex];
                                    var name = audioClip.name;
                                    audioItem.Name = name;
                                    ArrayHelper.AddArrayElement(ref audioItem.SubItems).Clip = audioClip;
                                    CurrentItemIndex = CurrentItemIndex + 1;
                                }
                                CurrentItemIndex = num2;
                                KeepChanges();
                            }
                        }
                        GUILayout.Label("use inspector lock!");
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        var index3 = PopupWithStyle("Item", CurrentItemIndex, GetItemNames(), popupStyleColored);
                        var flag2 = false;
                        if(GUILayout.Button("+", GUILayout.Width(30f)))
                        {
                            var flag3 = false;
                            if(currentItemCount > 0)
                                flag3 = currentCategory1.AudioItems[CurrentItemIndex].Name == "!!! Enter Unique Audio ID Here !!!";
                            if(!flag3)
                            {
                                index3 = currentCategory1.AudioItems != null ? currentCategory1.AudioItems.Length : 0;
                                ArrayHelper.AddArrayElement(ref currentCategory1.AudioItems);
                                currentCategory1.AudioItems[index3].Name = "!!! Enter Unique Audio ID Here !!!";
                                flag2 = true;
                                KeepChanges();
                            }
                        }
                        if(GUILayout.Button("-", GUILayout.Width(30f)) && currentItemCount > 0)
                        {
                            index3 = CurrentItemIndex >= currentCategory1.AudioItems.Length - 1 ? Mathf.Max(CurrentItemIndex - 1, 0) : CurrentItemIndex;
                            ArrayHelper.DeleteArrayElement(ref currentCategory1.AudioItems, CurrentItemIndex);
                            KeepChanges();
                        }
                        if(index3 != CurrentItemIndex)
                        {
                            CurrentItemIndex = index3;
                            CurrentSubitemIndex = 0;
                            _ValidateCurrentSubItemIndex();
                        }
                        var currentItem2 = CurrentItem;
                        EditorGUILayout.EndHorizontal();
                        if(currentItem2 != null)
                        {
                            GUILayout.BeginHorizontal();
                            if(flag2)
                                SetFocusForNextEditableField();
                            var flag3 = currentItem2.Name == "!!! Enter Unique Audio ID Here !!!";
                            var name = currentItem2.Name;
                            if(EditString(ref currentItem2.Name, "Name", flag3 ? textAttentionStyle : null,
                                          "You must specify a unique name here (=audioID). This is the ID used in the script code to play this audio item.") && !flag3)
                                _RenamePlaylistEntries(name, currentItem2.Name);
                            GUILayout.EndHorizontal();
                            var index4 = Popup("Move to Category", CurrentCategoryIndex, GetCategoryNames());
                            if(index4 != CurrentCategoryIndex)
                            {
                                var audioCategory = ac.AudioCategories[index4];
                                var currentCategory2 = CurrentCategory;
                                ArrayHelper.AddArrayElement(ref audioCategory.AudioItems, currentItem2);
                                ArrayHelper.DeleteArrayElement(ref currentCategory2.AudioItems, CurrentItemIndex);
                                CurrentCategoryIndex = index4;
                                KeepChanges();
                                ac.InitializeAudioItems();
                                CurrentItemIndex = audioCategory.AudioItems.Length - 1;
                            }
                            if(EditFloat01(ref currentItem2.Volume, "Volume", " %"))
                                _AdjustVolumeOfAllAudioItems(currentItem2, null);
                            EditFloat(ref currentItem2.Delay, "Delay", "sec", "Delays the playback");
                            if(EditFloat01(ref currentItem2.RandomVolume, "Random Volume", "±%"))
                                foreach(var subItem in currentItem2.SubItems)
                                    if(!subItem.IndividualSettings.Contains("RandomVolume"))
                                        subItem.RandomVolume = currentItem2.RandomVolume;
                            if(EditFloat(ref currentItem2.RandomPitch, "Random Pitch", "±semitone"))
                                foreach(var subItem in currentItem2.SubItems)
                                    if(!subItem.IndividualSettings.Contains("RandomPitch"))
                                        subItem.RandomPitch = currentItem2.RandomPitch;
                            if(EditFloat(ref currentItem2.RandomDelay, "Random Delay", "sec"))
                                foreach(var subItem in currentItem2.SubItems)
                                    if(!subItem.IndividualSettings.Contains("RandomDelay"))
                                        subItem.RandomDelay = currentItem2.RandomDelay;
                            EditFloat(ref currentItem2.MinTimeBetweenPlayCalls, "Min Time Between Play", "sec",
                                      "If the same audio item gets played multiple times within this time frame the playback is skipped. This can prevent unwanted audio artifacts.");
                            EditInt(ref currentItem2.MaxInstanceCount, "Max Instance Count", "",
                                    "Sets the maximum number of simultaneously playing audio files of this particular audio item. If the maximum number would be exceeded, the oldest playing audio gets stopped.");
                            EditBool(ref currentItem2.DestroyOnLoad, "Stop When Scene Loads",
                                     "If disabled, this audio item will continue playing even if a different scene is loaded.");
                            if(currentItem2.Loop == (AudioItem.LoopMode.LoopSubitem | AudioItem.LoopMode.LoopSequence))
                            {
                                currentItem2.Loop = AudioItem.LoopMode.LoopSequence;
                                KeepChanges();
                            }
                            currentItem2.Loop = (AudioItem.LoopMode)EnumPopup("Loop Mode", currentItem2.Loop,
                                                                              "The Loop mode determines how the audio subitems are looped. \n'LoopSubitem' means that the chosen sub-item will loop. \n'LoopSequence' means that one subitem is played after the other. In which order the subitems are chosen depends on the subitem pick mode.");
                            if(currentItem2.Loop == AudioItem.LoopMode.LoopSequence || currentItem2.Loop == AudioItem.LoopMode.PlaySequenceAndLoopLast ||
                               currentItem2.Loop == AudioItem.LoopMode.IntroLoopOutroSequence)
                            {
                                EditInt(ref currentItem2.LoopSequenceCount, "   Stop after subitems", "",
                                        "Playing will stop after this number of different subitems were played. Specify zero to play endlessly in LoopSequence mode or play all sub-items in <c>PlaySequenceAndLoopLast</c> and <c>IntroLoopOutroSequence</c> mode");
                                EditFloat(ref currentItem2.LoopSequenceOverlap, "   Overlap", "sec",
                                          "Positive values mean that subitems will play overlapping, negative values mean that a delay is inserted before playing the next subitem in the 'LoopSequence'.");
                                EditFloat(ref currentItem2.LoopSequenceRandomDelay, "   Random Delay", "sec",
                                          "A random delay between 0 and this value will be added between two subsequent subitems. Can be combined with the 'Overlap' value.");
                                EditFloat01(ref currentItem2.LoopSequenceRandomVolume, "   Random Volume", "±%",
                                            "A random volume value % will be added to each subitem played in the 'LoopSequence'. Will be combined with subitem random volume value.");
                                EditFloat(ref currentItem2.LoopSequenceRandomPitch, "   Random Pitch", "±semitone",
                                          "A random pitch between 0 and this value will be added to each subitem played in the 'LoopSequence'. Will be combined with subitem random pitch value.");
                            }
                            EditBool(ref currentItem2.OverrideAudioSourceSettings, "Override AudioSource Settings");
                            if(currentItem2.OverrideAudioSourceSettings)
                            {
                                EditFloat01(ref currentItem2.SpatialBlend, "   Spatial Blend", "%", "0% = 2D  100% = 3D");
                                EditFloat(ref currentItem2.AudioSourceMinDistance, "   Min Distance", "",
                                          "Overrides the 'Min Distance' parameter in the AudioSource settings of the AudioObject prefab (for 3d sounds)");
                                EditFloat(ref currentItem2.AudioSourceMaxDistance, "   Max Distance", "",
                                          "Overrides the 'Max Distance' parameter in the AudioSource settings of the AudioObject prefab (for 3d sounds)");
                            }
                            if(currentItem2.Loop == AudioItem.LoopMode.IntroLoopOutroSequence)
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                currentItem2.SubItemPickMode = AudioPickSubItemMode.StartLoopSequenceWithFirst;
                            }
                            currentItem2.SubItemPickMode = (AudioPickSubItemMode)EnumPopup("Pick Subitem Mode", currentItem2.SubItemPickMode,
                                                                                           "Determines which subitem is chosen when the audio item is played.");
                            if(currentItem2.Loop == AudioItem.LoopMode.IntroLoopOutroSequence)
                                EditorGUI.EndDisabledGroup();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("");
                            GUI.enabled = currentItem2 != null && currentItem2.Loop == AudioItem.LoopMode.DoNotLoop;
                            if(GUILayout.Button("Play", GUILayout.Width(60f)) && currentItem2 != null)
                                if(_IsAudioControllerInPlayMode())
                                    AudioController.Play(currentItem2.Name);
                                else if(Application.platform == RuntimePlatform.OSXEditor)
                                    Debug.Log("On MacOS playing audios is only supported during play mode.");
                                else
                                    PreviewAudioItem(currentItem2);
                            GUI.enabled = true;
                            EditorGUILayout.EndHorizontal();
                            VerticalSpace();
                            var subItemCount = currentItem2.SubItems != null ? currentItem2.SubItems.Length : 0;
                            CurrentSubitemIndex = Mathf.Clamp(CurrentSubitemIndex, 0, subItemCount - 1);
                            var currentSubItem1 = CurrentSubItem;
                            if(SubitemFoldout = EditorGUILayout.Foldout(SubitemFoldout, "Audio Sub-Item Settings", foldoutStyle))
                            {
                                EditorGUILayout.BeginHorizontal();
                                var rect = GUILayoutUtility.GetRect(0.0f, 30f, GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                                GUI.Label(rect, "Drop AudioClips here, use inspector lock!", boxStyle);
                                switch(current.type)
                                {
                                    case EventType.DragUpdated:
                                    case EventType.DragPerform:
                                        if(rect.Contains(current.mousePosition))
                                        {
                                            var flag4 = DragAndDrop.objectReferences.All(objectReference => objectReference as AudioClip != null);
                                            DragAndDrop.visualMode = !flag4 ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Copy;
                                            if((current.type == EventType.DragPerform) & flag4)
                                            {
                                                DragAndDrop.AcceptDrag();
                                                var num2 = subItemCount;
                                                CurrentSubitemIndex = num2;
                                                foreach(AudioClip objectReference in DragAndDrop.objectReferences)
                                                {
                                                    ArrayHelper.AddArrayElement(ref currentItem2.SubItems).Clip = objectReference;
                                                    CurrentSubitemIndex = CurrentSubitemIndex + 1;
                                                }
                                                CurrentSubitemIndex = num2;
                                                KeepChanges();
                                                current.Use();
                                            }
                                        }
                                        break;
                                }
                                if(GUILayout.Button("Add selected audio clips", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
                                {
                                    var selectedAudioclips = GetSelectedAudioclips();
                                    if(selectedAudioclips.Length != 0)
                                    {
                                        var num2 = subItemCount;
                                        CurrentSubitemIndex = num2;
                                        foreach(var audioClip in selectedAudioclips)
                                        {
                                            ArrayHelper.AddArrayElement(ref currentItem2.SubItems).Clip = audioClip;
                                            CurrentSubitemIndex = CurrentSubitemIndex + 1;
                                        }
                                        CurrentSubitemIndex = num2;
                                        KeepChanges();
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                CurrentSubitemIndex = PopupWithStyle("SubItem", CurrentSubitemIndex, GetSubitemNames(), popupStyleColored);
                                if(GUILayout.Button("+", GUILayout.Width(30f)))
                                {
                                    var flag4 = false;
                                    var audioSubItemType = AudioSubItemType.Clip;
                                    if(subItemCount > 0)
                                    {
                                        audioSubItemType = currentItem2.SubItems[CurrentSubitemIndex].SubItemType;
                                        switch(audioSubItemType)
                                        {
                                            case AudioSubItemType.Clip:
                                                flag4 = currentItem2.SubItems[CurrentSubitemIndex].Clip == null;
                                                break;
                                            case AudioSubItemType.Item:
                                                flag4 = string.IsNullOrEmpty(currentItem2.SubItems[CurrentSubitemIndex].ItemModeAudioID);
                                                break;
                                        }
                                    }
                                    if(!flag4)
                                    {
                                        CurrentSubitemIndex = subItemCount;
                                        ArrayHelper.AddArrayElement(ref currentItem2.SubItems);
                                        currentItem2.SubItems[CurrentSubitemIndex].SubItemType = audioSubItemType;
                                        KeepChanges();
                                    }
                                }
                                if(GUILayout.Button("-", GUILayout.Width(30f)) && subItemCount > 0)
                                {
                                    ArrayHelper.DeleteArrayElement(ref currentItem2.SubItems, CurrentSubitemIndex);
                                    if(CurrentSubitemIndex >= currentItem2.SubItems.Length)
                                        CurrentSubitemIndex = Mathf.Max(currentItem2.SubItems.Length - 1, 0);
                                    KeepChanges();
                                }
                                EditorGUILayout.EndHorizontal();
                                var currentSubItem2 = CurrentSubItem;
                                if(currentSubItem2 != null)
                                {
                                    _SubitemTypePopup(currentSubItem2);
                                    if(currentSubItem2.SubItemType == AudioSubItemType.Item)
                                        _DisplaySubItem_Item(currentSubItem2);
                                    else
                                        _DisplaySubItem_Clip(currentSubItem2, subItemCount, currentItem2);
                                }
                            }
                        }
                    }
                }
            }
            VerticalSpace();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Show Item Overview"))
                (EditorWindow.GetWindow(typeof(AudioItemOverview)) as AudioItemOverview).Show(ac);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if(EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Stop All Sounds") && EditorApplication.isPlaying && AudioController.DoesInstanceExist())
                    AudioController.StopAll();
                if(GUILayout.Button("Stop Music Only") && EditorApplication.isPlaying && AudioController.DoesInstanceExist())
                    AudioController.StopMusic();
                EditorGUILayout.EndHorizontal();
            }
            EndInspectorGUI();
        }

        private void _RenamePlaylistEntries(string originalName, string newName)
        {
            if(ac.MusicPlaylists == null)
                return;
            foreach(var playlist in ac.MusicPlaylists)
                for(var i = 0; i < playlist.PlaylistItems.Length; i++)
                    if(playlist.PlaylistItems[i] == originalName)
                        playlist.PlaylistItems[i] = newName;
        }

        private string[] _GenerateCategoryListIncludingNone(out int selectedParentCategoryIndex, AudioCategory selectedAudioCategory)
        {
            selectedParentCategoryIndex = 0;
            string[] strArray1;
            if(ac.AudioCategories != null)
            {
                strArray1 = new string[ac.AudioCategories.Length];
                var length = 1;
                var currentCategory = CurrentCategory;
                foreach(var audioCategory in ac.AudioCategories)
                    if(!_IsCategoryChildOf(audioCategory, currentCategory))
                    {
                        strArray1[length] = audioCategory.Name;
                        if(selectedAudioCategory == audioCategory)
                            selectedParentCategoryIndex = length;
                        ++length;
                        if(length == strArray1.Length)
                            break;
                    }
                if(length < strArray1.Length)
                {
                    var strArray2 = new string[length];
                    Array.Copy(strArray1, strArray2, length);
                    strArray1 = strArray2;
                }
            }
            else
            {
                strArray1 = new string[1];
            }
            strArray1[0] = "*none*";
            return strArray1;
        }

        private bool _IsCategoryChildOf(AudioCategory toTest, AudioCategory parent)
        {
            for(var audioCategory = toTest; audioCategory != null; audioCategory = audioCategory.ParentCategory)
            {
                if(audioCategory.AudioController == null)
                    audioCategory.AudioController = ac;
                if(audioCategory == parent)
                    return true;
            }
            return false;
        }

        private static bool _IsAudioControllerInPlayMode()
        {
            return EditorApplication.isPlaying && AudioController.DoesInstanceExist();
        }

        private void _ValidateCurrentCategoryIndex()
        {
            var currentCategoryCount = CurrentCategoryCount;
            if(currentCategoryCount > 0)
                CurrentCategoryIndex = Mathf.Clamp(CurrentCategoryIndex, 0, currentCategoryCount - 1);
            else
                CurrentCategoryIndex = -1;
        }

        private void _ValidateCurrentSubItemIndex()
        {
            var currentSubItemCount = CurrentSubItemCount;
            if(currentSubItemCount > 0)
                CurrentSubitemIndex = Mathf.Clamp(CurrentSubitemIndex, 0, currentSubItemCount - 1);
            else
                CurrentSubitemIndex = -1;
        }

        private void _ValidateCurrentItemIndex()
        {
            var currentItemCount = CurrentItemCount;
            if(currentItemCount > 0)
                CurrentItemIndex = Mathf.Clamp(CurrentItemIndex, 0, currentItemCount - 1);
            else
                CurrentItemIndex = -1;
        }

        private void _SubitemTypePopup(AudioSubItem subItem)
        {
            var content = new[]
            {
                "Single Audio Clip",
                "Other Audio Item"
            };
            var selectedIndex = 0;
            switch(subItem.SubItemType)
            {
                case AudioSubItemType.Clip:
                    selectedIndex = 0;
                    break;
                case AudioSubItemType.Item:
                    selectedIndex = 1;
                    break;
            }
            switch(Popup("SubItem Type", selectedIndex, content))
            {
                case 0:
                    subItem.SubItemType = AudioSubItemType.Clip;
                    break;
                case 1:
                    subItem.SubItemType = AudioSubItemType.Item;
                    break;
            }
        }

        public void AddToPlayList(string playlistName, string entryName)
        {
            var playlistByName = ac.GetPlaylistByName(playlistName);
            if(playlistByName == null)
                return;
            ArrayHelper.AddArrayElement(ref playlistByName.PlaylistItems, entryName);
            CurrentPlaylistEntryIndex = playlistByName.PlaylistItems.Length - 1;
            KeepChanges();
        }

        protected void EditAudioClip(ref AudioSubItem subItem, string label)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, StyleLabel);
            var audioClip = (AudioClip)EditorGUILayout.ObjectField(subItem.Clip, typeof(AudioClip), false);
            if((bool)audioClip)
            {
                EditorGUILayout.Space();
                GUILayout.Label(string.Format("{0:0.0} sec", audioClip.length), GUILayout.Width(60f));
                subItem.Clip = audioClip;
            }
            if(GUILayout.Button(new GUIContent("▶", "Preview AudioClip"), GUILayout.Width(20f)) && subItem != null)
                if(_IsAudioControllerInPlayMode())
                {
                    var currentAudioListener = AudioController.GetCurrentAudioListener();
                    var worldPosition = !(currentAudioListener != null) ? Vector3.zero : currentAudioListener.transform.position + currentAudioListener.transform.forward;
                    AudioController.Instance.PlayAudioSubItem(subItem, 1f, worldPosition, null, 0.0f, 0.0f, false, null);
                }
                else if(Application.platform == RuntimePlatform.OSXEditor)
                {
                    Debug.Log("On MacOS playing audios is only supported during play mode.");
                }
                else
                {
                    PreviewAudioSubItem(subItem);
                }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void _DisplaySubItem_Clip(AudioSubItem subItem, int subItemCount, AudioItem curItem)
        {
            if(subItem != null)
            {
                EditAudioClip(ref subItem, "Audio Clip");
                if(EditFloat01(ref subItem.Volume, "Volume", " %"))
                    _AdjustVolumeOfAllAudioItems(curItem, subItem);
                EditSubItemFloat01Inherited(subItem, ref subItem.RandomVolume, "RandomVolume", "Random Volume", "±%");
                EditFloat(ref subItem.Delay, "Delay", "sec");
                EditFloatPlusMinus1(ref subItem.Pan2D, "Pan2D", "%left/right");
                if(_IsRandomItemMode(curItem.SubItemPickMode))
                {
                    EditFloat01(ref subItem.Probability, "Probability", " %",
                                "Choose a higher value (in comparison to the probability values of the other audio clips) to increase the probability for this clip when using a random subitem pick mode.");
                    EditBool(ref subItem.DisableOtherSubitems, "Disable Other Subitems",
                             "If enabled all other subitems which do not have this option enabled will not be played. Useful for testing specific subitmes within a large list of subitems.");
                }
                EditFloat(ref subItem.PitchShift, "Pitch Shift", "semitone");
                EditSubItemFloatInherited(subItem, ref subItem.RandomPitch, "RandomPitch", "Random Pitch", "±semitone");
                EditSubItemFloatInherited(subItem, ref subItem.RandomDelay, "RandomDelay", "Random Delay", "sec");
                EditFloat(ref subItem.FadeIn, "Fade-in", "sec");
                EditFloat(ref subItem.FadeOut, "Fade-out", "sec");
                EditFloat(ref subItem.ClipStartTime, "Start at", "sec");
                EditFloat(ref subItem.ClipStopTime, "Stop at", "sec");
                EditBool(ref subItem.RandomStartPosition, "Random Start Position", "Starts playing at a random position. Useful when looping.");
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            EditorGUILayout.EndHorizontal();
        }

        private void _AdjustVolumeOfAllAudioItems(AudioItem curItem, AudioSubItem subItem)
        {
            if(!_IsAudioControllerInPlayMode())
                return;
            foreach(var playingAudioObject in AudioController.GetPlayingAudioObjects())
                if(curItem == playingAudioObject.AudioItem && (subItem == null || subItem == playingAudioObject.SubItem))
                    playingAudioObject.VolumeItem = playingAudioObject.AudioItem.Volume * playingAudioObject.SubItem.Volume;
        }

        private bool _IsRandomItemMode(AudioPickSubItemMode audioPickSubItemMode)
        {
            if(audioPickSubItemMode <= AudioPickSubItemMode.RandomNotSameTwice)
            {
                if(audioPickSubItemMode == AudioPickSubItemMode.Random || audioPickSubItemMode == AudioPickSubItemMode.RandomNotSameTwice)
                    return true;
            }
            else if(audioPickSubItemMode == AudioPickSubItemMode.TwoSimultaneously || audioPickSubItemMode == AudioPickSubItemMode.RandomNotSameTwiceOddsEvens)
            {
                return true;
            }
            return false;
        }

        private string _ChooseItem(string label)
        {
            var possibleAudioIds = _GetPossibleAudioIDs(true, "Choose Audio Item...");
            var index = PopupWithStyle(label, 0, possibleAudioIds, styleChooseItem);
            return index != 0 ? _GetPossibleAudioIDs(false, "Choose Audio Item...")[index] : null;
        }

        private bool EditSubItemFloatInherited(AudioSubItem subItem, ref float value, string attributeName, string label, string units)
        {
            var name = subItem.GetType().GetField(attributeName).Name;
            var individual = subItem.IndividualSettings.Contains(name);
            var reset = false;
            if(EditFloatInherited(ref value, label, units, individual, out reset))
            {
                if(!individual)
                    subItem.IndividualSettings.Add(name);
                return true;
            }
            if(reset)
            {
                subItem.IndividualSettings.Remove(name);
                value = (float)CurrentItem.GetType().GetField(name).GetValue(CurrentItem);
            }
            return false;
        }

        private bool EditSubItemFloat01Inherited(AudioSubItem subItem, ref float value, string attributeName, string label, string units)
        {
            var name = subItem.GetType().GetField(attributeName).Name;
            var individual = subItem.IndividualSettings.Contains(name);
            var reset = false;
            if(EditFloat01Inherited(ref value, label, units, individual, out reset))
            {
                if(!individual)
                    subItem.IndividualSettings.Add(name);
                return true;
            }
            if(reset)
            {
                subItem.IndividualSettings.Remove(name);
                value = (float)CurrentItem.GetType().GetField(name).GetValue(CurrentItem);
            }
            return false;
        }

        private void _DisplaySubItem_Item(AudioSubItem subItem)
        {
            EditFloat01(ref subItem.Probability, "Probability", " %");
            var selectedIndex = 0;
            var possibleAudioIds1 = _GetPossibleAudioIDs(false, "*undefined*");
            var possibleAudioIds2 = _GetPossibleAudioIDs(true, "*undefined*");
            if(subItem.ItemModeAudioID != null && subItem.ItemModeAudioID.Length > 0)
            {
                var lowerInvariant = subItem.ItemModeAudioID.ToLowerInvariant();
                for(var index = 1; index < possibleAudioIds1.Length; ++index)
                    if(possibleAudioIds1[index].ToLowerInvariant() == lowerInvariant)
                    {
                        selectedIndex = index;
                        break;
                    }
            }
            var flag = selectedIndex == 0;
            var index1 = Popup("AudioItem", selectedIndex, possibleAudioIds2);
            if(index1 > 0)
            {
                subItem.ItemModeAudioID = possibleAudioIds1[index1];
            }
            else
            {
                if(flag)
                    return;
                subItem.ItemModeAudioID = null;
            }
        }

        private string[] _GetPossibleAudioIDs(bool withCategoryName, string firstEntryName)
        {
            var audioIDs = new List<string> { firstEntryName };
            if(ac.AudioCategories != null)
                foreach(var audioCategory in ac.AudioCategories)
                    _GetAllAudioIDs(audioIDs, audioCategory, withCategoryName);
            return audioIDs.ToArray();
        }

        private static void _GetAllAudioIDs(List<string> audioIDs, AudioCategory c, bool withCategoryName)
        {
            if(c.AudioItems == null)
                return;
            audioIDs.AddRange(c.AudioItems.Where(audioItem => audioItem.Name.Length > 0)
                               .Select(audioItem => withCategoryName ? string.Format("{0}/{1}", c.Name, audioItem.Name) : audioItem.Name));
        }

        private static bool SwapArrayElements<T>(IList<T> array, int index1, int index2)
        {
            if(array == null || index1 < 0 || index2 < 0 || index1 >= array.Count || index2 >= array.Count)
                return false;
            var obj = array[index1];
            array[index1] = array[index2];
            array[index2] = obj;
            return true;
        }

        private static void VerticalSpace()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private string[] GetCategoryNames()
        {
            if(ac.AudioCategories == null)
                return new string[0];
            var strArray = new string[ac.AudioCategories.Length];
            for(var index = 0; index < ac.AudioCategories.Length; ++index)
            {
                strArray[index] = ac.AudioCategories[index].Name;
                if(strArray[index] == "!!! Enter Unique Category Name Here !!!")
                    strArray[index] = "---";
            }
            return strArray;
        }

        private string[] GetItemNames()
        {
            var currentCategory = CurrentCategory;
            if(currentCategory == null || currentCategory.AudioItems == null)
                return new string[0];
            var strArray = new string[currentCategory.AudioItems.Length];
            for(var index = 0; index < currentCategory.AudioItems.Length; ++index)
            {
                strArray[index] = currentCategory.AudioItems[index] != null ? currentCategory.AudioItems[index].Name : "";
                if(strArray[index] == "!!! Enter Unique Audio ID Here !!!")
                    strArray[index] = "---";
            }
            return strArray;
        }

        private string[] GetSubitemNames()
        {
            var currentItem = CurrentItem;
            if(currentItem == null || currentItem.SubItems == null)
                return new string[0];
            var strArray = new string[currentItem.SubItems.Length];
            for(var index = 0; index < currentItem.SubItems.Length; ++index)
                strArray[index] = (currentItem.SubItems[index] != null ? (int)currentItem.SubItems[index].SubItemType : 0) != 1
                                      ? string.Format("CLIP {0}: {1}", index,
                                                      currentItem.SubItems[index] != null
                                                          ? ((bool)(Object)currentItem.SubItems[index].Clip ? currentItem.SubItems[index].Clip.name : "*unset*")
                                                          : "")
                                      : string.Format("ITEM {0}: {1}", index, currentItem.SubItems[index].ItemModeAudioID ?? "*undefined*");
            return strArray;
        }

        private string[] GetPlaylistNames()
        {
            if(ac.MusicPlaylists == null)
                return new string[0];
            var strArray = new string[ac.MusicPlaylists.Length];
            for(var index = 0; index < ac.MusicPlaylists.Length; ++index)
            {
                strArray[index] = ac.MusicPlaylists[index].Name;
                if(strArray[index] == "!!! Enter Unique Playlist Name here !!!")
                    strArray[index] = "---";
            }
            return strArray;
        }

        private string[] GetPlaylistEntryNames()
        {
            if(ac.MusicPlaylists == null)
                return new string[0];
            if(ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems == null)
                return new string[0];
            var strArray = new string[ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems.Length];
            for(var index = 0; index < ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems.Length; ++index)
                strArray[index] = string.Format("{0}: {1}", index, ac.MusicPlaylists[CurrentPlaylistIndex].PlaylistItems[index]);
            return strArray;
        }

        private static AudioClip[] GetSelectedAudioclips()
        {
            var filtered = Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
            var audioClipArray = new AudioClip[filtered.Length];
            for(var index = 0; index < filtered.Length; ++index)
                audioClipArray[index] = (AudioClip)filtered[index];
            return audioClipArray;
        }

        private AudioCategory _GetCategory(string categoryName)
        {
            return ac.AudioCategories.FirstOrDefault(audioCategory => audioCategory.Name == categoryName);
        }

        private AudioItem _GetAudioItemByName(string audioID)
        {
            return ac.AudioCategories.SelectMany(audioCategory => audioCategory.AudioItems).FirstOrDefault(audioItem => audioItem.Name == audioID);
        }

        private void PreviewAudioItem(AudioItem item) { PreviewAudioSubItem(AudioControllerHelper._ChooseSingleSubItem(item)); }

        private void PreviewAudioSubItem(AudioSubItem item)
        {
            if(item.SubItemType == AudioSubItemType.Clip)
            {
                AudioUtility.PlayClip(item.Clip);
            }
            else
            {
                if(item.SubItemType != AudioSubItemType.Item)
                    return;
                var audioItemByName = _GetAudioItemByName(item.ItemModeAudioID);
                if(audioItemByName == null)
                    return;
                PreviewAudioItem(audioItemByName);
            }
        }
    }
}
