using Domain.Entities.BusinessCard;
using Domain.Enums;
using Domain.Errors;
using Domain.Extensions;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public sealed class BusinessCardRepository : BaseRepository, IBusinessCardRepository
    {
        private readonly ApplicationDbContext _context;

        public BusinessCardRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }


        public async Task<Result<BusinessCard?>> CreateAsync(string name, Gender gender, Email email, string address, string phoneNumber, DateTime dateOfBirth, string photo, CancellationToken cancellationToken = default)
        {

            BusinessCard? BusinessCard = await _context.BusinessCards.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Email.Trim().ToLower() == email.Value.Trim().ToLower(), cancellationToken);

            if (BusinessCard is null)
            {
                BusinessCard = new BusinessCard(Guid.NewGuid(), name, (int)gender, email.Value, address, phoneNumber, dateOfBirth, photo);

                var entityEntry = await _context.Set<BusinessCard>().AddAsync(BusinessCard, cancellationToken);
                _ = await _context.SaveChangesAsync(cancellationToken);

                BusinessCard = entityEntry.Entity;
            }
            else//Update existing
            {
                BusinessCard = await UpdateAsync(Guid.Parse(BusinessCard.Id), name, gender, email, address, phoneNumber, dateOfBirth, photo, cancellationToken);
            }

            return Result<BusinessCard?>.Success(BusinessCard);
        }

        public Task<Result<List<Guid>>> DeleteAsync(List<Guid> IDs, bool forceDelete, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BusinessCard?> GetByIdAsync(Guid ID, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<BusinessCard>> GetByIdsAsync(List<Guid>? IDs, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<BusinessCard>?> ListAsync(int? pageNumber = null, int? pageSize = null, string? search = null, string? orderBy = null, string? orderDirection = "asc", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<BusinessCard?>> UpdateAsync(Guid id, string name, Gender gender, Email email, string address, string phoneNumber, DateTime dateOfBirth, string photo, CancellationToken cancellationToken = default)
        {
            BusinessCard? businessCard = await _context.BusinessCards
               .SingleOrDefaultAsync(c => c.Id == id.ToString() && c.Email.ToLower().Trim() == email.Value.ToLower().Trim(), cancellationToken);

            IQueryable<BusinessCard> query = _context.BusinessCards.AsNoTracking()
             .Where(c => !c.IsDeleted);

            var users = query.ToList();
            if (businessCard is null)
                return Result.Failure<BusinessCard?>(ApplicationErrors.BusinessCards.Commands.BusinessCardNotFound);


            businessCard.Name = name;
            businessCard.Gender = (int)gender;
            businessCard.DateOfBirth = dateOfBirth;
            businessCard.Email = email.Value;
            businessCard.PhoneNumber = phoneNumber;
            businessCard.Address = address;
            businessCard.Photo = photo;

            _ = _context.BusinessCards.Update(businessCard);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(businessCard);
        }



       
    
    }
}
