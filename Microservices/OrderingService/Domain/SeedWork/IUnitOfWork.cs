using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderingService.Domain.SeedWork
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        bool SaveEntities();
    }
}