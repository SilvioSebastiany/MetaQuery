using FluentValidation;
using MetaQuery.Domain.Queries.Metadados;

namespace MetaQuery.Domain.Validators;

/// <summary>
/// Validador para ObterMetadadoPorIdQuery
/// </summary>
public class ObterMetadadoPorIdQueryValidator : AbstractValidator<ObterMetadadoPorIdQuery>
{
    public ObterMetadadoPorIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID deve ser maior que zero");
    }
}
