namespace Saveshare;

public sealed class Config {
    public string ServerAddr { get; set; } = "";
    public bool DiscordLinked { get; set; } = false;
    public List<string> WatchedWorlds { get; set; } = new();
}
