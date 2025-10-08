using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Usr;

public class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context) { }

    public async Task<UserEntity?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<UserEntity?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<IEnumerable<UserEntity>> GetActiveUsersAsync()
        => await _context.Users.Where(u => u.IsActive).ToListAsync();
}