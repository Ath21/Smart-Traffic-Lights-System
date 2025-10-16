using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Usr;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    public UserRepository(UserDbContext context) => _context = context;

    public async Task<UserEntity?> GetByIdAsync(int id)
        => await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

    public async Task<UserEntity?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<UserEntity?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<IEnumerable<UserEntity>> GetActiveUsersAsync()
        => await _context.Users.Where(u => u.IsActive).ToListAsync();

    public async Task InsertAsync(UserEntity entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserEntity entity)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserEntity entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string username, string email)
        => await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
}