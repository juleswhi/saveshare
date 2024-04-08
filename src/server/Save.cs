namespace SaveshareServer;

internal class Save {

    public Guid ID { get; set; }
    public string XML { get; set; }
    public string GameFile { get; set; }
    public ulong WorldID { get; set; }
    public ulong CurrentHostID { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } 

    public Save() {
        ID = Guid.NewGuid();
        XML = "";
        GameFile = "";
        Name = "";
    }
}
