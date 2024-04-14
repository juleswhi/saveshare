using StardewModdingAPI;

namespace Saveshare;

class ModEntry : Mod
{
    public override async void Entry(IModHelper helper)
    {
        Utils.Helper = helper;
        Utils.Monitor = Monitor;

        Connection.Connect();

        helper.Events.GameLoop.UpdateTicked += Saveshare.Ticks.CheckHealth;
        helper.Events.Input.ButtonPressed += Saveshare.Buttons.WatchWorldMenu;
        helper.Events.Input.ButtonPressed += Saveshare.Buttons.ConnectionStatusMenu;
        helper.Events.GameLoop.GameLaunched += Saveshare.Load.OnLoad;
        helper.Events.GameLoop.GameLaunched += Saveshare.Load.GenerateID;

        if(!Connection.CheckValidIp()) {
            Monitor.LogOnce($"Invalid IP Address in configuration file.",
                    LogLevel.Warn);
            return;
        }

        Connection.IsConnected = await Connection.CheckHealth();

        if(!Connection.IsConnected) {
            Monitor.Log(
                    $@"Could not establish connection with server. Please ensure IP Address is correct and server is online.",
                    LogLevel.Warn);
            return;
        }

        Monitor.Log($"Connected to the server!", LogLevel.Info);
    }
}
