using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioSDK.Editor
{
    public class AudioItemOverview : EditorWindow
    {
        private static Vector2 scrollPos;
        private static Dictionary<string, bool> lastFoldedOutCategories;
        private readonly Dictionary<string, bool> foldedOutCategories = new Dictionary<string, bool>();
        private Texture2D listItemBackgroundNormal0;
        private Texture2D listItemBackgroundNormal1;
        private Texture2D listItemBackgroundClicked;
        private Texture2D categoryBackgroundNormal;
        private Texture2D categoryBackgroundClicked;
        private AudioController selectedAc;
        private int selectedAcIndex;
        private AudioController[] audioControllerList;
        private string[] audioControllerNameList;
        private string searchString;
        private bool isInitialised;
        private GUIStyle headerStyleButton;
        private GUIStyle styleEmptyButton;
        private GUIStyle styleListItemButton0;
        private GUIStyle styleListItemButton1;
        private GUIStyle styleCategoryButtonHeader;
        private GUIStyle headerStyle;
        private int buttonSize;
        private AudioController foldoutsSetFromController;

        [UsedImplicitly]
        [MenuItem("Window/Audio Toolkit/Item Overview")]
        private static void ShowWindow()
        {
            GetWindow(typeof(AudioItemOverview));
        }

        public void Show(AudioController audioController)
        {
            if(!isInitialised)
                Initialise();
            _SetCurrentAudioController(audioController);
            _FindAudioController();
            Show();
        }

        [UsedImplicitly]
        private void OnGUI()
        {
            if(!isInitialised)
                Initialise();
            if(!(bool)selectedAc)
                _SetCurrentAudioController(_FindAudioController());
            if(selectedAc == null && Selection.activeGameObject != null)
                _SetCurrentAudioController(Selection.activeGameObject.GetComponent<AudioController>());
            if(audioControllerNameList == null)
            {
                _FindAudioController();
                if(audioControllerNameList == null && selectedAc != null)
                    audioControllerNameList = new[]
                    {
                        _GetPrefabName(selectedAc)
                    };
            }
            if(!(bool)selectedAc)
            {
                EditorGUILayout.LabelField("No AudioController found!");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                var num = EditorGUILayout.Popup(selectedAcIndex, audioControllerNameList, headerStyleButton);
                if(num != selectedAcIndex)
                {
                    selectedAcIndex = num;
                    _SetCurrentAudioController(audioControllerList[selectedAcIndex]);
                    _SelectCurrentAudioController();
                }
                if(foldoutsSetFromController != selectedAc)
                    _SetCategoryFoldouts();
                if(searchString == null)
                    searchString = "";
                searchString = EditorGUILayout.TextField("                  search item: ", searchString);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Button("      ", styleEmptyButton);
                EditorGUILayout.LabelField("Item", headerStyle);
                EditorGUILayout.LabelField("Sub Item", headerStyle);
                EditorGUILayout.EndHorizontal();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                if(selectedAc != null && selectedAc.AudioCategories != null)
                    for(var index1 = 0; index1 < selectedAc.AudioCategories.Length; ++index1)
                    {
                        var audioCategory = selectedAc.AudioCategories[index1];
                        if(string.IsNullOrEmpty(audioCategory.Name))
                        {
                            Debug.LogWarning("empty category.Name");
                        }
                        else if(!foldedOutCategories.ContainsKey(audioCategory.Name))
                        {
                            Debug.LogWarning("can not find category.Name" + audioCategory.Name);
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            if(GUILayout.Button((foldedOutCategories[audioCategory.Name] ? "-\t" : "+\t") + audioCategory.Name, styleCategoryButtonHeader))
                                foldedOutCategories[audioCategory.Name] = !foldedOutCategories[audioCategory.Name];
                            EditorGUILayout.EndHorizontal();
                            var source = new List<AudioItem>(audioCategory.AudioItems);
                            if(!string.IsNullOrEmpty(searchString))
                            {
                                var flag = false;
                                for(var index2 = 0; index2 < source.Count; ++index2)
                                    if(!source[index2].Name.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
                                        source.RemoveAt(index2--);
                                    else
                                        flag = true;
                                foldedOutCategories[audioCategory.Name] = flag && source.Count > 0;
                            }
                            if(foldedOutCategories[audioCategory.Name] && audioCategory.AudioItems != null)
                                foreach(var audioItem in source.OrderBy(x => x.Name).ToArray())
                                {
                                    var item = audioItem;
                                    var text1 = "      ";
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Button(text1, styleEmptyButton);
                                    GUILayout.Label(item.Name);
                                    EditorGUILayout.EndHorizontal();
                                    if(item.SubItems != null)
                                        for(var index2 = 0; index2 < item.SubItems.Length; ++index2)
                                        {
                                            var subItem = item.SubItems[index2];
                                            GUILayout.BeginHorizontal();
                                            GUILayout.Button(text1, styleEmptyButton);
                                            EditorGUILayout.BeginHorizontal();
                                            var str = "     ";
                                            string text2;
                                            string label;
                                            if(subItem.SubItemType == AudioSubItemType.Clip)
                                            {
                                                if(subItem.Clip != null)
                                                {
                                                    text2 = subItem.Clip.name;
                                                    label = str + "CLIP:";
                                                }
                                                else
                                                {
                                                    text2 = "*unset*";
                                                    label = str + "CLIP:";
                                                }
                                            }
                                            else
                                            {
                                                label = str + "ITEM:";
                                                text2 = subItem.ItemModeAudioID;
                                            }
                                            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(buttonSize));
                                            if(GUILayout.Button(text2, index2 % 2 == 0 ? styleListItemButton0 : styleListItemButton1, GUILayout.ExpandWidth(true)))
                                            {
                                                selectedAc.CurrentInspectorSelection.CurrentCategoryIndex = index1;
                                                selectedAc.CurrentInspectorSelection.CurrentItemIndex = Array.FindIndex(audioCategory.AudioItems, x => x.Name == item.Name);
                                                selectedAc.CurrentInspectorSelection.CurrentSubitemIndex = index2;
                                                _SelectCurrentAudioController();
                                            }
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.EndHorizontal();
                                        }
                                }
                        }
                    }
                EditorGUILayout.EndScrollView();
            }
        }

        private void _SetCurrentAudioController(AudioController ac)
        {
            selectedAc = ac;
            _SetCategoryFoldouts();
        }

        private void _SetCategoryFoldouts()
        {
            foldoutsSetFromController = selectedAc;
            if(selectedAc == null)
                return;
            foreach(var audioCategory in selectedAc.AudioCategories) {
                if(!foldedOutCategories.ContainsKey(audioCategory.Name))
                    foldedOutCategories[audioCategory.Name] = false;
            }
        }

        private void _SelectCurrentAudioController()
        {
            Selection.objects = new Object[]
            {
                selectedAc.gameObject
            };
        }

        private AudioController _FindAudioController()
        {
            audioControllerList = FindObjectsOfType(typeof(AudioController)) as AudioController[];
            if(audioControllerList == null || audioControllerList.Length == 0)
                return null;
            audioControllerNameList = new string[audioControllerList.Length];
            selectedAcIndex = -1;
            for(var index = 0; index < audioControllerList.Length; ++index)
            {
                audioControllerNameList[index] = audioControllerList[index].name;
                if(selectedAc == audioControllerList[index])
                    selectedAcIndex = index;
            }
            if(selectedAcIndex == -1)
                if(selectedAc != null)
                {
                    ArrayHelper.AddArrayElement(ref audioControllerNameList, _GetPrefabName(selectedAc));
                    ArrayHelper.AddArrayElement(ref audioControllerList, selectedAc);
                    selectedAcIndex = audioControllerNameList.Length - 1;
                }
                else
                {
                    selectedAcIndex = 0;
                }
            return selectedAcIndex >= 0 ? audioControllerList[selectedAcIndex] : null;
        }

        private string _GetPrefabName(AudioController ac) { return "PREFAB: " + ac.name; }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var colorArray = new Color[width * height];
            for(var index = 0; index < colorArray.Length; ++index)
                colorArray[index] = col;
            var texture2D = new Texture2D(width, height);
            var colors = colorArray;
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }

        private void Initialise()
        {
            isInitialised = true;
            var col1 = Color.gray * 0.6f;
            var col2 = col1 * 1.2f;
            var col3 = col2 * 1.2f;
            listItemBackgroundNormal0 = MakeTex(32, 32, col1);
            listItemBackgroundNormal1 = MakeTex(32, 32, col2);
            listItemBackgroundClicked = MakeTex(32, 32, col3);
            var col4 = new Color(0.0f, 0.0f, 0.2f);
            var col5 = col4 * 1.4f;
            categoryBackgroundNormal = MakeTex(32, 32, col4);
            categoryBackgroundClicked = MakeTex(32, 32, col5);
            buttonSize = 80;
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyleButton = new GUIStyle(EditorStyles.popup);
            var color = (double)headerStyleButton.normal.textColor.grayscale <= 0.5 ? new Color(0.6f, 0.1f, 0.0f) : new Color(0.9f, 0.9f, 0.5f);
            headerStyleButton.normal.textColor = color;
            headerStyleButton.focused.textColor = color;
            headerStyleButton.active.textColor = color;
            headerStyleButton.hover.textColor = color;
            styleEmptyButton = new GUIStyle(new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = buttonSize
            })
            {
                normal = headerStyle.normal,
                focused = headerStyle.focused,
                active = headerStyle.active,
                hover = headerStyle.hover
            };

            styleListItemButton0 = new GUIStyle(EditorStyles.miniButton)
            {
                normal = { background = listItemBackgroundNormal0 },
                active = { background = listItemBackgroundClicked }
            };

            styleListItemButton1 = new GUIStyle(styleListItemButton0)
            {
                normal = { background = listItemBackgroundNormal1 },
                active = { background = listItemBackgroundClicked }
            };

            styleCategoryButtonHeader = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                fontStyle = FontStyle.Bold,
                normal = { background = categoryBackgroundNormal },
                active = { background = categoryBackgroundClicked }
            };

            if(lastFoldedOutCategories != null)
                foreach(var foldedOutCategory in lastFoldedOutCategories)
                    foldedOutCategories[foldedOutCategory.Key] = foldedOutCategory.Value;
            lastFoldedOutCategories = foldedOutCategories;
        }
    }
}
