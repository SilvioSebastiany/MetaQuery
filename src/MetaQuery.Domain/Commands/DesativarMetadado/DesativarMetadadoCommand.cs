using MediatR;

namespace MetaQuery.Domain.Commands.DesativarMetadado
{
    public record DesativarMetadadoCommand : IRequest<bool>
    {   
        public int Id { get; init; }

        public DesativarMetadadoCommand(int id)
        {
            Id = id;
        }
    }
}
