using MediatR;

namespace MetaQuery.Domain.Commands.Metadados
{
    /// <summary>
    /// Command para criar um novo metadado de tabela dinâmica
    /// </summary>
    public record CriarMetadadoCommand : IRequest<int>
    {
        /// <summary>
        /// Nome da tabela
        /// </summary>
        public string Tabela { get; init; } = string.Empty;

        /// <summary>
        /// Campos disponíveis separados por vírgula (ex: ID,NOME,EMAIL)
        /// </summary>
        public string CamposDisponiveis { get; init; } = string.Empty;

        /// <summary>
        /// Nome da coluna de chave primária
        /// </summary>
        public string ChavePk { get; init; } = string.Empty;

        /// <summary>
        /// Vínculos no formato: TabelaDestino:CampoFK:CampoPK (ex: PEDIDOS:ID_CLIENTE:ID)
        /// </summary>
        public string? VinculoEntreTabela { get; init; }

        /// <summary>
        /// Descrição amigável da tabela
        /// </summary>
        public string? DescricaoTabela { get; init; }

        /// <summary>
        /// Descrição dos campos no formato: Campo:Descrição|Campo:Descrição
        /// </summary>
        public string? DescricaoCampos { get; init; }

        /// <summary>
        /// Indica se a tabela é visível para consultas da IA (default: true)
        /// </summary>
        public bool VisivelParaIA { get; init; } = true;
    }
}
