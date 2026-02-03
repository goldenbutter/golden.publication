using PublicationModel = Golden.Publication.Data.Models.Publication;

namespace Golden.Publication.Data;

public interface IPublicationRepository : IRepository<PublicationModel>
{
    Task<IReadOnlyList<PublicationModel>> GetByTypeAsync(string publicationType);

    Task<IReadOnlyList<PublicationModel>> GetByIsbnAsync(string isbn);
}