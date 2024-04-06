using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace Saveshare;

class ModEntry : Mod
{
    public override async void Entry(IModHelper helper)
    {
        Connection.BaseIp = Helper.ReadConfig<Config>().ServerAddr;
        Utils.Helper = helper;

        helper.Events.GameLoop.UpdateTicked += Saveshare.Ticks.CheckHealth;
        helper.Events.GameLoop.SaveLoaded += Saveshare.SaveLoaded.GetSaveInfo;

        // Ensure not blank
        if(!Connection.CheckValidIp()) {
            Monitor.LogOnce($"Invalid IP Address in configuration file.", 
                    LogLevel.Warn);
            return;
        }

        Connection.IsConnected = await Connection.CheckHealth();

        // Ensure connection
        if(!Connection.IsConnected) {
            Monitor.Log(
                    $@"Could not establish connection with server. Please ensure IP Address is correct and server is online.", 
                    LogLevel.Warn);
            return;
        }

        Monitor.Log($"Connected to the server!", LogLevel.Info);
    }
}
