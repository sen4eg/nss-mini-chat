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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PersistenceService(IServiceScopeFactory serviceScopeFactory) {
        _serviceScopeFactory = serviceScopeFactory;
    }

    private async Task<TContext> CreateContextAsync<TContext>() where TContext : DbContext {
        // Create a new scope
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        return context;
    }

    public async Task SaveAsync<T>(T entity) where T : class {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatContext>();
        var repository = new GenericRepository<T>(context);
        await repository.SaveAsync(entity);
    }

    public async Task UpdateAsync<T>(T entity) where T : class {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatContext>();
        var repository = new GenericRepository<T>(context);
        await repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync<T>(T entity) where T : class {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatContext>();
        var repository = new GenericRepository<T>(context);
        await repository.DeleteAsync(entity);
    }

    public async Task<T> GetByIdAsync<T>(long id) where T : class {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatContext>();
        var repository = new GenericRepository<T>(context);
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatContext>();
        var repository = new GenericRepository<T>(context);
        return await repository.GetAllAsync();
    }
}