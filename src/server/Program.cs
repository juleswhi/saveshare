using Newtonsoft.Json;

namespace SaveshareServer;
internal class Program {

    public const string s_configPath = "config.json";

    public static async Task Main(string[] args)  {
        await new Server(ReadConfig()).Run();
    }

    private static Config ReadConfig() {
        if(!File.Exists(s_configPath)) {
            Logger.Warn($"No config file found at path");
            Logger.Log($"Stopping server.");
            Environment.Exit(1);
        }

        var file = File.ReadAllText(s_configPath);
        Config? config = new();
        
        try {
            config = JsonConvert.DeserializeObject<Config>(file);
        }
        catch(Exception) {
            Logger.Warn($"There is not valid json in the config.json");
            Logger.Log($"Stopping Server.");
            Environment.Exit(1);
        }

        if(config is null) {
            Logger.Warn($"There is not valid json in the config.json");
            Logger.Log($"Stopping Server.");
            Environment.Exit(1);
        }

        Logger.Log($"Parsed Json");

        return config!;

    }
}
