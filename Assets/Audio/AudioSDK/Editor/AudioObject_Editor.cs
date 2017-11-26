using UnityEditor;
using UnityEngine;

namespace AudioSDK.Editor
{
    [CustomEditor(typeof(AudioObject))]
    public class AudioObject_Editor : EditorEx
    {
        protected AudioObject Ao;

        public override void OnInspectorGUI() { DrawInspector(); }

        private static string FormatVolume(float volume)
        {
            var num = 20f * Mathf.Log10(AudioObject.TransformVolume(volume));
            return string.Format("{0:0.000} ({1:0.0} dB)", volume, num);
        }

        private void DrawInspector()
        {
            Ao = (AudioObject)target;
            BeginInspectorGUI();
            ShowString(Ao.AudioID, "Audio ID:");
            ShowString(Ao.Category != null ? Ao.Category.Name : "---", "Audio Category:");
            ShowString(FormatVolume(Ao.Volume), "Item Volume:");
            ShowString(FormatVolume(Ao.VolumeTotal), "Total Volume:");
            ShowFloat((float)Ao.StartedPlayingAtTime, "Time Started:");
            if((bool)Ao.PrimaryAudioSource)
            {
                ShowString(string.Format("{0:0.00} half-tones", AudioObject.InverseTransformPitch(Ao.PrimaryAudioSource.pitch)), "Pitch:");
                if((bool)Ao.PrimaryAudioSource.clip)
                    ShowString(string.Format("{0} / {1}", Ao.PrimaryAudioSource.time, Ao.ClipLength), "Time:");
                if(Ao.ScheduledPlayingAtDspTime > 0.0)
                    ShowFloat((float)(Ao.ScheduledPlayingAtDspTime - AudioSettings.dspTime), "Scheduled Play In seconds: ");
            }
            if((bool)Ao.SecondaryAudioSource)
                ShowString(string.Format("Secondary: T:{0} Playing:{1}", Ao.SecondaryAudioSource.time, Ao.SecondaryAudioSource.isPlaying), "Time:");
            EditorGUILayout.BeginHorizontal();
            if(!Ao.IsPaused())
            {
                if(GUILayout.Button("Pause"))
                    Ao.Pause();
            }
            else if(GUILayout.Button("Unpause"))
            {
                Ao.Unpause();
            }
            if(GUILayout.Button("Stop"))
                Ao.Stop(0.5f);
            if(GUILayout.Button("FadeIn"))
                Ao.FadeIn(2f);
            if(GUILayout.Button("FadeOut"))
                Ao.FadeOut(2f);
            GUILayout.Button("Refresh");
            EditorGUILayout.EndHorizontal();
            EndInspectorGUI();
        }
    }
}
