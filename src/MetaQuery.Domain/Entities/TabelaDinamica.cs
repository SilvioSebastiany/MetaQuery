using System;
using System.Collections.Generic;
using System.Linq;
using MetaQuery.Domain.Notifications;

namespace MetaQuery.Domain.Entities
{
    /// <summary>
    /// Entity que representa os metadados de uma tabela no sistema
    /// Contém validações de domínio seguindo DDD
    /// Usa NotificationContext ao invés de exceptions para validações de negócio (Constitution 2.4)
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

        // Notification Pattern: lista interna de notificações de validação
        private readonly List<Notification> _notifications = new();

        /// <summary>
        /// Indica se a entidade está válida (sem notificações de erro)
        /// </summary>
        public bool IsValid => !_notifications.Any();

        /// <summary>
        /// Notificações de validação da entidade
        /// </summary>
        public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

        // Construtor privado para EF/Dapper
        private TabelaDinamica() { }

        /// <summary>
        /// Factory method para criar nova tabela
        /// </summary>
        /// <param name="tabela">Nome da tabela</param>
        /// <param name="camposDisponiveis">Lista de campos separados por vírgula</param>
        /// <param name="chavePk">Nome da coluna chave primária</param>
        /// <param name="vinculoEntreTabela">Vínculos com outras tabelas</param>
        /// <param name="descricaoTabela">Descrição da tabela</param>
        /// <param name="descricaoCampos">JSON com descrições dos campos</param>
        /// <param name="visivelParaIA">Se a tabela é visível para IA</param>
        /// <param name="notificationContext">Contexto de notificações para propagar erros de validação</param>
        /// <returns>Instância de TabelaDinamica (verificar IsValid antes de persistir)</returns>
        public static TabelaDinamica Criar(
            string tabela,
            string camposDisponiveis,
            string chavePk,
            string? vinculoEntreTabela = null,
            string? descricaoTabela = null,
            string? descricaoCampos = null,
            bool visivelParaIA = true,
            INotificationContext? notificationContext = null)
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

            // Propagar notificações para o contexto, se fornecido
            if (notificationContext != null && entity._notifications.Any())
            {
                notificationContext.AddNotifications(entity._notifications);
            }

            return entity;
        }

        // Métodos de atualização
        public void AtualizarCampos(string camposDisponiveis, INotificationContext? notificationContext = null)
        {
            if (string.IsNullOrWhiteSpace(camposDisponiveis))
            {
                var notification = new Notification("CamposDisponiveis", "Campos disponíveis não pode ser vazio");
                _notifications.Add(notification);
                notificationContext?.AddNotification(notification.Key, notification.Message);
                return;
            }

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

        // Validações de domínio usando Notification Pattern
        private void Validar()
        {
            if (string.IsNullOrWhiteSpace(Tabela))
                _notifications.Add(new Notification("Tabela", "Nome da tabela é obrigatório"));
            else if (Tabela.Length > 100)
                _notifications.Add(new Notification("Tabela", "Nome da tabela não pode ter mais de 100 caracteres"));

            if (string.IsNullOrWhiteSpace(CamposDisponiveis))
                _notifications.Add(new Notification("CamposDisponiveis", "Campos disponíveis é obrigatório"));

            if (string.IsNullOrWhiteSpace(ChavePk))
                _notifications.Add(new Notification("ChavePk", "Chave primária é obrigatória"));

            if (VinculoEntreTabela != null && VinculoEntreTabela.Length > 500)
                _notifications.Add(new Notification("VinculoEntreTabela", "Vínculo entre tabelas não pode ter mais de 500 caracteres"));
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
