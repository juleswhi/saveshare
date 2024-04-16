using System.Text;

namespace SaveshareServer;

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

