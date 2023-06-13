using System.Text.RegularExpressions;
using DisCatSharp;
using DisCatSharp.Entities;

namespace DiscordBot;

public static class Utility
{
    private static readonly int BarLength = 15;

    private static class DefaultOpts
    {
        public static readonly float HoursPerDay = 24;
        public static readonly float DaysPerWeek = 7;
        // public static readonly float WeeksPerMonth = 4;
        public static readonly float MonthsPerYear = 12;
        public static readonly float DaysPerYear = 365.25f;
    }

    // private static readonly Dictionary<string, List<string>> UnitMap = new Dictionary<string, List<string>>()
    // {
    //     { "ms", new List<string> {"ms", "milli", "millisecond", "milliseconds"} },
    //     { "s", new List<string> {"s", "sec", "secs", "second", "seconds"} },
    //     { "m", new List<string> {"m", "min", "mins", "minute", "minutes"} },
    //     { "h", new List<string> {"h", "hr", "hrs", "hour", "hours"} },
    //     { "d", new List<string> {"d", "day", "days"} },
    //     { "w", new List<string> {"w", "week", "weeks"} },
    //     { "mth", new List<string> {"mon", "mth", "mths", "month", "months"} },
    //     { "y", new List<string> {"y", "yr", "yrs", "year", "years"} },
    // };
    
    public static string ToDuration(this TimeSpan time)
    {
        return time.Hours > 0 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
    }
    public static List<List<T>> Partition<T>(this List<T> values, int chunkSize)
    {
        return values.Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }

    public static string ProgressBar(TimeSpan current, TimeSpan total, int? barLength = null)
    {
        barLength ??= BarLength;
        
        var diff = Convert.ToInt32(current.TotalSeconds / total.TotalSeconds * barLength); // Bar's length
        var bar = "\u25B6 ";
        for (int i = 0; i < barLength; i++)
        {
            if (i == diff) bar += "\uD83D\uDD18";
            else bar += "▬";
        }

        bar += $"[{current.ToDuration()} / {total.ToDuration()}]".InlineCode();
        return bar;
    }

    public static TimeSpan GetTime(string query)
    {
        Regex regex = new Regex("[0-9.]+[^0-9 ]*");

        var mc = regex.Matches(query);

        if (mc.Count == 0)
        {
            throw new Exception("can't find unit value");
        }

        Dictionary<string, float> GetUnitValues()
        {
            Dictionary<string, float> unitValues = new Dictionary<string, float> { { "ms", 0.001f }, { "s", 1 }, { "m", 60 }, { "h", 3600 } };

            unitValues.Add("d", DefaultOpts.HoursPerDay * unitValues["h"]);
            unitValues.Add("w", DefaultOpts.DaysPerWeek * unitValues["d"]);
            unitValues.Add("mth", DefaultOpts.DaysPerYear / DefaultOpts.MonthsPerYear * unitValues["d"]);
            unitValues.Add("y", DefaultOpts.DaysPerYear * unitValues["d"]);

            return unitValues;
        }

        int seconds = 0;
        foreach (Match match in mc)
        {
            Regex valueRegex = new Regex("[0-9.]+");
            Regex unitRegex = new Regex("[a-z]+");

            var value = Convert.ToInt32(valueRegex.Match(match.Value).Value);
            var unit = unitRegex.Match(match.Value);

            var unitValues = GetUnitValues();

            if (unitValues.TryGetValue(unit.Value, out float unitValue))
            {
                seconds += Convert.ToInt32(unitValue * value);
            }
            else
            {
                throw new Exception("can't find unit value");
            }
        }

        return new TimeSpan(0, 0, seconds);
    }

    public static string MakeYouTubeShareUrl(string uri, TimeSpan seekTime = default)
    {
        string? youTubeKey = GetYouTubeKey(uri);
        if (string.IsNullOrEmpty(youTubeKey))
            return String.Empty;

        if (seekTime.TotalSeconds > 0)
            return $"https://youtu.be/{youTubeKey}" + $"?t={(int) seekTime.TotalSeconds}";
        return $"https://youtu.be/{youTubeKey}";
    }
    public static string MakeYouTubeThumbnailUrl(string uri)
    {
        string? youTubeKey = GetYouTubeKey(uri);
        if (string.IsNullOrEmpty(youTubeKey))
            return String.Empty;

        return $"https://img.youtube.com/vi/{youTubeKey}/mqdefault.jpg";
    }
    public static string? GetYouTubeKey(string uri)
    {
        Regex pattern = new Regex(@"v=([0-9A-Za-z_-]{11})");

        Match match = pattern.Match(uri);
        if (match.Success)
        {
            string key = match.Groups[1].Value;
            return key;
        }

        return null;
    }

    public static string GetMemberDisplayName(DiscordMember member)
    {
        return string.IsNullOrEmpty(member.Nickname) ? member.Username : member.Nickname;
    }
}