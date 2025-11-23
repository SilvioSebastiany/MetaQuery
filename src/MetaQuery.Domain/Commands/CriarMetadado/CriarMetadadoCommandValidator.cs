using FluentValidation;

namespace MetaQuery.Domain.Commands.CriarMetadado
{
    /// <summary>
    /// Validator para CriarMetadadoCommand
    /// </summary>
    public class CriarMetadadoCommandValidator : AbstractValidator<CriarMetadadoCommand>
    {
        public CriarMetadadoCommandValidator()
        {
            RuleFor(x => x.Tabela)
                .NotEmpty()
                .WithMessage("O nome da tabela é obrigatório")
                .MaximumLength(100)
                .WithMessage("O nome da tabela não pode ter mais de 100 caracteres")
                .Matches("^[A-Z][A-Z0-9_]*$")
                .WithMessage("O nome da tabela deve estar em MAIÚSCULAS e conter apenas letras, números e underscores");

            RuleFor(x => x.CamposDisponiveis)
                .NotEmpty()
                .WithMessage("A lista de campos disponíveis é obrigatória");

            RuleFor(x => x.ChavePk)
                .NotEmpty()
                .WithMessage("A chave primária (PK) é obrigatória")
                .MaximumLength(100)
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
