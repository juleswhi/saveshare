using static SaveshareServer.Messages;
using System.Net;
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
            Logger.Warn(CONFIG_NULL);
            Logger.Log(SERVER_STOP);
            return;
        }

        Logger.Log(SERVER_START);
        Logger.Log(SERVER_PORT(_config));

        _listener = new();
        _cts = new();

        try {
            _listener.Prefixes.Add(_config.HostAddress);
        }

        catch(Exception ex) {
            Logger.Warn(INVALID_TCP(ex));
            return;
        }

        await Listen();
    }

    private async Task Listen() {
        if(_listener is null) {
            Stop();
            return;
        }

        _listener.Start();

        try {
            await HandleRequests();
        }

        catch(Exception) {
            Logger.Warn(LISTENER_FAILED);
            Logger.Log(SERVER_STOP);
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

            using var reader = new BinaryReader(
                    request.InputStream);

            byte[] res = reader.ReadBytes((int)request.ContentLength64);

            if(res is null || res.Length == 0) {
                Logger.Warn(NULL_PACKET);
                continue;
            }

            HttpPacket packet = HttpPacket.FromBytes(res);

            var handle = HandlePath(packet.Type);

            if(handle is null) {
                Logger.Warn(INVALID_PACKET);
                continue;
            }

            handle(response, packet);

            if(request is null || request.Url is null) {
                Logger.Warn(REQ_NULL);
                Logger.Log(SERVER_STOP);
                Stop();
                Environment.Exit(1);
            }
        }
    }

    private Action<HttpListenerResponse, HttpPacket>? HandlePath(HttpPacket.HttpPacketType type)
        => type switch {
            HttpPacket.HttpPacketType.HEALTH => Health,
            HttpPacket.HttpPacketType.XMLSAVE=> SaveXML,
            HttpPacket.HttpPacketType.XMLGET => GetXML,
            _ => null
        };

    private async void GetXML(HttpListenerResponse response, HttpPacket packet)
    {
        var worldid = ulong.Parse(packet.Data);
        var recentSave = await Database.GetSave(worldid);

        if(recentSave is null) return;

        var saveJson = JsonConvert.SerializeObject(
                (recentSave.XML,
                 recentSave.GameFile,
                 recentSave.WorldID,
                 recentSave.CurrentHostID,
                 recentSave.Version,
                 recentSave.Name));

        if(saveJson is null || saveJson == "") {
            return;
        }

        byte[] buffer = new HttpPacket {
            Version = _config!.TCP_PROTOCOL_VERSION,
            Type = HttpPacket.HttpPacketType.WORLD_DATA,
            Data = saveJson
        }.ToBytes();

        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private async void SaveXML(HttpListenerResponse response, HttpPacket packet) {
        (string xml, string gamefile, ulong worldid, ulong hostid, string name) =
           JsonConvert.DeserializeObject
               <(string, string, ulong, ulong, string)>
           (packet.Data);

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
                Version = version,
                Name = name
                }
                );


        byte[] buffer = new HttpPacket {
            Version = _config!.TCP_PROTOCOL_VERSION,
            Type = HttpPacket.HttpPacketType.NIL
        }.ToBytes();

        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void Health(HttpListenerResponse response, HttpPacket packet) {
        if(_config is null || !_config.Routes.Contains("health")) {
            return;
        }

        byte[] buffer = new HttpPacket {
            Version = _config!.TCP_PROTOCOL_VERSION,
            Type = HttpPacket.HttpPacketType.NIL
        }.ToBytes();

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
