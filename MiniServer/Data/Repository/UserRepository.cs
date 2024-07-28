using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniServer.Data.Model;
using Contact = MiniProtoImpl.Contact;


namespace MiniServer.Data.Repository; 

public interface IUserRepository
{
    Task<bool> UserExistsAsync(string email, string name);
    Task<bool> CredsExistsAsync(string name, string password);
    Task<User> CreateUserAsync(string name, string email, string password);
    ValueTask<User?> FindById(string id);
    Task<long?> FindWithCredentials(string name, string password);
    Task<List<Contact>> SearchUsersAsync(string requestQuery);
}

public class UserRepository : IUserRepository
{
    private readonly ChatContext _context;

    public UserRepository(ChatContext context)
    {
        _context = context;
    }

    public async Task<bool> UserExistsAsync(string email, string name)
    {
        return await _context.Users.AnyAsync(u => u.Email == email || u.Username == name);
    }

    public Task<bool> CredsExistsAsync(string name, string password) {
        return _context.Users.AnyAsync(u => u.Username == name && u.Password == password);
    }

    public async Task<User> CreateUserAsync(string name, string email, string password)
    {
        var user = new User
        {
            Username = name,
            Email = email,
            Password = password // In a real application, make sure to hash the password before storing it
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public ValueTask<User?> FindById(string id) {
        return _context.Users.FindAsync(id);
    }

    public Task<long?> FindWithCredentials(string name, string password) {
        return _context.Users
            .Where(u => u.Username == name && u.Password == password)
            .Select(u => (long?)u.UserId)
            .FirstOrDefaultAsync();
    }

    public Task<List<Contact>> SearchUsersAsync(string requestQuery) {
        return _context.Users
            .Where(u => u.Username.Contains(requestQuery) || u.Email.Contains(requestQuery))
            .Select(u => new Contact {
                Uid = u.UserId,
                Username = u.Username,
                Status = "Stranger" // Todo - implement status
            })
            .ToListAsync();
    }
}