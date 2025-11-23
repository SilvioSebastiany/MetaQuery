using FluentValidation;
using MetaQuery.Domain.Commands.Metadados;

namespace MetaQuery.Domain.Validators
{
    /// <summary>
    /// Validador para AtualizarMetadadoCommand usando FluentValidation
    /// </summary>
    public class AtualizarMetadadoCommandValidator : AbstractValidator<AtualizarMetadadoCommand>
    {
        public AtualizarMetadadoCommandValidator()
        {
            // Validação do ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID do metadado deve ser maior que zero");

            // Validação dos campos disponíveis
            RuleFor(x => x.CamposDisponiveis)
                .NotEmpty().WithMessage("Campos disponíveis são obrigatórios")
                .MaximumLength(1000).WithMessage("Lista de campos não pode exceder 1000 caracteres");

            // Validação da chave primária
            RuleFor(x => x.ChavePk)
                .NotEmpty().WithMessage("Chave primária é obrigatória")
                .Length(2, 100).WithMessage("Nome da chave primária deve ter entre 2 e 100 caracteres");

            // Validação do vínculo entre tabelas (opcional)
            When(x => !string.IsNullOrWhiteSpace(x.VinculoEntreTabela), () =>
            {
                RuleFor(x => x.VinculoEntreTabela)
                    .MaximumLength(1000).WithMessage("Vínculo entre tabelas não pode exceder 1000 caracteres");
            });

            // Validação da descrição da tabela (opcional)
            When(x => !string.IsNullOrWhiteSpace(x.DescricaoTabela), () =>
            {
                RuleFor(x => x.DescricaoTabela)
                    .MaximumLength(500).WithMessage("Descrição da tabela não pode exceder 500 caracteres");
            });

            // Validação da descrição dos campos (opcional)
            When(x => !string.IsNullOrWhiteSpace(x.DescricaoCampos), () =>
            {
                RuleFor(x => x.DescricaoCampos)
                    .MaximumLength(2000).WithMessage("Descrição dos campos não pode exceder 2000 caracteres");
            });
        }
    }
}
