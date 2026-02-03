using System.Linq;

namespace Golden.Publication.Api
{
    public static class PublicationMappers
    {
        public static PublicationListItemDto ToListItem(PublicationModel p) => new()
        {
            Id = p.Id.ToString(),
            PublicationType = p.PublicationType,
            Title = p.Title,
            Isbn = p.Isbn,
            Description = p.Description
        };

        public static PublicationDetailsDto ToDetails(PublicationModel p) => new()
        {
            Id = p.Id.ToString(),
            PublicationType = p.PublicationType,
            Title = p.Title,
            Isbn = p.Isbn,
            Description = p.Description,
            Versions = p.Versions.Select(v => new PublicationVersionDto
            {
                Id = v.Id.ToString(),
                PublicationGuid = v.PublicationGuid.ToString(),
                Version = v.Version,
                Language = v.Language,
                CoverTitle = v.CoverTitle
            }).ToList()
        };

        public static PublicationVersionDto ToVersion(PublicationVersionModel v) => new()
        {
            Id = v.Id.ToString(),
            PublicationGuid = v.PublicationGuid.ToString(),
            Version = v.Version,
            Language = v.Language,
            CoverTitle = v.CoverTitle
        };
    }
}