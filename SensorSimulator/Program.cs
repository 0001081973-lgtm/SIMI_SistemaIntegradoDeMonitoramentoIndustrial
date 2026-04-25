using Microsoft.Data.Sqlite;
using Shared;
using System.Net.Http.Json;

// ── Configurações ──────────────────────────────────────────────────────────────
const string API_URL = "https://localhost:7205/api/v1/sensores";
const string DB_PATH  = "simulator_log.db";

// ── Inicializa banco SQLite local do Simulator ────────────────────────────────
InicializarBanco(DB_PATH);

var http = new HttpClient();
var rng  = new Random();
int index = 0;

Console.WriteLine("=== SIMI — SensorSimulator iniciado ===");
Console.WriteLine($"  API alvo : {API_URL}");
Console.WriteLine($"  Banco    : {DB_PATH}");
Console.WriteLine("Pressione Ctrl+C para encerrar.\n");

while (true)
{
    var sensor = new SensorData
    {
        Id          = index,
        Temperatura = Math.Round(rng.NextDouble() * 80 + 15, 2),  // 15–95 °C
        Pressao     = Math.Round(rng.NextDouble() * 9  + 0.5, 2), // 0.5–9.5 bar
        Umidade     = Math.Round(rng.NextDouble() * 70 + 20, 2),  // 20–90 %
        Vibracao    = Math.Round(rng.NextDouble() * 45 + 0.1, 2), // 0.1–45.1 m/s²
        Origem      = "Simulator",
        Timestamp   = DateTime.UtcNow
    };

    // Persiste localmente antes de enviar
    PersistirLocal(DB_PATH, sensor);

    // Envia para a API
    try
    {
        var response = await http.PostAsJsonAsync(API_URL, sensor);
        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{sensor.Timestamp:HH:mm:ss}] ERRO {response.StatusCode}: {erro}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"[{sensor.Timestamp:HH:mm:ss}] #{index:D4} | " +
                $"Temp={sensor.Temperatura:F1}°C  " +
                $"Pressão={sensor.Pressao:F2}bar  " +
                $"Umidade={sensor.Umidade:F1}%  " +
                $"Vibração={sensor.Vibracao:F2}m/s²");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Falha na conexão: {ex.Message}");
    }

    Console.ResetColor();
    await Task.Delay(2000);
    index++;
}

// ── Funções auxiliares ────────────────────────────────────────────────────────

static void InicializarBanco(string dbPath)
{
    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS SensorData (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            Temperatura REAL    NOT NULL,
            Pressao     REAL    NOT NULL,
            Umidade     REAL    NOT NULL,
            Vibracao    REAL    NOT NULL,
            Origem      TEXT    NOT NULL,
            Timestamp   TEXT    NOT NULL
        );
        """;
    cmd.ExecuteNonQuery();
}

static void PersistirLocal(string dbPath, SensorData sensor)
{
    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        INSERT INTO SensorData (Temperatura, Pressao, Umidade, Vibracao, Origem, Timestamp)
        VALUES ($temp, $pressao, $umidade, $vibracao, $origem, $ts);
        """;
    cmd.Parameters.AddWithValue("$temp",     sensor.Temperatura);
    cmd.Parameters.AddWithValue("$pressao",  sensor.Pressao);
    cmd.Parameters.AddWithValue("$umidade",  sensor.Umidade);
    cmd.Parameters.AddWithValue("$vibracao", sensor.Vibracao);
    cmd.Parameters.AddWithValue("$origem",   sensor.Origem);
    cmd.Parameters.AddWithValue("$ts",       sensor.Timestamp.ToString("o"));
    cmd.ExecuteNonQuery();
}
