using System;

namespace MetaQuery.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representando um campo de tabela com seus metadados
    /// </summary>
    public class CampoTabela : IEquatable<CampoTabela>
    {
        public string Nome { get; }
        public string? Tipo { get; }
        public string? Descricao { get; }
        public bool Obrigatorio { get; }

        public CampoTabela(string nome, string? tipo = null, string? descricao = null, bool obrigatorio = false)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do campo não pode ser vazio", nameof(nome));

            Nome = nome.Trim().ToUpper();
            Tipo = tipo?.Trim();
            Descricao = descricao?.Trim();
            Obrigatorio = obrigatorio;
        }

        public bool Equals(CampoTabela? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nome == other.Nome;
        }

        public override bool Equals(object? obj) => Equals(obj as CampoTabela);
        public override int GetHashCode() => Nome.GetHashCode();
        public override string ToString() => $"{Nome}{(Obrigatorio ? " *" : "")}";
    }

    /// <summary>
    /// Value Object representando um vínculo entre tabelas
    /// </summary>
    public class VinculoTabela : IEquatable<VinculoTabela>
    {
        public string TabelaDestino { get; }
        public string CampoOrigem { get; }
        public string CampoDestino { get; }
        public TipoJoin TipoJoin { get; }

        public VinculoTabela(string tabelaDestino, string campoOrigem, string campoDestino = "ID", TipoJoin tipoJoin = TipoJoin.Inner)
        {
            if (string.IsNullOrWhiteSpace(tabelaDestino))
                throw new ArgumentException("Tabela destino não pode ser vazia", nameof(tabelaDestino));
            if (string.IsNullOrWhiteSpace(campoOrigem))
                throw new ArgumentException("Campo origem não pode ser vazio", nameof(campoOrigem));

            TabelaDestino = tabelaDestino.Trim().ToUpper();
            CampoOrigem = campoOrigem.Trim().ToUpper();
            CampoDestino = campoDestino.Trim().ToUpper();
            TipoJoin = tipoJoin;
        }

        public static VinculoTabela ParseFromString(string vinculo)
        {
            // Formato esperado: "TabelaDestino.CampoOrigem" ou "TabelaDestino.CampoOrigem:CampoDestino"
            if (string.IsNullOrWhiteSpace(vinculo))
                throw new ArgumentException("Vínculo não pode ser vazio", nameof(vinculo));

            var partes = vinculo.Split('.');
            if (partes.Length != 2)
                throw new ArgumentException($"Formato de vínculo inválido: {vinculo}. Use 'Tabela.Campo'", nameof(vinculo));

            var tabelaDestino = partes[0].Trim();
            var campos = partes[1].Split(':');
            var campoOrigem = campos[0].Trim();
            var campoDestino = campos.Length > 1 ? campos[1].Trim() : "ID";

            return new VinculoTabela(tabelaDestino, campoOrigem, campoDestino);
        }

        public bool Equals(VinculoTabela? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return TabelaDestino == other.TabelaDestino && CampoOrigem == other.CampoOrigem;
        }

        public override bool Equals(object? obj) => Equals(obj as VinculoTabela);
        public override int GetHashCode() => HashCode.Combine(TabelaDestino, CampoOrigem);
        public override string ToString() => $"{TabelaDestino}.{CampoOrigem}";
    }

    /// <summary>
    /// Enum para tipo de JOIN
    /// </summary>
    public enum TipoJoin
    {
        Inner,
        Left,
        Right,
        Full
    }

    /// <summary>
    /// Value Object para descrição estruturada de metadados
    /// </summary>
    public class MetadadoDescricao
    {
        public string Titulo { get; }
        public string? Descricao { get; }
        public Dictionary<string, string>? DescricaoCampos { get; }

        public MetadadoDescricao(string titulo, string? descricao = null, Dictionary<string, string>? descricaoCampos = null)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("Título não pode ser vazio", nameof(titulo));

            Titulo = titulo.Trim();
            Descricao = descricao?.Trim();
            DescricaoCampos = descricaoCampos;
        }

        public string ObterDescricaoCampo(string nomeCampo)
        {
            if (DescricaoCampos == null) return string.Empty;
            return DescricaoCampos.TryGetValue(nomeCampo.ToUpper(), out var desc) ? desc : string.Empty;
        }

        public override string ToString() => Titulo;
    }
}
