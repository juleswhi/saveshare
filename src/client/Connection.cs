using Newtonsoft.Json;
using System.Text;

namespace Saveshare;

internal static class Connection {

    public static string BaseIp { get; set; } = string.Empty;
    private static HttpClient s_client { get; set; }

    static Connection() {
        s_client = new();
    }

    public static async Task<bool> CheckHealth() {
        try {
            // TODO: Dynamically based on last char "/" 
            HttpResponseMessage response = 
                await s_client.GetAsync($"{BaseIp}health");

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch(Exception) {
            return false;
        }
    }

    public static async Task<string> SendXML(
            string xml,
            string worldsave, 
            ulong worldid,
            ulong hostid) {

        string json = JsonConvert.SerializeObject(
                (xml, worldsave, worldid, hostid)
                );

        StringContent content = new(json, Encoding.UTF8);

        HttpResponseMessage _ =
            await s_client.PostAsync($"{BaseIp}xmlsave", content);

        return $"Sent data!";
        
    }

    public static async Task<(string, string, ulong, ulong)> GetXML(ulong id) {
        string json = JsonConvert.SerializeObject(id);
        StringContent content = new(json, Encoding.UTF8);

        HttpResponseMessage response =
            await s_client.PostAsync($"{BaseIp}xmlget", content);

        if(!response.IsSuccessStatusCode) {
            return default;
        }

        var obj = JsonConvert.DeserializeObject
            <(string, string, ulong, ulong)>(await response.Content.ReadAsStringAsync());

        return obj;
    }

    public static bool CheckValidIp() {
        if(string.IsNullOrEmpty(BaseIp)) {
            return false;
        }
        return true;
    }
}
