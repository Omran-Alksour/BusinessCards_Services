using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Domain.Extensions
{
    public class Pagination<T> where T : class
    {
        public static async Task<PagedResponse<T>> GetWithOffsetPagination(IQueryable<T> data, int? pageNumber, int? pageSize, CancellationToken cancellationToken = default)
        {

            int totalRecords = await data.CountAsync();
            pageSize = pageSize!=null? pageSize:15;

            List<T> entities = pageNumber.HasValue && pageSize.HasValue
                ? await data
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync()
                : await data.ToListAsync(cancellationToken);

            int currentPageNumber = pageNumber ?? 1;
            int currentPageSize = pageSize ?? totalRecords;

            PagedResponse<T> pagedResponse = new(entities, currentPageNumber, currentPageSize, totalRecords);

            return pagedResponse;
        }


    }
}
