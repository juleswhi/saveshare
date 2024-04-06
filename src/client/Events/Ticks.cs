using StardewModdingAPI.Events;
using StardewValley;

namespace Saveshare;

internal static class Ticks {
    public static async void CheckHealth(object? sender, UpdateTickedEventArgs tickNumber) {
        if(!Game1.hasLoadedGame || !tickNumber.IsMultipleOf(120)) {
            return;
        }

        var result = await Connection.CheckHealth();

        if(result == Connection.IsConnected) {
            return;
        }

        string message; 
        int code;

        if(result) {
            message = $"Connected!";
            code = 1;
        }
        else {
            message = $"Disconnected :(";
            code = 3;
        }

        Game1.addHUDMessage(
                new HUDMessage(message, code));
    }
}
