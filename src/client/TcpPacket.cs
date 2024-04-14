using System.Net.Sockets;
using System.Text;

namespace Saveshare;

internal class TcpPacket {
    public byte Version { get; set; }
    public TcpCommand Command { get; set; }
    public ushort Length { get; set; }
    public string Data { get; set; } = string.Empty;

    public enum TcpCommand : byte {
        None = 0,
        Health = 1,
        GetXML = 2,
        SaveXML = 3,
        WorldData = 4
    }

    public byte[] ToBytes() {
        List<byte> bytes = new();

        bytes.AddRange(BitConverter.GetBytes(Version));
        bytes.AddRange(BitConverter.GetBytes((byte)Command));
        if(Data is not null && Data.Length != 0) {
            bytes.AddRange(BitConverter.GetBytes((ushort)Data.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(Data));
        }
        else {
            // Length is zero
            bytes.Add(0);
        }

        return bytes.ToArray();
    }

    public ByteArrayContent ToBuf() {
        return new ByteArrayContent(ToBytes());
    }

    private string? GetPath() => this.Command switch {
        TcpCommand.Health => "health",
        TcpCommand.GetXML => "xmlget",
        TcpCommand.SaveXML => "xmlsave",
        _ => null
    };

    public async Task<TcpPacket?> SendAsync(TcpClient? client) {
        if(client is null) return null;

        var stream = client.GetStream();

        Utils.Monitor?.Log($"Got Stream");

        var buf = ToBytes();
        await stream.WriteAsync(buf, 0, buf.Length);

        Utils.Monitor?.Log($"Wrote to stream");
        var resBuf = new byte[256];
        int numBytes = stream.Read(resBuf);

        var len = BitConverter.ToUInt16(new byte[] {  resBuf[2], resBuf[3] });

        List<byte> bytes = new();
        for(int i = 0; i < len; i++) {
            bytes.Add(resBuf[4+i]);
        }

        return new TcpPacket {
            Version = resBuf[0],
            Command = (TcpCommand)resBuf[1],
            Length = len,
            Data = Encoding.UTF8.GetString(bytes.ToArray())
        };
    }

    public static TcpPacket FromBytes(byte[] packet) {
        // V | C | L | L | D
        byte version = packet[0];
        byte cmd = packet[1];
        string data = Encoding.UTF8.GetString(packet[4..]);

        return new TcpPacket {
            Version = version,
            Command = (TcpCommand)cmd,
            Data = data
        };
    }
}

