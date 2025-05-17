using System;

namespace UserStore.Business.PasswordHasher;

public interface IPasswordHasher
{
    bool VerifyPassword(string hash, string password);
    string HashPassword(string password);
}
