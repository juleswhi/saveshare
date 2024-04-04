using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace Saveshare;

class ModEntry : Mod
{
    public Config? _config { get; private set; }
    private static bool s_isConnected { get; set; } = false;

    public override async void Entry(IModHelper helper)
    {
        // Grab config 
        _config = Helper.ReadConfig<Config>();
        Connection.BaseIp = _config.ServerAddr;

        helper.Events.GameLoop.UpdateTicked += OnTick;

        // Display connection statius at any point
        helper.Events.Input.ButtonPressed += (_, e) => {
            if(e.Button == SButton.F1) {
                string message = 
                    $@"Connection Status: ^{
                        (s_isConnected 
                         ? "Connected!" 
                         : "Not Connected :("
                        )}";
                Game1.activeClickableMenu = new DialogueBox(message);
            }
        };

        List<Response> saveGameChoices = new() {
            new Response($"1", $"Save Today"),
            new Response($"2", $"Save Tonight"),
            new Response($"3", $"Don't Save")
        };

        helper.Events.GameLoop.SaveLoaded += (s, e) => {
            helper.Events.Input.ButtonPressed += (bs, be) => {
                if(be.Button == SButton.F3) {
                    string message = $"Save options:";
                    Game1.currentLocation.createQuestionDialogue(
                            message, 
                            saveGameChoices.ToArray(), 
                            new GameLocation.afterQuestionBehavior(
                                SaveGameResponse));
                }
            };
        };
        
        // Ensure not blank
        if(!Connection.CheckValidIp()) {
            Monitor.LogOnce($"Invalid IP Address in configuration file.", 
                    LogLevel.Warn);
            return;
        }

        // http://domain.name/health
        s_isConnected = await Connection.CheckHealth();

        // Ensure connection
        if(!s_isConnected) {
            Monitor.Log(
                    $@"Could not establish connection with server. Please ensure IP Address is correct and server is online.", 
                    LogLevel.Warn);
            return;
        }

        Monitor.Log($"Connected to the server!", LogLevel.Info);
    }

    private async void OnTick(object? sender, UpdateTickedEventArgs tickNumber)
    {
        // Only run every ~2-ish seconds
        // Snappy enough but doesnt do too many requests
        if(!Game1.hasLoadedGame || !tickNumber.IsMultipleOf(120)) {
            return;
        }

        var result = await Connection.CheckHealth();

        if(result == s_isConnected) {
            return;
        }

        (string, int) message = ($"Connected To Server!", 1);

        if(!result) {
            message = ($"You have been disconnected from the server :(", 3);
        }

        Game1.addHUDMessage(
                new HUDMessage(message.Item1, message.Item2));

    }

    private void SaveGameResponse(Farmer who, string id) {
        Game1.addHUDMessage(new HUDMessage($"Farmer {who} chose {id}", 1));
    }
}
