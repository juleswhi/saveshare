namespace SaveshareServer;

internal static class Logger {

    public static void Log(string message) {
        Console.WriteLine($"{DateTime.Now.ToString()}: {message}");
    }

    public static void Warn(string message) {
        var before = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"--- {DateTime.Now.ToString()}: {message} ---");
        Console.ForegroundColor = before;
    }
}
