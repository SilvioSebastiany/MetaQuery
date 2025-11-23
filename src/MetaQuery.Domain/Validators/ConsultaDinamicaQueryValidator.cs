using FluentValidation;

namespace MetaQuery.Domain.Validators;

/// <summary>
/// Validador para ConsultaDinamicaQuery
/// </summary>
public class ConsultaDinamicaQueryValidator : AbstractValidator<Queries.ConsultaDinamicaQuery>
{
    private static readonly string[] TabelasPermitidas =
    {
        "CLIENTES", "PEDIDOS", "PRODUTOS", "CATEGORIAS", "ITENS_PEDIDO", "ENDERECOS"
    };

    public ConsultaDinamicaQueryValidator()
    {
        RuleFor(x => x.Tabela)
            .NotEmpty()
            .WithMessage("Tabela é obrigatória")
            .Must(tabela => TabelasPermitidas.Contains(tabela.ToUpper()))
            .WithMessage(query => $"Tabela '{query.Tabela}' não autorizada. Tabelas permitidas: {string.Join(", ", TabelasPermitidas)}");

        RuleFor(x => x.Profundidade)
            .InclusiveBetween(1, 3)
            .WithMessage("Profundidade deve estar entre 1 e 3");
    }
}
