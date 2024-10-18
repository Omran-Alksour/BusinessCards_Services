using Domain.Abstractions;

namespace Persistence
{
    internal sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDomainEventsToOutboxMessages();
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDomainEventsToOutboxMessages()
        {
        //For Example if we decide to use the Domain Events, Outbox Messages Pattern  with Background Jobs handling 
        }



    }
}