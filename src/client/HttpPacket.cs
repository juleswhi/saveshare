using System.Text;

namespace Saveshare;

internal class HttpPacket {

    public ushort Version { get; set; }
    public HttpPacketType Type { get; set; }
    public string Data { get; set; } = string.Empty;

    public enum HttpPacketType : ushort {
        HEALTH,
        XMLGET,
        XMLSAVE,
        WORLD_DATA,
        NIL
    }

    public byte[] ToBytes() {
        List<byte> bytes = new();

        bytes.AddRange(BitConverter.GetBytes(Version));
        bytes.AddRange(BitConverter.GetBytes((ushort)Type));
        if(Data is not null && Data.Length != 0) {
            bytes.AddRange(Encoding.UTF8.GetBytes(Data));
        }

        return bytes.ToArray();
    }

    public ByteArrayContent ToBuf() {
        return new ByteArrayContent(ToBytes());
    }

    private string? GetPath() => this.Type switch {
        HttpPacketType.HEALTH => "health",
        HttpPacketType.XMLGET => "xmlget",
        HttpPacketType.XMLSAVE => "xmlsave",
        _ => null
    };

    public async Task<HttpPacket?> PostAysnc(HttpClient client, string ip) {
        var buf = ToBuf();

        HttpResponseMessage res;

        try {
            res = await client.PostAsync($"{ip}", buf);
            res.EnsureSuccessStatusCode();
        }
        catch(Exception) {
            return null;
        }

        return HttpPacket.FromBytes(
                await res.Content.ReadAsByteArrayAsync());
    }

    public static HttpPacket FromBytes(byte[] packet) {
        ushort version = BitConverter.ToUInt16(new [] { packet[0], packet[1] });
        ushort type = BitConverter.ToUInt16(new [] { packet[2], packet[3] });
        string data = Encoding.UTF8.GetString(packet[4..]);

        return new HttpPacket {
            Version = version,
            Type = (HttpPacketType)type,
            Data = data
        };
    }
}

