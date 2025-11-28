using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Microchip.Interview.Data;

public interface IRepository<T>
{
    Task<T?> SingleAsync(Guid id);

    Task<IReadOnlyList<T>> WhereAsync(Expression<Func<T, bool>> predicate);

    Task<IReadOnlyList<T>> WhereAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
}
