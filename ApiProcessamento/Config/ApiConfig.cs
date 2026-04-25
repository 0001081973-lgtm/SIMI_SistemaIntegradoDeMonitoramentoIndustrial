namespace ApiProcessamento.Config
{
    /// <summary>
    /// Configurações da API carregadas do appsettings.json.
    /// </summary>
    public class ApiConfig
    {
        /// <summary>Temperatura máxima permitida em °C. Padrão: 80.</summary>
        public double MaxTemperatura { get; set; } = 80;

        /// <summary>Pressão máxima permitida em bar. Padrão: 10.</summary>
        public double MaxPressao { get; set; } = 10;

        /// <summary>Vibração máxima permitida em m/s². Padrão: 50.</summary>
        public double MaxVibracao { get; set; } = 50;
    }
}
