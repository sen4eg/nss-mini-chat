using Microsoft.EntityFrameworkCore;
using MiniServer.Data;
using MiniServer.Data.Repository;

namespace MiniServer.Services; 

public interface IPersistenceService {
    Task SaveAsync<T>(T entity) where T : class;
    Task UpdateAsync<T>(T entity) where T : class;
    Task DeleteAsync<T>(T entity) where T : class;
    Task<T> GetByIdAsync<T>(long id) where T : class;
    Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
}
public class PersistenceService : IPersistenceService {
    private readonly ChatContext _context;

    public PersistenceService(ChatContext context) {
        _context = context;
    }

    public async Task SaveAsync<T>(T entity) where T : class {
        var repository = new GenericRepository<T>(_context);
        await repository.SaveAsync(entity);
    }

    public async Task UpdateAsync<T>(T entity) where T : class {
        var repository = new GenericRepository<T>(_context);
        await repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync<T>(T entity) where T : class {
        var repository = new GenericRepository<T>(_context);
        await repository.DeleteAsync(entity);
    }

    public async Task<T> GetByIdAsync<T>(long id) where T : class {
        var repository = new GenericRepository<T>(_context);
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class {
        var repository = new GenericRepository<T>(_context);
        return await repository.GetAllAsync();
    }
}