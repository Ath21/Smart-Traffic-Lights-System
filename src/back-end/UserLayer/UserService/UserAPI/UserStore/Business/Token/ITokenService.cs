using System;
using UserData.Entities;

namespace UserStore.Business.Token;

public interface ITokenService
{
    (string toke, DateTime expiration) GenerateToken(User user);
}
