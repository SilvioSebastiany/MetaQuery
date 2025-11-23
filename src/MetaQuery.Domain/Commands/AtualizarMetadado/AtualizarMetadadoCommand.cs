using MediatR;

namespace MetaQuery.Domain.Commands.AtualizarMetadado
{
    /// <summary>
    /// Command para atualizar um metadado existente
    /// </summary>
    public record AtualizarMetadadoCommand : IRequest<bool>
    {
        /// <summary>
        /// ID do metadado a ser atualizado
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Campos disponíveis separados por vírgula
        /// </summary>
        public string CamposDisponiveis { get; init; } = string.Empty;

        /// <summary>
        /// Nome da coluna de chave primária
        /// </summary>
        public string ChavePk { get; init; } = string.Empty;

        /// <summary>
        /// Vínculos no formato: TabelaDestino:CampoFK:CampoPK
        /// </summary>
        public string? VinculoEntreTabela { get; init; }

        /// <summary>
        /// Descrição amigável da tabela
        /// </summary>
        public string? DescricaoTabela { get; init; }

        /// <summary>
        /// Descrição dos campos
        /// </summary>
        public string? DescricaoCampos { get; init; }

        /// <summary>
        /// Indica se a tabela é visível para IA
        /// </summary>
        public bool VisivelParaIA { get; init; }
    }
}
