using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Saveshare;

internal static class Load {
    public static async void OnLoad(object? sender, GameLaunchedEventArgs e) {

        Utils.Monitor?.Log($"Pulling Saves", LogLevel.Info);

        string path = Constants.SavesPath;

        var config = Utils.Helper?.ReadConfig<Config>();

        if(config is null) {
            return;
        }

        foreach(var save in config.WatchedWorlds) {
            (string xml, string worldsave, _, _, int version, string name) = await Connection.GetXML(save.WorldID);

            string newPath = $"{path}/{name}_{save.WorldID}/{name}_{save.WorldID}";

            if(!File.Exists(newPath)) {
                File.Create(newPath);
            }

            using StreamWriter sw = new StreamWriter(newPath);

            sw.Write($"{xml}");

            Utils.Monitor?.Log($"Using streamwriter in the corect path: {save.WorldName}", LogLevel.Info);
            Utils.Monitor?.Log($"Current Version: {version} of {save.WorldName}", LogLevel.Info);
        }
    }
}
