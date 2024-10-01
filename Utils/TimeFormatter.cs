using System;
using UnityEngine;

public class TimeFormatter
{
    public const string TIMER_FORMAT = "{0:D2}:{1:D2}";
    public const string CLOCK_FORMAT = "{0:D2}:{1:D2}:{2:D2}";
    public const string DAYS_FORMAT = "{0:D2}D:{1:D2}:{2:D2}:{3:D2}";

    public static string MilisecondsFormatTimer(float milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return string.Format(TIMER_FORMAT, Mathf.FloorToInt((float)timeSpan.TotalMinutes), timeSpan.Seconds);
    }

    public static string MilisecondsFormatClock(float milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return string.Format(CLOCK_FORMAT, Mathf.FloorToInt((float)timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds);
    }

    public static string MilisecondsFormatDays(float milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return string.Format(DAYS_FORMAT, Mathf.FloorToInt((float)timeSpan.TotalDays), timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    public static string SecondsFormatTimer(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return string.Format(TIMER_FORMAT, Mathf.FloorToInt((float)timeSpan.TotalMinutes), timeSpan.Seconds);
    }

    public static string SecondsFormatClock(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return string.Format(CLOCK_FORMAT, Mathf.FloorToInt((float)timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds);
    }

    public static string SecondsFormatDays(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return string.Format(DAYS_FORMAT, Mathf.FloorToInt((float)timeSpan.TotalDays), timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    public static string FormatAsTimer(float input)
    {
        var miliseconds = (input * 1000) % 1000;
        var seconds = input % 60;
        var minutes = Mathf.FloorToInt(input / 60);
        return minutes > 0 ? $"{minutes:00}:{seconds:00}.<size=30>{miliseconds:000}</size>" : $"{seconds:00}.<size=30>{miliseconds:000}</size>";
    }
}
