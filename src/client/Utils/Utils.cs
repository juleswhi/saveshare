using StardewValley;
using StardewModdingAPI;
namespace Saveshare;

internal static class Utils {

    public static IModHelper? Helper { get; set; }
    public static IMonitor? Monitor { get; set; }

    public static bool IsWatchingWorld() {
        if(Helper is null || !Game1.hasLoadedGame) {
            return false;
        }

        var config = Helper.ReadConfig<Config>();

        if(!config.WatchedWorlds.Any(x => x == Constants.SaveFolderName)) {
            return false;
        }

        return true;
    }
}
