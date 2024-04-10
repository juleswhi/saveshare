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
                Utils.Monitor?.Log($"Could not access XML in a timely manner :(", LogLevel.Warn);
                return;
            }

            (string xml, string worldsave, _, _, int version, string name) = (await getxml).Value;

            string newPath = $"{path}/{name}/{name}";

            try {
                if(!Directory.Exists($"{path}/{name}")) {
                    Directory.CreateDirectory($"{path}/{name}");
                }
            }
            catch(Exception) {
                Utils.Monitor?.Log($"Could not create directory.", LogLevel.Warn);
                return;
            }

            try {
                if(!File.Exists(newPath)) {
                    Utils.Monitor?.Log($"File does not exist, creating new one", LogLevel.Info);
                    File.Create(newPath);
                }
            }
            catch(Exception) {
                Utils.Monitor?.Log($"Could not create game file.", LogLevel.Warn);
                return;
            }

            try {
                if(!File.Exists($"{path}/{name}/SaveGameInfo")) {
                    Utils.Monitor?.Log($"File does not exist, creating new one", LogLevel.Info);
                    File.Create($"{path}/{name}/SaveGameInfo");
                }
            }
            catch(Exception) {
                Utils.Monitor?.Log($"Could not create game save file.", LogLevel.Warn);
                return;
            }

            try {
                File.WriteAllText(newPath, xml);
                File.WriteAllText($"{path}/{name}/SaveGameInfo", worldsave);
            }
            catch(Exception) {
                Utils.Monitor?.Log($"Could not write to files.", LogLevel.Warn);
            }
        }
    }
}
