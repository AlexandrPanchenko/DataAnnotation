using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public class DataGenericRepository<T> : IGenericDataRepository<T> where T : class
{
    protected readonly ApplicationDataAccess.ApplicationDataContext _context;
    public DataGenericRepository(ApplicationDataAccess.ApplicationDataContext context)
    {
        _context = context;
    }
    public IQueryable<T> GetAll()
    {
        return _context.Set<T>();
    }
    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
    public void RemoveRange(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> Add(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }


    public async Task<T> GetById(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression);
    }

    public async Task AddRange(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
    }
    public Task<bool> Any(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().AnyAsync(expression);
    }
}