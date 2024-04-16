using System.Net.Sockets;
using System.Text;

namespace Saveshare;

internal class TcpPacket {
    public byte Version { get; set; }
    public byte StatusCode { get; set; }
    public TcpCommand Command { get; set; }
    public ushort Length { get; set; }
    public string Data { get; set; } = string.Empty;

    public enum TcpCommand : byte {
        None = 0,
        Health = 1,
        Get = 2,
        Save = 3,
    }

    public byte[] ToBytes() {
        List<byte> bytes = new();

        bytes.Add(Version); // Version
        bytes.Add(StatusCode); // Status
        bytes.Add((byte)Command); // Command
        if(Data is not null && Data.Length != 0) {
            bytes.AddRange(BitConverter.GetBytes((ushort)Data.Length)); // Length
            bytes.AddRange(Encoding.UTF8.GetBytes(Data)); // Data
        }
        else {
            bytes.Add(0);
        }

        return bytes.ToArray();
    }

    private string? GetPath() => this.Command switch {
        TcpCommand.Health => "health",
        TcpCommand.Get => "xmlget",
        TcpCommand.Save => "xmlsave",
        _ => null
    };

    public async Task<TcpPacket?> SendAsync(TcpClient? client) {
        if(client is null) return null;

        var stream = client.GetStream();

        Utils.Monitor?.Log($"Got Stream");

        var buffer = ToBytes();
        await stream.WriteAsync(buffer, 0, buffer.Length);

        Utils.Monitor?.Log($"Wrote to stream");
        var res = new byte[256];
        int numBytes = stream.Read(res);

        stream.Close();

        var len = BitConverter.ToUInt16(new byte[] {  res[3], res[4] });

        List<byte> bytes = new();
        for(int i = 0; i < len; i++) {
            bytes.Add(res[5+i]);
        }

        return new TcpPacket {
            Version = res[0],
            StatusCode = res[1],
            Command = (TcpCommand)res[2],
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

