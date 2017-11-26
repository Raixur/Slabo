using UnityEngine;

namespace AudioSDK
{
    public class AudioFader
    {
        private float fadeOutTotalTime = -1f;
        private double fadeOutStartTime = -1.0;
        private float fadeInTotalTime = -1f;
        private double fadeInStartTime = -1.0;

        public double Time { get; set; }

        public bool IsFadingOutComplete
        {
            get
            {
                if(fadeOutStartTime > 0.0)
                {
                    if(fadeOutTotalTime >= 0.0)
                        return Time >= fadeOutStartTime + fadeOutTotalTime;
                    return false;
                }
                if(fadeOutTotalTime >= 0.0)
                    return Time >= fadeOutTotalTime;
                return false;
            }
        }

        public bool IsFadingOut
        {
            get
            {
                if(fadeOutStartTime > 0.0)
                {
                    if(fadeOutTotalTime >= 0.0 && Time >= fadeOutStartTime)
                        return Time < fadeOutStartTime + fadeOutTotalTime;
                    return false;
                }
                if(fadeOutTotalTime >= 0.0)
                    return Time < fadeOutTotalTime;
                return false;
            }
        }

        public bool IsFadingOutOrScheduled
        {
            get { return fadeOutTotalTime >= 0.0; }
        }

        public bool IsFadingIn
        {
            get
            {
                if(fadeInStartTime > 0.0)
                {
                    if(fadeInTotalTime > 0.0 && Time >= fadeInStartTime)
                        return Time - fadeInStartTime < fadeInTotalTime;
                    return false;
                }
                if(fadeInTotalTime > 0.0)
                    return Time < fadeInTotalTime;
                return false;
            }
        }

        public void Set0()
        {
            Time = 0.0;
            fadeOutTotalTime = -1f;
            fadeOutStartTime = -1.0;
            fadeInTotalTime = -1f;
            fadeInStartTime = -1.0;
        }

        public void FadeIn(float fadeInTime, bool stopCurrentFadeOut = false) { FadeIn(fadeInTime, Time, stopCurrentFadeOut); }

        public void FadeIn(float fadeInTime, double startToFadeTime, bool stopCurrentFadeOut = false)
        {
            if(IsFadingOutOrScheduled & stopCurrentFadeOut)
            {
                var fadeOutValue = GetFadeOutValue();
                fadeOutTotalTime = -1f;
                fadeOutStartTime = -1.0;
                fadeInTotalTime = fadeInTime;
                fadeInStartTime = startToFadeTime - fadeInTime * (double)fadeOutValue;
            }
            else
            {
                fadeInTotalTime = fadeInTime;
                fadeInStartTime = startToFadeTime;
            }
        }

        public void FadeOut(float fadeOutLength, float startToFadeTime)
        {
            if(IsFadingOutOrScheduled)
            {
                var num1 = Time + startToFadeTime + fadeOutLength;
                var num2 = fadeOutStartTime + fadeOutTotalTime;
                if(num2 < num1)
                    return;
                var num3 = Time - fadeOutStartTime;
                var num4 = startToFadeTime + (double)fadeOutLength;
                var num5 = num2 - Time;
                if(num5 == 0.0)
                    return;
                var num6 = num3 * num4 / num5;
                fadeOutStartTime = Time - num6;
                fadeOutTotalTime = (float)(num4 + num6);
            }
            else
            {
                fadeOutTotalTime = fadeOutLength;
                fadeOutStartTime = Time + startToFadeTime;
            }
        }

        public float Get()
        {
            bool finishedFadeOut;
            return Get(out finishedFadeOut);
        }

        public float Get(out bool finishedFadeOut)
        {
            var num = 1f;
            finishedFadeOut = false;
            if(IsFadingOutOrScheduled)
            {
                num *= GetFadeOutValue();
                if(num == 0.0)
                {
                    finishedFadeOut = true;
                    return 0.0f;
                }
            }
            if(IsFadingIn)
                num *= GetFadeInValue();
            return num;
        }

        private float GetFadeOutValue() { return 1f - GetFadeValue((float)(Time - fadeOutStartTime), fadeOutTotalTime); }

        private float GetFadeInValue() { return GetFadeValue((float)(Time - fadeInStartTime), fadeInTotalTime); }

        private static float GetFadeValue(float t, float dt)
        {
            if(dt > 0.0)
                return Mathf.Clamp(t / dt, 0.0f, 1f);
            return t <= 0.0 ? 0.0f : 1f;
        }
    }
}
