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
            (string xml, string worldsave, var a, var b, int version) = await Connection.GetXML(save.WorldID);

            using StreamWriter sw = new StreamWriter(
                    $"{path}/{save.WorldName}/{save.WorldName}");

            sw.Write($"{xml}");

            Utils.Monitor?.Log($"Using streamwriter in the corect path: {save.WorldName}", LogLevel.Info);
            Utils.Monitor?.Log($"Current Version: {version} of {save.WorldName}", LogLevel.Info);
        }
    }
}
