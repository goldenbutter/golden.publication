using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Golden.Publication.Api;
using Golden.Publication.Data;
// using Golden.Publication.Data.Models;

namespace Golden.Publication.Api.Domain
{
    public sealed class PublicationService
    {
        private readonly IPublicationRepository _repo;

        public PublicationService(IPublicationRepository repo)
        {
            _repo = repo;
        }

        // Note the alias types below
        public async Task<(IReadOnlyList<PublicationModel> Items, int Total)> SearchAsync(PublicationQuery q)
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

            // Searching: Description contains
            if (!string.IsNullOrWhiteSpace(q.Description))
            {
                var d = q.Description.Trim();
                all = all.Where(p => p.Description.Contains(d, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(q.SortBy))
            {
                var fields = q.SortBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var desc = q.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

                IOrderedEnumerable<PublicationModel>? ordered = null;

                foreach (var f in fields)
                {
                    Func<PublicationModel, object?> keySelector = f.ToLowerInvariant() switch
                    {
                        "title"             => p => p.Title,
                        "publication_type"  => p => p.PublicationType,
                        "isbn"              => p => p.Isbn,
                        "description"       => p => p.Description,
                        _                   => p => p.Title
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

        public Task<PublicationModel?> GetDetailsAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid))
                return Task.FromResult<PublicationModel?>(null);

            return _repo.SingleAsync(guid);
        }
    }
}