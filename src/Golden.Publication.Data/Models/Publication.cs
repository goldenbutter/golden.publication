using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Golden.Publication.Data.Models;

public class Publication
{
    [XmlElement("id")]
    public Guid Id { get; set; }

    [XmlElement("publication_type")]
    public string PublicationType { get; set; } = string.Empty;

    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("isbn")]
    public string Isbn { get; set; } = string.Empty;

    [XmlArray("versions")]
    [XmlArrayItem("version")]
    public List<PublicationVersion> Versions { get; set; } = new();
}
