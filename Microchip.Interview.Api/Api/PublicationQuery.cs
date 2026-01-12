using System.ComponentModel.DataAnnotations;

namespace Microchip.Interview.Api.Api
{
    public sealed class PublicationQuery
    {
        // Searching / filtering
        public string? Title { get; set; }
        public string? Isbn { get; set; }
        public string? Description { get; set; }

        // Paging
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        // Sorting by one or more fields: "title,publication_type,isbn"
        public string? SortBy { get; set; }

        // Direction across all fields
        public string SortDir { get; set; } = "asc";
    }
}
