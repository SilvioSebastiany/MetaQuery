using System.Data;

namespace MetaQuery.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDbTransaction BeginTransaction();
        void Commit();
        void Rollback();
        IDbTransaction? Transaction { get; }
    }
}
