using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Saveshare;

internal static class Buttons {

    public static void WatchWorldMenu
        (object? s, ButtonPressedEventArgs e) {
            if(!Game1.hasLoadedGame || e.Button != SButton.F3) 
                return;

            Response[] saveChoices = new Response[] {
                new Response($"1", $"Save"),
                new Response($"3", $"Don't Save")
            };

            Response[] watchChoices = new Response[] {
                new Response($"1", $@"{(
                            Utils.IsWatchingWorld() 
                            ? "Stop watching world"
                            : "Watch world"
                            )}"),
                    new Response($"2", $"Back to game"),
            };

            if(Game1.player.IsMainPlayer) {
                Game1.currentLocation.createQuestionDialogue(
                        $"Save Options:", 
                        saveChoices.ToArray(), 
                        new GameLocation.afterQuestionBehavior(
                            SaveGameResponse));
            }

            else {
                Game1.currentLocation.createQuestionDialogue(
                        $"Watch Options:", 
                        watchChoices.ToArray(), 
                        new GameLocation.afterQuestionBehavior(
                            WatchWorldResponse));
            }
        }

    public static void ConnectionStatusMenu
        (object? s, ButtonPressedEventArgs e) {
            if(e.Button != SButton.F1) 
                return;

            string connStatus = Connection.IsConnected 
                ? "Connected!" 
                : "Not Connected :(";

            string message = $"Connection Status: ^{connStatus}";
            Game1.activeClickableMenu = new DialogueBox(message);
        }

    private static async void SaveGameResponse(Farmer who, string id) {

        if(Utils.Helper is null ||
            Constants.SaveFolderName is null) {
            return;
        }

        var config = Utils.Helper.ReadConfig<Config>();

        if(id == "1" && !Utils.IsWatchingWorld()) {
            config.WatchedWorlds.Add(
                    new WorldWatch {
                    WorldID = Game1.uniqueIDForThisGame,
                    WorldName = Constants.SaveFolderName
                    });

        }
        else if (id == "3") {
            config.WatchedWorlds = config.WatchedWorlds.Where(
                    x => x.WorldID != Game1.uniqueIDForThisGame).ToList();
        }

        Utils.Helper.WriteConfig<Config>(config);

        string xmlPath = Utils.PlayerFile();
        string gamePath = Utils.GameFile();

        if(xmlPath == "" || gamePath == "") {
            return;
        }

        string xml = File.ReadAllText(xmlPath);

        string gameData = File.ReadAllText(gamePath);

        await Connection.SendXML(
                xml, 
                gameData, 
                Game1.uniqueIDForThisGame, 
                (ulong)Game1.player.UniqueMultiplayerID);
        Utils.Monitor?.Log($"Sent xml to server", LogLevel.Info);

        Game1.addHUDMessage(
                new HUDMessage($"Sent Save File To Server!", 1));
    }

    private static void WatchWorldResponse(Farmer who, string id) {
        if(
            id != "1"
            || Utils.IsWatchingWorld()
            || Utils.Helper is null
            || Constants.SaveFolderName is null) {
            return;
        }

        var config = Utils.Helper.ReadConfig<Config>();

        config.WatchedWorlds.Add(
                new WorldWatch {
                WorldID = Game1.uniqueIDForThisGame,
                WorldName = Constants.SaveFolderName
                });

        Utils.Helper.WriteConfig<Config>(config);
    }
}
