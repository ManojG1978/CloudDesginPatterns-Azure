using OrderingService.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace OrderingService.Infrastructure.Idempotency
{
    public class RequestManager : IRequestManager
    {
        public async Task<bool> ExistAsync(Guid id)
        {
            //Some implementation to check for duplicates
            return await Task.FromResult(false);
        }

        public async Task CreateRequestForCommandAsync<T>(Guid id)
        {
            var exists = await ExistAsync(id);

            var request = exists
                ? throw new OrderingDomainException($"Request with {id} already exists")
                : new ClientRequest
                {
                    Id = id,
                    Name = typeof(T).Name,
                    Time = DateTime.UtcNow
                };

        }
    }
}