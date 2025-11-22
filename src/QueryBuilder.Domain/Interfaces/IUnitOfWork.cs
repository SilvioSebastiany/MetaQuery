using System.Data;

namespace QueryBuilder.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDbTransaction BeginTransaction();
        void Commit();
        void Rollback();
        IDbTransaction? Transaction { get; }
    }
}
