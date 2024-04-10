namespace Saveshare;

public sealed class Config {
    public string ServerAddr { get; set; } = "http://your-server-here:8080/";
    public bool DiscordLinked { get; set; } = false;
    public ushort PacketVersion { get; set; } = 1;
    public List<string> WatchedWorlds { get; set; } = new();
}
