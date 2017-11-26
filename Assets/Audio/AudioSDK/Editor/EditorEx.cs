using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioSDK.Editor
{
    public abstract class EditorEx : UnityEditor.Editor
    {
        protected GUILayoutOption LabelFieldOption;
        protected GUIStyle StyleLabel;
        protected GUIStyle StyleUnit;
        protected GUIStyle StyleFloat;
        protected GUIStyle StyleFloatIndi;
        protected GUIStyle StylePopup;
        protected GUIStyle StyleEnum;
        private bool setFocusNextField;
        private bool userChanges;
        private int fieldIndex;

        protected virtual void LogUndo(string label) { }

        protected void SetFocusForNextEditableField() { setFocusNextField = true; }

        protected void ShowFloat(float f, string label) { EditorGUILayout.LabelField(label, f.ToString()); }

        protected void ShowString(string text, string label) { EditorGUILayout.LabelField(label, text); }

        private float GetFloat(float f, string label, string unit, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var num = EditorGUILayout.FloatField(f, StyleFloat);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            EditorGUILayout.EndHorizontal();
            return num;
        }

        private float GetFloatInherited(float f, string label, string unit, bool individual, out bool reset, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var num = EditorGUILayout.FloatField(f, individual ? StyleFloatIndi : StyleFloat);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            GUI.enabled = individual;
            reset = (GUILayout.Button(new GUIContent("R", "Reset to AudioItem value"), GUILayout.Width(20f)) ? 1 : 0) != 0;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            return num;
        }

        private int GetInt(int f, string label, string unit, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(label, tooltip), StyleLabel);
            var num = EditorGUILayout.IntField(f, StyleFloat);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            EditorGUILayout.EndHorizontal();
            return num;
        }

        private float GetFloat(float f, string label, float sliderMin, float sliderMax, string unit)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, StyleLabel);
            var num = GUILayout.HorizontalSlider(EditorGUILayout.FloatField(f, StyleFloat, GUILayout.Width(50f)), sliderMin, sliderMax);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            EditorGUILayout.EndHorizontal();
            return num;
        }

        private float GetFloatPercent(float f, string label, string unit, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var num = GUILayout.HorizontalSlider(EditorGUILayout.IntField(Mathf.RoundToInt(f * 100f), StyleFloat, GUILayout.Width(50f)) / 100f, 0.0f, 1f);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            EditorGUILayout.EndHorizontal();
            return num;
        }

        private float GetFloatPercentInherited(float f, string label, string unit, bool individual, out bool reset, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var num1 = f;
            var num2 = Mathf.RoundToInt(num1 * 100f);
            var num3 = num2;
            var num4 = EditorGUILayout.IntField(num2, individual ? StyleFloatIndi : StyleFloat, GUILayout.Width(50f));
            if(num3 != num4)
                num1 = num4 / 100f;
            var num5 = GUILayout.HorizontalSlider(num1, 0.0f, 1f);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            GUI.enabled = individual;
            reset = (GUILayout.Button(new GUIContent("R", "Reset to AudioItem value"), GUILayout.Width(20f)) ? 1 : 0) != 0;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            return num5;
        }

        private float GetFloatPlusMinusPercent(float f, string label, string unit)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, StyleLabel);
            var num = GUILayout.HorizontalSlider(EditorGUILayout.IntField(Mathf.RoundToInt(f * 100f), StyleFloat, GUILayout.Width(50f)) / 100f, -1f, 1f);
            GUILayout.Label(!string.IsNullOrEmpty(unit) ? unit : " ", StyleUnit);
            EditorGUILayout.EndHorizontal();
            return num;
        }

        protected bool EditFloat(ref float f, string label)
        {
            var num = GetFloat(f, label, null);
            if(Mathf.Abs(num - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = num;
            return true;
        }

        protected bool EditFloat(ref float f, string label, string unit, string tooltip = null)
        {
            var num = GetFloat(f, label, unit, tooltip);
            if(Mathf.Abs(num - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = num;
            return true;
        }

        protected bool EditFloatInherited(ref float f, string label, string unit, bool individual, out bool reset, string tooltip = null)
        {
            var floatInherited = GetFloatInherited(f, label, unit, individual, out reset, tooltip);
            if(Mathf.Abs(floatInherited - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = floatInherited;
            return true;
        }

        protected bool EditFloat01Inherited(ref float f, string label, string unit, bool individual, out bool reset, string tooltip = null)
        {
            var percentInherited = GetFloatPercentInherited(f, label, unit, individual, out reset, tooltip);
            if(Mathf.Abs(percentInherited - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = percentInherited;
            return true;
        }

        private float GetFloat01(float f, string label) { return Mathf.Clamp01(GetFloatPercent(f, label, null)); }

        private float GetFloat01(float f, string label, string unit, string tooltip = null) { return Mathf.Clamp01(GetFloatPercent(f, label, unit, tooltip)); }

        private float GetFloatPlusMinus1(float f, string label, string unit) { return Mathf.Clamp(GetFloatPlusMinusPercent(f, label, unit), -1f, 1f); }

        private float GetFloatWithinRange(float f, string label, float minValue, float maxValue)
        {
            return Mathf.Clamp(GetFloat(f, label, minValue, maxValue, null), minValue, maxValue);
        }

        protected bool EditFloatWithinRange(ref float f, string label, float minValue, float maxValue)
        {
            var floatWithinRange = GetFloatWithinRange(f, label, minValue, maxValue);
            if(Mathf.Abs(floatWithinRange - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = floatWithinRange;
            return true;
        }

        protected bool EditInt(ref int f, string label)
        {
            var num = GetInt(f, label, null);
            if(num == f)
                return false;
            LogUndo(label);
            f = num;
            return true;
        }

        protected bool EditInt(ref int f, string label, string unit, string tooltip = null)
        {
            var num = GetInt(f, label, unit, tooltip);
            if(num == f)
                return false;
            LogUndo(label);
            f = num;
            return true;
        }

        protected bool EditFloat01(ref float f, string label)
        {
            var float01 = GetFloat01(f, label);
            if(Mathf.Abs(float01 - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = float01;
            return true;
        }

        protected bool EditFloat01(ref float f, string label, string unit, string tooltip = null)
        {
            var float01 = GetFloat01(f, label, unit, tooltip);
            if(Mathf.Abs(float01 - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = float01;
            return true;
        }

        protected bool EditFloatPlusMinus1(ref float f, string label, string unit)
        {
            var floatPlusMinus1 = GetFloatPlusMinus1(f, label, unit);
            if(Mathf.Abs(floatPlusMinus1 - f) <= 1.40129846432482E-45)
                return false;
            LogUndo(label);
            f = floatPlusMinus1;
            return true;
        }

        private bool GetBool(bool b, string label, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var num = EditorGUILayout.Toggle((b ? 1 : 0) != 0, GUILayout.Width(20f)) ? 1 : 0;
            EditorGUILayout.EndHorizontal();
            return num != 0;
        }

        protected bool EditBool(ref bool b, string label, string tooltip = null)
        {
            var flag = GetBool(b, label, tooltip);
            if(flag == b)
                return false;
            LogUndo(label);
            b = flag;
            return true;
        }

        protected bool EditPrefab<T>(ref T prefab, string label, string tooltip = null) where T : Object
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var obj = (T)EditorGUILayout.ObjectField(prefab, typeof(T), false);
            EditorGUILayout.EndHorizontal();
            if(!(obj != prefab))
                return false;
            LogUndo(label);
            prefab = obj;
            return true;
        }

        protected bool EditString(ref string txt, string label, GUIStyle styleText = null, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            BeginEditableField();
            var str = styleText == null ? EditorGUILayout.TextField(txt) : EditorGUILayout.TextField(txt, styleText);
            EndEditableField();
            EditorGUILayout.EndHorizontal();
            if(str == txt)
                return false;
            LogUndo(label);
            txt = str;
            return true;
        }

        protected int Popup(string label, int selectedIndex, string[] content, string tooltip = null, bool sortAlphabetically = true)
        {
            return PopupWithStyle(label, selectedIndex, content, StylePopup, tooltip, sortAlphabetically);
        }

        protected int PopupWithStyle(string label, int selectedIndex, string[] content, GUIStyle style, string tooltip = null, bool sortAlphabetically = true)
        {
            List<ContentWithIndex> contentWithIndexList = null;
            if(content.Length == 0)
                sortAlphabetically = false;
            string[] displayedOptions;
            if(sortAlphabetically)
            {
                contentWithIndexList = _CreateContentWithIndexList(content);
                displayedOptions = new string[content.Length];
                var num = 0;
                foreach(var contentWithIndex in contentWithIndexList)
                    displayedOptions[num++] = contentWithIndex.Content;
            }
            else
            {
                displayedOptions = content;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            int num1;
            if(sortAlphabetically)
            {
                var index = EditorGUILayout.Popup(contentWithIndexList.FindIndex(x => x.Index == selectedIndex), displayedOptions, style);
                num1 = contentWithIndexList[index].Index;
            }
            else
            {
                num1 = EditorGUILayout.Popup(selectedIndex, displayedOptions, style);
            }
            EditorGUILayout.EndHorizontal();
            if(num1 != selectedIndex)
                LogUndo(label);
            return num1;
        }

        private List<ContentWithIndex> _CreateContentWithIndexList(string[] content)
        {
            var source = new List<ContentWithIndex>();
            for(var index = 0; index < content.Length; ++index)
                source.Add(new ContentWithIndex(content[index], index));
            return source.OrderBy(x => x.Content).ToList();
        }

        protected Enum EnumPopup(string label, Enum selectedEnum, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), LabelFieldOption);
            var enum1 = EditorGUILayout.EnumPopup(selectedEnum, StyleEnum);
            EditorGUILayout.EndHorizontal();
            var enum2 = selectedEnum;
            if(Equals(enum1, enum2))
                return enum1;
            LogUndo(label);
            return enum1;
        }

        private void EndEditableField()
        {
            if(!setFocusNextField)
                return;
            setFocusNextField = false;
        }

        private void BeginEditableField()
        {
            fieldIndex = fieldIndex + 1;
            if(!setFocusNextField)
                return;
            GUI.SetNextControlName(GetCurrentFieldControlName());
        }

        private string GetCurrentFieldControlName() { return string.Format("field{0}", fieldIndex); }

        protected void BeginInspectorGUI()
        {
            serializedObject.Update();
            SetStyles();
            fieldIndex = 0;
            userChanges = false;
        }

        protected void SetStyles()
        {
            LabelFieldOption = GUILayout.Width(180f);
            StyleLabel = new GUIStyle(EditorStyles.label);
            StyleUnit = new GUIStyle(EditorStyles.label);
            StyleFloat = new GUIStyle(EditorStyles.numberField);
            StylePopup = new GUIStyle(EditorStyles.popup);
            StyleEnum = new GUIStyle(EditorStyles.popup);
            StyleLabel.fixedWidth = 180f;
            StyleUnit.fixedWidth = 65f;
        }

        protected void EndInspectorGUI()
        {
            if(GUI.changed || userChanges)
                EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }

        protected void KeepChanges() { userChanges = true; }

        public class ContentWithIndex
        {
            public string Content;
            public int Index;

            public ContentWithIndex(string content, int index)
            {
                Content = content;
                Index = index;
            }
        }
    }
}
