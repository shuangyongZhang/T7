#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace LB
{
    public static class AndroidReportPrinter
    {
        public static bool PrintHtml(string html, string jobName)
        {
            if (string.IsNullOrEmpty(html)) return false;
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass plugin = new AndroidJavaClass("com.lvbaizhineng.print.T7PrintPlugin"))
            {
                if (activity == null || plugin == null) return false;
                plugin.CallStatic("printHtml", activity, html, jobName);
                return true;
            }
        }
    }
}
#endif
