using FluentValidation;
using MetaQuery.Domain.Commands.Metadados;

namespace MetaQuery.Domain.Validators
{
    /// <summary>
    /// Validador para CriarMetadadoCommand usando FluentValidation
    /// </summary>
    public class CriarMetadadoCommandValidator : AbstractValidator<CriarMetadadoCommand>
    {
        public CriarMetadadoCommandValidator()
        {
            // Validação do nome da tabela
            RuleFor(x => x.Tabela)
                .NotEmpty().WithMessage("Nome da tabela é obrigatório")
                .Length(3, 100).WithMessage("Nome da tabela deve ter entre 3 e 100 caracteres")
                .Matches(@"^[A-Z][A-Z0-9_]*$")
                .WithMessage("Nome da tabela deve começar com letra maiúscula e conter apenas letras, números e underscore");

            // Validação dos campos disponíveis
            RuleFor(x => x.CamposDisponiveis)
                .NotEmpty().WithMessage("Campos disponíveis são obrigatórios")
                .MaximumLength(1000).WithMessage("Lista de campos não pode exceder 1000 caracteres")
                .Must(conterCamposValidos)
                .WithMessage("Campos disponíveis devem ser separados por vírgula e conter apenas letras, números e underscore");

            // Validação da chave primária
            RuleFor(x => x.ChavePk)
                .NotEmpty().WithMessage("Chave primária é obrigatória")
                .Length(2, 100).WithMessage("Nome da chave primária deve ter entre 2 e 100 caracteres")
                .Matches(@"^[A-Z][A-Z0-9_]*$")
                .WithMessage("Chave primária deve começar com letra maiúscula e conter apenas letras, números e underscore");

            // Validação do vínculo entre tabelas (opcional, mas se informado deve ser válido)
            When(x => !string.IsNullOrWhiteSpace(x.VinculoEntreTabela), () =>
            {
                RuleFor(x => x.VinculoEntreTabela)
                    .MaximumLength(1000).WithMessage("Vínculo entre tabelas não pode exceder 1000 caracteres")
                    .Must(vinculo => ValidarFormatoVinculo(vinculo!))
                    .WithMessage("Vínculo entre tabelas deve seguir o formato: TABELA:CAMPO_FK:CAMPO_PK (ex: PEDIDOS:ID_CLIENTE:ID)");
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

        /// <summary>
        /// Valida se a lista de campos está no formato correto (separados por vírgula)
        /// </summary>
        private bool conterCamposValidos(string campos)
        {
            if (string.IsNullOrWhiteSpace(campos))
                return false;

            var listaCampos = campos.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            if (listaCampos.Length == 0)
                return false;

            // Cada campo deve ser um identificador válido (letras, números, underscore)
            foreach (var campo in listaCampos)
            {
                var campoTrimmed = campo.Trim();
                if (string.IsNullOrWhiteSpace(campoTrimmed))
                    return false;

                if (!System.Text.RegularExpressions.Regex.IsMatch(campoTrimmed, @"^[A-Z][A-Z0-9_]*$"))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Valida se o vínculo está no formato correto: TABELA:CAMPO_FK:CAMPO_PK
        /// </summary>
        private bool ValidarFormatoVinculo(string vinculo)
        {
            if (string.IsNullOrWhiteSpace(vinculo))
                return true; // Vínculo é opcional

            var vinculos = vinculo.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var v in vinculos)
            {
                var partes = v.Split(':', StringSplitOptions.RemoveEmptyEntries);
                
                // Deve ter exatamente 3 partes: TABELA:CAMPO_FK:CAMPO_PK
                if (partes.Length != 3)
                    return false;

                // Cada parte deve ser um identificador válido
                foreach (var parte in partes)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(parte.Trim(), @"^[A-Z][A-Z0-9_]*$"))
                        return false;
                }
            }

            return true;
        }
    }
}
