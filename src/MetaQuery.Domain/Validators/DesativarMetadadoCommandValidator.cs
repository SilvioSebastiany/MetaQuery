using FluentValidation;
using MetaQuery.Domain.Commands.Metadados;

namespace MetaQuery.Domain.Validators
{
    /// <summary>
    /// Validador para DesativarMetadadoCommand usando FluentValidation
    /// </summary>
    public class DesativarMetadadoCommandValidator : AbstractValidator<DesativarMetadadoCommand>
    {
        public DesativarMetadadoCommandValidator()
        {
            // Validação do ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID do metadado deve ser maior que zero");
        }
    }
}
