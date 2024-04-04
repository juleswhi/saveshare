using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace SaveshareServer;

internal class Server {

    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Config? _config;

    public Server(Config config) {
        _config = config;
    }

    public async Task Run() {

        if(_config is null) {
            Logger.Warn($"Configuration file was null");
            Logger.Log($"Shutting Down..");
            return;
        }

        Logger.Log($"Starting Server..");
        Logger.Log($"Server running on: {_config.HostAddress}");

        _listener = new();
        _cts = new();

        try {
            _listener.Prefixes.Add(_config.HostAddress);
        } 

        catch(Exception ex) {
            Logger.Warn($"Invalid IP/TCP Address. " + ex.Message);
            return;
        }

        Logger.Log($"Added addy to prefixes");

        await Listen();
    }

    private async Task Listen() {
        if(_listener is null) {
            Stop();
            return;
        }

        _listener.Start();

        Logger.Log($"Http server started");

        try {
            await HandleRequests();
        }

        catch(Exception) {
            Logger.Warn($"Listener Failed. Panicing");
            Logger.Log($"Shutting Down.");
            Environment.Exit(1);
        }
    }

    private async Task HandleRequests() {

        if(_listener is null || _cts is null) {
            return;
        }

        while(!_cts.IsCancellationRequested) {

            var context = await _listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            using var reader = new StreamReader(
                    request.InputStream, 
                    request.ContentEncoding);

            string res = await reader.ReadToEndAsync();

            if(request is null || request.Url is null) {
                Logger.Warn($"Request is null. Panicing");
                Logger.Log($"Shutting Down..");
                Stop();
                Environment.Exit(1);
            }

            var handle = HandlePath(request!.Url!.AbsolutePath);
            if(handle is null) {
                Logger.Warn(@$"
                        {request!.Url!.AbsoluteUri} is not a valid endpoint.
                        ");
            }
            else {
                handle(response, res);
            }
        }

        Logger.Log($"CTS Requested");
    }

    private Action<HttpListenerResponse, string>? HandlePath(string path)
        => path switch {
            "/health" => Health,
            "/xmlsave" => SaveXML,
            "/xmlget" => GetXML,
                _ => null
        };

    private async void GetXML(HttpListenerResponse response, string json)
    {
        Logger.Log($"Request get some sweet sweet xml");
        Logger.Log($"Accessing database");
        await Database.GetSave(Convert.ToUInt64(json));

        byte[] buffer = Encoding.UTF8.GetBytes("XML Here");

        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private async void SaveXML(HttpListenerResponse response, string json) {
        Logger.Log($"Save XML request");

        // string, string, string, ulong, ulong
        (string xml, string gamefile, ulong worldid, ulong hostid) =
           JsonConvert.DeserializeObject
               <(string, string, ulong, ulong)>
           (json);

        var prevSave = await Database.GetSave(worldid);

        int version = 1;

        if(prevSave is not null) {
            version = prevSave.Version + 1;
        }

        await Database.CreateSave(
                new Save() {
                XML = xml,
                WorldID = worldid,
                GameFile = gamefile,
                CurrentHostID = hostid,
                Version = version
                }
                );

        byte[] buffer = [];

        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void Health(HttpListenerResponse response, string _) {
        if(_config is null || !_config.Routes.Contains("health")) {
            return;
        }

        // Logger.Log($"Received Health Check");

        byte[] buffer = Encoding.UTF8.GetBytes("Service Online.");
        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;

        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    private void Stop() {
        _cts?.Cancel();
        _listener?.Stop();
    }
}
