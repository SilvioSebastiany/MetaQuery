namespace MetaQuery.Infra.CrossCutting.Settings
{
    /// <summary>
    /// Configurações de conexão com banco de dados
    /// </summary>
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
        public bool EnableDetailedErrors { get; set; } = false;
    }
}
