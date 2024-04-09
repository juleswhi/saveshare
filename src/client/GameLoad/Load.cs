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
            Utils.Monitor?.Log($"save name is: {save}", LogLevel.Info);
            _ = ulong.TryParse(save.Split("_")[^1], out var id);
            Utils.Monitor?.Log($"save id is: {id}", LogLevel.Info);

            var getxml = Connection.GetXML(id);

            var timeout = Task.Delay(TimeSpan.FromSeconds(7));

            var completed = await Task.WhenAny(getxml, timeout);

            if(completed != getxml) {
                Utils.Monitor?.Log($"Getting XML Took wayy to long", LogLevel.Warn);
                return;
            }

            (string xml, string worldsave, _, _, int version, string name) = (await getxml).Value;

            Utils.Monitor?.Log($"Name is: {name}", LogLevel.Info);

            string newPath = $"{path}/{name}/{name}";

            Utils.Monitor?.Log($"Path is: {newPath}", LogLevel.Info);

            if(!Directory.Exists($"{path}/{name}")) {
                Utils.Monitor?.Log($"Direcotry does not exist, creating new one", LogLevel.Info);
                Directory.CreateDirectory($"{path}/{name}");
            }

            if(!File.Exists(newPath)) {
                Utils.Monitor?.Log($"File does not exist, creating new one", LogLevel.Info);
                File.Create(newPath);
            }

            if(!File.Exists($"{path}/{name}/SaveGameInfo")) {
                Utils.Monitor?.Log($"File does not exist, creating new one", LogLevel.Info);
                File.Create($"{path}/{name}/SaveGameInfo");
            }

            File.WriteAllText(newPath, xml);
            File.WriteAllText($"{path}/{name}/SaveGameInfo", worldsave);

            Utils.Monitor?.Log($"corect path: {newPath}, where version is: {version}", LogLevel.Info);
        }
    }
}
