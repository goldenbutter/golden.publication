using Microchip.Interview.Data.Models;

namespace Microchip.Interview.Data;

public interface IPublicationRepository : IRepository<Publication>
{
    Task<IReadOnlyList<Publication>> GetByTypeAsync(string publicationType);

    Task<IReadOnlyList<Publication>> GetByIsbnAsync(string isbn);
}
