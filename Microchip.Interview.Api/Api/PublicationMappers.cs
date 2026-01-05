using System.Linq;
using Microchip.Interview.Data.Models;

namespace Microchip.Interview.Api.Api
{
    public static class PublicationMappers
    {
        public static PublicationListItemDto ToListItem(Publication p) => new()
        {
            Id = p.Id.ToString(),
            PublicationType = p.PublicationType,
            Title = p.Title,
            Isbn = p.Isbn
        };

        public static PublicationDetailsDto ToDetails(Publication p) => new()
        {
            Id = p.Id.ToString(),
            PublicationType = p.PublicationType,
            Title = p.Title,
            Isbn = p.Isbn,
            Versions = p.Versions.Select(v => new PublicationVersionDto
            {
                Id = v.Id.ToString(),
                PublicationGuid = v.PublicationGuid.ToString(),
                Version = v.Version,
                Language = v.Language,
                CoverTitle = v.CoverTitle
            }).ToList()
        };
    }
}
