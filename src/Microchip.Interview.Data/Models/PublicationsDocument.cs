using System.Xml.Serialization;

namespace Microchip.Interview.Data.Models;

/// <summary>
/// This class is for serialization purposes ONLY and should only be used for that reason
/// within this assembly. 
/// </summary>
[XmlRoot("publications")]
public class PublicationsDocument
{
    [XmlElement("publication")]
    public List<Publication> Items { get; set; } = new();
}
