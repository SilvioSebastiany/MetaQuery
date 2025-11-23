using FluentValidation;
using MetaQuery.Domain.Queries.Metadados;

namespace MetaQuery.Domain.Validators;

/// <summary>
/// Validador para ObterMetadadoPorTabelaQuery
/// </summary>
public class ObterMetadadoPorTabelaQueryValidator : AbstractValidator<ObterMetadadoPorTabelaQuery>
{
    public ObterMetadadoPorTabelaQueryValidator()
    {
        RuleFor(x => x.NomeTabela)
            .NotEmpty()
            .WithMessage("Nome da tabela é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da tabela deve ter no máximo 100 caracteres")
            .Matches(@"^[A-Z_][A-Z0-9_]*$")
            .WithMessage("Nome da tabela deve conter apenas letras maiúsculas, números e underscore, começando com letra");
    }
}
