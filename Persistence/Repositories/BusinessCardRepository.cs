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



        public async Task<BusinessCard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.BusinessCards
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id.ToString(), cancellationToken);
        }


        public async Task<PagedResponse<BusinessCard>> ListAsync(int? pageNumber = null, int? pageSize = null, string? search = null, string? orderBy = null, string? orderDirection = "asc", CancellationToken cancellationToken = default)
        {
            IQueryable<BusinessCard> query = _context.BusinessCards.AsNoTracking()
                .Where(c => !c.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = ApplySearchFilters(query, search);
                pageNumber = 1;
            }


            if (!string.IsNullOrEmpty(orderBy))
            {
                query = query.AsQueryable().OrderByDynamic(orderBy, orderDirection);
            }
            else
            {
                query = query.OrderBy(s => s.Name);
            }

            return await Pagination<BusinessCard>.GetWithOffsetPagination(query, pageNumber, pageSize, cancellationToken);
        }

        private IQueryable<BusinessCard> ApplySearchFilters(IQueryable<BusinessCard> query, string search)
        {
            if (string.IsNullOrEmpty(search))
                return query;

            search = search.ToLower();

            int? genderCode = null;
            if ("male".Contains(search, StringComparison.OrdinalIgnoreCase) || search.Contains("male", StringComparison.OrdinalIgnoreCase))
            {
                genderCode = 1;
            }
            else if ("female".Contains(search, StringComparison.OrdinalIgnoreCase) || search.Contains("female", StringComparison.OrdinalIgnoreCase))
            {
                genderCode = 2;
            }

            var monthMapping = new Dictionary<int, string>
                    {
                        { 1, "Jan" }, { 2, "Feb" }, { 3, "Mar" }, { 4, "Apr" },
                        { 5, "May" }, { 6, "Jun" }, { 7, "Jul" }, { 8, "Aug" },
                        { 9, "Sep" }, { 10, "Oct" }, { 11, "Nov" }, { 12, "Dec" }
                    };
              
            var monthSearch = monthMapping.FirstOrDefault(x => x.Value.ToLower().Contains(search)).Key;

            return query.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                (monthSearch != 0 && c.DateOfBirth.Month == monthSearch) || 
                EF.Functions.Like(c.PhoneNumber, $"%{search}%") ||
                (genderCode.HasValue && c.Gender == genderCode) ||
                EF.Functions.Like(c.Email, $"%{search}%") ||

                c.DateOfBirth.Year.ToString().Contains(search) || 
                c.DateOfBirth.Day.ToString().Contains(search)
            );
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




        public async Task<Result<List<Guid>>> DeleteAsync(List<Guid> IDs, bool forceDelete, CancellationToken cancellationToken = default)
        {
            var deletedIds = new List<Guid>();

            var stringIds = IDs.Select(id => id.ToString()).ToList();

            var businessCards = await _context.BusinessCards
                .Where(c => stringIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!businessCards.Any())
            {
                return Result.Failure<List<Guid>>(ApplicationErrors.BusinessCards.Commands.BusinessCardNotFound);
            }

            foreach (var businessCard in businessCards)
            {
                if (forceDelete)
                {
                    _context.BusinessCards.Remove(businessCard);
                }
                else
                {
                    businessCard.Delete();
                    _context.BusinessCards.Update(businessCard);
                }

                deletedIds.Add(Guid.Parse(businessCard.Id));  
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<Guid>>.Success(deletedIds);
        }



        public Task<List<BusinessCard>> GetByIdsAsync(List<Guid>? IDs, CancellationToken cancellationToken = default)
        {
            IQueryable<BusinessCard> query = _context.BusinessCards.AsNoTracking();

            if (IDs == null || !IDs.Any())
            {
                query = query.Where(c => !c.IsDeleted);
            }
            else
            {
                var stringIds = IDs.Select(id => id.ToString()).ToList();
                query = query.Where(c => !c.IsDeleted && stringIds.Contains(c.Id)); 
            }

            return query.ToListAsync(cancellationToken);
        }

       
    
    }
}
