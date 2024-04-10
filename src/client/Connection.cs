using Newtonsoft.Json;

namespace Saveshare;

internal static class Connection {
    public static string BaseIp { get; set; } = string.Empty;
    public static bool IsConnected { get; set; } = false;
    private static HttpClient s_client { get; set; }

    static Connection() {
        s_client = new();
    }

    // http://domain.name/health
    public static async Task<bool> CheckHealth() {
        return await new HttpPacket {
            Version = 1,
            Type = HttpPacket.HttpPacketType.HEALTH
        }.PostAysnc(s_client, BaseIp) is not null;
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

        await new HttpPacket {
            Version = 1,
            Type = HttpPacket.HttpPacketType.XMLSAVE,
            Data = json
        }.PostAysnc(s_client, BaseIp);
    }

    public static async Task<(string, string, ulong, ulong, int, string)?> GetXML(ulong id) {
        var packet = await (new HttpPacket {
            Version = 1,
            Type = HttpPacket.HttpPacketType.XMLGET,
            Data = JsonConvert.SerializeObject(id)
        }.PostAysnc(s_client, BaseIp));

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
