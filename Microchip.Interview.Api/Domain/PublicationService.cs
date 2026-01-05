using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microchip.Interview.Api.Api;
using Microchip.Interview.Data;
using Microchip.Interview.Data.Models;

namespace Microchip.Interview.Api.Domain
{
    public sealed class PublicationService
    {
        private readonly IPublicationRepository _repo;

        public PublicationService(IPublicationRepository repo)
        {
            _repo = repo;
        }

        public async Task<(IReadOnlyList<Publication> Items, int Total)> SearchAsync(PublicationQuery q)
        {
            // Load all (no filter) then apply searching/sorting/paging here
            var all = await _repo.WhereAsync(p => true);

            // Searching: Title contains
            if (!string.IsNullOrWhiteSpace(q.Title))
            {
                var t = q.Title.Trim();
                all = all.Where(p => p.Title.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Searching: ISBN contains
            if (!string.IsNullOrWhiteSpace(q.Isbn))
            {
                var i = q.Isbn.Trim();
                all = all.Where(p => p.Isbn.Contains(i, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(q.SortBy))
            {
                var fields = q.SortBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var desc = q.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

                IOrderedEnumerable<Publication>? ordered = null;

                foreach (var f in fields)
                {
                    Func<Publication, object?> keySelector = f.ToLowerInvariant() switch
                    {
                        "title" => p => p.Title,
                        "publication_type" => p => p.PublicationType,
                        "isbn" => p => p.Isbn,
                        _ => p => p.Title
                    };

                    ordered = ordered == null
                        ? (desc ? all.OrderByDescending(keySelector) : all.OrderBy(keySelector))
                        : (desc ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector));
                }

                if (ordered != null)
                    all = ordered.ToList();
            }

            var total = all.Count;

            // Paging
            var skip = (q.PageNumber - 1) * q.PageSize;
            var page = all.Skip(skip).Take(q.PageSize).ToList();

            return (page, total);
        }

        public Task<Publication?> GetDetailsAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid))
                return Task.FromResult<Publication?>(null);

            return _repo.SingleAsync(guid);
        }
    }
}

