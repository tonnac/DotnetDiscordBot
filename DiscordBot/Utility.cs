using DisCatSharp;

namespace DiscordBot;

public static class Utility
{
    private static readonly int BarLength = 15;
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
}