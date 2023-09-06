namespace DiscordBot;

public class Roulette
{
    public Roulette(DateTime time, string winner, List<string> members)
    {
        Time = time;
        Winner = winner;
        Members = members;
    }
    public Roulette(DateTime time, string winner, List<string> members, string messageLink)
    {
        Time = time;
        Winner = winner;
        Members = members;
        MessageLink = messageLink;
    }

    public DateTime Time { get; }
    public string Winner { get; }
    public string? MessageLink { get; }
    public List<string> Members { get; }
}