using System.Linq.Expressions;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IGenericDataRepository<T> where T : class
{
    Task<T> GetById(int id);
    IQueryable<T> GetAll();
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> Find(Expression<Func<T, bool>> expression);
    Task<T> Add(T entity);
    Task AddRange(IEnumerable<T> entities);
    void Remove(T entity);
    public Task<bool> Any(Expression<Func<T, bool>> expression);
}