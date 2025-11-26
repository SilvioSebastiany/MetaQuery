using MediatR;

namespace MetaQuery.Domain.Commands.CriarMetadado
{
    public record CriarMetadadoCommand : IRequest<int>
    {
        public string Tabela { get; init; } = string.Empty;
        public string CamposDisponiveis { get; init; } = string.Empty;
        public string ChavePk { get; init; } = string.Empty;
        public string? VinculoEntreTabela { get; init; }
        public string? DescricaoTabela { get; init; }
        public string? DescricaoCampos { get; init; }
        public bool VisivelParaIA { get; init; } = true;
    }
}
