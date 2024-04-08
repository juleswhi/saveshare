using StardewModdingAPI.Events;
using StardewValley;

namespace Saveshare;

internal static class Saves {
    public static async void GetSaveInfo(object? s, SaveLoadedEventArgs e) {
            if(!Game1.hasLoadedGame) return;

            var worldid = Game1.uniqueIDForThisGame;
            (string xml, string gamefile, ulong id, ulong hostid, int version, string name) 
                = await Connection.GetXML(worldid);
    }
}
