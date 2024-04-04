using Newtonsoft.Json;

namespace SaveshareServer;

internal class Config {
    public string HostAddress { get; set; } = string.Empty;

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
