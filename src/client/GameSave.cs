namespace Saveshare;


internal class GameSave {
    public string XML { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public int Version { get; set; } = 0;
}
