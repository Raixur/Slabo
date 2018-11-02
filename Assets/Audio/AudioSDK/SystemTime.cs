using System;

namespace AudioSDK
{
    public static class SystemTime
    {
        private static readonly double TimeAtLaunch = Time;

        public static double Time
        {
            get
            {
                return DateTime.Now.Ticks * 1E-07;
            }
        }

        public static double TimeSinceLaunch
        {
            get
            {
                return Time - TimeAtLaunch;
            }
        }
    }
}
