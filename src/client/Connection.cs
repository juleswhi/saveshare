using System.Net.Sockets;
using Newtonsoft.Json;

namespace Saveshare;

internal static class Connection {
    public static string BaseIp { get; set; } = string.Empty;
    public static int Port { get; set; } = -1;
    public static bool IsConnected { get; set; } = false;
    private static TcpClient? s_client { get; set; }

    public static void Connect() {
        if(Utils.Helper is null)
            return;

        Connection.BaseIp = Utils.Helper.ReadConfig<Config>().ServerAddr;
        Connection.Port = Utils.Helper.ReadConfig<Config>().Port;

        Utils.Monitor?.Log($"IP: {Connection.BaseIp}, PORT: {Connection.Port}", StardewModdingAPI.LogLevel.Info);

        s_client = new(BaseIp, Port);

        if(s_client is null) {
            Utils.Monitor?.Log($"Client is null", StardewModdingAPI.LogLevel.Info);
        }

        Utils.Monitor?.Log($"connected to client", StardewModdingAPI.LogLevel.Info);
    }

    public static async Task<bool> CheckHealth() {
        return await new TcpPacket {
            Version = 1,
            Command = TcpPacket.TcpCommand.Health
        }.SendAsync(s_client) is not null;
    }

    public static async Task SendXML(
            string xml,
            string worldsave,
            ulong worldid,
            ulong hostid,
            string name) {

        string json = JsonConvert.SerializeObject(
                (xml, worldsave, worldid, hostid, name)
                );

        await new TcpPacket {
            Version = 1,
            Command = TcpPacket.TcpCommand.SaveXML,
            Data = json
        }.SendAsync(s_client);
    }

    public static async Task<(string, string, ulong, ulong, int, string)?> GetXML(ulong id) {
        var packet = await (new TcpPacket {
            Version = 1,
            Command = TcpPacket.TcpCommand.GetXML,
            Data = JsonConvert.SerializeObject(id)
        }.SendAsync(s_client));

        if(packet is null) {
            return default;
        }

        return JsonConvert.DeserializeObject
            <(string, string, ulong, ulong, int, string)>
            (packet.Data);
    }

    public static bool CheckValidIp() {
        if(string.IsNullOrEmpty(BaseIp)) {
            return false;
        }
        return true;
    }
}
