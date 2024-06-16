using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniServer.Data.Model;


namespace MiniServer.Data.Repository; 

public interface IUserRepository
{
    Task<bool> UserExistsAsync(string email, string name);
    Task<bool> CredsExistsAsync(string name, string password);
    Task CreateUserAsync(string name, string email, string password);
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

    public async Task CreateUserAsync(string name, string email, string password)
    {
        var user = new User
        {
            Username = name,
            Email = email,
            Password = password // In a real application, make sure to hash the password before storing it
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}