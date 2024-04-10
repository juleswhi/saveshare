using Newtonsoft.Json;

namespace SaveshareServer;

internal class Config {
    public string HostAddress { get; set; } = string.Empty;

    public ushort TCP_PROTOCOL_VERSION { get; set; } = 1;

    public string[] Routes = [
        "health",
        "xml-save",
        "xml-load"
    ];

    public void SerialiseWrite() {
        File.WriteAllText(
                Program.s_configPath, 
                JsonConvert.SerializeObject(this)
                );
    }
}
