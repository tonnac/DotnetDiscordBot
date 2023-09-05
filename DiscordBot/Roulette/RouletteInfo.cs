namespace DiscordBot;

public class RouletteInfo
{
    public RouletteInfo(int numberOfMan, int losses, int takingPartCount)
    {
        NumberOfMan = numberOfMan;
        Losses = losses;
        TakingPartCount = takingPartCount;
    }

    public int NumberOfMan { get; }
    public int Losses { get; }
    public int TakingPartCount { get; }
}