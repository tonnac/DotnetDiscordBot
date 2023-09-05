namespace DiscordBot;

public class Roulette
{
    public Roulette(DateTime time, string winner, List<string> members)
    {
        Time = time;
        Winner = winner;
        Members = members;
    }

    public DateTime Time { get; }
    public string Winner { get; }
    public List<string> Members { get; }
}