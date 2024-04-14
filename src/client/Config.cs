namespace Saveshare;

public sealed class Config {
    public string ServerAddr { get; set; } = "your-server-here";
    public int Port { get; set; } = 8080;
    public bool DiscordLinked { get; set; } = false;
    public ushort PacketVersion { get; set; } = 1;
    public List<string> WatchedWorlds { get; set; } = new();
    public ulong UniqueID { get; set; } = 0;
}
