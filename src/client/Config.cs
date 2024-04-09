namespace Saveshare;

public sealed class Config {
    public string ServerAddr { get; set; } = "http://YOUR-SERVER:8080";
    public bool DiscordLinked { get; set; } = false;
    public List<string> WatchedWorlds { get; set; } = new();
}
