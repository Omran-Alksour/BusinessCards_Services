using Domain.Errors;
using Domain.Primitives;
using Domain.Shared;

namespace Persistence.Repositories;

public class BaseRepository
{
    private readonly ApplicationDbContext _context;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Result> Delete<T>(EntityT<T> entityToDelete)
    {
        if (entityToDelete is IValidatable validatable)
        {
            _ = validatable.ValidateRemove();
        }

        if (entityToDelete is IDeletable entity)
        {
            entity.Delete();
            return Task.FromResult(Result.Success());
        }

        _ = _context.Remove(entityToDelete!);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> UnDelete<T>(EntityT<T> entityToDelete)
    {
        if (entityToDelete is IDeletable entity)
        {
            entity.UnDelete();
            return Task.FromResult(Result.Success());
        }
        return Task.FromResult(Result.Failure(PersistenceErrors.BaseRepository.UnDelete.EntityNotIDeletable));
    }
}