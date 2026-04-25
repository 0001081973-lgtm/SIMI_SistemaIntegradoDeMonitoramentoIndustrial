namespace Shared
{
    /// <summary>
    /// Representa os dados coletados por um sensor industrial.
    /// </summary>
    public class SensorData
    {
        /// <summary>Identificador único do registro do sensor.</summary>
        public int Id { get; set; }

        /// <summary>Temperatura medida em graus Celsius (°C).</summary>
        public double Temperatura { get; set; }

        /// <summary>Pressão medida em bar.</summary>
        public double Pressao { get; set; }

        /// <summary>Umidade relativa do ar em porcentagem (%).</summary>
        public double Umidade { get; set; }

        /// <summary>
        /// Vibração medida em metros por segundo ao quadrado (m/s²).
        /// Sinal industrial adicionado para monitoramento de máquinas rotativas
        /// e detecção precoce de falhas mecânicas.
        /// </summary>
        public double Vibracao { get; set; }

        /// <summary>Identificador da origem do dado (ex: "Simulator", "Interface").</summary>
        public string Origem { get; set; } = "Desconhecido";

        /// <summary>Data e hora da coleta do dado.</summary>
        public DateTime Timestamp { get; set; }
    }
}
