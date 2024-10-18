

using Domain.Entities.BusinessCard;
using Domain.Enums;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IBusinessCardRepository
{

    Task<Result<BusinessCard?>> CreateAsync(string name, Gender gender, Email email, string address, string phoneNumber, DateTime dateOfBirth, string photo, CancellationToken cancellationToken = default);

    Task<List<BusinessCard>> GetByIdsAsync(List<Guid>? IDs, CancellationToken cancellationToken = default);

    Task<BusinessCard?> GetByIdAsync(Guid ID, CancellationToken cancellationToken = default);

    Task<Result<List<Guid>>> DeleteAsync(List<Guid> IDs, bool forceDelete, CancellationToken cancellationToken = default);




    Task<Result<BusinessCard?>> UpdateAsync(Guid ID, string name, Gender gender, Email email, string address, string phoneNumber, DateTime dateOfBirth, string photo, CancellationToken cancellationToken = default);


    Task<PagedResponse<BusinessCard>?> ListAsync(int? pageNumber = null, int? pageSize = null, string? search = null, string? orderBy = null, string? orderDirection = "asc", CancellationToken cancellationToken = default);





}
