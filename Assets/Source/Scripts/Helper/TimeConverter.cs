
using System;

public static class TimeConverter
{
    /// <summary>
    /// 将总秒数转换为 "分:秒" 格式（如 02:05）
    /// </summary>
    /// <param name="totalSeconds">总秒数</param>
    /// <returns>格式化后的字符串，例如 "02:05"</returns>
    public static string FormatTimeToMinutesSeconds(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    /// <summary>
    /// 将 "分:秒" 格式转换为总秒数
    /// </summary>
    /// <param name="timeString">时间字符串，格式如 "02:05"</param>
    /// <returns>总秒数</returns>
    public static int FormatTimeToSeconds(string timeString)
    {
        string[] parts = timeString.Split(':');
        int minutes = int.Parse(parts[0]);
        int seconds = int.Parse(parts[1]);

        return minutes * 60 + seconds;
    }
}
