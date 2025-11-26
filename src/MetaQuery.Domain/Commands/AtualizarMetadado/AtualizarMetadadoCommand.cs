using MediatR;

namespace MetaQuery.Domain.Commands.AtualizarMetadado
{
    public record AtualizarMetadadoCommand : IRequest<bool>
    {
        public int Id { get; init; }
        public string CamposDisponiveis { get; init; } = string.Empty;
        public string ChavePk { get; init; } = string.Empty;
        public string? VinculoEntreTabela { get; init; }
        public string? DescricaoTabela { get; init; }
        public string? DescricaoCampos { get; init; }
        public bool VisivelParaIA { get; init; }
    }
}
