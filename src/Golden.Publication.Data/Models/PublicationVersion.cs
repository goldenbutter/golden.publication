using System.Xml.Serialization;

namespace Golden.Publication.Data.Models;

public class PublicationVersion
{
    [XmlElement("id")]
    public Guid Id { get; set; }

    [XmlElement("publication_guid")]
    public Guid PublicationGuid { get; set; }

    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;

    [XmlElement("language")]
    public string Language { get; set; } = "en-US";

    [XmlElement("cover_title")]
    public string CoverTitle { get; set; } = string.Empty;
}
