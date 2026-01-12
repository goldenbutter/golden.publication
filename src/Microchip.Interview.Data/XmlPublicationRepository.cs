using Microchip.Interview.Data.Models;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Microchip.Interview.Data;

public sealed class XmlPublicationRepository : IPublicationRepository
{
    private readonly List<Publication> _publications;

    public XmlPublicationRepository(string xmlFilePath)
    {
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException("publications.xml not found.", xmlFilePath);

        var serializer = new XmlSerializer(typeof(PublicationsDocument));
        using var stream = File.OpenRead(xmlFilePath);
        var document = (PublicationsDocument)serializer.Deserialize(stream)!;

        _publications = document.Items ?? [];

        // 🔥 DEBUG: Print all loaded publications 
        Console.WriteLine("=== DEBUG: Loaded Publications ==="); 
        foreach (var p in _publications) 
        { 
                Console.WriteLine($"Publication: ID={p.Id} | Title={p.Title}");
        } 
         Console.WriteLine("=== END DEBUG ===");
    }

    public Task<Publication?> SingleAsync(Guid id)
    {
        var result = _publications.SingleOrDefault(p => p.Id == id);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Publication>> WhereAsync(Expression<Func<Publication, bool>> predicate)
    {
        var query = _publications.AsQueryable().Where(predicate);
        IReadOnlyList<Publication> result = query.ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Publication>> WhereAsync(
        Expression<Func<Publication, bool>> predicate,
        Func<IQueryable<Publication>, IOrderedQueryable<Publication>> orderBy)
    {
        var query = _publications.AsQueryable().Where(predicate);
        var ordered = orderBy(query);
        IReadOnlyList<Publication> result = ordered.ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Publication>> GetByTypeAsync(string publicationType)
    {
        var result = _publications
            .Where(p => p.PublicationType.Equals(publicationType, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<Publication>)result);
    }

    public Task<IReadOnlyList<Publication>> GetByIsbnAsync(string isbn)
    {
        var result = _publications
            .Where(p => p.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<Publication>)result);
    }
}