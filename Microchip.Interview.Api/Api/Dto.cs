using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Microchip.Interview.Api.Api
{
    // List item DTO: required fields only
    public sealed class PublicationListItemDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("publication_type")]
        public string PublicationType { get; set; } = default!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = default!;

        [JsonPropertyName("isbn")]
        public string Isbn { get; set; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;
    }

    // Details + versions
    public sealed class PublicationDetailsDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("publication_type")]
        public string PublicationType { get; set; } = default!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = default!;

        [JsonPropertyName("isbn")]
        public string Isbn { get; set; } = default!;

        [JsonPropertyName("versions")]
        public List<PublicationVersionDto> Versions { get; set; } = new();
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;
    }

    public sealed class PublicationVersionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("publication_guid")]
        public string PublicationGuid { get; set; } = default!;

        [JsonPropertyName("version")]
        public string Version { get; set; } = default!;

        [JsonPropertyName("language")]
        public string Language { get; set; } = default!;

        [JsonPropertyName("cover_title")]
        public string CoverTitle { get; set; } = default!;
    }
}
