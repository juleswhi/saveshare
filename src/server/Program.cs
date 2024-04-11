using static SaveshareServer.Messages;
using Newtonsoft.Json;

namespace SaveshareServer;

internal class Program {

    public const string s_configPath = "config.json";

    public static async Task Main(string[] args)  {
        await new Server(ReadConfig()).Run();
    }

    private static Config ReadConfig() {
        if(!File.Exists(s_configPath)) {
            Logger.Warn(CONFIG_NULL);
            Logger.Log(SERVER_STOP);
            Environment.Exit(1);
        }

        var file = File.ReadAllText(s_configPath);
        Config? config = new();
        
        try {
            config = JsonConvert.DeserializeObject<Config>(file);
        }
        catch(Exception) {
            Logger.Warn(INVALID_JSON);
            Logger.Log(SERVER_STOP);
            Environment.Exit(1);
        }

        if(config is null) {
            Logger.Warn(INVALID_JSON);
            Logger.Log(SERVER_STOP);
            Environment.Exit(1);
        }

        return config!;
    }
}
