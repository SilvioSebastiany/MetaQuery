namespace MetaQuery.Infra.Data.DTOs
{
    /// <summary>
    /// DTO para mapeamento direto com Dapper (elimina uso de dynamic)
    /// Representa um registro da tabela TABELA_DINAMICA do Oracle
    /// IMPORTANTE: Propriedades devem ter mesmo nome das colunas Oracle (case-insensitive no Dapper)
    /// </summary>
    public record MetadadoDto
    {
        public int Id { get; init; }

        public string Tabela { get; init; } = string.Empty;
        public string CamposDisponiveis { get; init; } = string.Empty;
        public string ChavePk { get; init; } = string.Empty;
        public string? VinculoEntreTabela { get; init; }
        public string? DescricaoTabela { get; init; }
        public string? DescricaoCampos { get; init; }
        public int VisivelParaIa { get; init; }
        public DateTime DataCriacao { get; init; }
        public DateTime? DataAtualizacao { get; init; }
        public int Ativo { get; init; }
    }
}
