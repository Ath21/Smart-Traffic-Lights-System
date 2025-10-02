using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TrafficLightData.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) =>
        await _dbSet.AddAsync(entity);

    public async Task UpdateAsync(T entity) =>
        _dbSet.Update(entity);

    public async Task DeleteAsync(T entity) =>
        _dbSet.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}