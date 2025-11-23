using FluentValidation;

namespace MetaQuery.Domain.Commands.AtualizarMetadado
{
    /// <summary>
    /// Validator para AtualizarMetadadoCommand
    /// </summary>
    public class AtualizarMetadadoCommandValidator : AbstractValidator<AtualizarMetadadoCommand>
    {
        public AtualizarMetadadoCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do metadado deve ser maior que zero");

            RuleFor(x => x.CamposDisponiveis)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.CamposDisponiveis))
                .WithMessage("Se informado, a lista de campos disponíveis não pode estar vazia");

            RuleFor(x => x.ChavePk)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.ChavePk))
                .WithMessage("Se informada, a chave primária não pode estar vazia")
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.ChavePk))
                .WithMessage("A chave primária não pode ter mais de 100 caracteres");

            RuleFor(x => x.DescricaoTabela)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.DescricaoTabela))
                .WithMessage("A descrição da tabela não pode ter mais de 500 caracteres");

            RuleFor(x => x.DescricaoCampos)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.DescricaoCampos))
                .WithMessage("A descrição dos campos não pode ter mais de 2000 caracteres");
        }
    }
}
