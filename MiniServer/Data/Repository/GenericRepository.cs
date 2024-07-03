using Microsoft.EntityFrameworkCore;

namespace MiniServer.Data.Repository; 

public interface IGenericRepository<T> where T : class { // CRUD(E)L
    Task SaveAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<T> GetByIdAsync(long id);
    Task<IEnumerable<T>> GetAllAsync();
}

public class GenericRepository<T> : IGenericRepository<T> where T : class {
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context) {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task SaveAsync(T entity) {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity) {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity) {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<T> GetByIdAsync(long id) {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync() {
        return await _dbSet.ToListAsync();
    }
}