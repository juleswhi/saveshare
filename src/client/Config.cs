namespace Saveshare;

public sealed class Config {
    public string ServerAddr { get; set; } = "";
    public bool DiscordLinked { get; set; } = false;
    public List<WorldWatch> WatchedWorlds { get; set; } = new();
}
