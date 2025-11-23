using FluentValidation;

namespace MetaQuery.Domain.Commands.DesativarMetadado
{
    /// <summary>
    /// Validator para DesativarMetadadoCommand
    /// </summary>
    public class DesativarMetadadoCommandValidator : AbstractValidator<DesativarMetadadoCommand>
    {
        public DesativarMetadadoCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do metadado deve ser maior que zero");
        }
    }
}
