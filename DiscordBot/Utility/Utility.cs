namespace DiscordBot.Utility;

public static class Utility
{
    public static string ToDuration(this TimeSpan time)
    {
        return time.Hours > 0 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
    }
}