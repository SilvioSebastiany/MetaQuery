using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaQuery.Domain.Entities
{
    /// <summary>
    /// Entity que representa os metadados de uma tabela no sistema
    /// Contém validações de domínio seguindo DDD
    /// </summary>
    public class TabelaDinamica
    {
        public int Id { get; private set; }
        public string Tabela { get; private set; }
        public string CamposDisponiveis { get; private set; }
        public string ChavePk { get; private set; }
        public string? VinculoEntreTabela { get; private set; }
        public string? DescricaoTabela { get; private set; }
        public string? DescricaoCampos { get; private set; } // JSON com descrições detalhadas
        public bool VisivelParaIA { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public bool Ativo { get; private set; }

        // Construtor privado para EF/Dapper
        private TabelaDinamica() { }

        // Factory method para criar nova tabela
        public static TabelaDinamica Criar(
            string tabela,
            string camposDisponiveis,
            string chavePk,
            string? vinculoEntreTabela = null,
            string? descricaoTabela = null,
            string? descricaoCampos = null,
            bool visivelParaIA = true)
        {
            var entity = new TabelaDinamica
            {
                Tabela = tabela,
                CamposDisponiveis = camposDisponiveis,
                ChavePk = chavePk,
                VinculoEntreTabela = vinculoEntreTabela,
                DescricaoTabela = descricaoTabela,
                DescricaoCampos = descricaoCampos,
                VisivelParaIA = visivelParaIA,
                DataCriacao = DateTime.Now,
                Ativo = true
            };

            entity.Validar();
            return entity;
        }

        // Métodos de atualização
        public void AtualizarCampos(string camposDisponiveis)
        {
            if (string.IsNullOrWhiteSpace(camposDisponiveis))
                throw new ArgumentException("Campos disponíveis não pode ser vazio", nameof(camposDisponiveis));

            CamposDisponiveis = camposDisponiveis;
            DataAtualizacao = DateTime.Now;
        }

        public void AtualizarVinculo(string? vinculoEntreTabela)
        {
            VinculoEntreTabela = vinculoEntreTabela;
            DataAtualizacao = DateTime.Now;
        }

        public void AtualizarDescricao(string? descricaoTabela, string? descricaoCampos = null)
        {
            DescricaoTabela = descricaoTabela;
            if (descricaoCampos != null)
                DescricaoCampos = descricaoCampos;
            DataAtualizacao = DateTime.Now;
        }

        public void AlterarVisibilidadeIA(bool visivel)
        {
            VisivelParaIA = visivel;
            DataAtualizacao = DateTime.Now;
        }

        public void Desativar()
        {
            Ativo = false;
            DataAtualizacao = DateTime.Now;
        }

        public void Reativar()
        {
            Ativo = true;
            DataAtualizacao = DateTime.Now;
        }

        // Validações de domínio
        private void Validar()
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(Tabela))
                erros.Add("Nome da tabela é obrigatório");
            else if (Tabela.Length > 100)
                erros.Add("Nome da tabela não pode ter mais de 100 caracteres");

            if (string.IsNullOrWhiteSpace(CamposDisponiveis))
                erros.Add("Campos disponíveis é obrigatório");

            if (string.IsNullOrWhiteSpace(ChavePk))
                erros.Add("Chave primária é obrigatória");

            if (VinculoEntreTabela != null && VinculoEntreTabela.Length > 500)
                erros.Add("Vínculo entre tabelas não pode ter mais de 500 caracteres");

            if (erros.Any())
                throw new ArgumentException($"Erros de validação: {string.Join(", ", erros)}");
        }

        // Métodos auxiliares
        public List<string> ObterListaCampos()
        {
            return CamposDisponiveis
                .Split(',')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();
        }

        public List<string> ObterVinculos()
        {
            if (string.IsNullOrWhiteSpace(VinculoEntreTabela))
                return new List<string>();

            return VinculoEntreTabela
                .Split(',')
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();
        }

        public bool TemVinculo(string nomeTabela)
        {
            return ObterVinculos()
                .Any(v => v.StartsWith($"{nomeTabela}.", StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return $"Tabela: {Tabela} | PK: {ChavePk} | Campos: {CamposDisponiveis.Length} caracteres";
        }
    }
}
