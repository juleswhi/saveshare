using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Saveshare;

internal static class Load {
    public static async void OnLoad(object? sender, GameLaunchedEventArgs e) {

        Utils.Monitor?.Log($"Pulling Saves", LogLevel.Info);

        string path = Constants.SavesPath;

        var config = Utils.Helper?.ReadConfig<Config>();

        if(config is null) {
            Utils.Monitor?.Log($"Configuration file was null", LogLevel.Warn);
            return;
        }

        foreach(var save in config.WatchedWorlds) {
            _ = ulong.TryParse(save.Split("_")[0], out var id);

            (string xml, string worldsave, _, _, int version, string name) = await Connection.GetXML(id);

            Utils.Monitor?.Log($"Name is: {name}", LogLevel.Info);

            string newPath = $"{path}/{name}_{id}/{name}_{id}";

            Utils.Monitor?.Log($"Path is: {newPath}", LogLevel.Info);

            if(!File.Exists(newPath)) {
                Utils.Monitor?.Log($"File does not exist, creating new one", LogLevel.Info);
                File.Create(newPath);
            }

            File.WriteAllText(newPath, xml);

            Utils.Monitor?.Log($"corect path: {newPath}, where version is: {version}", LogLevel.Info);
        }
    }
}
