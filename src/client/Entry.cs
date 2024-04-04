using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace Saveshare;

class ModEntry : Mod
{
    public Config? _config { get; private set; }
    private IModHelper? s_helper { get; set; }
    private static bool s_isConnected { get; set; } = false;

    public override async void Entry(IModHelper helper)
    {
        // Grab config 
        _config = Helper.ReadConfig<Config>();
        Connection.BaseIp = _config.ServerAddr;

        s_helper = helper;

        helper.Events.GameLoop.UpdateTicked += OnTick;
        helper.Events.GameLoop.UpdateTicked += (s, e) => {
            if(!e.IsMultipleOf(300)) {
                return;
            }

            HandleMultiplayerPeers();
        };

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

        // Save types
        List<Response> saveGameChoices = new() {
            new Response($"1", $"Save Now"),
            new Response($"2", $"Save After Day Ends"),
            new Response($"3", $"Don't Save")
        };

        helper.Events.Input.ButtonPressed += (bs, be) => {
            if(!Game1.hasLoadedGame) {
                return;
            }

            if(be.Button == SButton.F3) {
                string message = $"Save options:";
                Game1.currentLocation.createQuestionDialogue(
                        message, 
                        saveGameChoices.ToArray(), 
                        new GameLocation.afterQuestionBehavior(
                            SaveGameResponse));
            }
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

    private void SaveGameResponse(Farmer who, string id) {
        Game1.addHUDMessage(new HUDMessage($"Farmer {who} chose {id}", 1));

        if(id != "1") {
            return;
        }

        string path = PlayerFile();

        if(path == "") {
            return;
        }

        Monitor.Log($"Found path: {path}", LogLevel.Info);

        string file = File.ReadAllText(path);

        // Monitor.Log($"{file}", LogLevel.Info);
    }

    private string PlayerFile() {
        string path = GetSaveDirectory();
        var filename = path.Split('/')[^1];
        return $"{path}/{filename}";
    }

    private string GameFile() {
        string path = GetSaveDirectory();
        return $"{path}/SaveGameInfo";
    }

    private string GetSaveDirectory() {
        if(s_helper is null) return "";

        string configFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

        string saveDirectory = 
            $"{configFolder}/StardewValley/Saves/";

        if (Directory.Exists(saveDirectory))
        {
            string[] dirs = Directory.GetDirectories(saveDirectory);
            foreach (string dir in dirs)
            {
                if(dir.Contains(Game1.GetSaveGameName())) {
                    Monitor.Log($"{dir.ToString()}", LogLevel.Info);
                    return dir;
                }
                
            }
            
        }
        return "";
    }


    private void HandleMultiplayerPeers() {
        if(s_helper is null) return;

        foreach(var peer in s_helper.Multiplayer.GetConnectedPlayers()) {
            if(!peer.HasSmapi) {
                return;
            }

        }
    }
}
