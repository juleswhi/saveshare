using System.Data.SQLite;
namespace SaveshareServer;

internal static class Database {

    private static SQLiteConnection s_conn { get; set; }

    static Database() {
        s_conn = new("Data Source=Saveshare.db");
    }

    public static async Task<Save?> GetSave(ulong id) {
        s_conn.Open();
        string sql = @"
            select *
            from Saves
            where WorldID = @WorldID and Version = (
                    SELECT MAX(Version)
                    FROM Saves
                    WHERE WorldID = @WorldID
                    );
            ";

        using var command = new SQLiteCommand(sql, s_conn);

        command.Parameters.AddWithValue("@WorldID", id);

        using var reader = await command.ExecuteReaderAsync();

        while(reader.Read()) {
            var save = new Save {
                   ID = Guid.Parse(reader.GetString(0)),
                   XML = reader.GetString(1),
                   WorldID = (ulong)reader.GetInt64(2),
                   CurrentHostID = (ulong)reader.GetInt64(3),
                   Version = reader.GetInt32(4),
                   GameFile = reader.GetString(5),
                   Name = reader.GetString(6)
            };

            s_conn.Close();
            return save;
        }

        s_conn.Close();
        return null;
    }

    public static async Task<string> GetXML(ulong id) {
        s_conn.Open();
        string query = @"
            select xml
            from Saves
            where WorldID = @WorldID and Version = (
                    SELECT MAX(Version)
                    FROM Saves
                    WHERE WorldID = @WorldID
                    );
            ";

        using var command = new SQLiteCommand(query, s_conn);

        command.Parameters.AddWithValue("@WorldID", id);

        object? result = await command.ExecuteScalarAsync();

        if(result is null) {
            return "";
        }

        Logger.Log($"{result.ToString()}");

        s_conn.Close();

        return result.ToString()!;
    }

    // create xml

    public static async Task CreateSave(Save save) {
        s_conn.Open();

        string sql = 
            @"insert into Saves 
                (ID, xml, WorldID, CurrentHostID, Version, GameFile, Name)
                values
                (@ID, @XML, @WorldID, @CurrentHostID, @Version, @GameFile, @Name)";

        using var command = new SQLiteCommand(sql, s_conn);

        command.Parameters.AddWithValue("@ID", save.ID.ToString());
        command.Parameters.AddWithValue("@XML", save.XML);
        command.Parameters.AddWithValue("@WorldID", save.WorldID);
        command.Parameters.AddWithValue("@CurrentHostID", save.CurrentHostID);
        command.Parameters.AddWithValue("@Version", save.Version);
        command.Parameters.AddWithValue("@GameFile", save.GameFile);
        command.Parameters.AddWithValue("@Name", save.Name);

        await command.ExecuteNonQueryAsync();

        s_conn.Close();
    }
}
