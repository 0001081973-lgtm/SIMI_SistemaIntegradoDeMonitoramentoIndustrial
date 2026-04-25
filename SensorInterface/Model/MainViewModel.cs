using Microsoft.Data.Sqlite;
using SensorInterface.Command;
using SensorInterface.ViewModels;
using Shared;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Input;

namespace SensorInterface.Model
{
    public class MainViewModel : BaseViewModel
    {
        private const string API_URL = "https://localhost:7205/api/v1/sensores";
        private const string DB_PATH = "interface_log.db";

        // ── Coleções de binding ───────────────────────────────────────────────
        public ObservableCollection<SensorData> Sensores { get; set; } = new();

        // ── Estatísticas ──────────────────────────────────────────────────────
        private double _mediaTemperat;
        public double MediaTemperatura
        {
            get => _mediaTemperat;
            set => SetField(ref _mediaTemperat, value);
        }

        private double _mediaVibracao;
        public double MediaVibracao
        {
            get => _mediaVibracao;
            set => SetField(ref _mediaVibracao, value);
        }

        private int _totalRegistros;
        public int TotalRegistros
        {
            get => _totalRegistros;
            set => SetField(ref _totalRegistros, value);
        }

        private string _status = "Aguardando...";
        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand CarregarSensoresCommand { get; }
        public ICommand LimparCommand { get; }

        public MainViewModel()
        {
            InicializarBanco();
            CarregarSensoresCommand = new RelayCommand(CarregarSensores);
            LimparCommand           = new RelayCommand(Limpar);
        }

        private async void CarregarSensores()
        {
            Status = "Carregando dados da API...";
            try
            {
                var http  = new HttpClient();
                var dados = await http.GetFromJsonAsync<List<SensorData>>(API_URL);

                if (dados is null || dados.Count == 0)
                {
                    Status = "Nenhum dado retornado pela API.";
                    return;
                }

                // Persiste localmente cada registro novo
                foreach (var s in dados)
                    PersistirLocal(s);

                // Atualiza a coleção observável
                Sensores.Clear();
                foreach (var s in dados)
                    Sensores.Add(s);

                // Atualiza estatísticas
                TotalRegistros   = dados.Count;
                MediaTemperatura = Math.Round(dados.Average(s => s.Temperatura), 2);
                MediaVibracao    = Math.Round(dados.Average(s => s.Vibracao),    2);

                Status = $"✔ {dados.Count} registros carregados em {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                Status = $"✖ Erro: {ex.Message}";
            }
        }

        private void Limpar()
        {
            Sensores.Clear();
            TotalRegistros   = 0;
            MediaTemperatura = 0;
            MediaVibracao    = 0;
            Status           = "Lista limpa.";
        }

        // ── SQLite ────────────────────────────────────────────────────────────
        private static void InicializarBanco()
        {
            using var conn = new SqliteConnection($"Data Source={DB_PATH}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS SensorData (
                    Id          INTEGER PRIMARY KEY,
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

        private static void PersistirLocal(SensorData sensor)
        {
            using var conn = new SqliteConnection($"Data Source={DB_PATH}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            // INSERT OR IGNORE evita duplicatas por ID
            cmd.CommandText = """
                INSERT OR IGNORE INTO SensorData (Id, Temperatura, Pressao, Umidade, Vibracao, Origem, Timestamp)
                VALUES ($id, $temp, $pressao, $umidade, $vibracao, $origem, $ts);
                """;
            cmd.Parameters.AddWithValue("$id",      sensor.Id);
            cmd.Parameters.AddWithValue("$temp",    sensor.Temperatura);
            cmd.Parameters.AddWithValue("$pressao", sensor.Pressao);
            cmd.Parameters.AddWithValue("$umidade", sensor.Umidade);
            cmd.Parameters.AddWithValue("$vibracao",sensor.Vibracao);
            cmd.Parameters.AddWithValue("$origem",  sensor.Origem);
            cmd.Parameters.AddWithValue("$ts",      sensor.Timestamp.ToString("o"));
            cmd.ExecuteNonQuery();
        }
    }
}
