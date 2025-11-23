namespace MetaQuery.Infra.Data.DTOs
{
    /// <summary>
    /// DTO para mapeamento direto com Dapper (elimina uso de dynamic)
    /// Representa um registro da tabela TABELA_DINAMICA do Oracle
    /// IMPORTANTE: Propriedades devem ter mesmo nome das colunas Oracle (case-insensitive no Dapper)
    /// </summary>
    public record MetadadoDto
    {
        /// <summary>ID único do metadado</summary>
        public int Id { get; init; }

        /// <summary>Nome da tabela</summary>
        public string Tabela { get; init; } = string.Empty;

        /// <summary>Campos disponíveis separados por vírgula</summary>
        public string CamposDisponiveis { get; init; } = string.Empty;

        /// <summary>Campo chave primária</summary>
        public string ChavePk { get; init; } = string.Empty;

        /// <summary>Vínculos com outras tabelas</summary>
        public string? VinculoEntreTabela { get; init; }

        /// <summary>Descrição da tabela</summary>
        public string? DescricaoTabela { get; init; }

        /// <summary>Descrição dos campos</summary>
        public string? DescricaoCampos { get; init; }

        /// <summary>Visível para IA (0=false, 1=true)</summary>
        public int VisivelParaIa { get; init; }

        /// <summary>Data de criação</summary>
        public DateTime DataCriacao { get; init; }

        /// <summary>Data de atualização</summary>
        public DateTime? DataAtualizacao { get; init; }

        /// <summary>Registro ativo (0=false, 1=true)</summary>
        public int Ativo { get; init; }
    }
}
