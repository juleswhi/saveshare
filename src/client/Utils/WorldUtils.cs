using StardewValley;
using StardewModdingAPI;
namespace Saveshare;

internal static class Utils {

    public static IModHelper? Helper { get; set; }


    public static bool IsWatchingWorld() {
        if(Helper is null || !Game1.hasLoadedGame) {
            return false;
        }

        var config = Helper.ReadConfig<Config>();

        if(!config.WatchedWorlds.Contains(Game1.uniqueIDForThisGame)) {
            return false;
        }

        return true;
    }

    public static string PlayerFile() {
        string? path = GetSaveDirectory();
        if(path is null) {
            return "";
        }

        var filename = path.Split('/')[^1];
        return $"{path}/{filename}";
    }

    public static string GameFile() {
        string? path = GetSaveDirectory();

        if(path is null) {
            return "";
        }

        return $"{path}/SaveGameInfo";
    }

    private static string? GetSaveDirectory() {
        if(Helper is null) return null;

        string configFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

        string saveDirectory = 
            Constants.SavesPath;

        string[] dirs = Directory.GetDirectories(saveDirectory);
        foreach (string dir in dirs)
        {
            if(dir.Contains(Game1.GetSaveGameName())) {
                return dir;
            }

        }

        return null;
    }

}
