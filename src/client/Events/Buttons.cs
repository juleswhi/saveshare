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


            // TODO: 
            // Check if main user has saves on, and then let the user press f3

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
            if(e.Button != SButton.F5) 
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
                    Constants.SaveFolderName
                    );

        }
        else if (id == "3") {
            config.WatchedWorlds = config.WatchedWorlds.Where(
                    x => x != Constants.SaveFolderName).ToList();
        }

        Utils.Helper.WriteConfig<Config>(config);

        string xmlPath = $"{Constants.SavesPath}/{Constants.SaveFolderName}/{Constants.SaveFolderName}";
        string gamePath = $"{Constants.SavesPath}/{Constants.SaveFolderName}/SaveGameInfo";

        Utils.Monitor?.Log($"xml path: {xmlPath}", LogLevel.Info);
        Utils.Monitor?.Log($"game path: {gamePath}", LogLevel.Info);

        if(xmlPath == "" || gamePath == "") {
            return;
        }

        string xml = File.ReadAllText(xmlPath);
        string gameData = File.ReadAllText(gamePath);

        await Connection.SendXML(
                xml, 
                gameData, 
                Game1.uniqueIDForThisGame, 
                (ulong)Game1.player.UniqueMultiplayerID,
                Constants.SaveFolderName
                );
        Utils.Monitor?.Log($"Sent xml to server", LogLevel.Info);

        Game1.addHUDMessage(
                new HUDMessage($"Sent Save File To Server!", 1));
    }

    private static async void WatchWorldResponse(Farmer who, string id) {
        if(
            id != "1"
            || Utils.Helper is null
            || Constants.SaveFolderName is null) {
            return;
        }

        var isWatching = Utils.IsWatchingWorld();
        var config = Utils.Helper.ReadConfig<Config>();

        if(isWatching) {
            config.WatchedWorlds = config.WatchedWorlds.Where(
                    x => x != Constants.SaveFolderName).ToList();
        }

        else {
            var world = await Connection.GetXML(Game1.uniqueIDForThisGame);

            if(world is null) {
                Utils.Monitor?.Log(
                        $"This world: {Game1.uniqueIDForThisGame} does not have a save file uploaded. Please tell the host to save", 
                        LogLevel.Warn);
                Game1.addHUDMessage(
                        new HUDMessage($"No Save File Found :(", 3));
            }

            else {
                config.WatchedWorlds.Add(
                        world.Value.Item6
                        );
            }


        }

        Utils.Helper.WriteConfig<Config>(config);
    }
}
