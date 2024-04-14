using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Saveshare;

internal static class Load {

    public static void GenerateID(object? sender, GameLaunchedEventArgs e) {
        var config = Utils.Helper?.ReadConfig<Config>();

        if(config is null) {
            return;
        }
    }

    public static async void OnLoad(object? sender, GameLaunchedEventArgs e) {
        string path = Constants.SavesPath;
        var config = Utils.Helper?.ReadConfig<Config>();

        if(config is null) {
            Utils.Monitor?.Log($"Configuration file was null", LogLevel.Warn);
            return;
        }

        foreach(var save in config.WatchedWorlds) {
            var parsed = ulong.TryParse(save.Split("_")[^1], out var id);

            if(!parsed) {
                Utils.Monitor?.Log($"Invalid save in saveshare/config.json", LogLevel.Warn);
                continue;
            }

            // Give each query a 5 second timeout
            var getxml = Connection.GetXML(id);
            var timeout = Task.Delay(TimeSpan.FromSeconds(5));
            var completed = await Task.WhenAny(getxml, timeout);

            if(completed != getxml) {
                Utils.Monitor?.Log($"Could not access XML in a timely manner :(", LogLevel.Warn);
                continue;
            }

            (string xml, string worldsave, _, _, int version, string name) = (await getxml).Value;

            string worldPath = $"{path}/{name}";
            string gamePath = $"{path}/{name}/{name}";
            string savePath = $"{path}/{name}/SaveGameInfo";

            if(!TryCreateDirectory(worldPath))
                continue;

            if(!TryCreateFile(gamePath))
                continue;

            if(!TryCreateFile(savePath))
                continue;

            try {
                File.WriteAllText(gamePath, xml);
                File.WriteAllText(savePath, worldsave);
            }
            catch(Exception) {
                Utils.Monitor?.Log($"Could not write to files. Could be a permissions issue.", LogLevel.Warn);
            }
        }
    }

    private static bool TryCreateFile(string path) {
        try {
            if(!File.Exists(path)) {
                File.Create(path);
            }
            return true;
        }
        catch(Exception) {
            Utils.Monitor?.Log($"Could not create file: {path}.", LogLevel.Warn);
            return false;
        }
    }

    private static bool TryCreateDirectory(string path) {
        try {
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            return true;
        }
        catch(Exception) {
            return false;
        }

    }
}
