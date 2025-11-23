using MediatR;

namespace MetaQuery.Domain.Commands.DesativarMetadado
{
    /// <summary>
    /// Command para desativar (soft delete) um metadado
    /// </summary>
    public record DesativarMetadadoCommand : IRequest<bool>
    {
        /// <summary>
        /// ID do metadado a ser desativado
        /// </summary>
        public int Id { get; init; }

        public DesativarMetadadoCommand(int id)
        {
            Id = id;
        }
    }
}
