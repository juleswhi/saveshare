namespace SaveshareServer;

internal static class Messages {
    public const string SERVER_START = $"Starting Server..";
    public const string SERVER_STOP = $"Stopping Server."; 

    public const string CONFIG_NULL = $"Configuration file was null.";
    public const string REQ_NULL = $"Request was null.";
    public const string LISTENER_FAILED = $"Listener failed. Panicing";

    public const string INVALID_JSON = $"Invalid json.";

    public const string NULL_PACKET = $"Null packet received.";
    public const string INVALID_PACKET = $"Invalid packet type.";

    public static readonly Func<Exception, string> INVALID_TCP = 
        ex => $"Invalid IP/TCP Address. " + ex.Message;
    public static readonly Func<Config, string> SERVER_PORT = 
        c => $"Server running on: {c.HostAddress}";
}
