using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PublicationModel = Golden.Publication.Data.Models.Publication;

namespace Golden.Publication.Data;

public sealed class EfPublicationRepository : IPublicationRepository
{
    private readonly PublicationDbContext _context;

    public EfPublicationRepository(PublicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicationModel?> SingleAsync(Guid id)
    {
        return await _context.Publications
            .Include(p => p.Versions)
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<PublicationModel>> WhereAsync(
        Expression<Func<PublicationModel, bool>> predicate)
    {
        return await _context.Publications
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PublicationModel>> WhereAsync(
        Expression<Func<PublicationModel, bool>> predicate,
        Func<IQueryable<PublicationModel>, IOrderedQueryable<PublicationModel>> orderBy)
    {
        var query = _context.Publications.Where(predicate);
        return await orderBy(query).ToListAsync();
    }

    public async Task<IReadOnlyList<PublicationModel>> GetByTypeAsync(string publicationType)
    {
        return await _context.Publications
            .Where(p => p.PublicationType.ToLower() == publicationType.ToLower())
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PublicationModel>> GetByIsbnAsync(string isbn)
    {
        return await _context.Publications
            .Where(p => p.Isbn.ToLower() == isbn.ToLower())
            .ToListAsync();
    }
}
