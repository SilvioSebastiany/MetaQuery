using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MetaQuery.Domain.Entities;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Infra.Data.DTOs;

namespace MetaQuery.Infra.Data.Repositories
{
    /// <summary>
    /// Repositório de metadados usando Dapper
    /// </summary>
    public class MetadadosRepository : IMetadadosRepository
    {
        private readonly IDbConnection _connection;

        // Cache de PropertyInfo para evitar reflection repetido
        private static readonly System.Reflection.PropertyInfo IdProperty =
            typeof(TabelaDinamica).GetProperty("Id") ?? throw new InvalidOperationException("Propriedade 'Id' não encontrada");

        private static readonly System.Reflection.PropertyInfo DataCriacaoProperty =
            typeof(TabelaDinamica).GetProperty("DataCriacao") ?? throw new InvalidOperationException("Propriedade 'DataCriacao' não encontrada");

        private static readonly System.Reflection.PropertyInfo DataAtualizacaoProperty =
            typeof(TabelaDinamica).GetProperty("DataAtualizacao") ?? throw new InvalidOperationException("Propriedade 'DataAtualizacao' não encontrada");

        public MetadadosRepository(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<TabelaDinamica?> ObterPorIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    ID as Id,
                    TABELA as Tabela,
                    CAMPOS_DISPONIVEIS as CamposDisponiveis,
                    CHAVE_PK as ChavePk,
                    VINCULO_ENTRE_TABELA as VinculoEntreTabela,
                    DESCRICAO_TABELA as DescricaoTabela,
                    DESCRICAO_CAMPOS as DescricaoCampos,
                    VISIVEL_PARA_IA as VisivelParaIa,
                    DATA_CRIACAO as DataCriacao,
                    DATA_ATUALIZACAO as DataAtualizacao,
                    ATIVO as Ativo
                FROM TABELA_DINAMICA
                WHERE ID = :Id";

            var dto = await _connection.QueryFirstOrDefaultAsync<MetadadoDto>(sql, new { Id = id });
            return dto != null ? MapToEntity(dto) : null;
        }

        public async Task<TabelaDinamica?> ObterPorNomeTabelaAsync(string nomeTabela)
        {
            const string sql = @"
                SELECT
                    ID as Id,
                    TABELA as Tabela,
                    CAMPOS_DISPONIVEIS as CamposDisponiveis,
                    CHAVE_PK as ChavePk,
                    VINCULO_ENTRE_TABELA as VinculoEntreTabela,
                    DESCRICAO_TABELA as DescricaoTabela,
                    DESCRICAO_CAMPOS as DescricaoCampos,
                    VISIVEL_PARA_IA as VisivelParaIa,
                    DATA_CRIACAO as DataCriacao,
                    DATA_ATUALIZACAO as DataAtualizacao,
                    ATIVO as Ativo
                FROM TABELA_DINAMICA
                WHERE UPPER(TABELA) = UPPER(:NomeTabela)";

            var dto = await _connection.QueryFirstOrDefaultAsync<MetadadoDto>(sql, new { NomeTabela = nomeTabela });
            return dto != null ? MapToEntity(dto) : null;
        }

        public async Task<IEnumerable<TabelaDinamica>> ObterTodosAsync(bool apenasAtivos = true)
        {
            var sql = @"
                SELECT
                    ID as Id,
                    TABELA as Tabela,
                    CAMPOS_DISPONIVEIS as CamposDisponiveis,
                    CHAVE_PK as ChavePk,
                    VINCULO_ENTRE_TABELA as VinculoEntreTabela,
                    DESCRICAO_TABELA as DescricaoTabela,
                    DESCRICAO_CAMPOS as DescricaoCampos,
                    VISIVEL_PARA_IA as VisivelParaIa,
                    DATA_CRIACAO as DataCriacao,
                    DATA_ATUALIZACAO as DataAtualizacao,
                    ATIVO as Ativo
                FROM TABELA_DINAMICA";

            if (apenasAtivos)
                sql += " WHERE ATIVO = 1";

            sql += " ORDER BY TABELA";

            var dtos = await _connection.QueryAsync<MetadadoDto>(sql);
            return dtos.Select(MapToEntity);
        }

        public async Task<IEnumerable<TabelaDinamica>> ObterVisiveisParaIAAsync()
        {
            const string sql = @"
                SELECT
                    ID as Id,
                    TABELA as Tabela,
                    CAMPOS_DISPONIVEIS as CamposDisponiveis,
                    CHAVE_PK as ChavePk,
                    VINCULO_ENTRE_TABELA as VinculoEntreTabela,
                    DESCRICAO_TABELA as DescricaoTabela,
                    DESCRICAO_CAMPOS as DescricaoCampos,
                    VISIVEL_PARA_IA as VisivelParaIa,
                    DATA_CRIACAO as DataCriacao,
                    DATA_ATUALIZACAO as DataAtualizacao,
                    ATIVO as Ativo
                FROM TABELA_DINAMICA
                WHERE VISIVEL_PARA_IA = 1 AND ATIVO = 1
                ORDER BY TABELA";

            var dtos = await _connection.QueryAsync<MetadadoDto>(sql);
            return dtos.Select(MapToEntity);
        }

        public async Task<int> CriarAsync(TabelaDinamica tabela)
        {
            const string sql = @"
                INSERT INTO TABELA_DINAMICA (
                    TABELA, CAMPOS_DISPONIVEIS, CHAVE_PK,
                    VINCULO_ENTRE_TABELA, DESCRICAO_TABELA,
                    DESCRICAO_CAMPOS, VISIVEL_PARA_IA,
                    DATA_CRIACAO, ATIVO
                ) VALUES (
                    :Tabela, :CamposDisponiveis, :ChavePk,
                    :VinculoEntreTabela, :DescricaoTabela,
                    :DescricaoCampos, :VisivelParaIA,
                    :DataCriacao, :Ativo
                )
                RETURNING ID INTO :Id";

            var parameters = new DynamicParameters();
            parameters.Add("Tabela", tabela.Tabela);
            parameters.Add("CamposDisponiveis", tabela.CamposDisponiveis);
            parameters.Add("ChavePk", tabela.ChavePk);
            parameters.Add("VinculoEntreTabela", tabela.VinculoEntreTabela);
            parameters.Add("DescricaoTabela", tabela.DescricaoTabela);
            parameters.Add("DescricaoCampos", tabela.DescricaoCampos);
            parameters.Add("VisivelParaIA", tabela.VisivelParaIA ? 1 : 0);
            parameters.Add("DataCriacao", DateTime.Now);
            parameters.Add("Ativo", tabela.Ativo ? 1 : 0);
            parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync(sql, parameters);
            return parameters.Get<int>("Id");
        }

        public async Task AtualizarAsync(TabelaDinamica tabela)
        {
            const string sql = @"
                UPDATE TABELA_DINAMICA SET
                    CAMPOS_DISPONIVEIS = :CamposDisponiveis,
                    CHAVE_PK = :ChavePk,
                    VINCULO_ENTRE_TABELA = :VinculoEntreTabela,
                    DESCRICAO_TABELA = :DescricaoTabela,
                    DESCRICAO_CAMPOS = :DescricaoCampos,
                    VISIVEL_PARA_IA = :VisivelParaIA,
                    DATA_ATUALIZACAO = :DataAtualizacao,
                    ATIVO = :Ativo
                WHERE ID = :Id";

            await _connection.ExecuteAsync(sql, new
            {
                Id = tabela.Id,
                CamposDisponiveis = tabela.CamposDisponiveis,
                ChavePk = tabela.ChavePk,
                VinculoEntreTabela = tabela.VinculoEntreTabela,
                DescricaoTabela = tabela.DescricaoTabela,
                DescricaoCampos = tabela.DescricaoCampos,
                VisivelParaIA = tabela.VisivelParaIA ? 1 : 0,
                DataAtualizacao = DateTime.Now,
                Ativo = tabela.Ativo ? 1 : 0
            });
        }

        public async Task DeletarAsync(int id)
        {
            const string sql = "DELETE FROM TABELA_DINAMICA WHERE ID = :Id";
            await _connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExisteAsync(string nomeTabela)
        {
            const string sql = "SELECT COUNT(*) FROM TABELA_DINAMICA WHERE UPPER(TABELA) = UPPER(:NomeTabela)";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { NomeTabela = nomeTabela });
            return count > 0;
        }

        public async Task<IEnumerable<TabelaDinamica>> ObterPorVinculoAsync(string nomeTabela)
        {
            const string sql = @"
                SELECT
                    ID as Id,
                    TABELA as Tabela,
                    CAMPOS_DISPONIVEIS as CamposDisponiveis,
                    CHAVE_PK as ChavePk,
                    VINCULO_ENTRE_TABELA as VinculoEntreTabela,
                    DESCRICAO_TABELA as DescricaoTabela,
                    DESCRICAO_CAMPOS as DescricaoCampos,
                    VISIVEL_PARA_IA as VisivelParaIa,
                    DATA_CRIACAO as DataCriacao,
                    DATA_ATUALIZACAO as DataAtualizacao,
                    ATIVO as Ativo
                FROM TABELA_DINAMICA
                WHERE VINCULO_ENTRE_TABELA LIKE :Pattern AND ATIVO = 1";

            var dtos = await _connection.QueryAsync<MetadadoDto>(sql, new { Pattern = $"%{nomeTabela}.%" });
            return dtos.Select(MapToEntity);
        }

        /// <summary>
        /// Mapeia o DTO tipado para a entidade TabelaDinamica
        /// Elimina uso de dynamic e reflection - compile-time type safety
        /// </summary>
        private static TabelaDinamica MapToEntity(MetadadoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validação de campos obrigatórios
            if (string.IsNullOrWhiteSpace(dto.Tabela))
                throw new InvalidOperationException("TABELA não pode ser null ou vazia");
            if (string.IsNullOrWhiteSpace(dto.CamposDisponiveis))
                throw new InvalidOperationException("CAMPOS_DISPONIVEIS não pode ser null ou vazio");
            if (string.IsNullOrWhiteSpace(dto.ChavePk))
                throw new InvalidOperationException("CHAVE_PK não pode ser null ou vazia");

            try
            {
                // Conversão de NUMBER(1) para boolean (Oracle: 0=false, 1=true)
                bool visivelParaIA = dto.VisivelParaIa != 0;

                // Criar entidade usando factory method
                var entity = TabelaDinamica.Criar(
                    tabela: dto.Tabela,
                    camposDisponiveis: dto.CamposDisponiveis,
                    chavePk: dto.ChavePk,
                    vinculoEntreTabela: dto.VinculoEntreTabela,
                    descricaoTabela: dto.DescricaoTabela,
                    descricaoCampos: dto.DescricaoCampos,
                    visivelParaIA: visivelParaIA
                );

                // Setar propriedades privadas usando PropertyInfo cacheados
                IdProperty.SetValue(entity, dto.Id);
                DataCriacaoProperty.SetValue(entity, dto.DataCriacao);

                if (dto.DataAtualizacao.HasValue)
                {
                    DataAtualizacaoProperty.SetValue(entity, dto.DataAtualizacao.Value);
                }

                // ⚠️ IMPORTANTE: Mapear o campo ATIVO corretamente do banco
                // Oracle retorna 0/1 como NUMBER, não como boolean
                if (dto.Ativo == 0)
                {
                    // Se está inativo no banco (ATIVO = 0), desativar a entidade
                    entity.Desativar();
                }

                return entity;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Erro ao mapear MetadadoDto para TabelaDinamica. Tabela: {dto.Tabela}. Erro: {ex.Message}",
                    ex);
            }
        }
    }
}
